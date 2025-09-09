
using FellowOakDicom;
using FellowOakDicom.Media;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Helpers;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DicomUtility.Transfer;
using NV.CT.DicomUtility.Transfer.DicomDataModifier;
using NV.MPS.Exception;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace NV.CT.JobService
{
    public class ExportToRawDataExecutor : ITaskExecutor
    {
        private readonly ILogger _logger;
        private readonly IRawDataService _rawDataService;
        private readonly IStudyService _studyService;
        private readonly IPatientService _patientService;
        private readonly IScanTaskService _scanTaskService;
        private readonly IReconTaskService _reconTaskService;
        private readonly ISeriesService _seriesService;
        private List<string> _fileList;
        private string _patientNameListString = string.Empty;
        private string _jobTaskID = string.Empty;
        public event EventHandler<ExecuteStatusInfo>? ExecuteStatusChanged;

        private volatile CancellationTokenSource _cancellationTokenSource;

        public List<string> SrcPaths { get; }

        public List<string> RTDDicomPaths { get; }

        public List<string> RawDataIDList { get; }

        public string DestRootPath { get; }

        public string PatientId { get; }

        public string StudyId { get; }

        public int ProcessedCount { get; private set; }

        public int TotalCount => _fileList.Count;

        public string PatientNameListString => this._patientNameListString;

        public void Cancel()
        {
            if (this._cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public void Start()
        {
            ActivateExport();
        }
        public ExportToRawDataExecutor(ILogger logger,
                                     CancellationTokenSource cancellationTokenSource, string jobTaskID,
                                     string[] patientNames,
                                     string[] srourcePaths,
                                     string[] idList,
                                     string[] rtdDicomList,
                                     string destRootPath, 
                                     IRawDataService rawDataService,
                                     IStudyService studyService,
                                     IPatientService patientService,
                                     IScanTaskService scanTaskService,
                                     IReconTaskService reconTaskService,
                                     ISeriesService seriesService,
                                     string studyId)
        {
            _logger = logger;
            _cancellationTokenSource = cancellationTokenSource;
            _jobTaskID = jobTaskID;
            _rawDataService = rawDataService;
            _studyService = studyService;
            _patientService = patientService;
            _scanTaskService = scanTaskService;
            _reconTaskService = reconTaskService;
            _seriesService = seriesService;
            _fileList = new List<string>();
            SrcPaths = new List<string>();
            RTDDicomPaths = new List<string>();
            RawDataIDList =new List<string>();
            RawDataIDList.AddRange(idList);
            SrcPaths.AddRange(srourcePaths);
            RTDDicomPaths.AddRange(rtdDicomList);
            DestRootPath = destRootPath;
            _patientNameListString = string.Join(",", patientNames);
            StudyId = studyId;
            InitExportList();
            InitExportDicomList();
            PatientId = GetPatientId(studyId);
        }
        private void CheckIfAskedToCancel()
        {
            if (this._cancellationTokenSource is null)
            {
                return;
            }

            if (this._cancellationTokenSource.Token.IsCancellationRequested)
            {
                //收到取消通知后，立即通知Processor取消后续处理
                this._logger.LogTrace("ExportToRawDataExecutor received cancellation request.");
                Trace.WriteLine("=== ExportToRawDataExecutor : cancelled.");
                this._cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }
        private void InitExportList()
        {
            //处理所有Export源目录中的文件内容，添加到待导出列表。
            foreach (var srcPath in SrcPaths)
            {
                this.CheckIfAskedToCancel();
                if (Directory.Exists(srcPath))
                {
                    //默认当前文件夹下所有文件需要传输，不考虑后缀名
                    var dir = new DirectoryInfo(srcPath);
                    _fileList.AddRange(dir.GetFiles("*.*", SearchOption.AllDirectories).Select(x => x.FullName).ToList());
                }
                else if (File.Exists(srcPath))
                {
                    _fileList.Add(srcPath);
                }
                else
                {
                    _logger.LogError($"[ExportToRawDataExecutor] The source folder does not exist:{srcPath}");
                    throw new DirectoryNotFoundException(srcPath);
                }
            }
        }
        private void InitExportDicomList()
        {
            //处理所有Export源目录中的文件内容，添加到待导出列表。
            foreach (var srcPath in RTDDicomPaths)
            {
                this.CheckIfAskedToCancel();
                if (Directory.Exists(srcPath))
                {
                    //默认当前文件夹下所有文件需要传输，不考虑后缀名
                    var dir = new DirectoryInfo(srcPath);
                    _fileList.AddRange(dir.GetFiles("*.*", SearchOption.AllDirectories).Select(x => x.FullName).ToList());
                }
                else if (File.Exists(srcPath))
                {
                    _fileList.Add(srcPath);
                }
                else
                {
                    _logger.LogError($"[ExportToRawDataExecutor] The source folder does not exist:{srcPath}");
                    throw new DirectoryNotFoundException(srcPath);
                }
            }
        }
        private void ActivateExport()
        {
            _logger.LogInformation($"[ExportToRawDataExecutor] Start export from sources: {string.Join("//", SrcPaths)} ");

            ProcessedCount = 0;
            //发送导出开始通知
            ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Started, this._patientNameListString));
            this.CheckIfAskedToCancel();
            try
            {
                 CopyToDisk();
                 var saveDir=CopyDicomToDisk();
                 SaveReconJson(StudyId,saveDir);
                //发送导入结束通知
                _logger.LogInformation($"[ExportToRawDataExecutor] finished for {_patientNameListString}, progress is:{ProcessedCount}/{TotalCount}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Succeeded, _patientNameListString));
            }
            catch (OperationCanceledException canceledException)
            {
                this._logger.LogWarning($"ExportToRawDataExecutor is cancelled for [{_patientNameListString}], the exception is: {canceledException.Message}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Cancelled, _patientNameListString));
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ExportToRawDataExecutor]:Failed to export for:{_patientNameListString}, the error message is:{ex.Message}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Failed, _patientNameListString, ex.Message));
            }
        }

        private void CopyToDisk()
        {
            if (SrcPaths is null)
            {
                return;
            }
            int index = 0;
            foreach (var srcPath in SrcPaths)
            {
                this.CheckIfAskedToCancel();
                var dir = new DirectoryInfo(srcPath);
                var studyInstanceUIDDir = dir?.Parent?.Name;
                var scanUIDDir = dir?.Name;
                if (studyInstanceUIDDir is null || scanUIDDir is null)
                    continue;
                string rootDir = Path.Combine(DestRootPath, PatientId, studyInstanceUIDDir, scanUIDDir);
                if (File.Exists(srcPath))
                {
                    ProcessedCount=FileOperationHelper.CopyFile(srcPath, rootDir, ProcessedCount);
                }
                else if (Directory.Exists(srcPath))
                {
                    ProcessedCount=FileOperationHelper.CopyFolder(srcPath, rootDir, ProcessedCount);
                }
                this._rawDataService.UpdateExportStatusById(RawDataIDList[index], true);
                index++;
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.InProgress, this._patientNameListString));
            }
        }
        private string CopyDicomToDisk()
        {
            if (RTDDicomPaths is null)
            {
                return string.Empty;
            }
            string saveDir = string.Empty;
            foreach (var srcPath in RTDDicomPaths)
            {
                this.CheckIfAskedToCancel();
                var dir = new DirectoryInfo(srcPath);
                var studyInstanceUIDDir = dir?.Parent?.Name;
                var seriesInstanceUIDDir = dir?.Name;
                if (studyInstanceUIDDir is null || seriesInstanceUIDDir is null)
                    continue;
                string rootDir = Path.Combine(DestRootPath, PatientId, studyInstanceUIDDir, seriesInstanceUIDDir);
                saveDir= Path.Combine(DestRootPath, PatientId, studyInstanceUIDDir); 
                if (File.Exists(srcPath))
                {
                    ProcessedCount = FileOperationHelper.CopyFile(srcPath, rootDir, ProcessedCount);
                }
                else if (Directory.Exists(srcPath))
                {
                    ProcessedCount = FileOperationHelper.CopyFolder(srcPath, rootDir, ProcessedCount);
                }
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.InProgress, this._patientNameListString));
            }
            return saveDir;
        }
        private void SaveReconJson(string studyId,string dir)
        {
            var studyEntity = _studyService.GetStudyById(studyId);
            var patientEntity= _patientService.GetPatientById(studyEntity.InternalPatientId);
            var rawDataModelList = _rawDataService.GetRawDataListByStudyId(studyId);
            var scanTaskModelList = _scanTaskService.GetAll(studyId);
            var reconTaskModelList = _reconTaskService.GetAll(studyId).FindAll(r => r.IsRTD == true);
            var seriesModelList = _seriesService.GetTopoTomoSeriesByStudyId(studyId);
            WriteJson(dir, "study.json", studyEntity);
            WriteJson(dir, "patient.json", patientEntity);
            WriteJson(dir, "rawdata.json", rawDataModelList);
            WriteJson(dir, "scantask.json", scanTaskModelList);
            WriteJson(dir, "recontask.json", reconTaskModelList);
            WriteJson(dir, "series.json", seriesModelList);
        }
        private void WriteJson(string dir,string fileName, object obj)
        {
            string fileFullName = Path.Combine(dir, fileName);
            File.WriteAllText(fileFullName, JsonConvert.SerializeObject(obj));
        }
        private string GetPatientId(string studyId)
        {
            var studyEntity = _studyService.GetStudyById(studyId);
            var patientEntity = _patientService.GetPatientById(studyEntity.InternalPatientId);
            return patientEntity.PatientId;
        }
    }
}
