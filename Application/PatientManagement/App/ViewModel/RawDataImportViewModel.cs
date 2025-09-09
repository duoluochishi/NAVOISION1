//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/13 11:01:27     V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using NV.CT.CTS.Enums;
using NV.CT.DicomUtility.DicomImage;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.Language;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using NV.CT.PatientManagement.ApplicationService.Contract.Models;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Globalization;

namespace NV.CT.PatientManagement.ViewModel
{
    public class RawDataImportViewModel : BaseViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly ILogger<RawDataImportViewModel> _logger;
        private readonly IStudyApplicationService _studyApplicationService;
        private readonly IAuthorization _authorizationService;
        private readonly IJobRequestService _jobRequestService;
        private const string DEFAULT_PATH = "D:\\";

        private bool _isImportButtonEnabled;
        public bool IsImportButtonEnabled
        {
            get { return this._isImportButtonEnabled; }
            set
            {
                SetProperty(ref this._isImportButtonEnabled, value);
            }
        }

        private string? _messageContent = string.Empty;
        public string? MessageContent
        {
            get { return this._messageContent; }
            set
            {
                SetProperty(ref this._messageContent, value);
            }
        }

        private string? _omittedChosenPath = string.Empty;
        public string? OmittedChosenPath
        {
            get { return this._omittedChosenPath; }
            set
            {
                SetProperty(ref this._omittedChosenPath, value);
            }
        }

        private string? _chosenPath = string.Empty;
        public string? ChosenPath
        {
            get { return this._chosenPath; }
            set
            {
                SetProperty(ref this._chosenPath, value);
            }
        }

        public RawDataImportViewModel(ILogger<RawDataImportViewModel> logger, IStudyApplicationService studyApplicationService, 
            IAuthorization authorizationService,IJobRequestService jobRequestService, IDialogService dialogService)
        {
            _logger = logger;
            _studyApplicationService = studyApplicationService;
            _authorizationService = authorizationService;
            _jobRequestService = jobRequestService;
            _dialogService = dialogService;

            Commands.Add(PatientManagementConstants.COMMAND_SELECT_PATH, new DelegateCommand(OnSelectPath));
            Commands.Add(PatientManagementConstants.COMMAND_IMPORT, new DelegateCommand<object>(OnImport));
            Commands.Add(PatientManagementConstants.COMMAND_CLOSE, new DelegateCommand<object>(OnClose, _ => true));
        }

        private void OnSelectPath()
        {
            using (var folderBrowser = new System.Windows.Forms.FolderBrowserDialog())
            {
                folderBrowser.InitialDirectory = DEFAULT_PATH;
                if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    this.ChosenPath = folderBrowser.SelectedPath;
                    this.OmittedChosenPath = this.TrimStringLength(this.ChosenPath, 100);
                }
            }

