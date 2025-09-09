//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using FellowOakDicom;
using MaterialDesignThemes.Wpf.Controls.MarkableTextBox;
using Newtonsoft.Json;
using NV.CT.ClientProxy.DataService;
using NV.CT.ClientProxy.JobService;
using NV.CT.ClientProxy.Workflow;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.PostProcessEnums;
using NV.CT.FacadeProxy.Common.Models.PostProcess;
using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.Model;
using NV.CT.ImageViewer.View;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.Protocol.Models;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.CT.UI.Controls.Archive;
using NV.CT.UI.Controls.Export;
using NV.CT.WorkflowService.Contract;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;

namespace NV.CT.ImageViewer.ViewModel;

public class SeriesViewModel : BaseViewModel
{
    private readonly ILogger<SeriesViewModel> _logger;
    private readonly IMapper _mapper;
    private readonly IViewerService _viewerService;
    private readonly ISeriesService _seriesService;
    private readonly IRawDataService _rawDataService;
    private readonly IStudyService _studyService;
    private readonly IDialogService _dialogService;
    private readonly IAuthorization _authorization;
    private readonly IJobRequestService _jobRequestService;
    private readonly IOfflineTaskService _offlineService;
    private readonly IOfflineConnection _offlineConnection;
    private readonly IPrint _printService;
    private readonly IApplicationCommunicationService _applicationCommunicationService;

    private const string SELECTED_SERIES = "Selectedseries";
    private const string SELECTED_IMAGES = "Selectedimages";

    private const int MaxPostProcessCount = 8;
    private ObservableCollection<SeriesModel> seriesModels = new();
    public ObservableCollection<SeriesModel> SeriesModels
    {
        get => seriesModels;
        set => SetProperty(ref seriesModels, value);
    }

    private ObservableCollection<ImageModel> imageModels = new();
    /// <summary>
    /// 序列图集合
    /// </summary>
    public ObservableCollection<ImageModel> ImageModels
    {
        get => imageModels;
        set => SetProperty(ref imageModels, value);
    }
    private ObservableCollection<SeriesModel> series2DModels = new();
    public ObservableCollection<SeriesModel> Series2DModels
    {
        get => series2DModels;
        set => SetProperty(ref series2DModels, value);
    }

    private ObservableCollection<ImageModel> image2DModels = new();
    /// <summary>
    /// 序列图集合
    /// </summary>
    public ObservableCollection<ImageModel> Image2DModels
    {
        get => image2DModels;
        set => SetProperty(ref image2DModels, value);
    }
    private SeriesModel? _selectedSeriesItem;
    public SeriesModel? SelectedSeriesItem
    {
        get => _selectedSeriesItem;
        set
        {
            if (SetProperty(ref _selectedSeriesItem, value))
            {
                if (value is not null)
                {
                    IsSeriesItemPostProcessMenuEnable = value.SeriesType == SeriesType.SeriesTypeImage;
                }
                _selectedSeriesItem = value;
            }
        }
    }

    private ImageModel? _selectImageModel;
    /// <summary>
    /// 选中序列
    /// </summary>
    public ImageModel? SelectImageModel
    {
        get => _selectImageModel;
        set
        {
            if (SetProperty(ref _selectImageModel, value))
            {
                if (value is not null)
                {
                    IsImageModelPostProcessMenuEnable = value.SeriesType == SeriesType.SeriesTypeImage;
                }
                _selectImageModel = value;
            }
        }
    }

    private bool _imagemodelOrSeries = true;
    public bool ImageModelOrSeries
    {
        get =>  _imagemodelOrSeries; 
        set => SetProperty(ref _imagemodelOrSeries,value);
    }
    private int _selected2DOR3D;

    public int Selected2DOR3D
    {
        get => _selected2DOR3D; 
        set => SetProperty(ref _selected2DOR3D, value);
    }

