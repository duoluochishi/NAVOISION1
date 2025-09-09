//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------

using FellowOakDicom;
using FellowOakDicom.Media;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.DicomUtility.Contract;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace NV.CT.DicomUtility.Transfer.Import
{
    /// <summary>
    /// 按文件夹目录导入Dicom
    /// </summary>

    public class ImportByDirExecutor: IImportTaskExecutor
    {
        #region Members
        private readonly ILogger _logger;
        private volatile CancellationTokenSource _cancellationTokenSource;

        private List<string> _fileList = new();
        private List<string> _invalidDicomFileList = new();
        private List<string> _validDicomFileList = new();
        private object _lockerForAddFile = new object();

        private string _jobTaskID = string.Empty;
        private string? _sourceRootPath;
        private string? _targetRootPath;
        private string? _patientNameListString;
        #endregion

        #region Properties

        public int ProcessedCount { get; private set; }
        public int TotalCount => _validDicomFileList.Count;

        public string PatientNameListString => this._patientNameListString;

        #endregion

        public event EventHandler<ExecuteStatusInfo>? ExecuteStatusChanged;

        //public event EventHandler<List<string>>? ExecuteValidDicomFileChanged;

        #region Constructor
        public ImportByDirExecutor(string jobTaskID, string sourceRootPath,string targetRootPath, ILogger logger, CancellationTokenSource cancellationTokenSource)
        {
            _jobTaskID = jobTaskID;
            _sourceRootPath = sourceRootPath;
            _targetRootPath = targetRootPath;
            _logger = logger;
            _cancellationTokenSource = cancellationTokenSource;

            InitFilesList();
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            ActivateImport();
        }

        #endregion

        #region Private Methods

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
                _logger.LogError($"[ImportByDirExecutor] The source folder does not exist:{this._sourceRootPath}");
                throw new DirectoryNotFoundException(this._sourceRootPath);
            }
        }

        /// <summary>
        /// 获取Patient概要信息
        /// </summary>
        /// <param name="fileList">文件列表</param>
        private ConcurrentDictionary<string, string> FetchGeneralPatientInformation(List<string> fileList)
        {
            var patientDictionary = new ConcurrentDictionary<string, string>();

            foreach (var file in fileList)
            {
                //validate file
                if (!DicomFile.HasValidHeader(file))
                {
                    _logger.LogError($"[ImportByDirExecutor][ReadDicomFile]: The file does not match Dicom3.0 standard! The file name is:{file}");
                    _invalidDicomFileList.Add(file);
                    continue;
                }

                try
                {
                    var dicomFile = DicomFile.Open(file);
                    if (!dicomFile.Dataset.Contains(DicomTag.StudyID))
                    {
                        _logger.LogError($"[ImportByDirExecutor][ReadDicomFile]: The file does not contain StudyID! The file name is:{file}");
                        _invalidDicomFileList.Add(file);
                        continue;
                    }

                    patientDictionary.GetOrAdd(dicomFile.Dataset.GetString(DicomTag.PatientID), dicomFile.Dataset.GetString(DicomTag.PatientName));
                    _validDicomFileList.Add(file);

                }
                catch (Exception ex)
                {
                    _invalidDicomFileList.Add(file);
                    _logger.LogError($"[ImportByDirExecutor][ReadDicomFile]: Not a valid dicom file! It is failed to open file:{file} with error reason:{ex.Message}");
                    continue;
                }
            }
            return patientDictionary;
        }

        private  void ActivateImport()
        {   
            _logger.LogInformation($"[ImportByDirExecutor]: start importing from path: {this._sourceRootPath}");

            var distinctPatientList = FetchGeneralPatientInformation(_fileList);
            //ExecuteValidDicomFileChanged?.Invoke(this,_validDicomFileList);
            ProcessedCount = 0;
            this._patientNameListString = string.Join(",", distinctPatientList.Values);

            //发送导入开始通知
            ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Started, _patientNameListString));

            var importedPatientList = new List<DicomPatientInfo>();
            try
            {
                var dicomDirectory = GetDicomDirAndCopyFiles(_validDicomFileList);
                BuildDicomDirectory(dicomDirectory, importedPatientList);

                //发送导入结束通知
                _logger.LogInformation($"[ImportByDirExecutor] finished for {this._sourceRootPath}, progress is:{ProcessedCount}/{TotalCount}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Succeeded, _patientNameListString, importedPatientList));
            }
            catch (OperationCanceledException canceledException)
            {
                this._logger.LogWarning($"ImportByDirExecutor is cancelled for [{_sourceRootPath}], the exception is:{canceledException.Message}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Cancelled, _patientNameListString));
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ImportByDirExecutor]:Failed to import from source path:{this._sourceRootPath}, the error message is:{ex.Message}");
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.Failed, _patientNameListString, ex.Message));
                return;
            }
        }

        /// <summary>
        /// 加载DICOM文件
        /// </summary>
        /// <param name="validDicomFileList"></param>
        private   DicomDirectory GetDicomDirAndCopyFiles(List<string> validDicomFileList)
        {
            var dicomDir = new DicomDirectory();
            foreach (var file in validDicomFileList)
            {
                CheckIfAskedToCancel();
                DicomFile dicomFile;
                try
                {
                    dicomFile =  DicomFile.Open(file); 
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[ImportByDirExecutor][ReadDicomFile]: Not a valid dicom file! It is failed to open file:{file} with error reason:{ex.Message}");
                    continue;
                }

                ProcessedCount++;
                ExecuteStatusChanged?.Invoke(this, new ExecuteStatusInfo(_jobTaskID, TotalCount, ProcessedCount, ExecuteStatus.InProgress, this._patientNameListString));

                Task readDicomTask =new Task(() =>
                {
                    CopyFileToDisk(file, dicomFile);                        
                    lock (_lockerForAddFile)
                    {
                        dicomDir.AddFile(dicomFile);
                    }
                });
                readDicomTask.RunSynchronously();
            }
            return dicomDir;
        }

        /// <summary>
        /// 把导入的文件拷贝到本地磁盘  
        /// </summary>
        /// <param name="sourceFileName"></param>
        /// <param name="dicomFile"></param>
        private void CopyFileToDisk(string sourceFileName, DicomFile dicomFile)
        {
            DicomDataset dataset = dicomFile.Dataset;
            if (!dataset.Contains(DicomTag.SOPInstanceUID) && !string.IsNullOrWhiteSpace(dicomFile.FileMetaInfo.MediaStorageSOPInstanceUID.UID))
            {
                dataset = dataset.AddOrUpdate(DicomTag.SOPInstanceUID, dicomFile.FileMetaInfo.MediaStorageSOPInstanceUID.UID);
            }
            else
            {
                dataset = dataset.AddOrUpdate(DicomTag.SOPInstanceUID, DicomUIDGenerator.GenerateDerivedFromUUID());
            }

            if (!dataset.Contains(DicomTag.InstanceNumber))
            {
                dataset = dataset.AddOrUpdate(DicomTag.InstanceNumber, 1);
            }

            //此处做特殊处理,因为我们之前的剂量报告 studyID长度填写有问题
            string studyId = dataset.GetString(DicomTag.StudyID);
            if (!string.IsNullOrEmpty(studyId) && studyId.Length > 16)
            {
                dataset = dataset.AddOrUpdate(DicomTag.StudyID, studyId.Substring(studyId.Length - 16, 16));
            }
  
            string destinationFilePath = GetTargetPathByDicomFile(dicomFile.Dataset.GetString(DicomTag.StudyInstanceUID), 
                                                                  dicomFile.Dataset.GetString(DicomTag.SeriesInstanceUID), 
                                                                  dicomFile.Dataset.GetString(DicomTag.InstanceNumber));
            try
            {   
                var bytes =  File.ReadAllBytes(sourceFileName);
                File.WriteAllBytes(destinationFilePath, bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ImportByDirExecutor][CopyFileToDisk]: failed to copy file{ex.Message} , the error reason is: {ex.Message}");
                throw;
            }
        }

        private void BuildDicomDirectory(DicomDirectory dicomDirectory, List<DicomPatientInfo> importedPatientList)
        {
            foreach (var patientRecord in dicomDirectory.RootDirectoryRecordCollection)
            {
                var patient = new DicomPatientInfo
                {
                    PatientId = DicomContentHelper.GetDicomTag<string>(patientRecord, DicomTag.PatientID),
                    PatientName = DicomContentHelper.GetDicomTag<string>(patientRecord, DicomTag.PatientName),
                    PatientSex = DicomContentHelper.GetDicomTag<string>(patientRecord, DicomTag.PatientSex),
                    PatientBirthDate = DicomContentHelper.GetDicomTag<string>(patientRecord, DicomTag.PatientBirthDate),
                    PatientBirthDateTime = DicomContentHelper.GetDicomDateTime(patientRecord, DicomTag.PatientBirthDate, DicomTag.PatientBirthTime),
                    AdmittingDate = DicomContentHelper.GetDicomTag<string>(patientRecord, DicomTag.AdmittingDate),
                    AdmittingTime = DicomContentHelper.GetDicomTag<string>(patientRecord, DicomTag.AdmittingTime),
                    AdmittingDateTime = DicomContentHelper.GetDicomDateTime(patientRecord, DicomTag.AdmittingDate, DicomTag.AdmittingDate),
                    CurrentPatientLocation = DicomContentHelper.GetDicomTag<string>(patientRecord, DicomTag.CurrentPatientLocation),
                    MedicalAlerts = DicomContentHelper.GetDicomTag<string>(patientRecord, DicomTag.MedicalAlerts),
                    PatientAddress = DicomContentHelper.GetDicomTag<string>(patientRecord, DicomTag.PatientAddress),
                    PatientSize = DicomContentHelper.GetDicomTag<string>(patientRecord, DicomTag.PatientSize),
                    PatientWeight = DicomContentHelper.GetDicomTag<string>(patientRecord, DicomTag.PatientWeight),
                    StudyList = new List<DicomStudyInfo>()
                };
                if (!string.IsNullOrEmpty(patient.PatientName))
                {
                    string[] patientName = patient.PatientName.Split("^".ToCharArray());
                    patient.PatientFirstName = patientName.Length > 1 ? patientName[1] : patientName[0];
                }
                importedPatientList.Add(patient);
                foreach (var studyRecord in patientRecord.LowerLevelDirectoryRecordCollection)
                {
                    var studyDateTime = DicomContentHelper.GetDicomDateTime(studyRecord, DicomTag.StudyDate, DicomTag.StudyTime);
                    if (studyDateTime.Year == 1)
                    {
                        var contentDateTime = DicomContentHelper.GetDicomDateTime(studyRecord, DicomTag.ContentDate, DicomTag.ContentTime);
                        if (contentDateTime.Year > 1)
                        {
                            studyDateTime = contentDateTime;
                        }
                    }

                    var study = new DicomStudyInfo
                    {
                        StudyID = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.StudyID),
                        StudyInstanceUID = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.StudyInstanceUID),
                        AccessionNumber = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.AccessionNumber),
                        Manufacturer = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.Manufacturer),
                        ManufacturerModelName = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.ManufacturerModelName),
                        StudyDateTime = studyDateTime,
                        StudyDate = studyDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        StudyTime = studyDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        PatientAge = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.PatientAge),
                        InstitutionName = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.InstitutionName),
                        AdmittingDiagnosesDescription = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.AdmittingDiagnosesDescription),
                        BodyPartExamined = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.BodyPartExamined),
                        PerformingPhysicianName = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.PerformingPhysicianName),
                        ProtocolName = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.ProtocolName),
                        ReferringPhysicianName = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.ReferringPhysicianName),
                        StudyDescription = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.StudyDescription),
                        CurrentPatientLocation = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.CurrentPatientLocation),
                        MedicalAlerts = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.MedicalAlerts),
                        PatientAddress = DicomContentHelper.GetDicomTag<string>(studyRecord, DicomTag.PatientAddress),
                        PatientSize = patient.PatientSize,
                        PatientWeight = patient.PatientWeight,
                        SeriesList = new List<DicomSeriesInfo>()
                    };
                    patient.StudyList.Add(study);
                    List<string> modalities = new List<string>();
                    foreach (var seriesRecord in studyRecord.LowerLevelDirectoryRecordCollection)
                    {
                        string seriesDate = string.Empty;
                        string seriesTime = string.Empty;
                        var seriesDateTime = DicomContentHelper.GetDicomDateTime(seriesRecord, DicomTag.SeriesDate, DicomTag.SeriesTime);
                        if (seriesDateTime.Year > 1)
                        {
                            seriesDate = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.SeriesDate);
                            seriesTime = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.SeriesTime);
                            seriesDateTime = DicomContentHelper.GetDicomDateTime(seriesRecord, DicomTag.SeriesDate, DicomTag.SeriesTime);
                        }
                        else
                        {
                            seriesDate = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.ContentDate);
                            seriesTime = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.ContentTime);
                            seriesDateTime = DicomContentHelper.GetDicomDateTime(seriesRecord, DicomTag.ContentDate, DicomTag.ContentTime);
                        }
                        var series = new DicomSeriesInfo
                        {
                            SeriesNumber = DicomContentHelper.GetDicomTag<int?>(seriesRecord, DicomTag.SeriesNumber),
                            SeriesInstanceUID = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.SeriesInstanceUID),
                            Modality = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.Modality),
                            SeriesDate = seriesDate,
                            SeriesTime = seriesTime,
                            SeriesDateTime = seriesDateTime,
                            SeriesDescription = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.SeriesDescription),
                            PatientPosition = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.PatientPosition),
                            BodyPartExamined = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.BodyPartExamined),
                            ImageType = Constants.IMAGE_TYPE_TOMO,
                            WindowWidth = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.WindowWidth),
                            WindowLevel = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.WindowCenter),

                            ImageList = new List<DicomImageInfo>()
                        };
                        if (!modalities.Contains(series.Modality))
                        {
                            modalities.Add(series.Modality);
                        }

                        var sopClassUID = DicomContentHelper.GetDicomTag<string>(seriesRecord, DicomTag.SOPClassUID);
                        if (sopClassUID == Constants.SOP_CLASS_UID_SR)
                        {
                            //process SR type
                            series.BodyPartExamined = study.BodyPartExamined;
                            series.ImageType = Constants.SERIES_TYPE_SR;
                            series.SeriesDescription = Constants.SERIES_TYPE_SR;
                        }

                        if (series.SeriesDescription == Constants.TOPOGRAM)
                        {
                            series.ImageType = Constants.IMAGE_TYPE_TOPO;
                        }
                        else if (series.SeriesDescription == Constants.DOSE_REPORT)
                        {
                            series.ImageType = Constants.SERIES_TYPE_DOSE_REPORT;
                        }

                        study.SeriesList.Add(series);
                        foreach (var imageRecord in seriesRecord.LowerLevelDirectoryRecordCollection)
                        {
                            var image = new DicomImageInfo
                            {
                                Path = GetTargetPathByDicomFile(study.StudyInstanceUID,
                                                                series.SeriesInstanceUID,
                                                                imageRecord.GetSingleValueOrDefault<int>(DicomTag.InstanceNumber, 1).ToString()),
                                InstanceNumber = DicomContentHelper.GetDicomTag<string>(imageRecord, DicomTag.InstanceNumber),
                                SOPInstanceUID = DicomContentHelper.GetDicomTag<string>(imageRecord, DicomTag.SOPInstanceUID),
                                ContentDate = DicomContentHelper.GetDicomTag<string>(imageRecord, DicomTag.ContentDate),
                                ContentTime = DicomContentHelper.GetDicomTag<string>(imageRecord, DicomTag.ContentTime),
                                ContentDateTime = DicomContentHelper.GetDicomDateTime(imageRecord, DicomTag.ContentDate, DicomTag.ContentTime),
                                ImageType = DicomContentHelper.GetDicomTag<string>(imageRecord, DicomTag.ImageType)
                            };
                            series.ImageList.Add(image);
                        }
                    }
                    study.Modalities = string.Join(",", modalities);
                }
            }
        }

        private string GetTargetPathByDicomFile(string studyInstanceUID, string seriesInstanceUID, string seriesInstanceNumber)
        {            
            var currentDestinationPath = Path.Combine(this._targetRootPath, $"{studyInstanceUID}", seriesInstanceUID);
            if (!Directory.Exists(currentDestinationPath))
            {
                Directory.CreateDirectory(currentDestinationPath);
            }

            return Path.Combine(currentDestinationPath, $"{seriesInstanceNumber}.dcm");
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
                this._logger.LogTrace("ImportByDirExecutor received cancellation request.");
                Trace.WriteLine("=== ImportByDirExecutor : cancelled.");
                this._cancellationTokenSource.Token.ThrowIfCancellationRequested();
            }
        }
        #endregion
    }
}