            this.SetButtonEnable();
        }

        private string TrimStringLength(string content, int length)
        {
            if (string.IsNullOrEmpty(content) || content.Length <= length)
            {
                return content;
            }

            return $"{content.Substring(0, length)}...";
        }

        private void OnImport(object paraWindow)
        {
            this.MessageContent = string.Empty;
            
            if (!Directory.Exists(this.ChosenPath))
            {
                this.MessageContent = LanguageResource.Message_Error_PathNotExist;
                return;
            }
            string fileName = Path.Combine(ChosenPath, "study.json");
            if (!File.Exists(fileName))
            {
                this.MessageContent = LanguageResource.Message_Error_ImportTaskFailed;
                return;
            }
            this.MessageContent = LanguageResource.ToolTip_Importing;
            try
            {
                var importJobTask = new ImportJobRequest();
                importJobTask.Id = Guid.NewGuid().ToString();
                importJobTask.WorkflowId = Guid.NewGuid().ToString();
                importJobTask.InternalPatientID = string.Empty;
                importJobTask.InternalStudyID = string.Empty;
                importJobTask.Priority = 5;
                importJobTask.JobTaskType = JobTaskType.ImportJob;
                importJobTask.Creator = _authorizationService.GetCurrentUser() is null ? string.Empty : _authorizationService.GetCurrentUser().Account;
                importJobTask.SourcePath = ChosenPath;
                importJobTask.VirtualSourcePath = this.ChosenPath;
                importJobTask.IsRawDataImport = true;
                importJobTask.Parameter = importJobTask.ToJson();

                var result = this._jobRequestService.EnqueueJobRequest(importJobTask);
                OnClose(paraWindow);
                if (result.Status == CommandExecutionStatus.Success)
                {
                    _dialogService.ShowDialog(false, MessageLeveles.Info,
                                              LanguageResource.Message_Info_CloseInformationTitle,
                                              LanguageResource.Message_Info_ImportTaskStarted,
                                              null, ConsoleSystemHelper.WindowHwnd);
                }
                else
                {
                    _dialogService.ShowDialog(false, MessageLeveles.Error,
                                              LanguageResource.Message_Info_CloseInformationTitle,
                                              LanguageResource.Message_Error_ImportTaskFailed,
                                              null, ConsoleSystemHelper.WindowHwnd);
                }
                //if (result)
                //{
                //    _dialogService.ShowDialog(false, MessageLeveles.Info,
                //                              LanguageResource.Message_Info_CloseInformationTitle,
                //                              LanguageResource.Message_Info_ImportTaskStarted,
                //                              null, ConsoleSystemHelper.WindowHwnd);
                //}
                //else
                //{
                //    _dialogService.ShowDialog(false, MessageLeveles.Error,
                //                              LanguageResource.Message_Info_CloseInformationTitle,
                //                              LanguageResource.Message_Error_ImportTaskFailed,
                //                              null, ConsoleSystemHelper.WindowHwnd);
                //}
            }
            catch (Exception ex) 
            {
                this._logger.LogWarning($"Failed in OnImportRawData with exception:{ex.Message}");
                this.MessageContent = LanguageResource.Content_Import_Failed;
            }

        }
        /// <summary>
        /// 通过ScanReconParameters.json文件导入Raw数据的逻辑；
        /// 恢复数据库记录时使用；
        /// </summary>
        private void OnImport()
        {
            Task.Run(() =>
            {
                var result = _studyApplicationService.ImportRaw(this.ChosenPath);
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(RuntimeConfig.Console.MCSAppData.Path, result.Item1));
                foreach (var item in directoryInfo.GetDirectories())
                {
                    if (item is null) continue;
                    if (item.Name.StartsWith("1") || item.Name.StartsWith("2") || item.Name.StartsWith("9"))
                        continue;
                    var dicomDetails = DicomImageHelper.Instance.GetDicomDetails(item.GetFiles("*.dcm")[0].FullName);
                    if (dicomDetails != null)
                    {
                        SeriesModel seriesModel = new SeriesModel();
                        seriesModel.InternalStudyId = result.Item2;
                        seriesModel.SeriesDescription = dicomDetails.FirstOrDefault(r => r.TagID == "(0008,103e)").TagValue;
                        string dateStr = dicomDetails.FirstOrDefault(r => r.TagID == "(0008,0021)").TagValue + " " + dicomDetails.FirstOrDefault(r => r.TagID == "(0008,0031)").TagValue;
                        DateTime dateTime = DateTime.ParseExact(
                               dateStr,
                               "yyyyMMdd HHmmss",
                               CultureInfo.InvariantCulture
                           );
                        seriesModel.SeriesDate = dateTime;
                        seriesModel.SeriesTime = dateTime;
                        seriesModel.SeriesNumber = dicomDetails.FirstOrDefault(r => r.TagID == "(0020,0011)").TagValue;
                        seriesModel.SeriesInstanceUID = dicomDetails.FirstOrDefault(r => r.TagID == "(0020,000e)").TagValue;
                        seriesModel.ImageCount = item.GetFiles("*.dcm").Length;
                        seriesModel.BodyPart = dicomDetails.FirstOrDefault(r => r.TagID == "(0018,0015)").TagValue;
                        if (seriesModel.ImageCount > 1)
                        {
                            seriesModel.ImageType = Constants.IMAGE_TYPE_TOMO;
                        }
                        else
                        {
                            seriesModel.ImageType = Constants.IMAGE_TYPE_TOPO;
                        }
                        seriesModel.ReconEndDate = dateTime;
                        seriesModel.PatientPosition = dicomDetails.FirstOrDefault(r => r.TagID == "(0018,5100)").TagValue;
                        seriesModel.ScanId = string.Empty;
                        seriesModel.ReconId = string.Empty;
                        seriesModel.SeriesPath = item.FullName;
                        _studyApplicationService.AddSeries(seriesModel);
                    }
                }

            });
        }

        public void Reset()
        {
            this.ChosenPath = string.Empty;
            this.OmittedChosenPath = string.Empty;
            this.MessageContent = string.Empty;
            this.SetButtonEnable();
        }

        private void SetButtonEnable()
        {
            if (string.IsNullOrEmpty(this.ChosenPath))
            {
                this.IsImportButtonEnabled = false;
            }
            else if (!Directory.Exists(this.ChosenPath) && !File.Exists(this.ChosenPath))
            {
                this.IsImportButtonEnabled = false;
            }
            else
            {
                this.IsImportButtonEnabled = true;
            }
        }

        private void OnClose(object parameter)
        {
            this.Reset();

            if (parameter is Window window)
            {
                window.Hide();
            }
        }

    }
}
