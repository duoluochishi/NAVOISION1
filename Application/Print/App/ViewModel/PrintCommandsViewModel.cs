//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:43:11    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.ClientProxy.DataService;
using NV.CT.DatabaseService.Contract;
using NV.CT.DicomUtility.DicomCodeStringLib;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.Language;
using NV.CT.Print.Events;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;

namespace NV.CT.Print.ViewModel
{
    public class PrintCommandsViewModel : BaseViewModel
    {
        private readonly ILogger<PrintCommandsViewModel>? _logger;
        private readonly IDialogService _dialogService;
        private readonly IJobRequestService _jobRequestService;
        private readonly IAuthorization _authorizationService;
        private readonly IPrintConfigManager _printConfigManager;
        private readonly string _printConfigRootPath = RuntimeConfig.Console.PrintConfig.Path;
        private readonly string _configFileExtension = ".json";
        private readonly string _latest = "Latest";
        private readonly string _history = "History";
        private readonly string _printing = "Printing";
        private string _savingImagePath = string.Empty;

        private ConcurrentDictionary<int, string> _imagePathMap = new ConcurrentDictionary<int, string>();

        private bool _isPriting = false; //用于区别预览的OnNotifyReadyPage事件

        private bool _hasPages = false;
        public bool HasPages
        {
            get
            { 
                return _hasPages;
            }
            set
            { 
                SetProperty(ref _hasPages, value);
            }
        }

        private bool _isPrintEnabled = false;
        public bool IsPrintEnabled
        {
            get
            {
                return _isPrintEnabled;
            }
            set
            {
                SetProperty(ref _isPrintEnabled, value);
            }
        }

        private bool _hasSelectedPages = false;
        public bool HasSelectedPages
        {
            get
            {
                return _hasSelectedPages;
            }
            set
            {
                SetProperty(ref _hasSelectedPages, value);
            }
        }

        private bool _isOverviewChecked = false;
        public bool IsOverviewChecked
        {
            get
            {
                return _isOverviewChecked;
            }
            set
            {
                SetProperty(ref _isOverviewChecked, value);

                if(value is true)
                {
                    EventAggregator.Instance.GetEvent<DisplayModeChangedEvent>().Publish(PrintDisplayMode.Overview);
                }
            }
        }

        private bool _isBrowserChecked = true;
        public bool IsBrowserChecked
        {
            get
            {
                return _isBrowserChecked;
            }
            set
            {
                SetProperty(ref _isBrowserChecked, value);

                if (value is true)
                {
                    EventAggregator.Instance.GetEvent<DisplayModeChangedEvent>().Publish(PrintDisplayMode.Browser);
                }
            }
        }

        public string CurrentUserAccount 
        {
            get;
            set;
        } = string.Empty;

        public PrintCommandsViewModel(ILogger<PrintCommandsViewModel> logger,
                                      IDialogService dialogService,
                                      IJobRequestService jobRequestService,
                                      IAuthorization authorizationService,
                                      IPrintConfigManager printConfigManager)
        {
            _logger = logger;
            _dialogService = dialogService;
            _jobRequestService = jobRequestService;
            _authorizationService = authorizationService;
            _printConfigManager = printConfigManager;

            Commands.Add(PrintConstants.COMMAND_PRINT, new DelegateCommand(OnPrint));
            Commands.Add(PrintConstants.COMMAND_CANCEL, new DelegateCommand(OnCancel));

            Global.Instance.ImageViewer.NotifyReadyPage += OnNotifyReadyPage;
            _authorizationService.CurrentUserChanged += OnCurrentUserChanged;

            EventAggregator.Instance.GetEvent<SelectedPagesChangedEvent>().Subscribe(OnHasSelectedPages);

            this.CurrentUserAccount = _authorizationService.GetCurrentUser() is null ? string.Empty : _authorizationService.GetCurrentUser().Account;
            this.SetPrintEnabledStatus();
        }

        private void OnCurrentUserChanged(object? sender, CT.Models.UserModel e)
        {
            this.CurrentUserAccount = e is null ? string.Empty : e.Account;
            this.SetPrintEnabledStatus();
        }

        private void OnNotifyReadyPage(object? sender, (int pageNumber, System.Drawing.Bitmap value) e)
        {
            //由于此回调函数被打印和预览共用，这里加标记予以区别处理。
            if (!_isPriting)
            {
                //如果不是打印请求，则不予处理。
                return;
            }

            // 持久化图片到指定路径
            _logger?.LogDebug($"== Received print page number to save:{e.pageNumber}");

            string studyId = Global.Instance.PrintingStudy.Id;
           
            if (!Directory.Exists(_savingImagePath))
            {
                try
                {
                    Directory.CreateDirectory(_savingImagePath);
                }
                catch
                {
                    string message = $"OnNotifyReadyPage: Failed to create path:[{_savingImagePath}].";
                    _logger?.LogError(message);
                    throw new DirectoryNotFoundException(message);
                }
            }

            string imageFilePath = Path.Combine(_savingImagePath, $"{e.pageNumber.ToString()}.bmp");
            try 
            {
                e.value.Save(imageFilePath, ImageFormat.Bmp);
            }
            catch(Exception ex)
            {
                this._logger?.LogError($"PrintCommandsViewModel: Failed to save image with exception:{ex.Message}");
            } 

            _imagePathMap.AddOrUpdate(e.pageNumber, imageFilePath, (i, s) => imageFilePath);

        }        

