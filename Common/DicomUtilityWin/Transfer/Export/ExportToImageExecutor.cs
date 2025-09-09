//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20      V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
using FellowOakDicom;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.DicomUtility.Contract;
using NV.CT.SystemInterface.MCSDVDCDBurner;
using NV.CT.DicomUtility.DicomImage;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;

namespace NV.CT.DicomUtility.Transfer.Export
{
    public class ExportToImageExecutor : ITaskExecutor
    {
        #region Members
        private readonly ILogger _logger;
        private volatile CancellationTokenSource _cancellationTokenSource;
        private List<string> _fileList;
        private const string TIME_FORMAT = "yyyyMMddHHmmssfffff";
        private string _staticAnonymouseIndex = string.Empty;
        private string _jobTaskID = string.Empty;
        private string _patientNameListString = string.Empty;

        #endregion

        #region Properties
        public List<string> SrcPaths { get; }
        public ImageFormat ImageFormat { get; private set; }

        public string DestRootPath { get; }

        public string BinPath { get; }

        public string TempBurnPath { get; }

        public bool IsBurnToCDROM { get; }

        public DicomTransferSyntax TransferSynctax { get; }

        public int ProcessedCount { get; private set; }

        public int TotalCount => _fileList.Count;

        public string PatientNameListString => this._patientNameListString;

        #endregion

        public event EventHandler<ExecuteStatusInfo>? ExecuteStatusChanged;

        #region Constructor 
        public ExportToImageExecutor(ILogger logger,
                                     CancellationTokenSource cancellationTokenSource,
                                     string jobTaskID,
                                     string[] patientNames,
                                     string[] sourcePaths, 
                                     string destRootPath, 
                                     string binPath, 
                                     ImageFormat imageFormat,
                                     bool isBurnToCDROM,
                                     SupportedTransferSyntax syntax = SupportedTransferSyntax.ImplicitVRLittleEndian)
        {
            _logger = logger;
            _cancellationTokenSource = cancellationTokenSource;
            _jobTaskID = jobTaskID;
            _fileList = new List<string>();
            SrcPaths = new List<string>();
            SrcPaths.AddRange(sourcePaths);
            DestRootPath = destRootPath;
            _patientNameListString = string.Join(",", patientNames);
            _staticAnonymouseIndex = DateTime.Now.ToString(TIME_FORMAT);
            BinPath = binPath;
            TempBurnPath = Path.Combine(BinPath, Constants.TEMPORARY_BURNING_PATH);
            ImageFormat = imageFormat;
            IsBurnToCDROM = isBurnToCDROM;
            TransferSynctax = TransferSyntaxMapper.GetMappedTransferSyntax(syntax);

            InitExportList();
        }

        #endregion

        #region Public methods

        public void Start()
        {
            ActivateExport();
        }

        public void Cancel()
        {
            if (this._cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        #endregion

        #region Private methods

        private void InitExportList()
        {
            //处理所有Export源目录中的文件内容，添加到待导出列表。
            foreach (var srcPath in SrcPaths)
            {
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
                    _logger.LogError($"[ExportToImageExecutor] The source folder does not exist:{srcPath}");
                    throw new DirectoryNotFoundException(srcPath);
                }
            }
        }

        private void ActivateExport()
        {            
            _logger.LogInformation($"[ExportToImageExecutor] Start export from sources: {string.Join("//", SrcPaths)} ");

            ProcessedCount = 0;
            //发送导出开始通知
            ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Started, this._patientNameListString));
            this.CheckIfAskedToCancel();
            try
            {
                if (this.IsBurnToCDROM)
                {
                    this.BurnToCDROM();
                }
                else
                {
                    this.CopyToDisk();
                }

                //发送导入结束通知
                _logger.LogInformation($"[ExportToImageExecutor] finished for {_patientNameListString}, progress is:{ProcessedCount}/{TotalCount}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Succeeded, _patientNameListString));
            }
            catch (OperationCanceledException canceledException)
            {
                this._logger.LogWarning($"ExportToImageExecutor is cancelled for [{_patientNameListString}], the exception is:{canceledException.Message}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Cancelled, _patientNameListString));
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ExportToImageExecutor]:Failed to export for:{_patientNameListString}, the error message is:{ex.Message}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Failed, _patientNameListString, ex.Message));
            }
        }

