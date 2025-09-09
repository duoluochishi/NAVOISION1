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
using FellowOakDicom.Imaging.Codec;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.DicomUtility.Contract;
using NV.CT.SystemInterface.MCSDVDCDBurner;
using System.Diagnostics;
using System.IO;
using FellowOakDicom.Media;
using NV.CT.DicomUtility.Transfer.DicomDataModifier;

namespace NV.CT.DicomUtility.Transfer.Export
{
    /// <summary>
    /// Dicom 导出执行
    /// 支持Cancel，支持设置匿名处理类，支持设置字段修正类，支持事件反馈。
    /// </summary>
    public class ExportToDicomExecutor: ITaskExecutor
    {
        #region Members
        private readonly ILogger _logger;
        private IDicomDataModificationHandler _anonymouseHandler;
        private IDicomDataModificationHandler _correctionHandler;
        private volatile CancellationTokenSource _cancellationTokenSource;
        private List<string> _fileList;
        private const string TIME_FORMAT = "yyyyMMddHHmmssfffff";
        private const string DICOMDIR = "DICOMDIR";
        private string _jobTaskID = string.Empty;
        private string _patientNameListString = string.Empty;
        private string _staticAnonymouseIndex = string.Empty;
        private string _tempBurnPath = string.Empty;
        private string _sourcePathOfViewer = string.Empty;
        #endregion

        #region Properties
        public List<string> SrcPaths { get; }

        public string DestRootPath { get; }

        public string BinPath { get; }

        public bool IsAnonymouse { get; }

        public bool IsCorrected { get; }

        public bool IsAddViewer { get; }

        public bool IsBurnToCDROM { get; }

        public DicomTransferSyntax TransferSynctax { get; }

        public int ProcessedCount { get; private set; }

        public int TotalCount => _fileList.Count;

        public string PatientNameListString => this._patientNameListString;

        #endregion

        public event EventHandler<ExecuteStatusInfo>? ExecuteStatusChanged;

        #region Constructor
        public ExportToDicomExecutor(ILogger logger,
                                     CancellationTokenSource cancellationTokenSource,
                                     string jobTaskID,
                                     string[] patientNames,
                                     string[] srourcePaths,
                                     string destRootPath,
                                     string binPath,
                                     bool isAnonymouse = false,
                                     bool isCorrected = false,
                                     bool isBurnToCDROM = false,
                                     bool isAddViewer = false,
                                     SupportedTransferSyntax syntax = SupportedTransferSyntax.ImplicitVRLittleEndian)
        {
            _logger = logger;
            _cancellationTokenSource = cancellationTokenSource;
            _jobTaskID = jobTaskID;
            SrcPaths = new List<string>();
            _fileList = new List<string>();
            _anonymouseHandler = new DicomAnonymouseHandler();
            _correctionHandler = new DicomCorrectionHandler();

            SrcPaths.AddRange(srourcePaths);
            DestRootPath = destRootPath;
            _patientNameListString = string.Join(",", patientNames);
            BinPath = binPath;
            IsAnonymouse = isAnonymouse;
            IsCorrected = isCorrected;
            IsBurnToCDROM = isBurnToCDROM;
            IsAddViewer = isAddViewer;
            TransferSynctax = TransferSyntaxMapper.GetMappedTransferSyntax(syntax);
            _staticAnonymouseIndex = DateTime.Now.ToString(TIME_FORMAT);
            _tempBurnPath = Path.Combine(BinPath, $"{Constants.TEMPORARY_BURNING_PATH}_{_staticAnonymouseIndex}");
            _sourcePathOfViewer = Path.Combine(BinPath, "Tools", "CDViewer");

            InitExportList();

        }

        #endregion

        #region Public Methods
        public void Start()
        {
            ActivateExport();
        }

        public void Cancel()
        {
            if(this._cancellationTokenSource is not null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        #endregion

        #region Methods
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
                    _logger.LogError($"[ExportToDicomExecutor] The source folder does not exist:{srcPath}");
                    throw new DirectoryNotFoundException(srcPath);
                }
            }
        }

