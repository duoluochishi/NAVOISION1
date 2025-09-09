//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.CT.AppService.Contract;
using NV.CT.ClientProxy.DataService;
using NV.CT.DatabaseService.Contract;
using NV.CT.DicomUtility.DoseReportSR;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.Language;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using NV.CT.PatientManagement.Helpers;
using NV.CT.PatientManagement.Models;
using NV.CT.UI.Controls.Archive;
using NV.CT.UI.Controls.Export;
using NV.CT.WorkflowService.Contract;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Collections;

namespace NV.CT.PatientManagement.ViewModel;

public class SeriesViewModel : BaseViewModel
{
    #region Members
    private readonly ILogger<SeriesViewModel> _logger;
    private IList? _selectedItems;
    private readonly ISeriesApplicationService _seriesApplicationService;
    private readonly IStudyApplicationService _studyApplicationService;
    private readonly IApplicationCommunicationService _applicationCommunicationService;
    private readonly IViewer _viewer;
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly IAuthorization _authorizationService;
    private readonly IJobRequestService _jobRequestService;
    private readonly IRawDataService _rawDataService;
    private string _currentStudyID = string.Empty;

    private DelegateCommand _deleteCommand;
    private DelegateCommand _exportCommand;
    private DelegateCommand _archiveCommand;
    private DelegateCommand _browserCommand;
    private DelegateCommand _browserRawDataCommand;
    #endregion

    #region Properties

    private bool _isClickedOnDataRowOfDatagrid = false;
    /// <summary>
    /// 标记鼠标是否点击了DataGrid的数据行
    /// </summary>
    public bool IsClickedOnDataRowOfDatagrid
    {
        get => _isClickedOnDataRowOfDatagrid;
        set => _isClickedOnDataRowOfDatagrid = value;
    }

    private ObservableCollection<SeriesModel>? _seriesModels = new ObservableCollection<SeriesModel>();
    public ObservableCollection<SeriesModel>? SeriesModels
    {
        get => _seriesModels;
        set 
        { 
            SetProperty(ref _seriesModels, value);
        }
    }

    private SeriesModel? _selectedItem;
    public SeriesModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetProperty(ref _selectedItem, value);