        private void BurnToCDROM()
        {
            string targetDriverName = new DirectoryInfo(this.DestRootPath).Root.Name;
            var recorder = RecorderManager.GetAvailableRecorderList().FirstOrDefault(r => r.DriverName == targetDriverName);
            if (recorder is null)
            {
                string errorMessage = $"No available CDROM found.";
                _logger.LogError($"[ExportToImageExecutor] {errorMessage}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo( _jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Failed, _patientNameListString, errorMessage));
                return;
            }

            //remove temporary files
            if (Directory.Exists(TempBurnPath))
            {
                Directory.Delete(TempBurnPath, true);
            }

            foreach (var dicomFile in _fileList)
            {
                this.CheckIfAskedToCancel();
                CopyFileToTempBurnPath(dicomFile);
                ProcessedCount++;
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.InProgress, _patientNameListString));
            }
            this.AddFilesToBurn(recorder);
            try
            {
                this.CheckIfAskedToCancel();
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, 10, 6, ExecuteStatus.InProgress, this._patientNameListString));
                recorder.Burn(Constants.NAME_OF_BURNING);
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, 10, 10, ExecuteStatus.InProgress, this._patientNameListString));
            }
            catch (Exception ex)
            {
                string errorMessage = $"[ExportToImageExecutor]:Failed to burn from source path:{string.Join("//", SrcPaths)}, the error message is:{ex.Message}";
                _logger.LogError(errorMessage);
                throw;
            }
            finally
            {
                //remove generated temporary files
                if (Directory.Exists(TempBurnPath))
                {
                    Directory.Delete(TempBurnPath, true);
                }
            }
        }

        private void AddFilesToBurn(IRecorder recorder)
        {
            var sourcePaths = Directory.GetDirectories(TempBurnPath);
            foreach (var sourcePath in sourcePaths)
            {
                var fileToBurn = recorder.AddMediaFile();
                fileToBurn.Path = sourcePath;
                fileToBurn.Type = MediaType.Directory;
            }

        }

        private void CopyFileToTempBurnPath(string dicomFile)
        {
            var folderPath = GetExportedFolderPath(dicomFile, TempBurnPath);
            var filePath = Path.Combine(folderPath, Path.GetFileName(dicomFile));
            filePath = Path.ChangeExtension(filePath, ImageFormat.ToString());
            SaveDicomImage(dicomFile, filePath);
        }

        private string GetExportedFolderPath(string file, string parentPath)
        {
            var dcmFile = DicomFile.Open(file);
            //若PatientID为空或Anonymouse，使用匿名根目录
            var patientId = DicomContentHelper.GetDicomTag<string>(dcmFile.Dataset, DicomTag.PatientID);
            if (string.IsNullOrEmpty(patientId))
            {
                patientId = $"Anonymouse_{_staticAnonymouseIndex}";
            }
            var studyInstanceUID = DicomContentHelper.GetDicomTag<string>(dcmFile.Dataset, DicomTag.StudyInstanceUID);
            var seriesInstanceUID = DicomContentHelper.GetDicomTag<string>(dcmFile.Dataset, DicomTag.SeriesInstanceUID);

            var folderPath = Path.Combine(parentPath, $"{patientId}_{ImageFormat.ToString()}", studyInstanceUID, seriesInstanceUID);

            return folderPath;
        }

        private void CopyToDisk()
        {
            foreach (var file in _fileList)
            {
                this.CheckIfAskedToCancel();

                var folderPath = GetExportedFolderPath(file, DestRootPath);
                var filePath = Path.Combine(folderPath, Path.GetFileName(file));
                filePath = Path.ChangeExtension(filePath, ImageFormat.ToString());
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                SaveDicomImage(file, filePath);
                ProcessedCount++;
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.InProgress, _patientNameListString));
            }
        }

        private bool SaveDicomImage(string dicomfile, string targetPath)
        {
            //validate file
            if (!DicomFile.HasValidHeader(dicomfile))
            {
                _logger.LogWarning($"[ExportToImageExecutor] The file does not match Dicom3.0 standard! The file name is:{dicomfile}");
                return false;
            }

            try
            {
                DicomImageHelper.GenerateBitmapImage(dicomfile).Save(targetPath, ImageFormat);
            }
            catch (Exception ex)
            {
                this._logger.LogWarning($"[ExportToImageExecutor] Failed to save DICOM image for file:{dicomfile}, the exception is:{ex.Message}");
                return false;
            }
            return true;
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
                this._logger.LogTrace("ExportToImageExecutor received cancellation request.");
                Trace.WriteLine("=== ExportToImageExecutor : cancelled.");
                this._cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }

        #endregion
    }
}