        private void ActivateExport()
        {
            _logger.LogInformation($"[ExportToDicomExecutor] Start export from sources: {string.Join("//",SrcPaths)} ");

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
                _logger.LogInformation($"[ExportToDicomExecutor] finished for {_patientNameListString}, progress is:{ProcessedCount}/{TotalCount}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Succeeded, _patientNameListString));
            }
            catch(OperationCanceledException canceledException)
            {
                this._logger.LogWarning($"ExportToDicomExecutor is cancelled for [{_patientNameListString}], the exception is: {canceledException.Message}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Cancelled, _patientNameListString));
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ExportToDicomExecutor]:Failed to export for:{_patientNameListString}, the error message is:{ex.Message}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Failed, _patientNameListString, ex.Message));
            }
        }

        private bool DoExport(DicomDirectory dicomDirectory, string file)
        {
            //validate file
            if (!DicomFile.HasValidHeader(file))
            {
                _logger.LogWarning($"[ExportToDicomExecutor] The file does not match Dicom3.0 standard! The file name is:{file}");
                return false;
            }

            var originalFile = DicomFile.Open(file);
            var dcmFile = originalFile;

            if(TransferSynctax != originalFile.Dataset.InternalTransferSyntax)
            {
                dcmFile = new DicomFile(originalFile.Dataset.Clone(TransferSynctax));
            }

            if (IsCorrected)
            {
                _correctionHandler?.ModifyDicomData(dcmFile.Dataset);
            }

            if (IsAnonymouse)
            {
                _anonymouseHandler?.ModifyDicomData(dcmFile.Dataset);
            }

            var folderPath = GetExportedFolderPath(dcmFile, DestRootPath);
            var filePath = Path.Combine(folderPath, Path.GetFileName(file));
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            dcmFile.Save(filePath);
            dicomDirectory.AddFile(dcmFile);
            return true;
        }

        /// <summary>
        /// 目标路径组成：目录/PatientID/studyInstanceUID/seriesInstanceUID/文件名
        /// 匿名或PatientID为空，使用当前匿名索引替换PatientID
        /// </summary>
        /// <param name="dcmFile"></param>
        /// <param name="parentPath"></param>
        /// <returns></returns>
        private string GetExportedFolderPath(DicomFile dcmFile,string parentPath)
        { 
            //若PatientID为空或Anonymouse，使用匿名根目录
            var patientId = DicomContentHelper.GetDicomTag<string>(dcmFile.Dataset, DicomTag.PatientID);
            if (string.IsNullOrEmpty(patientId) || IsAnonymouse)
            {
                patientId = $"Anonymouse{_staticAnonymouseIndex}";
            }
            var studyInstanceUID = DicomContentHelper.GetDicomTag<string>(dcmFile.Dataset, DicomTag.StudyInstanceUID);
            var seriesInstanceUID = DicomContentHelper.GetDicomTag<string>(dcmFile.Dataset, DicomTag.SeriesInstanceUID);

            var folderPath = Path.Combine(parentPath, patientId, studyInstanceUID, seriesInstanceUID);

            return folderPath;
        }               

        private void BurnToCDROM()
        {
            string targetDriverName = new DirectoryInfo(this.DestRootPath).Root.Name;
            var recorder = RecorderManager.GetAvailableRecorderList().FirstOrDefault(r => r.DriverName == targetDriverName);
            if (recorder is null)
            {
                string errorMessage = "No available CDROM found.";
                _logger.LogError($"[ExportToDicomExecutor] {errorMessage}");
                throw new DriveNotFoundException(errorMessage);
            }

            //拷贝 Viewer
            if (IsAddViewer == true)
            {
                AddViewerToBurn(recorder);
            }

            //remove generated temporary files
            if (Directory.Exists(_tempBurnPath))
            {
                Directory.Delete(_tempBurnPath, true);
            }

            var dicomDir = new DicomDirectory();
            foreach (var dicomFile in _fileList)
            {
                this.CheckIfAskedToCancel();
                CopyFileToTempBurnPath(dicomDir, dicomFile);
                ProcessedCount++;
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.InProgress, this._patientNameListString));
            }
            string dicomDirFilePath = Path.Combine(_tempBurnPath, DICOMDIR);
            dicomDir.Save(dicomDirFilePath);

            this.AddFilesToBurn(recorder, dicomDirFilePath);

            try
            {
                this.CheckIfAskedToCancel();
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, 10, 6, ExecuteStatus.InProgress, this._patientNameListString));
                recorder.Burn(Constants.NAME_OF_BURNING);
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, 10, 10, ExecuteStatus.InProgress, this._patientNameListString));
            }
            catch (Exception ex)
            {
                string errorMessage = $"Failed to burn from source path:{string.Join("//", SrcPaths)}, the error message is:{ex.Message}";
                _logger.LogWarning($"[ExportToDicomExecutor] {errorMessage}");
                throw;
            }
            finally
            {
                //remove generated temporary files
                if (Directory.Exists(_tempBurnPath))
                {
                    Directory.Delete(_tempBurnPath, true);
                }
            }
        }

        private void CopyFileToTempBurnPath(DicomDirectory dicomDirectory, string file)
        {
            //validate file
            if (!DicomFile.HasValidHeader(file))
            {
                _logger.LogWarning($"[ExportToDicomExecutor] The file does not match Dicom3.0 standard! The file name is:{file}");
                return;
            }

            var originalFile = DicomFile.Open(file);
            var dcmFile = originalFile;
            if (TransferSynctax != originalFile.Dataset.InternalTransferSyntax)
            {
                dcmFile = new DicomFile(originalFile.Dataset.Clone(TransferSynctax));
            }

            if (IsCorrected)
            {
                _correctionHandler?.ModifyDicomData(dcmFile.Dataset);
            }

            if (IsAnonymouse)
            {
                _anonymouseHandler?.ModifyDicomData(dcmFile.Dataset);
            }

            var folderPath = GetExportedFolderPath(dcmFile, _tempBurnPath);
            var filePath = Path.Combine(folderPath, Path.GetFileName(file));
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            dcmFile.Save(filePath);
            dicomDirectory.AddFile(dcmFile);
        }

        private void AddFilesToBurn(IRecorder recorder, string dicomDirFilePath)
        {
            var sourcePaths = Directory.GetDirectories(_tempBurnPath);
            foreach (var sourcePath in sourcePaths)
            {
                var fileToBurn = recorder.AddMediaFile();
                fileToBurn.Path = sourcePath;
                fileToBurn.Type = MediaType.Directory;
            }

            var dicomDirFile = recorder.AddMediaFile();
            dicomDirFile.Path = dicomDirFilePath;
            dicomDirFile.Type = MediaType.File;
        }

        private void AddViewerToBurn(IRecorder recorder)
        {            
            if (!Directory.Exists(_sourcePathOfViewer))
            {
                var errorMessage = $"Failed to copy viewer! The source path does not exist:{_sourcePathOfViewer}";
                _logger.LogWarning($"[ExportToDicomExecutor AddViewer] {errorMessage}");
                throw new DirectoryNotFoundException(errorMessage);
            }

            var viewer = recorder.AddMediaFile();
            viewer.Path = _sourcePathOfViewer;
            viewer.Type = MediaType.Directory;
        }

        private void CopyToDisk()
        {
            var dicomDir = new DicomDirectory();
            foreach (var dicomFile in _fileList)
            {
                this.CheckIfAskedToCancel();
                DoExport(dicomDir, dicomFile);
                ProcessedCount++;
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.InProgress, this._patientNameListString));
            }

            string dicomDirFileName = Path.Combine(DestRootPath, DICOMDIR); 
            dicomDir.Save(dicomDirFileName);

            //拷贝 Viewer
            if (IsAddViewer == true)
            {
                this.CheckIfAskedToCancel();
                AddViewerForCopy();
            }
        }

        private void AddViewerForCopy()
        {
            if (!Directory.Exists(_sourcePathOfViewer))
            {
                string errorMessage = $"Failed to copy viewer, the source path does not exist:{_sourcePathOfViewer}.";
                _logger.LogWarning($"[ExportToDicomExecutor AddViewer] {errorMessage}");
                throw new Exception(errorMessage);
            }

            var directoryInfo = new DirectoryInfo(DestRootPath);
            var targetPath = directoryInfo.Root.FullName;
            if (!Directory.Exists(targetPath))
            {
                string errorMessage = $"Failed to copy viewer! The target path does not exist:{targetPath}";
                _logger.LogWarning($"[ExportToDicomExecutor AddViewer] {errorMessage}");
                throw new Exception(errorMessage);
            }

            CopyDirectory(_sourcePathOfViewer, targetPath);
        }

        private void CopyDirectory(string sourcePath, string targetPath)
        {
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            var files = Directory.GetFiles(sourcePath, "*.*");
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                string targetFilePath = Path.Combine(targetPath, fileName);
                File.Copy(file, targetFilePath, true);
            }

            var subFolders = Directory.GetDirectories(sourcePath);
            foreach (var subFolder in subFolders)
            {
                var directoryInfo = new DirectoryInfo(subFolder);
                if ((directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                string folderName = Path.GetFileName(subFolder);
                string targetFolderName = Path.Combine(targetPath, folderName);
                CopyDirectory(subFolder, targetFolderName);
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
                this._logger.LogTrace("ExportToDicomExecutor received cancellation request.");
                Trace.WriteLine("=== ExportToDicomExecutor : cancelled.");
                this._cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }

        #endregion
    }
}