    public SeriesViewModel(IViewerService viewerService, IMapper mapper, ILogger<SeriesViewModel> logger, IStudyService studyService, ISeriesService seriesService, IRawDataService rawDataService, IDialogService dialogService, IJobRequestService jobRequestService, IAuthorization authorization, IOfflineTaskService offlineService, IApplicationCommunicationService applicationCommunicationService, IPrint printService, IOfflineConnection offlineConnection)
	{
		_logger = logger;
		_mapper = mapper;
		_offlineService = offlineService;
		_authorization = authorization;
		_studyService = studyService;
		_seriesService = seriesService;
		_rawDataService = rawDataService;
		_viewerService = viewerService;
		_dialogService = dialogService;
		_jobRequestService = jobRequestService;
        _offlineConnection = offlineConnection;

        _applicationCommunicationService = applicationCommunicationService;
        _printService = printService;
        _seriesService.Refresh += _seriesService_Refresh;      
        _offlineService.TaskDone += OfflineReconService_ReconDone;

        // post process
        Commands.Add(CommandName.ShowPostProcessSettingCommand, new DelegateCommand(OnShowPostProcessWindowCommand));
        Commands.Add(CommandName.ApplyPostProcessCommand, new DelegateCommand<object>(OnApplyPostProcessCommand, _ => true));
        Commands.Add(CommandName.COMMAND_ADD, new DelegateCommand<object>(PostProcessItemAdded, _ => true));
        Commands.Add(CommandName.COMMAND_REMOVE, new DelegateCommand<string>(PostProcessItemRemoved, _ => true));
        Commands.Add(CommandName.COMMAND_CLOSE, new DelegateCommand<object>(PostProcessSettingWindowClosed, _ => true));

        Commands.Add(CommandName.ArchiveCommand, new DelegateCommand<string>(OnArchiveCommand));
        Commands.Add(CommandName.BrowseCommand, new DelegateCommand(OnBrowseCommand));
        Commands.Add(CommandName.TableBrowseCommand, new DelegateCommand(OnTableBrowseCommand));
        Commands.Add(CommandName.BrowseRawDataCommand, new DelegateCommand(OnBrowseRawDataCommand));
        Commands.Add(CommandName.TableBrowseRawDataCommand, new DelegateCommand(OnTableBrowseRawDataCommand));
        Commands.Add(CommandName.PrintCommand, new DelegateCommand<string?>(printType =>
        {
            if (!string.IsNullOrEmpty(printType))
            {
                var image2DViewModel = CTS.Global.ServiceProvider.GetService<Image2DViewModel>();

                if (printType == SELECTED_SERIES)
                {
                    var selectedImageProperties = GetSelectedSeriesPrintProperties();
                    image2DViewModel.SendAppendingImages(SelectImageModel.StudyId, selectedImageProperties);

                }
                else if (printType == SELECTED_IMAGES)
                {
                    image2DViewModel?.CurrentImageViewer.PrintSelectedImages();
                }
                OnOpenPrintProcess();
            }
        }));

        if (NeedInit())
        {
            GetSeriesModelsByStudyId(Global.Instance.StudyId);
            SelectFirstSeries(Global.Instance.SeriesId);
        }
        _viewerService.ViewerChanged += OnSeriesChanged;

        PostDenoiseTypeList = EnumExtension.EnumToList(typeof(PostDenoiseType));
        InitKernelList();

        ResetPostProcessParameters();
        EventAggregator.Instance.GetEvent<ViewSceneChangedEvent>().Subscribe(UpdateSeriesData);

    }
    public void UpdateSeriesData(int tabIndex)
    {
        Selected2DOR3D = tabIndex;
        UpdateSeriesOrImageModels(tabIndex);
    }
    private void UpdateSeriesOrImageModels(int tabIndex)
    {
        Application.Current?.Dispatcher?.Invoke(() => {
            if (tabIndex == 0)
            {
                if (Series2DModels.Count != 0)
                {
                    SeriesModels = Series2DModels.Clone();

                }
                if (Image2DModels.Count != 0)
                {
                    ImageModels.Clear();
                    foreach (var image in Image2DModels) { ImageModels.Add(image); }
                }
            }
            else
            {
                var doseSeries = SeriesModels.FirstOrDefault(r => r.SeriesType == SeriesType.DoseReport);
                var topoSeries = SeriesModels.Where(r => r.ImageType == Constants.IMAGE_TYPE_TOPO).ToList();
                var screenshotSeries = SeriesModels.FirstOrDefault(r => r.SeriesType == SeriesType.ScreenshotTypeImage);
                if (doseSeries is not null)
                    SeriesModels.Remove(doseSeries);
                foreach (var topoSerie in topoSeries) { SeriesModels.Remove(topoSerie); }
                if (screenshotSeries is not null)
                    SeriesModels.Remove(screenshotSeries);
                var doseModel = ImageModels.FirstOrDefault(r => r.SeriesType == SeriesType.DoseReport);
                var topoModels = ImageModels.Where(r => r.ImageType == Constants.IMAGE_TYPE_TOPO).ToList();
                var screenshotModel = ImageModels.FirstOrDefault(r => r.SeriesType == SeriesType.ScreenshotTypeImage);
                if (doseModel is not null)
                    ImageModels.Remove(doseModel);
                foreach (var topoModel in topoModels) { ImageModels.Remove(topoModel); }
                if (screenshotModel is not null)
                    ImageModels.Remove(screenshotModel);
            }
        });
    }
    private void _seriesService_Refresh(object? sender, EventArgs<(DatabaseService.Contract.Models.SeriesModel, DataOperateType)> e)
    {
        var seriesModel = _mapper.Map<DatabaseService.Contract.Models.SeriesModel>(e.Data.Item1);
        if (e.Data.Item2 == DataOperateType.Delete)
        {
            var existingSeriesModel = SeriesModels?.FirstOrDefault(s => s.Id == seriesModel.Id);
            var existingImageModel = ImageModels?.FirstOrDefault(r=>r.SeriesId== seriesModel.Id);
            if (existingSeriesModel != null&& existingImageModel !=null)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    SeriesModels?.Remove(existingSeriesModel);
                    ImageModels?.Remove(existingImageModel);
                    SelectImageModel = ImageModels?.Count > 0 ? ImageModels[0] : null;
                });
            }
        }
    }

    public void OnSeriesChanged(object? sender, string parameters)
    {
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            EventAggregator.Instance.GetEvent<StudyChangedEvent>().Publish(true);
            Global.Instance.StudyId = Global.Instance.ParseParameters(parameters);
            GetSeriesModelsByStudyId(Global.Instance.StudyId);
            SelectFirstSeries(Global.Instance.GetSeriesId(parameters));
            UpdateSeriesOrImageModels(Selected2DOR3D);
        });
    }
    private void OnOpenPrintProcess()
    {
        if (SelectImageModel is null)
            return;

        _logger.LogInformation($"Start print process for StudyID:{SelectImageModel.StudyId}");

        if (!_printService.ChceckExists())
        {
            _logger.LogInformation("No existing print task for now.");
            Task.Run(() =>
            {
                _printService.StartPrint(SelectImageModel.StudyId);
                _applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_PRINT));
            });
        }
        else
        {
            _dialogService.ShowDialog(true, MessageLeveles.Warning,
                                      LanguageResource.Message_Info_CloseWarningTitle,
                                      LanguageResource.Message_Warning_CurrentPrinting, arg =>
                                      {
                                          if (arg.Result == ButtonResult.OK)
                                          {
                                              Task.Run(() =>
                                              {
                                                  _printService.StartPrint(SelectImageModel.StudyId);
                                                  _applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_PRINT));
                                              });
                                          }
                                      }, ConsoleSystemHelper.WindowHwnd);
        }
    }

    public virtual bool NeedInit()
	{
		return false;
	}

	/// <summary>
	/// 同步Recon序列
	/// </summary>
	private void OfflineReconService_ReconDone(object? sender, EventArgs<OfflineTaskInfo> e)
	{
        _logger.LogInformation($"Image Viewer OfflineReconService_ReconDone {JsonConvert.SerializeObject(e)}");
        var seriesModel = _seriesService.GetSeriesByReconId(e.Data.ReconId);
        if (seriesModel!=null&& Global.Instance.StudyId== seriesModel.InternalStudyId)
        {
            _logger.LogInformation($"Image Viewer seriesModel {JsonConvert.SerializeObject(seriesModel)}");
            _logger.LogInformation($"OnRefresh StudyId:{Global.Instance.StudyId}");
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                GetSeriesModelsByStudyId(seriesModel.InternalStudyId);
                if(_postProcessParametersWindow?.Visibility == Visibility.Visible)
                {
                    SelectFirstSeries(SelectPostProcessSeriesKey);
                }
            });
        }
    }

    /// <summary>
    /// Footer底部的归档
    /// </summary>
    private void OnArchiveCommand(string archiveType)
    {
        if (archiveType == "remote")
        {
            ExportToRemote();
        }
        else if (archiveType == "local")
        {
            ExportToLocal();
        }
    }

    private void OnTableBrowseCommand()
    {
        if (SelectedSeriesItem is null)
            return;

        if (Directory.Exists(SelectedSeriesItem.SeriesPath))
        {
            Process.Start("explorer.exe", SelectedSeriesItem.SeriesPath);
        }
        else if (File.Exists(SelectedSeriesItem.SeriesPath))
        {
            Process.Start("explorer.exe", new FileInfo(SelectedSeriesItem.SeriesPath).Directory.FullName);
        }
        else
        {
            _dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"The path does not exist:{SelectedSeriesItem.SeriesPath}", null, ConsoleSystemHelper.WindowHwnd);
        }
    }

    private void OnBrowseCommand()
    {
        var selectedImageList = ImageModels.Where(x => x.IsSelected).ToList();
        if (selectedImageList.Count == 0)
        {
            return;
        }

        foreach (var image in selectedImageList)
        {
            if (Directory.Exists(image.SeriesPath))
            {
                Process.Start("explorer.exe", image.SeriesPath);
            }
            else if (File.Exists(image.SeriesPath))
            {
                Process.Start("explorer.exe", new FileInfo(image.SeriesPath).Directory.FullName);
            }
            else
            {
                _dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"The path does not exist:{image.SeriesPath}", null, ConsoleSystemHelper.WindowHwnd);
            }
        }
    }

    private void OnTableBrowseRawDataCommand()
    {
        var selectedImageList = new List<SeriesModel> { SelectedSeriesItem };
        if (selectedImageList.Count == 0)
        {
            return;
        }

        var rawdataList = _rawDataService.GetRawDataListByStudyId(SelectImageModel.StudyId);

        var distinctScanIdList = new List<string>();
        foreach (var image in selectedImageList)
        {
            var series = _seriesService.GetSeriesById(image.Id);
            if (series is null || distinctScanIdList.IndexOf(series.ScanId) >= 0)
            {
                continue;
            }

            distinctScanIdList.Add(series.ScanId);
            var rawdata = rawdataList.SingleOrDefault(r => r.ScanId == series.ScanId);
            if (rawdata is null)
            {
                _dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"No scan data found.", null, ConsoleSystemHelper.WindowHwnd);
                continue;
            }

            if (Directory.Exists(rawdata.Path))
            {
                Process.Start("explorer.exe", rawdata.Path);
            }
            else if (File.Exists(rawdata.Path))
            {
                Process.Start("explorer.exe", new FileInfo(rawdata.Path).Directory.FullName);
            }
            else
            {
                _dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"The path does not exist:{rawdata.Path}", null, ConsoleSystemHelper.WindowHwnd);
            }
        }
    }

    private void OnBrowseRawDataCommand()
    {
        var selectedImageList = ImageModels.Where(x => x.IsSelected).ToList();
        if (selectedImageList.Count == 0)
        {
            return;
        }

        var rawdataList = _rawDataService.GetRawDataListByStudyId(SelectImageModel.StudyId);

        var distinctScanIdList = new List<string>();
        foreach (var image in selectedImageList)
        {
            var series = _seriesService.GetSeriesById(image.SeriesId);
            if (series is null || distinctScanIdList.IndexOf(series.ScanId) >= 0)
            {
                continue;
            }

            distinctScanIdList.Add(series.ScanId);
            var rawdata = rawdataList.SingleOrDefault(r => r.ScanId == series.ScanId);
            if (rawdata is null)
            {
                _dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"No scan data found.", null, ConsoleSystemHelper.WindowHwnd);
                continue;
            }

            if (Directory.Exists(rawdata.Path))
            {
                Process.Start("explorer.exe", rawdata.Path);
            }
            else if (File.Exists(rawdata.Path))
            {
                Process.Start("explorer.exe", new FileInfo(rawdata.Path).Directory.FullName);
            }
            else
            {
                _dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"The path does not exist:{rawdata.Path}", null, ConsoleSystemHelper.WindowHwnd);
            }
        }
    }

    private void ExportToRemote()
    {
        var selectedImageList = ImageModels.Where(x => x.IsSelected).ToList();
        if (selectedImageList.Count == 0)
        {
            _dialogService.ShowDialog(false, MessageLeveles.Error,
                LanguageResource.Message_Info_CloseInformationTitle,
                LanguageResource.Message_Error_NoSelectedSeries,
                null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var vm = CTS.Global.ServiceProvider.GetService<ArchiveWindowViewModel>();
        if (vm != null)
        {
            vm.Show((archiveModel, useTls, anonymous) =>
            {
                var targetSeriesIdList = selectedImageList.Select(s => s.SeriesId).Distinct().ToList();
                var studyModels = _studyService.GetStudiesByIds(new string[] { SelectImageModel.StudyId });
                var patientId = studyModels.First().InternalPatientId;
                AddArchiveTask(patientId, SelectImageModel.StudyId, targetSeriesIdList, ArchiveLevel.Series, archiveModel, useTls, anonymous);
            });
        }

        var target = CTS.Global.ServiceProvider.GetService<ArchiveWindow>();
        if (target != null)
        {
            target.ShowWindowDialog();
        }
    }

    private void ExportToLocal()
    {
        var selectedImageList = ImageModels.Where(x => x.IsSelected).ToList();
        if (selectedImageList.Count == 0)
        {
            _dialogService.ShowDialog(false, MessageLeveles.Error,
                LanguageResource.Message_Info_CloseInformationTitle,
                LanguageResource.Message_Error_NoSelectedSeries,
                null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var vm = CTS.Global.ServiceProvider.GetService<ExportWindowViewModel>();
        if (vm != null)
        {
            vm.Show(exportSelectionModel =>
            {
                var targetSeriesIdList = selectedImageList.Select(s => s.SeriesId).Distinct().ToList();
                AddExortTask(SelectImageModel.StudyId, targetSeriesIdList, exportSelectionModel);
            });
        }

        var target = CTS.Global.ServiceProvider.GetService<ExportWindow>();
        if (target != null)
        {
            target.ShowWindowDialog();
        }
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
        archiveJobTask.Creator = _authorization.GetCurrentUser() is null ? string.Empty : _authorization.GetCurrentUser().Account;
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

        var result = _jobRequestService.EnqueueJobRequest(archiveJobTask);
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

    private void AddExortTask(string studyId, List<string> targetSeriesIdList, ExportSelectionModel exportSelectionModel)
    {
        var exportJobRequest = new ExportJobRequest();

        var selectedSeriesList = SeriesModels.Where(s => targetSeriesIdList.Contains(s.Id)).ToList();
        foreach (var selectedSeries in selectedSeriesList)
        {
            exportJobRequest.InputFolders.Add(selectedSeries.SeriesPath);
        }
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
        exportJobRequest.Creator = _authorization.GetCurrentUser() is null ? string.Empty : _authorization.GetCurrentUser().Account;
        exportJobRequest.OperationLevel = OperationLevel.Series;
        exportJobRequest.StudyId = studyId;
        exportJobRequest.SeriesIdList = targetSeriesIdList;
        exportJobRequest.Parameter = exportJobRequest.ToJson();

        var result = _jobRequestService.EnqueueJobRequest(exportJobRequest);
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

    /// <summary>
    /// 默认选中第一个序列
    /// </summary>
    public void SelectFirstSeries(string seriesId)
    {
        if (ImageModels.Count > 0)
        {
            if (string.IsNullOrEmpty(seriesId))
            {
                SelectImageModel = ImageModels[0];
            }
            else
            {
                SelectImageModel = ImageModels.First(r => r.SeriesId == seriesId);
            }
            EventAggregator.Instance.GetEvent<SelectedSeriesChangedEvent>().Publish(SelectImageModel);  
        }
    }

    private List<string> FetchPatientNameList(string[] studyIds)
    {
        var studyModel = CTS.Global.ServiceProvider.GetService<StudyViewModel>();
        var patientNameList = studyModel.VStudyModels.Where(s => studyIds.Contains(s.StudyId)).Select(s => s.PatientName).ToList();
        return patientNameList;
    }

    public void GetSeriesModelsByStudyId(string studyId)
    {
		var result = _viewerService.GetSeriesByStudyId(studyId).Where(FilterSeriesModel).ToList().ToObservableCollection();
		SeriesModels = _mapper.Map<ObservableCollection<ImageViewer.ApplicationService.Contract.Models.SeriesModel>, ObservableCollection<SeriesModel>>(result);

		ImageModels.Clear();
        Image2DModels.Clear();
        foreach (var item in SeriesModels)
		{
            item.StudyId = studyId;
			GetImageModelsBySeriesPath(studyId, item.Id, item.SeriesPath, item.SeriesDescription, item.SeriesType,item.SeriesNumber, item.ReconId,item.ImageType);
		}
        Series2DModels = SeriesModels.Clone();
    }

    private bool FilterSeriesModel(ImageViewer.ApplicationService.Contract.Models.SeriesModel? item)
    {
        if (item == null)
            return false;
        //TODO:等 dose report可以看了，放开这里
        //return item.SeriesType == "image";
        return item.SeriesType == SeriesType.SeriesTypeImage || item.SeriesType == SeriesType.DoseReport||item.SeriesType==SeriesType.ScreenshotTypeImage;
    }

    private void GetImageModelsBySeriesPath(string studyId, string seriesId, string seriesPath, string seriesDescription, string seriestype, string seriesNumber, string reconId,string imageType)
    {
        bool isFile;
        FileInfo? fileInfo;
        var fileExtension = Path.GetExtension(seriesPath);
        if (fileExtension == ".dcm")
        {
            //是文件
            if (!File.Exists(seriesPath))
                return;

            fileInfo = new FileInfo(seriesPath);
            isFile = true;
        }
        else
        {
            //是目录
            if (!Directory.Exists(seriesPath))
                return;

            fileInfo = Directory.GetFiles(seriesPath, "*.dcm").Select(n => new FileInfo(n)).FirstOrDefault();

            if (fileInfo is null)
                return;

            isFile = false;
        }

        //if ((File.GetAttributes(seriesPath) & FileAttributes.Directory) == FileAttributes.Directory)
        //{
        //}

        string title = seriesDescription ?? "";
        var fullTitle = title;
        if (title.Length > 14)
        {
            title = title.Remove(14, title.Length - 14) + "...";
        }

        try
        {
            ImageModel imageModel = new ImageModel
            {
                StudyId = studyId,
                ImageSource =
                    DicomImageHelper.Instance.GenerateThumbImage(fileInfo.FullName ?? ""
                        , 75, 75)
                ,
                Title = title,
                FullTitle = fullTitle,
                SeriesId = seriesId,
                IsFile = isFile,
                SeriesPath = seriesPath,
                IsEnable = true,
                SeriesType = seriestype,
                SeriesNumber = seriesNumber,
                ReconId = reconId,
                ImageType= imageType,
                MouseLeftButtonDownCommand = new DelegateCommand<object>(SwitchIsSelected)
            };
            ImageModels.Add(imageModel);
            Image2DModels.Add(imageModel);
        }
        catch (Exception ex)
        {
            _logger.LogError($"generate thumb image error {ex.Message}");
        }
    }

    public void SwitchIsSelected(object obj)
    {
        if (obj == null) return;
        string seriesId = (string)obj;
        var imageModel = ImageModels.FirstOrDefault(s => s.SeriesId == seriesId);
        if (imageModel != null)
        {
            imageModel.IsSelected = !imageModel.IsSelected;
        }
    }
    private void HideWindow(Window window)
    {
        if (window is not null)
        {
            window.Hide();
        }

    }

    private List<PrintingImageProperty> GetSelectedSeriesPrintProperties()
    {
        var selectedImageProperties = new List<PrintingImageProperty>();

        var selectedSereis = _seriesService.GetSeriesById(SelectImageModel.SeriesId);
        if (selectedSereis.SeriesPath.EndsWith(".dcm") && File.Exists(selectedSereis.SeriesPath))
        {
            selectedImageProperties.Add(new PrintingImageProperty() { SeriesUID = selectedSereis.SeriesInstanceUID, ImagePath = selectedSereis.SeriesPath });
        }
        else
        {
            var sortedDicomFiles = GetValidDicomFiles(selectedSereis.SeriesPath);
            foreach (var dicomFile in sortedDicomFiles)
            {
                selectedImageProperties.Add(new PrintingImageProperty() { SeriesUID = selectedSereis.SeriesInstanceUID, ImagePath = dicomFile.Value });
            }
        }

        return selectedImageProperties;
    }

    private SortedList<int, string> GetValidDicomFiles(string directory)
    {
        if (string.IsNullOrEmpty(directory))
        {
            return new SortedList<int, string>();
        }

        if (!Directory.Exists(directory))
        {
            return new SortedList<int, string>();
        }

        var files = (new DirectoryInfo(directory)).GetFiles();
        var sortedDicomFiles = new SortedList<int, string>();
        foreach (var file in files)
        {
            if (!DicomFile.HasValidHeader(file.FullName))
            {
                continue;
            }

            var dicomFile = DicomFile.Open(file.FullName);
            if (dicomFile.Dataset.TryGetSingleValue<int>(DicomTag.InstanceNumber, out int instanceNumber))
            {
                sortedDicomFiles.Add(instanceNumber, file.FullName);
                continue;
            }
        }

        return sortedDicomFiles;
    }

    // post process

    private MarkControlStatus _postProcessStatus = MarkControlStatus.Default;
    public MarkControlStatus PostProcessMarkStatus
    {
        get => _postProcessStatus;
        set => SetProperty(ref _postProcessStatus, value);
    }

    private int _motionArtifactReduceLevel = 5;
    public int MotionArtifactReduceLevel
    {
        get => _motionArtifactReduceLevel;
        set
        {
            SetProperty(ref _motionArtifactReduceLevel, value);
        }
    }

    private int _pitchArtifactReduceLevel = 5;
    public int PitchArtifactReduceLevel
    {
        get => _pitchArtifactReduceLevel;
        set
        {
            SetProperty(ref _pitchArtifactReduceLevel, value);
        }
    }

    private int _sharpLevel = 1;
    public int SharpLevel
    {
        get => _sharpLevel;
        set
        {
            SetProperty(ref _sharpLevel, value);
        }
    }

    private int _denoiseLevel = 1;
    public int DenoiseLevel
    {
        get => _denoiseLevel;
        set
        {
            SetProperty(ref _denoiseLevel, value);
        }
    }

    private bool _isPostProcessSettingEnable = true;

    public bool IsPostProcessSettingEnable
    {
        get => _isPostProcessSettingEnable;
        set
        {
            SetProperty(ref _isPostProcessSettingEnable, value);
        }
    }


    private KeyValuePair<int, string> _selectPostProcessDenoiseType;
    public KeyValuePair<int, string> SelectPostProcessDenoiseType
    {
        get => _selectPostProcessDenoiseType;
        set
        {
            SetProperty(ref _selectPostProcessDenoiseType, value);
        }
    }

    private ObservableCollection<KeyValuePair<int, string>> _kernelList = new();
    public ObservableCollection<KeyValuePair<int, string>> KernelList
    {
        get => _kernelList;
        set => SetProperty(ref _kernelList, value);
    }

    private KeyValuePair<int, string> _selectedKernel;
    public KeyValuePair<int, string> SelectedKernel
    {
        get => _selectedKernel;
        set
        {
            SetProperty(ref _selectedKernel, value);
        }
    }

    public void OnApplyPostProcessCommand(object parameter)
    {
        if (parameter is not Window window)
            return;

        if (CurrentPostProcessModels.Count == 0 || (SelectPostProcessSeriesKey.IsNullOrEmpty()))
        {
            window.Hide();
            _logger.LogError($"[PostProcess] Apply post process failed due to: {CurrentPostProcessModels.Count} or {SelectPostProcessSeriesKey}");
            return;
        }

        if (!_offlineConnection.GetConnectionStatus())
        {
            _dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_Title, LanguageResource.Message_Warring_OfflineConnectFailed, null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var (studyId, reconId, seriesId, seriesPath, isValid) = GetPostProcessTargetInfo(ImageModelOrSeries);
        if (!isValid)
        {
            _logger.LogError($"[PostProcess] Apply post process failed due to invalid parameters. studyId:{studyId}, reconId:{reconId}, seriesId:{seriesId}, seriesPath:{seriesPath}");
            window.Hide();
            return;
        }

        var modifiedSeriesDescription = UpdatePostProcessSeriesDescription();
        
        var postProcessModels = DeepCopyPostProcessList(CurrentPostProcessModels);
        var fliterType = (FilterType)SelectedKernel.Key;
        var result = _offlineService.CreatePostProcessTask(
            studyId,
            string.Empty,
            reconId,
            seriesId,
            modifiedSeriesDescription,
            seriesPath,
            postProcessModels
            );

        // clear the post process after applied
        CurrentPostProcessModels.Clear();
        SelectedPostProcessItems.Clear();
        SelectPostProcessSeriesKey = string.Empty;
        
        window.Hide();

        if (null != result)
        {
            if (result.Status == CommandExecutionStatus.Success) {
                _dialogService.Show(false, MessageLeveles.Info, LanguageResource.Message_Info_CloseConfirmTitle, $"Series name: \"{modifiedSeriesDescription}\" post process job has been started successfully.\nPlease go to JobViewer to check job status.", null, ConsoleSystemHelper.WindowHwnd);
            } else
            {
                _logger.LogError($"[PostProcess]Start post process from image viewer failed due to task result not succeed");
            }
        } else
        {
            _logger.LogError($"[PostProcess]Start post process from image viewer failed due to no task result");
        }
    }

    /// <summary>
    /// 获取用于后处理的参数信息
    /// </summary>
    /// <returns>
    /// (studyId, reconId, seriesId, seriesPath, isValid)
    /// </returns>
    private (string studyId, string reconId, string seriesId, string seriesPath, bool isValid) GetPostProcessTargetInfo(bool imagemodeChecked)
    {
        _logger.LogInformation($"GetPostProcessTargetInfo imagemodeChecked :{imagemodeChecked.ToString()}");
        //_logger.LogInformation($"GetPostProcessTargetInfo SelectImageModel :{JsonConvert.SerializeObject(SelectImageModel)}");
        if (imagemodeChecked)
        {
            var selectPostProcessImageModel = ImageModels.FirstOrDefault(r => r.SeriesId == SelectPostProcessSeriesKey);
            if (selectPostProcessImageModel == null)
            {
                return (string.Empty, string.Empty, string.Empty, string.Empty, false);
            }
            return (selectPostProcessImageModel.StudyId, selectPostProcessImageModel.ReconId, selectPostProcessImageModel.SeriesId, selectPostProcessImageModel.SeriesPath, true);
            
                
        } else
        {
            var selectPostProcessSeriesModel = SeriesModels.FirstOrDefault(r => r.Id == SelectPostProcessSeriesKey);
            if (selectPostProcessSeriesModel == null)
            {
                return (string.Empty, string.Empty, string.Empty, string.Empty, false);
            }
            return (selectPostProcessSeriesModel.StudyId, selectPostProcessSeriesModel.ReconId, selectPostProcessSeriesModel.Id, selectPostProcessSeriesModel.SeriesPath, true);
        }
    }

    public void PostProcessSettingWindowClosed(object parameter)
    {
        if (parameter is Window window)
        {
            window.Hide();
            SelectPostProcessSeriesKey = string.Empty;
        }
    }

    private PostProcessParametersWindow? _postProcessParametersWindow;

    public void OnShowPostProcessWindowCommand()
    {
        if (_postProcessParametersWindow is null)
        {
            _postProcessParametersWindow = CTS.Global.ServiceProvider?.GetRequiredService<PostProcessParametersWindow>();
        }
        if (_postProcessParametersWindow is not null)
        {
            SelectPostProcessSeriesKey = ImageModelOrSeries ? SelectedImageModelId : SelectedSeriesId;
            PostProcessSeriesDescription = ImageModelOrSeries ? $"{SelectImageModel?.FullTitle}" ?? string.Empty : SelectedSeriesItem?.SeriesDescription ?? string.Empty;

            LoadImageSeriesPostProcessParameters(SelectPostProcessSeriesKey);
            
            WindowDialogShow.Show(_postProcessParametersWindow);
        }
    }


    private string postProcessSeriesDescription = string.Empty;
    public string PostProcessSeriesDescription
    {
        get => postProcessSeriesDescription;
        set
        {
            SetProperty(ref postProcessSeriesDescription, value);
        }
    }

    private string SelectedImageModelId => SelectImageModel?.SeriesId ?? string.Empty;
    private string SelectedSeriesId => SelectedSeriesItem?.Id ?? string.Empty;
    private bool IsImageTypeSelected => (ImageModelOrSeries) ? (SelectImageModel?.SeriesType == SeriesType.SeriesTypeImage) : (SelectedSeriesItem?.SeriesType == SeriesType.SeriesTypeImage);

    private ObservableCollection<KeyValuePair<int, string>> _postDenoiseTypeList = new();
    public ObservableCollection<KeyValuePair<int, string>> PostDenoiseTypeList
    {
        get => _postDenoiseTypeList;
        set => SetProperty(ref _postDenoiseTypeList, value);
    }

    private ObservableCollection<KeyValuePair<string, string>> _selectedPostProcessItems = new();
    public ObservableCollection<KeyValuePair<string, string>> SelectedPostProcessItems
    {
        get => _selectedPostProcessItems;
        set => SetProperty(ref _selectedPostProcessItems, value);
    }

    public List<PostProcessModel> CurrentPostProcessModels
    {
        get
        {
            if (SelectPostProcessSeriesKey.IsEmpty()) return new List<PostProcessModel>();
            if (!PostProcessModelDict.TryGetValue(SelectPostProcessSeriesKey, out var list))
            {
                list = new List<PostProcessModel>();
                PostProcessModelDict[SelectPostProcessSeriesKey] = list;
            }
            return list;
        }
        set
        {
            if (SelectPostProcessSeriesKey.IsEmpty()) return;
            PostProcessModelDict[SelectPostProcessSeriesKey] = value;

            IsApplyButtonEnable = value.Count > 0 && IsImageTypeSelected;
        }
    }

    private Dictionary<string, List<PostProcessModel>> _postProcessModelDict = new();
    public Dictionary<string, List<PostProcessModel>> PostProcessModelDict
    {
        get => _postProcessModelDict;
        set => SetProperty(ref _postProcessModelDict, value);
    }

    private void PostProcessItemRemoved(string methodName)
    {
        if (SelectedPostProcessItems.Count <= 0 || CurrentPostProcessModels.Count <= 0) return;

        var itemToRemove = SelectedPostProcessItems.FirstOrDefault(item => item.Key == methodName);
        if (!itemToRemove.Equals(default(KeyValuePair<string, string>)))
        {
            SelectedPostProcessItems.Remove(itemToRemove);
        }

        var modelToRemove = CurrentPostProcessModels.FirstOrDefault(model => model.Type.ToString() == methodName);
        var modifiedList = CurrentPostProcessModels;
        if (modelToRemove != null)
        {
            modifiedList.Remove(modelToRemove);
        }
        CurrentPostProcessModels = modifiedList;
    }

    private void PostProcessItemAdded(object type)
    {
        if (type is not PostProcessType)
        {
            _logger.LogError($"[PostProcess]PostPrcoessType not supported");
            return;
        }
        var postProcessType = (PostProcessType)type;

        if (CurrentPostProcessModels.Count >= MaxPostProcessCount) return;
        if (CurrentPostProcessModels is null || CurrentPostProcessModels.Count == 0)
        {
            CurrentPostProcessModels = new List<PostProcessModel>();
        }
        var modifiedList = CurrentPostProcessModels;
        var count = modifiedList.Count;
        var filterType = SelectedKernel.Key.ToString();
        switch (postProcessType)
        {
            case PostProcessType.MotionArtifactReduce:
                modifiedList.Add(new PostProcessModel
                {
                    Index = count,
                    Type = PostProcessType.MotionArtifactReduce,
                    Parameters = new List<ParameterModel>
                        {
                            new ParameterModel { Name = ProtocolParameterNames.POST_PROCESS_ARGUMENT_MOTION_ARTIFACT_REDUCE_LEVEL, Value = MotionArtifactReduceLevel.ToString() }
                        }
                });
                break;

            case PostProcessType.PitchArtifactReduce:
                modifiedList.Add(new PostProcessModel
                {
                    Index = count,
                    Type = PostProcessType.PitchArtifactReduce,
                    Parameters = new List<ParameterModel>
                        {
                            new ParameterModel { Name = ProtocolParameterNames.POST_PROCESS_ARGUMENT_PITCH_ARTIFACT_REDUCE_LEVEL, Value = PitchArtifactReduceLevel.ToString() }
                        }
                });
                break;

            case PostProcessType.Sharp:
                modifiedList.Add(new PostProcessModel
                {
                    Index = count,
                    Type = PostProcessType.Sharp,
                    Parameters = new List<ParameterModel>
                        {
                            new ParameterModel { Name = ProtocolParameterNames.POST_PROCESS_ARGUMENT_SHARP_LEVEL, Value = SharpLevel.ToString() }
                        }
                });
                break;

            case PostProcessType.ConeAngleArtifactReduce:
                modifiedList.Add(new PostProcessModel
                {
                    Index = count,
                    Type = PostProcessType.ConeAngleArtifactReduce,
                    Parameters = new List<ParameterModel>
                        {
                            new ParameterModel { Name = ProtocolParameterNames.RECON_FILTER_TYPE, Value = filterType }
                        }
                });
                break;
            
            // no parameters
            case PostProcessType.SkullArtifactReduce:
            case PostProcessType.SparseArtifactReduce20:
            case PostProcessType.SparseArtifactReduce10:
            case PostProcessType.StreakArtifactReduce:
            case PostProcessType.WindmillArtifactReduce:
                modifiedList.Add(new PostProcessModel
                {
                    Index = count,
                    Type = postProcessType
                });
                break;

            case PostProcessType.Denoise:
                modifiedList.Add(new PostProcessModel
                {
                    Index = count,
                    Type = PostProcessType.Denoise,
                    Parameters = new List<ParameterModel>
                        {
                            new ParameterModel { Name = ProtocolParameterNames.POST_PROCESS_ARGUMENT_DENOISE_LEVEL, Value = DenoiseLevel.ToString() },
                            new ParameterModel { Name = ProtocolParameterNames.POST_PROCESS_ARGUMENT_DENOISE_TYPE, Value = SelectPostProcessDenoiseType.Value.ToString() }
                        }
                });
                break;

            default:
                _logger.LogWarning($"Unsupported PostProcessType: {postProcessType}");
                break;
        }
        CurrentPostProcessModels = modifiedList;
        SelectedPostProcessItems = ConvertPostProcessList(CurrentPostProcessModels);
    }

    private void LoadImageSeriesPostProcessParameters(string postProcessSeriesKey)
    {
        if (postProcessSeriesKey.IsNullOrEmpty())
        {
            return;
        }

        if (!PostProcessModelDict.TryGetValue(postProcessSeriesKey, out var postProcessList))
        {
            postProcessList = new List<PostProcessModel>();
            PostProcessModelDict[postProcessSeriesKey] = postProcessList;
        }
        CurrentPostProcessModels = postProcessList;
        SelectedPostProcessItems = ConvertPostProcessList(CurrentPostProcessModels);
        ResetPostProcessParameters();
    }

    private void ResetPostProcessParameters()
    {
        SelectPostProcessDenoiseType = PostDenoiseTypeList[0];
        DenoiseLevel = 1;
        SharpLevel = 1;

        MotionArtifactReduceLevel = 5;
        PitchArtifactReduceLevel = 5;
        SelectedKernel = KernelList[0];
    }

    private void InitKernelList()
    {
        ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
        foreach (var item in EnumExtension.EnumToList(typeof(FilterType)))
        {
            list.Add(new KeyValuePair<int, string>(item.Key, item.Value.Replace("Plus", "+")));
        }
        KernelList = list;
    }

    private ObservableCollection<KeyValuePair<string, string>> ConvertPostProcessList(List<PostProcessModel> postProcessModels)
    {
        if (postProcessModels == null || postProcessModels.Count == 0)
        {
            return new();
        }

        var convertedList = postProcessModels.Select(p =>
        {
            if (p.Parameters != null && p.Parameters.Any())
            {
                if (p.Type == PostProcessType.ConeAngleArtifactReduce)
                {
                    var kernel = KernelList.FirstOrDefault(t => t.Key.ToString() == p.Parameters[0].Value);
                    return new KeyValuePair<string, string>(p.Type.ToString(), kernel.Value);
                }
                return new KeyValuePair<string, string>(
                    p.Type.ToString(),
                    string.Join(", ", p.Parameters.Select(param => $"{param.Value}"))
                );
            }
            else
            {
                // If no parameters are available, just include the type
                return new KeyValuePair<string, string>(
                   p.Type.ToString(), "N\\A"
               );
            }
        }).ToList();

        return new ObservableCollection<KeyValuePair<string, string>>(convertedList);
    }

    private List<PostProcessModel> DeepCopyPostProcessList(List<PostProcessModel> originalList)
    {
        return originalList.Select(postProcess => new PostProcessModel
        {
            Index = postProcess.Index,
            Type = postProcess.Type,
            Parameters = postProcess.Parameters?.Select(param => new ParameterModel
            {
                Name = param.Name,
                Value = param.Value,
            }).ToList() ?? new List<ParameterModel>(),
        }).ToList();
    }

    private bool _isImageModelPostProcessMenuEnable = true;
    public bool IsImageModelPostProcessMenuEnable
    {
        get => _isImageModelPostProcessMenuEnable;
        set => SetProperty(ref _isImageModelPostProcessMenuEnable, value);
    }

    private bool _isSeriesItemPostProcessMenuEnable = true;
    public bool IsSeriesItemPostProcessMenuEnable
    {
        get => _isSeriesItemPostProcessMenuEnable;
        set => SetProperty(ref _isSeriesItemPostProcessMenuEnable, value);
    }

    private bool _isApplyButtonEnable = false;

    public bool IsApplyButtonEnable
    {
        get => _isApplyButtonEnable;
        set
        {
            SetProperty(ref _isApplyButtonEnable, value);
        }
    }

    private string SeriesDescriptionChanged(string seriesDescription, PostProcessType type)
    {
        switch(type)
        {
            case PostProcessType.Sharp:
                seriesDescription = seriesDescription + "S";
                break;

            case PostProcessType.Denoise:
                seriesDescription = seriesDescription + "D";
                break;

            case PostProcessType.ConeAngleArtifactReduce:
                seriesDescription = seriesDescription + "C";
                break;

            case PostProcessType.MotionArtifactReduce:
                seriesDescription = seriesDescription + "M";
                break;

            case PostProcessType.SkullArtifactReduce:
                seriesDescription = seriesDescription + "B";
                break;

            case PostProcessType.SparseArtifactReduce20:
            case PostProcessType.SparseArtifactReduce10:
            case PostProcessType.WindmillArtifactReduce:
            case PostProcessType.PitchArtifactReduce:
            case PostProcessType.None:
                break;
        }
        return seriesDescription;
        
    }

    private string UpdatePostProcessSeriesDescription()
    {
        if (CurrentPostProcessModels == null || !CurrentPostProcessModels.Any())
        {
            _logger.LogInformation("[PostProcess] No post processes available for the current selected series model.");
            return string.Empty;
        }

        var modifiedSeriesDescription = string.Empty;
        var allPostProcessesInfo = CurrentPostProcessModels.Select(postProcess =>
        {
            var parameters = postProcess.Parameters != null && postProcess.Parameters.Any()
                ? string.Join(", ", postProcess.Parameters.Select(p => $"{p.Name}: {p.Value}"))
                : "No parameters";

            modifiedSeriesDescription = SeriesDescriptionChanged(modifiedSeriesDescription, postProcess.Type);
            return $"Index: {postProcess.Index}, Type: {postProcess.Type}, Parameters: {parameters}";
        });

        var info = string.Join(" ; ", allPostProcessesInfo);
        _logger.LogInformation($"[PostProcess] All PostProcesses for ImageSeriesModel ID: {SelectPostProcessSeriesKey} - {info}");
        return $"{PostProcessSeriesDescription}+{modifiedSeriesDescription}";


    }

    private string _selectPostProcessSeriesKey = string.Empty;
    /// <summary>
    /// 选中后处理序列
    /// </summary>
    public string SelectPostProcessSeriesKey
    {
        get => _selectPostProcessSeriesKey;
        set
        {
            SetProperty(ref _selectPostProcessSeriesKey, value);
            
        }
    }
}