        private void OnPrint()
        {
            _imagePathMap.Clear();
            _isPriting = true;
            _savingImagePath = Path.Combine(RuntimeConfig.Console.PrintConfig.Path, _printing, Global.Instance.PrintingStudy.Id, $"{Global.Instance.PrintingStudy.Id}-{DateTime.Now.ToString("yyyyMMddHHmmssfff")}");

            int scale = 3;
            var selectPagesViewModel = CTS.Global.ServiceProvider.GetRequiredService<SelectPagesViewModel>();
            var selectedPageNumbers = selectPagesViewModel.PageWithCheckModelList.Where(p => p.IsChecked).Select(p =>p.PageNumber).ToArray();
            foreach (var number in selectedPageNumbers)
            {
                _logger?.LogDebug($"== start print page number:{number-1}");
                Global.Instance.ImageViewer.GetSinglePrintPageByIndex(number-1, scale);
            }

            _isPriting = false;

            //创建历史打印配置文件
            _printConfigManager.Save(Global.Instance.PrintingStudy.Id);
            string currentPrintConfigFilePath = Path.Combine(this._printConfigRootPath, this._latest, Global.Instance.PrintingStudy.Id, $"{Global.Instance.PrintingStudy.Id}{_configFileExtension}");
            string bakRootPath = Path.Combine(this._printConfigRootPath, this._history, Global.Instance.PrintingStudy.Id);
            string bakPrintConfigFilePath = Path.Combine(bakRootPath, $"{Global.Instance.PrintingStudy.Id}_History{DateTime.Now.ToString("yyyyMMddHHmmssfff")}{_configFileExtension}");
            
            if (!Directory.Exists(bakRootPath))
            {
                Directory.CreateDirectory(bakRootPath);
            }
            File.Copy(currentPrintConfigFilePath, bakPrintConfigFilePath);

            //正式开始打印
            StartPrint(bakPrintConfigFilePath);
        }

        private void StartPrint(string printConfigFilePath)
        {
            var imageOperationViewModel = CTS.Global.ServiceProvider.GetService<ImageOperationViewModel>();
            var selectPrinterViewModel = CTS.Global.ServiceProvider.GetService<SelectPrinterViewModel>();

            var printJobRequest = new PrintJobRequest();
            printJobRequest.Id = Guid.NewGuid().ToString();
            printJobRequest.WorkflowId = Guid.NewGuid().ToString();
            printJobRequest.InternalPatientID = Global.Instance.PrintingStudy.InternalPatientId;
            printJobRequest.InternalStudyID = Global.Instance.PrintingStudy.Id;
            printJobRequest.Priority = 5;
            printJobRequest.JobTaskType = JobTaskType.PrintJob;
            printJobRequest.Creator = this.CurrentUserAccount;
            printJobRequest.PrintConfigPath = printConfigFilePath;

            printJobRequest.PatientId = Global.Instance.PrintingStudy.InternalPatientId;
            printJobRequest.StudyId = Global.Instance.PrintingStudy.Id;
            printJobRequest.SeriesID = string.Empty; //imageOperationViewModel.SelectedPrintingSeriesModel.Id;
            printJobRequest.CallingAE = selectPrinterViewModel.SelectedServerAETitle.AECaller;
            printJobRequest.CalledAE = selectPrinterViewModel.SelectedServerAETitle.AETitle;
            printJobRequest.Host = selectPrinterViewModel.SelectedServerAETitle.IP;
            printJobRequest.Port = selectPrinterViewModel.SelectedServerAETitle.Port;
            printJobRequest.NumberOfCopies = selectPrinterViewModel.Copies; 
            printJobRequest.PageSize = selectPrinterViewModel.SelectedPageSize is null ? FilmSizeIDCS.FILMSIZE_14INX17IN : selectPrinterViewModel.SelectedPageSize.ValueText;
            printJobRequest.Orientation = selectPrinterViewModel.SelectedOrientation is null ? Constants.DICOM_ORIENTATION_PORTRAIT : selectPrinterViewModel.SelectedOrientation.ValueText;
            printJobRequest.Layout = string.Empty;

            foreach (var imagePath in _imagePathMap)
            {
                printJobRequest.ImagePathList.Add(imagePath.Value);
            }

            printJobRequest.Parameter = printJobRequest.ToJson();
            var result = this._jobRequestService.EnqueueJobRequest(printJobRequest);
            if (result.Status == CommandExecutionStatus.Success)
            {
                _dialogService.ShowDialog(false, MessageLeveles.Info,
                          LanguageResource.Message_Info_CloseInformationTitle,
                          LanguageResource.Message_Info_PrintTaskQueued,
                          null, ConsoleSystemHelper.WindowHwnd);
            }
            else
            {
                _dialogService.ShowDialog(false, MessageLeveles.Error,
                              LanguageResource.Message_Info_CloseInformationTitle,
                              LanguageResource.Message_Error_PrintTaskFailed,
                              null, ConsoleSystemHelper.WindowHwnd);
            }
        }

        private void OnCancel()
        {


        }

        private bool GetServerConnectState(string ip)
        {
            var ping = new Ping();
            try
            {
                PingReply pr = ping.Send(ip, 500);
                if (pr.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect Print Server IP {ip} with Status Error,{ex.Message},{ex.StackTrace}");
                return false;
            }
        }

        private void OnHasSelectedPages(bool hasSelectedPages)
        {
            HasSelectedPages = hasSelectedPages;
            this.SetPrintEnabledStatus();
        }

        private void SetPrintEnabledStatus()
        {
            IsPrintEnabled = HasSelectedPages && AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_PRINT); ;
        }

    }
}