            if (_selectedItem is not null)
            {
                _seriesApplicationService.RaiseSelectItemChanged(new[] { _selectedItem.Id, _selectedItem.SeriesPath, _selectedItem.ImageType });

                this.IsPermissionBrowserRawDataEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_EXPORT) && !string.IsNullOrEmpty(this.SelectedItem?.ScanId);
            }
            else
            {
                _seriesApplicationService.RaiseSelectItemChanged(new string[] { });
            }
            
        }
    }

    private bool _deleteMenuItemIsEnabled;
    public bool DeleteMenuItemIsEnabled
    {
        get => _deleteMenuItemIsEnabled;
        set => SetProperty(ref _deleteMenuItemIsEnabled, value);
    }

    private bool _isPermissionArchiveEnabled;
    public bool IsPermissionArchiveEnabled
    {
        get => _isPermissionArchiveEnabled;
        set => SetProperty(ref _isPermissionArchiveEnabled, value);
    }

    private bool _isPermissionExportEnabled;
    public bool IsPermissionExportEnabled
    {
        get => _isPermissionExportEnabled;
        set => SetProperty(ref _isPermissionExportEnabled, value);
    }

    private bool _isPermissionDeletionEnabled;
    public bool IsPermissionDeletionEnabled
    {
        get => _isPermissionDeletionEnabled;
        set => SetProperty(ref _isPermissionDeletionEnabled, value);
    }

    private bool _isPermissionBrowserEnabled;
    public bool IsPermissionBrowserEnabled
    {
        get => _isPermissionBrowserEnabled;
        set => SetProperty(ref _isPermissionBrowserEnabled, value);
    }

    private bool _isPermissionBrowserRawDataEnabled;
    public bool IsPermissionBrowserRawDataEnabled
    {
        get => _isPermissionBrowserRawDataEnabled;
        set => SetProperty(ref _isPermissionBrowserRawDataEnabled, value);
    }
    private bool _isPermissionOpenImageViewerEnabled;
    public bool IsPermissionOpenImageViewerEnabled
    {
        get => _isPermissionOpenImageViewerEnabled;
        set => SetProperty(ref _isPermissionOpenImageViewerEnabled, value);
    }
    #endregion

    #region Constructor

    public SeriesViewModel(ILogger<SeriesViewModel> logger,
                           ISeriesApplicationService seriesApplicationService, 
                           IMapper mapper, 
                           IStudyApplicationService studyApplicationService, 
                           IDialogService dialogService,
                           IAuthorization authorizationService,
                           IJobRequestService jobRequestService,
                           IRawDataService rawDataService, 
                           IApplicationCommunicationService applicationCommunicationService,
                           IViewer viewer)
    {
        _logger = logger;
        _mapper = mapper;
        _dialogService = dialogService;
        _seriesApplicationService = seriesApplicationService;
        _studyApplicationService = studyApplicationService;
        _authorizationService = authorizationService;
        _jobRequestService = jobRequestService;
        _rawDataService = rawDataService;
        _applicationCommunicationService = applicationCommunicationService;
        _viewer = viewer;

        _deleteCommand = new DelegateCommand(OnDelete);
        Commands.Add(PatientManagementConstants.COMMAND_DELETE, _deleteCommand);
        _exportCommand = new DelegateCommand(OnExport);
        Commands.Add(PatientManagementConstants.COMMAND_EXPORT, _exportCommand);
        _archiveCommand = new DelegateCommand(OnArchive);
        Commands.Add(PatientManagementConstants.COMMAND_ARCHIVE, _archiveCommand);
        _browserCommand = new DelegateCommand(OnBrowse);
        Commands.Add(PatientManagementConstants.COMMAND_BROWSER, _browserCommand);
        _browserRawDataCommand = new DelegateCommand(OnBrowseRawData);
        Commands.Add(PatientManagementConstants.COMMAND_BROWSER_RAW_DATA, _browserRawDataCommand);

        Commands.Add(PatientManagementConstants.COMMAND_CLICKITEM, new DelegateCommand<object>(OnClicked));
        Commands.Add(PatientManagementConstants.COMMAND_SELECTION_CHANGED, new DelegateCommand<object>(OnSelectionChanged));
        Commands.Add(PatientManagementConstants.COMMAND_OPEN_VIEWER, new DelegateCommand(OnOpenViewer));

        _studyApplicationService.SelectItemChanged += OnSelectedStudyChanged;
        _seriesApplicationService.Refresh += OnRefresh;
        _authorizationService.CurrentUserChanged += OnCurrentUserChanged;

        this.InitializeDataList();
    }

    private void OnCurrentUserChanged(object? sender, CT.Models.UserModel e)
    {
        this.SetPermissions();
    }

    #endregion

    #region private methods

    #region Commands Events
    private void OnOpenViewer()
    {
        if (SelectedItem is null)
            return;
        if (!IsPermissionOpenImageViewerEnabled)
            return;
        if (SelectedItem.SeriesType == Constants.SERIES_TYPE_DOSE_REPORT || SelectedItem.SeriesType == Constants.SERIES_TYPE_SR)
            return;
        Task.Run(() =>
        {
            var studyId = SelectedItem.StudyId;
            var seriesID = SelectedItem.Id;
            string sendstring = studyId + "," + seriesID;
            _applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_VIEWER));
            _viewer.StartViewer(sendstring);
        });
    }
    private void OnDelete()
    {
        var studyViewListViewModel = Global.Instance.ServiceProvider.GetService<StudyViewModel>();
        if (studyViewListViewModel is null || studyViewListViewModel.SelectedItem is null)
        {
            return;
        }

        if (studyViewListViewModel.SelectedItem.IsProtected)
        {
            _dialogService.ShowDialog(false,
                                      MessageLeveles.Info,
                                      LanguageResource.Message_Info_CloseInformationTitle,
                                      LanguageResource.Message_Info_Deletion_Protected,
                                      null,
                                      ConsoleSystemHelper.WindowHwnd);
            return;
        }

        if (_selectedItems == null || _selectedItems.Count == 0)
        {
            return;
        }

        _dialogService.ShowDialog(true, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, LanguageResource.Message_Confirm_Delete, arg =>
        {
            if (arg.Result == ButtonResult.OK)
            {
                //TODO 以后需要优化，批量删除
                foreach (var item in _selectedItems)
                {
                    var seriesModel= _mapper.Map<ApplicationService.Contract.Models.SeriesModel>(item);
                    _seriesApplicationService.Delete(seriesModel);
                    this._logger.LogInformation($"Delete Series {JsonConvert.SerializeObject(seriesModel)}");
                }
            }
        }, ConsoleSystemHelper.WindowHwnd);
    }

    private void OnSelectionChanged(object obj)
    {
        if (obj == null)
        {
            _selectedItems = null;
            return;
        }

        _selectedItems = (IList)obj;
    }

    private void OnExport()
    {
        if (_selectedItems == null || _selectedItems.Count == 0)
        {
            return;
        }

        var exportWindow = Global.Instance.ServiceProvider.GetRequiredService<ExportWindow>();
        var exportViewModel = exportWindow.DataContext as ExportWindowViewModel;

        var callback = new Action<ExportSelectionModel>(exportSelectionModel => {
            this.BeginExport(exportSelectionModel);
        });
        exportViewModel?.Show(callback);
        exportWindow.ShowWindowDialog();
    }

    private void OnArchive()
    {
        //如果没有任何可导入的序列，则提示并返回
        if (_selectedItems == null || _selectedItems.Count == 0)
        {
            _dialogService.ShowDialog(false, MessageLeveles.Error,
                                      LanguageResource.Message_Info_CloseInformationTitle,
                                      LanguageResource.Message_Error_CannotArchiveNoAnySeries,
                                      null, ConsoleSystemHelper.WindowHwnd);

            return;
        }

        var archiveWindow = Global.Instance.ServiceProvider.GetRequiredService<ArchiveWindow>();
        if (archiveWindow is null)
        {
            this._logger.LogWarning("archiveWindow is null in OnArchive.");
            return;
        } 
        var archiveViewModel = archiveWindow.DataContext as ArchiveWindowViewModel;
        var callback = new Action<ArchiveModel,bool,bool>((archiveModel,useTls,anonymous) => 
        {
            this.BeginArchive(archiveModel, useTls, anonymous);
        });
        archiveViewModel?.Show(callback);
        archiveWindow.ShowWindowDialog();
    }

    private void OnBrowse()
    {
        if (_selectedItems == null || _selectedItems.Count == 0)
        {
            return;
        }

        foreach (SeriesModel selectedItem in _selectedItems)
        {
            if (string.IsNullOrEmpty(selectedItem.SeriesPath))
            {
                _dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"The path does is empty.", null, ConsoleSystemHelper.WindowHwnd);
                continue;
            }
            else if (Directory.Exists(selectedItem.SeriesPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", selectedItem.SeriesPath);
            }
            else if (File.Exists(selectedItem.SeriesPath))
            {
                System.Diagnostics.Process.Start("explorer.exe", new FileInfo(selectedItem.SeriesPath).Directory.FullName);
            }
            else
            {
                this._logger.LogInformation($"The Series path does not exist:{selectedItem.SeriesPath}");
                _dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"The path does not exist:{selectedItem.SeriesPath}", null, ConsoleSystemHelper.WindowHwnd);
                continue;
            }
        }
    }

    private void OnBrowseRawData()
    {
        if (_selectedItems == null || _selectedItems.Count == 0)
        {
            return;
        }

        var distinctScanIdList = new List<SeriesModel>();
        foreach (SeriesModel selectedItem in _selectedItems)
        {
            if (!distinctScanIdList.Any(s => s.ScanId == selectedItem.ScanId))
            {
                distinctScanIdList.Add(selectedItem);
            }
        }

        var rawdataList = _rawDataService.GetRawDataListByStudyId(_currentStudyID);
        foreach (SeriesModel selectedItem in distinctScanIdList)
        {
            if (string.IsNullOrEmpty(selectedItem.ScanId))
            {
                continue;
            }

            var rawdata = rawdataList.SingleOrDefault(r => r.ScanId == selectedItem.ScanId);
            if (rawdata is null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(rawdata.Path))
            {
                _dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"The path is empty.", null, ConsoleSystemHelper.WindowHwnd);
                continue;
            }
            else if (Directory.Exists(rawdata.Path))
            {
                System.Diagnostics.Process.Start("explorer.exe", rawdata.Path);
            }
            else if (File.Exists(rawdata.Path))
            {
                System.Diagnostics.Process.Start("explorer.exe", new FileInfo(rawdata.Path).Directory.FullName);
            }
            else
            {
                this._logger.LogInformation($"The path does not exist:{rawdata.Path}");
                _dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"The path does not exist:{rawdata.Path}", null, ConsoleSystemHelper.WindowHwnd);
                continue;
            }
        }
    }    

    private void OnClicked(object obj)
    {
        //TODO: SeriesType will change to enum
        if (IsClickedOnDataRowOfDatagrid && obj is SeriesModel { SeriesType: "Dose SR" } seriesModel)
        {
            if (!File.Exists(seriesModel.SeriesPath))
            {
                _logger.LogDebug($"Path of Dose SR Structured Repor does not exist: {seriesModel.SeriesPath}");
                return;
            }

            var reportItem = StructureReportReader.Instance.GetStructuredReportItem(seriesModel.SeriesPath);
            var radiationInfo = StructureReportReader.Instance.GetXRayRadiationDoseSRIOD(seriesModel.SeriesPath);

            var vm = Global.Instance.ServiceProvider.GetRequiredService<DoseReportDetailViewModel>();
            vm.ReportItem = reportItem;
            vm.RadiationInfo = radiationInfo;

            _seriesApplicationService.SeriesItemChanged();

            //针对弹窗的代码
            var doseWindow = Global.Instance.ServiceProvider.GetRequiredService<DoseReportDetailWindow>();
            doseWindow.ShowWindowDialog();
        }
    }

    #endregion

    #region Events
    private void OnRefresh(object? sender, EventArgs<(ApplicationService.Contract.Models.SeriesModel, DataOperateType)> e)
    {
        var seriesModel = _mapper.Map<SeriesModel>(e.Data.Item1);
        if (e.Data.Item2 == DataOperateType.Delete)
        {
            var existingSeriesModel = SeriesModels?.FirstOrDefault(s => s.Id == seriesModel.Id);
            if (existingSeriesModel != null)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    SeriesModels?.Remove(existingSeriesModel);
                    SelectedItem = SeriesModels?.Count > 0 ? SeriesModels[0] : null;                   
                });

            }
        }
        else if (e.Data.Item2 == DataOperateType.Update)
        {
            var existingSeriesModel = SeriesModels?.FirstOrDefault(s => s.Id == seriesModel.Id);
            if (existingSeriesModel != null)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    existingSeriesModel.ArchiveStatus = seriesModel.ArchiveStatus;
                    existingSeriesModel.PrintStatus = seriesModel.PrintStatus;
                    existingSeriesModel.ImageCount = seriesModel.ImageCount;
                });
            }
        }
        else if (e.Data.Item2 == DataOperateType.Add)
        {
            var existingSeriesModel = SeriesModels?.FirstOrDefault(s => s.Id == seriesModel.Id);
            if (existingSeriesModel == null && seriesModel.StudyId == this._currentStudyID)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    var addedSeriesModel = _mapper.Map<SeriesModel>(e.Data.Item1);
                    SeriesModels?.Add(addedSeriesModel);
                });
            }
        }

        Application.Current?.Dispatcher.Invoke(() =>
        {
            this.SetPermissions();
        });  
    }

    private void OnSelectedStudyChanged(object? sender, EventArgs<string> e) 
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {            
            this.LoadDataByStudyId(e.Data);           
        });
    }

    private void InitializeDataList()
    {
        var studyViewListViewModel = Global.Instance.ServiceProvider.GetService<StudyViewModel>();
        if (studyViewListViewModel is null || studyViewListViewModel.SelectedItem is null)
        {
            return;
        }

        this.LoadDataByStudyId(studyViewListViewModel.SelectedItem.StudyId);
    }

    private void LoadDataByStudyId(string studyId)
    {
        SeriesModels?.Clear();
        this._currentStudyID = studyId;
        if (string.IsNullOrEmpty(studyId))
        {
            SelectedItem = null;
            return;
        }

        var result = _seriesApplicationService.GetSeriesByStudyId(studyId);
        result.ForEach(a =>
        {
            var seriesModel = _mapper.Map<SeriesModel>(a);
            seriesModel.SeriesNumber = string.IsNullOrEmpty(a.SeriesNumber) ? string.Empty : a.SeriesNumber;
            SeriesModels?.Add(seriesModel);
        });

        SelectedItem = SeriesModels?.FirstOrDefault();
        this.SetPermissions();
    }

    #endregion

    private void SetPermissions()
    {
        if (SelectedItem is null)
        {
            this.IsPermissionDeletionEnabled = false;
            this.IsPermissionArchiveEnabled = false;
            this.IsPermissionExportEnabled = false;
            this.IsPermissionBrowserEnabled = false;
            this.IsPermissionBrowserRawDataEnabled = false;
            this.IsPermissionOpenImageViewerEnabled = false;
            return;
        }

        this.IsPermissionDeletionEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_DATA_DELETION);
        this.IsPermissionArchiveEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_ARCHIVE);
        this.IsPermissionExportEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_EXPORT);
        this.IsPermissionBrowserEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_EXPORT);
        this.IsPermissionBrowserRawDataEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_EXPORT) && !string.IsNullOrEmpty(this.SelectedItem?.ScanId);
        this.IsPermissionOpenImageViewerEnabled = AuthorizationHelper.ValidatePermission(SystemPermissionNames.PATIENT_MANAGEMENT_IMAGE_VIEWER);
    }

    private void BeginArchive(ArchiveModel archiveConfigModel,bool useTls,bool isAnonymous)
    {
        if (archiveConfigModel is null)
        {
            this._logger.LogWarning("Parameter archiveConfigModel of BeginArchive is null.");
            return;
        }

        var targetSeriesIdList = _selectedItems.Cast<SeriesModel>().Select(s => s.Id).ToList();
        var studyModels = _studyApplicationService.GetStudiesByIds(new string[] { this._currentStudyID });
        var patientId = studyModels.First().InternalPatientId;
        AddArchiveTask(patientId, this._currentStudyID, targetSeriesIdList, ArchiveLevel.Series, archiveConfigModel, useTls, isAnonymous);
    }

    private void AddArchiveTask(string patientId, string studyId, List<string> targetSeriesIdList, ArchiveLevel archiveLevel, ArchiveModel archiveConfigModel, bool useTls, bool isAnonymous)
    {
        var archiveJobTask = new ArchiveJobRequest();
        archiveJobTask.Id = Guid.NewGuid().ToString();
        archiveJobTask.WorkflowId = Guid.NewGuid().ToString();
        archiveJobTask.InternalPatientID = patientId;
        archiveJobTask.InternalStudyID = studyId;
        archiveJobTask.Priority = 5;
        archiveJobTask.JobTaskType = JobTaskType.ArchiveJob;
        archiveJobTask.Creator = _authorizationService.GetCurrentUser() is null ? string.Empty : _authorizationService.GetCurrentUser().Account;
        archiveJobTask.ArchiveLevel = archiveLevel;
        archiveJobTask.StudyId = studyId;
        archiveJobTask.SeriesIdList = targetSeriesIdList;
        archiveJobTask.Host = archiveConfigModel.Host;
        archiveJobTask.Port = int.Parse(archiveConfigModel.Port);
        archiveJobTask.AECaller = archiveConfigModel.AECaller;
        archiveJobTask.AETitle = archiveConfigModel.AETitle;
        archiveJobTask.DicomTransferSyntax = archiveConfigModel.TransferSyntax;
        archiveJobTask.Parameter = archiveJobTask.ToJson();
        archiveJobTask.UseTls = useTls;
        archiveJobTask.Anonymous = isAnonymous;

        archiveJobTask.Parameter = archiveJobTask.ToJson();

        var result = this._jobRequestService.EnqueueJobRequest(archiveJobTask);

        if (result.Status == CommandExecutionStatus.Success)
        {
            _dialogService.ShowDialog(false, MessageLeveles.Info,
                                      LanguageResource.Message_Info_CloseInformationTitle,
                                      LanguageResource.Message_Info_ArchiveTaskStarted,
                                      null, ConsoleSystemHelper.WindowHwnd);

        }
        else
        {
            _dialogService.ShowDialog(false, MessageLeveles.Error,
                                      LanguageResource.Message_Info_CloseInformationTitle,
                                      LanguageResource.Message_Error_ArchiveTaskFailed,
                                      null, ConsoleSystemHelper.WindowHwnd);

        }
    }

    private void BeginExport(ExportSelectionModel exportSelectionModel)
    {
        if (exportSelectionModel is null)
        {
            this._logger.LogWarning("Parameter exportSelectionModel of BeginExport is null.");
            return;
        }

        //如果没有任何可导入的序列，则提示并返回
        if (_selectedItems is null || _selectedItems.Count == 0)
        {
            _dialogService.ShowDialog(false, MessageLeveles.Error,
                                      LanguageResource.Message_Info_CloseInformationTitle,
                                      LanguageResource.Message_Error_NoSelectedSeries,
                                      null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var selectedSeriesModels = new List<SeriesModel>();
        foreach (SeriesModel model in _selectedItems)
        {
            selectedSeriesModels.Add(model); 
        }

        var targetSeriesIdList = selectedSeriesModels.Select(s => s.Id).ToList();
        var targetSeriesPathList = selectedSeriesModels.Select(s => s.SeriesPath).ToList();

        this.AddExortTask(SelectedItem.StudyId, targetSeriesIdList, targetSeriesPathList, exportSelectionModel);
    }

    private void AddExortTask(string studyId, List<string> targetSeriesIdList, List<string> targetSeriesPathList, ExportSelectionModel exportSelectionModel)
    {
        var exportJobRequest = new ExportJobRequest();

        exportJobRequest.InputFolders.AddRange(targetSeriesPathList);
        exportJobRequest.PatientNames = FetchPatientNameList(new string[] { studyId });
        exportJobRequest.IsAnonymouse = exportSelectionModel.IsAnonymouse;
        exportJobRequest.IsExportedToDICOM = exportSelectionModel.IsExportedToDICOM;
        exportJobRequest.IsExportedToImage = exportSelectionModel.IsExportedToImage;
        exportJobRequest.OutputVirtualPath = exportSelectionModel.OutputVirtualPath;
        exportJobRequest.OutputFolder = exportSelectionModel.OutputFolder;
        exportJobRequest.IsBurnToCDROM = exportSelectionModel.IsBurnToCDROM;
        exportJobRequest.IsAddViewer = exportSelectionModel.IsAddViewer;
        exportJobRequest.DicomTransferSyntax = exportSelectionModel.DicomTransferSyntax;
        exportJobRequest.PictureType = exportSelectionModel.PictureType;

        exportJobRequest.Id = Guid.NewGuid().ToString();
        exportJobRequest.WorkflowId = Guid.NewGuid().ToString();
        exportJobRequest.InternalPatientID = string.Empty;
        exportJobRequest.InternalStudyID = string.Empty;
        exportJobRequest.Priority = 5;
        exportJobRequest.JobTaskType = JobTaskType.ExportJob;
        exportJobRequest.Creator = _authorizationService.GetCurrentUser() is null ? string.Empty : _authorizationService.GetCurrentUser().Account;
        exportJobRequest.OperationLevel = OperationLevel.Series;
        exportJobRequest.StudyId = studyId;
        exportJobRequest.SeriesIdList = targetSeriesIdList;
        exportJobRequest.Parameter = exportJobRequest.ToJson();

        var result = this._jobRequestService.EnqueueJobRequest(exportJobRequest);
        if (result.Status == CommandExecutionStatus.Success)
        {
            _dialogService.ShowDialog(false, MessageLeveles.Info,
                                      LanguageResource.Message_Info_CloseInformationTitle,
                                      LanguageResource.Message_Info_ExportingTaskStarted,
                                      null, ConsoleSystemHelper.WindowHwnd);
        }
        else
        {
            _dialogService.ShowDialog(false, MessageLeveles.Error,
                                      LanguageResource.Message_Info_CloseInformationTitle,
                                      LanguageResource.Message_Error_ExportTaskFailed,
                                      null, ConsoleSystemHelper.WindowHwnd);
        }
    }

    private List<string> FetchPatientNameList(string[] studyIds)
    {
        var studyViewModel = Global.Instance.ServiceProvider.GetService<StudyViewModel>();
        var patientNameList = studyViewModel.VStudies.Where(s => studyIds.Contains(s.StudyId)).Select(s => s.PatientName).ToList();
        return patientNameList;
    }

    #endregion

}