using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Helpers;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.DicomUtility.Transfer;
using System.Diagnostics;
using System.IO;

namespace NV.CT.JobService
{
    public class ImportByRawDataExecutor : IImportTaskExecutor
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IRawDataService _rawDataService;
        private readonly IStudyService _studyService;
        private readonly IPatientService _patientService;
        private readonly IScanTaskService _scanTaskService;
        private readonly IReconTaskService _reconTaskService;
        private readonly ISeriesService _seriesService;
        public event EventHandler<ExecuteStatusInfo>? ExecuteStatusChanged;
        private volatile CancellationTokenSource _cancellationTokenSource;

        private string _jobTaskID = string.Empty;
        private string? _sourceRootPath;
        private string? _targetRootPath;
        private string? _patientNameListString= string.Empty;
        public string PatientNameListString => this._patientNameListString;
        private List<string> _fileList = new();
        public int ProcessedCount { get; private set; }
        public int TotalCount => _fileList.Count-6;

        public List<string> SrcPaths { get; }

        public List<string> RTDDicomPaths { get; }

        public ImportByRawDataExecutor(string jobTaskID, string sourceRootPath, string targetRootPath, ILogger logger, CancellationTokenSource cancellationTokenSource,
                                                IRawDataService rawDataService,
                                     IStudyService studyService,
                                     IPatientService patientService,
                                     IScanTaskService scanTaskService,
                                     IReconTaskService reconTaskService,
                                     ISeriesService seriesService,
                                     IMapper mapper)
        {
            _jobTaskID = jobTaskID;
            _sourceRootPath = sourceRootPath;
            _targetRootPath = targetRootPath;
            _logger = logger;
            _cancellationTokenSource = cancellationTokenSource;
            _rawDataService = rawDataService;
            _studyService = studyService;
            _patientService = patientService;
            _scanTaskService = scanTaskService;
            _reconTaskService = reconTaskService;
            _seriesService = seriesService;
            _mapper = mapper;
            SrcPaths = new List<string>();
            RTDDicomPaths = new List<string>();
            InitFilesList();

        }
        private void InitFilesList()
        {
            //处理所有Export源目录中的文件内容，添加到待导出列表。
            if (Directory.Exists(this._sourceRootPath))
            {
                //默认当前文件夹下所有文件需要传输，不考虑后缀名
                var dir = new DirectoryInfo(this._sourceRootPath);
                _fileList.AddRange(dir.GetFiles("*.*", SearchOption.AllDirectories).Select(x => x.FullName).ToList());
            }
            else if (File.Exists(this._sourceRootPath))
            {
                _fileList.Add(this._sourceRootPath);
            }
            else
            {
                _logger.LogError($"[ImportByRawDataExecutor] The source folder does not exist:{this._sourceRootPath}");
                throw new DirectoryNotFoundException(this._sourceRootPath);
            }
        }
        public void Start()
        {
            ActivateImport();
        }
        private  void ActivateImport()
        {
            ProcessedCount = 0;
            if (_sourceRootPath is null)
                return;
            //发送导入开始通知
            ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Started, _patientNameListString));
            try
            {
                SrcPaths.AddRange(GetRawDataPath(_sourceRootPath));
                RTDDicomPaths.AddRange(GetSeriesPath(_sourceRootPath));
                CopyToDisk();
                CopyDicomToDisk();
                InsertData(_sourceRootPath);
                //发送导入结束通知
                _logger.LogInformation($"[ImportByRawDataExecutor] finished for {this._sourceRootPath}, progress is:{ProcessedCount}/{TotalCount}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Succeeded, _patientNameListString));
            }
            catch (OperationCanceledException canceledException)
            {
                this._logger.LogWarning($"ImportByRawDataExecutor is cancelled for [{_sourceRootPath}], the exception is:{canceledException.Message}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Cancelled, _patientNameListString));
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ImportByRawDataExecutor]:Failed to import from source path:{this._sourceRootPath}, the error message is:{ex.Message}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Failed, _patientNameListString, ex.Message));
                return;
            }
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
        private void CopyToDisk()
        {
            if (SrcPaths is null|| _sourceRootPath is null)
            {
                return;
            }
            foreach (var srcPath in SrcPaths)
            {
                this.CheckIfAskedToCancel();
                var dir = new DirectoryInfo(srcPath);
                string sourceDir = Path.Combine(_sourceRootPath,dir.Name);
                if (File.Exists(sourceDir))
                {
                    ProcessedCount = FileOperationHelper.CopyFile(sourceDir, srcPath, ProcessedCount);
                }
                else if (Directory.Exists(sourceDir))
                {
                    ProcessedCount = FileOperationHelper.CopyFolder(sourceDir, srcPath, ProcessedCount);
                }
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.InProgress, this._patientNameListString));
            }
        }
        private void CopyDicomToDisk()
        {
            if (RTDDicomPaths is null || _sourceRootPath is null)
            {
                return ;
            }
            string saveDir = string.Empty;
            foreach (var srcPath in RTDDicomPaths)
            {
                this.CheckIfAskedToCancel();
                var dir = new DirectoryInfo(srcPath);
                string sourceDir = Path.Combine(_sourceRootPath, dir.Name);
                if (File.Exists(sourceDir))
                {
                    ProcessedCount = FileOperationHelper.CopyFile(sourceDir, srcPath, ProcessedCount);
                }
                else if (Directory.Exists(sourceDir))
                {
                    ProcessedCount = FileOperationHelper.CopyFolder(sourceDir, srcPath, ProcessedCount);
                }
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.InProgress, this._patientNameListString));
            }
        }
        private List<string> GetRawDataPath(string sourcePath)
        {
            List<string> stringRawDataPath = new List<string>();
            string rawDataFileName = Path.Combine(sourcePath, "rawdata.json");
            var rawDataModelList = JsonConvert.DeserializeObject<List<RawDataModel>>(File.ReadAllText(rawDataFileName));
            if (rawDataModelList is null)
                return null;
            if (rawDataModelList != null)
            {
                foreach (var rawDataItem in rawDataModelList)
                {
                    stringRawDataPath.Add(rawDataItem.Path);
                }
            }
            return stringRawDataPath;
        }
        private List<string> GetSeriesPath(string sourcePath)
        {
            List<string> stringSeriesPath = new List<string>();
            string seriesDataFileName = Path.Combine(sourcePath, "series.json");
            var seriesDataModelList = JsonConvert.DeserializeObject<List<SeriesModel>>(File.ReadAllText(seriesDataFileName));
            if (seriesDataModelList is null)
                return null;
            if (seriesDataModelList != null)
            {
                foreach (var seriesDataItem in seriesDataModelList)
                {
                    stringSeriesPath.Add(seriesDataItem.SeriesPath);
                }
            }
            return stringSeriesPath;
        }
        private void InsertData(string sourcePath)
        {
            string studyFileName = Path.Combine(sourcePath, "study.json");
            var studyEntity = JsonConvert.DeserializeObject<StudyEntity>(File.ReadAllText(studyFileName));
            string patientFileName = Path.Combine(sourcePath, "patient.json");
            var patientEntity = JsonConvert.DeserializeObject<PatientEntity>(File.ReadAllText(patientFileName));
            _studyService.Insert(false, false, _mapper.Map<PatientModel>(patientEntity), _mapper.Map<StudyModel>(studyEntity));
            string seriesFileName = Path.Combine(sourcePath, "series.json");
            var seriesModelList = JsonConvert.DeserializeObject<List<SeriesModel>>(File.ReadAllText(seriesFileName));
            if (seriesModelList is null)
                return;
            foreach (var seriesItem in seriesModelList)
            {
                _seriesService.Add(seriesItem);
            }
            string rawDataFileName = Path.Combine(sourcePath, "rawdata.json");
            var rawDataModelList = JsonConvert.DeserializeObject<List<RawDataModel>>(File.ReadAllText(rawDataFileName));
            if (rawDataModelList is null)
                return;
            foreach (var rawDataItem in rawDataModelList)
            {
                _rawDataService.Add(rawDataItem);
            }
            string scanTaskFileName = Path.Combine(sourcePath, "scantask.json");
            var scanTaskModelList = JsonConvert.DeserializeObject<List<ScanTaskModel>>(File.ReadAllText(scanTaskFileName));
            if (scanTaskModelList != null)
            {
                foreach (var scanTaskItem in scanTaskModelList)
                {
                    _scanTaskService.Insert(scanTaskItem);
                }
            }
            string reconTaskFileName = Path.Combine(sourcePath, "recontask.json");
            var reconTaskModelList = JsonConvert.DeserializeObject<List<ReconTaskModel>>(File.ReadAllText(reconTaskFileName));
            if (reconTaskModelList is null)
                return;
            foreach (var reconTaskItem in reconTaskModelList)
            {
                _reconTaskService.Insert(reconTaskItem);
            }
        }

    }
}
