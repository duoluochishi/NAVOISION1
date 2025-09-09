using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Language;
using NV.CT.SmartPositioning.Common;
using NV.CT.DicomUtility.DicomImage;
using System.Collections.Generic;
using Newtonsoft.Json;
using NV.CT.ErrorCodes;
using System.Text;
using NV.CT.Examination.ApplicationService.Impl.ProtocolExtension;

namespace NV.CT.Recon.ViewModel;

public class ReconRangeViewModel : BaseViewModel
{
	private readonly IReconTaskService _reconTaskService;
	private readonly ISeriesService _seriesService;
	private readonly IImageAnnotationService _imageAnnotationService;
	private readonly ILogger<ReconRangeViewModel> _logger;
	private readonly IProtocolHostService _protocolHostService;
	private readonly ISelectionManager _selectionManager;
	private readonly IStudyHostService _studyHostService;
	private readonly IOfflineReconService _offlineReconService;
	private readonly IDialogService _dialogService;
	private readonly IRawDataService _rawDataService;
	private readonly IOfflineConnectionService _offlineConnectionService;
	private ParameterDetailWindow? _parameterDetailWindow;

	public ReconRangeViewModel(ILogger<ReconRangeViewModel> logger, ISeriesService seriesService, IReconTaskService reconTaskService, IImageAnnotationService imageAnnotationService, IProtocolHostService protocolHostService, ISelectionManager selectionManager, IStudyHostService studyHostService, IOfflineReconService offlineReconService, IDialogService dialogService, IRawDataService rawDataService, IOfflineConnectionService offlineConnectionService)
	{
		_logger = logger;
		_protocolHostService = protocolHostService;
		_reconTaskService = reconTaskService;
		_seriesService = seriesService;
		_selectionManager = selectionManager;
		_imageAnnotationService = imageAnnotationService;
		_studyHostService = studyHostService;
		_offlineReconService = offlineReconService;
		_dialogService = dialogService;
		_rawDataService = rawDataService;
		_offlineConnectionService= offlineConnectionService;
		_protocolHostService.StructureChanged += ProtocolHostService_StructureChanged;
		_protocolHostService.PerformStatusChanged += ProtocolPerformStatusService_PerformStatusChanged;

		_selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;

		EventAggregator.Instance.GetEvent<CommandNameChangedEvent>().Subscribe(HandleUserCommand);
		EventAggregator.Instance.GetEvent<SelectedPlanningBaseChangedEvent>().Subscribe(SelectedPBChanged);
		EventAggregator.Instance.GetEvent<SelectedReconRangeChangedEvent>().Subscribe(SelectedRRChanged);
			
		Init();
	}

	[UIRoute]
	private void ProtocolPerformStatusService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		if (e.Data.Model.GetType() != typeof(ReconModel))
		{
			return;
		}
		ReconModel reconModel = (ReconModel)e.Data.Model;
		var recon = ReconRangeList.SelectMany(n => n.Recons).FirstOrDefault(t => t.ScanID.Equals(reconModel.Parent.Descriptor.Id) && t.ID.Equals(reconModel.Descriptor.Id));
		if (recon is null)
		{
			_logger.LogInformation($"Generate thumbnails,NewStatus:{e.Data.NewStatus}");
			return;
		}
		_logger.LogInformation($"Generate thumbnails Recon model ID is :{recon.ID}");
		switch (e.Data.NewStatus)
		{
			case PerformStatus.Waiting:
				recon.Image = WaitingImage;
				recon.PerformStatus = PerformStatus.Waiting;
				break;
			case PerformStatus.Performing:
				recon.Image = PerformingImage;
				recon.PerformStatus = PerformStatus.Performing;
				break;
			case PerformStatus.Performed:
				SetPerformedImage(recon, reconModel);
				//更新ReconRangeGroup里面的属性
				var thisScanGroup=ReconRangeList.FirstOrDefault(n => n.ScanId == recon.ScanID);
				var targetRecon = thisScanGroup?.Recons.FirstOrDefault(n => n.ID == recon.ID);
				if (targetRecon != null)
				{
					targetRecon.PerformStatus = PerformStatus.Performed;
					targetRecon.ImagePath = reconModel.ImagePath;

					//这里重建完成后发事件到PB里面去处理图像相关业务
					EventAggregator.Instance.GetEvent<ReconFinishedEvent>().Publish(targetRecon);
				}			
				break;
			case PerformStatus.Unperform:
				recon.PerformStatus= PerformStatus.Unperform;
				recon.Image = null;
				break;
		}

		//Processing状态为 重建中和等待中的 都可以取消
		recon.IsReconProcessing = e.Data.NewStatus == PerformStatus.Performing || e.Data.NewStatus==PerformStatus.Waiting;
		recon.IsCutEnable = !(e.Data.NewStatus == PerformStatus.Performing || e.Data.NewStatus == PerformStatus.Waiting);
		recon.ReconModel.Status = e.Data.NewStatus;
	}

	[UIRoute]
	private void ProtocolHostService_StructureChanged(object? sender, EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)> e)
	{
		if (e is null)
		{
			return;
		}
		if (e.Data.Parent is ScanModel scanModel && e.Data.Current is ReconModel reconModel)
		{
			switch (e.Data.ChangeType)
			{
				case StructureChangeType.Add:
					ScanReconAdded(scanModel, reconModel);
					break;
				case StructureChangeType.Delete:
					ScanReconDeleted(scanModel, reconModel);
					break;
			}
		}
	}

	[UIRoute]
	private void ScanReconAdded(ScanModel scanModel, ReconModel reconModel)
	{
		var rrGroup = ReconRangeList.FirstOrDefault(n => n.ScanId == scanModel.Descriptor.Id);
		if (rrGroup is null)
			return;

		//var currentItemIndex = rrGroup.Recons.Count + 1;
		var reconItem = ConvertTo(scanModel, reconModel, rrGroup.ScanIndex);
		rrGroup.Recons.Add(reconItem);
		//如果这一组里面没有一个选中，那么就选中第一个
		if (!rrGroup.Recons.Any(n => n.IsSelected))
		{
			var firstReconItem = rrGroup.Recons.FirstOrDefault();
			if (firstReconItem is null)
				return;
			firstReconItem.IsSelected = true;
		}
	}

	[UIRoute]
	private void ScanReconDeleted(ScanModel scanModel, ReconModel reconModel)
	{
		//_logger?.LogInformation($"scanModel id {scanModel.Descriptor.Id},reconId is {reconModel.Descriptor.Id}");
		var rrGroup = ReconRangeList.FirstOrDefault(n => n.ScanId == scanModel.Descriptor.Id);
		if (rrGroup is null)
			return;

		var targetRecon = rrGroup.Recons.FirstOrDefault(n => n.ID.Equals(reconModel.Descriptor.Id));
		if (targetRecon is null)
			return;

		rrGroup.Recons.Remove(targetRecon);
		ResetIndex(rrGroup);

		//重新选中当前Scan下面的第一个
		var firstReconItem = rrGroup.Recons.FirstOrDefault();
		if (firstReconItem is null)
			return;
		firstReconItem.IsSelected = true;
	}

	private void ResetIndex(ReconRangeGroupModel rrGroup)
	{
		rrGroup.Recons.ForEach(item =>
		{
			item.Content = GetReconIndicator(rrGroup.ScanIndex, item.SeriesNumber);
		});
	}

	private void SelectionManager_SelectionReconChanged(object? sender, EventArgs<ReconModel> e)
	{
		//do nothing
	}

	/// <summary>
	/// 显示参数详细弹窗，扫描参数和重建参数
	/// </summary>
	public void ShowParameterDetailWindow()
	{
		var parameterDetailViewModel = CTS.Global.ServiceProvider?.GetRequiredService<ParameterDetailViewModel>();
		if (parameterDetailViewModel is not null)
		{
			parameterDetailViewModel.IsShowScan = false;
		}

		if (_parameterDetailWindow is null)
		{
			_parameterDetailWindow = CTS.Global.ServiceProvider?.GetRequiredService<ParameterDetailWindow>();
		}
		WindowDialogShow.DialogShow(_parameterDetailWindow);
	}

	private void HandleUserCommand(CommandModel commandModel)
	{
		if (commandModel.CommandData?.ReconGroupType == ReconGroupType.PlanningBase)
			return;

		var reconItem = commandModel.CommandData;
		if (reconItem is null)
			return;

		switch (commandModel.CommandName)
		{
			case CommandName.ShowParameterCommand:
				_selectionManager.SelectRecon(reconItem.ReconModel);
				ShowParameterDetailWindow();
				break;
			case CommandName.ReconOnCommand:
				if (!_offlineConnectionService.IsConnected)
				{
					_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, LanguageResource.Message_Warring_OfflineConnectFailed, null, ConsoleSystemHelper.WindowHwnd);
					return;
				}
				var selectedRecon = ProtocolHelper.GetRecon(_protocolHostService.Instance, reconItem.ScanID, reconItem.ID);
				string mesage = string.Empty;
				if (selectedRecon is ReconModel && !ReconCalculateExtension.ReconParamCalculate(selectedRecon, out mesage))
				{
					_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, mesage, null, ConsoleSystemHelper.WindowHwnd);
					return;
				}
				ProtocolHelper.SetNextSeriesNumber(_protocolHostService.Instance, selectedRecon);
				_studyHostService.UpdateProtocol(_studyHostService.Instance, _protocolHostService.Instance);
                _logger.LogDebug($"CreateOfflineReconTask: ({_studyHostService.StudyId}, {reconItem.ScanID}, {reconItem.ID}) => {JsonConvert.SerializeObject(selectedRecon)}");
                _offlineReconService.CreateReconTask(_studyHostService.StudyId, reconItem.ScanID, reconItem.ID);
				break;
			case CommandName.ReconRepeatCommand:

				_protocolHostService.CopyRecon(reconItem.ScanID, reconItem.ID);
				break;
			case CommandName.ReconCutCommand:
				_protocolHostService.DeleteRecon(reconItem.ScanID, reconItem.ID);
				//TODO:需要判断是否已经开始离线重建以及是否完成（或取消）
				_studyHostService.UpdateProtocol(_studyHostService.Instance, _protocolHostService.Instance);
				break;
			case CommandName.ReconCancelCommand:
				_offlineReconService.CloseReconTask(_studyHostService.StudyId, reconItem.ScanID, reconItem.ID);
				break;
			case CommandName.BrowserCommand:
				HandleBrowserCommand(reconItem);
				break;
			case CommandName.BrowserRawDataCommand:
				HandleBrowserRawData(reconItem);
				break;
		}
	} 

	private void HandleBrowserRawData(ScanReconModel reconItem)
	{
		var rawdataList = _rawDataService.GetRawDataListByStudyId(Global.Instance.StudyId);
	
		var rawdata = rawdataList.FirstOrDefault(r => r.ScanId == reconItem.ScanID);

		if (rawdata is null)
		{
			_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"No scan data found.", null, ConsoleSystemHelper.WindowHwnd);
			return;
		}

		_logger.LogInformation($"recon browser raw data path : {rawdata?.Path},with recon id {reconItem.ID}, scan id {reconItem.ScanID} ");
		
		if (Directory.Exists(rawdata?.Path))
		{
			Process.Start("explorer.exe", rawdata.Path);
		}
		else if (File.Exists(rawdata?.Path))
		{
			Process.Start("explorer.exe", new FileInfo(rawdata.Path).Directory.FullName);
		}
		else
		{
			_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"The path does not exist:{rawdata?.Path}", null, ConsoleSystemHelper.WindowHwnd);
		}
	}

	private void HandleBrowserCommand(ScanReconModel reconItem)
	{
		var imagePath = reconItem.ImagePath;
		_logger.LogInformation($"recon browser series folder : {imagePath} with recon id {reconItem.ID},scan id {reconItem.ScanID} ");

		if (Directory.Exists(imagePath))
		{
			Process.Start("explorer.exe", imagePath);
		}
		else if (File.Exists(imagePath))
		{
			Process.Start("explorer.exe", new FileInfo(imagePath).Directory.FullName);
		}
		else
		{
			_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, $"Recon image path does not exist:{imagePath}", null, ConsoleSystemHelper.WindowHwnd);
		}
	}

	/// <summary>
	/// Planning Base的recon item变化了，影响Recon Range的选中
	/// </summary>
	private void SelectedPBChanged(ScanReconModel scanReconModel)
	{
		var reconRangeGroup = ReconRangeList.FirstOrDefault(n => n.ScanId == scanReconModel.ScanID);
		if (reconRangeGroup != null)
		{
			SelectedReconRangeModel = reconRangeGroup;

			var firstReconItem = SelectedReconRangeModel.Recons.FirstOrDefault();
			if (firstReconItem is null)
				return;
			firstReconItem.IsSelected = true;
		}
	}

	private void SelectedRRChanged(ScanReconModel scanReconModel)
	{
		if (scanReconModel is null)
			return;

		_selectionManager.SelectRecon(scanReconModel.ReconModel);
	}

	private void Init()
	{
		ReconRangeList.Clear();

		var index = 0;
		var rtdReconIds = _reconTaskService.GetAll(Global.Instance.StudyId).Where(n => n.IsRTD).Select(n => n.ReconId).ToList();
		var planningBaseSeries = _seriesService.GetSeriesByStudyId(Global.Instance.StudyId).Where(n => rtdReconIds.Contains(n.ReconId)).ToList();
		var scanList = ProtocolHelper.Expand(_protocolHostService.Instance);
		//为了保持协议项的顺序
		foreach (var itemTuple in scanList)
		{
			var scanIndex = ++index;
			var scanModel = itemTuple.Scan;

			//Recon Ranges和Planning Base都不需要 topo
			if (scanModel.ScanImageType == ScanImageType.Topo)
				continue;

			var scanRTDSeries = planningBaseSeries.FirstOrDefault(n => n.ScanId == scanModel.Descriptor.Id);

			if (scanRTDSeries is null)
				continue;

			var scanId = scanModel.Descriptor.Id;
			var reconList = ReconModelConvertToViewModelList(scanModel, scanIndex);

			var rrGroupModel = ConstructReconRangeGroupModel(scanIndex, scanRTDSeries.SeriesPath ?? string.Empty
				, scanModel.Descriptor.Id, scanId);

			//构造ReconRanges列表
			var nonRtdReconList = reconList.Where(n => !n.IsRTD).ToList();
			nonRtdReconList.ForEach(n => n.ReconGroupType = ReconGroupType.ReconRanges);
			rrGroupModel.Recons = nonRtdReconList.ToObservableCollection();
			ReconRangeList.Add(rrGroupModel);

			ScanIdDictionary.Add(scanModel.Descriptor.Id, scanModel.Descriptor.Name);
		}
	}

	/// <summary>
	/// 将 ScanModel里面的Recon列表转为
	/// </summary>
	private List<ScanReconModel> ReconModelConvertToViewModelList(ScanModel scanModel, int scanIndex)
	{
		int i = 1;
		var list = new List<ScanReconModel>();
		foreach (var reconItem in scanModel.Children)
		{
			var rtdReconModel = ConvertTo(scanModel, reconItem, scanIndex);
			list.Add(rtdReconModel);
			if (reconItem is not null && !reconItem.IsRTD)
			{
				i++;
			}
		}

		return list;
	}

	private ScanReconModel ConvertTo(ScanModel scanModel, ReconModel reconModel, int scanIndex)
	{
		//currentScanModel = scanModel;

		ScanReconModel scanReconModel = new ScanReconModel();
		scanReconModel.ScanID = scanModel.Descriptor.Id;
		scanReconModel.ID = reconModel.Descriptor.Id;
		scanReconModel.ReconTaskID = reconModel.Descriptor.Id;
		scanReconModel.SeriesNumber = reconModel.SeriesNumber;
		if (!reconModel.IsRTD)
		{
			scanReconModel.Content = GetReconIndicator(scanIndex, scanReconModel.SeriesNumber);
		}
		else
		{
			scanReconModel.Content = $"{scanIndex}-RT";
		}
		scanReconModel.IsRTD = reconModel.IsRTD;
		scanReconModel.ReconModel = reconModel;
		scanReconModel.IsTomo = scanModel.ScanImageType == ScanImageType.Tomo;
		scanReconModel.ImagePath = reconModel.ImagePath;

		//if (scanModel.ScanOption == ScanOption.DUALSCOUT)
		//{
		//	int indexR = scanModel.Children.IndexOf(reconModel);
		//	if (indexR < scanModel.Children.Count && (scanModel.TubePositions[indexR] == TubePosition.Angle0 || scanModel.TubePositions[indexR] == TubePosition.Angle180))
		//	{
		//		scanReconModel.Content = "AP";
		//	}
		//	else
		//	{
		//		scanReconModel.Content = "LAT";
		//	}
		//}
		SetReconModelBackgroundImage(scanReconModel, reconModel);
		return scanReconModel;
	}

	private string GetReconIndicator(int scanIndex, int seriesNumber)
	{
		return $"{scanIndex}-{seriesNumber}";
	}

	private void SetReconModelBackgroundImage(ScanReconModel scanReconModel, ReconModel reconModel)
	{
		switch (reconModel.Status)
		{
			case PerformStatus.Performed:
				SetPerformedImage(scanReconModel, reconModel);
				break;
			case PerformStatus.Waiting:
				scanReconModel.Image = WaitingImage;
				break;
			case PerformStatus.Performing:
				scanReconModel.Image = PerformingImage;
				break;
			default:
				scanReconModel = GetRTDImage(scanReconModel, reconModel);
				break;
		}
		SetTooltipMessage(scanReconModel, reconModel);
	}

	private void SetTooltipMessage(ScanReconModel scanReconModel, ReconModel reconModel)
	{
		try
		{
			var errorCodes = reconModel.ErrorCodes;

			if (reconModel.FailureReason != FailureReasonType.None && errorCodes.Any())
			{
				StringBuilder sb = new StringBuilder();
				foreach (var error in ErrorCodeHelper.GetErrorCodeList(errorCodes))
				{
					sb.AppendLine($"Error:{error.Code}, {error.Reason}");
				}
				scanReconModel.ToolTipMessage = sb.ToString();
			}
			else
			{
				scanReconModel.ToolTipMessage = null;
			}
		}
		catch (Exception ex)
		{
			scanReconModel.ToolTipMessage = ex.Message;
		}
	}

	private void SetPerformedImage(ScanReconModel scanReconModel, ReconModel reconModel)
	{
		_logger.LogDebug($"Generate thumbnails failed: (ScanId, ReconId,FailureReason) => ({scanReconModel.ScanID},{reconModel.Descriptor.Id}, {reconModel.FailureReason})");
		switch (reconModel.FailureReason)
		{
			case FailureReasonType.None:
				if (!string.IsNullOrEmpty(reconModel.ImagePath) && Directory.Exists(reconModel.ImagePath))
				{
					var image = GetGenerateThumbImageByPath(reconModel.ImagePath);
					if (image is null)
					{
						scanReconModel = GetRTDImage(scanReconModel, reconModel);
					}
					else
					{
						scanReconModel.Image = image;
					}
				}
				else
				{
					_logger.LogInformation($"Generate thumbnails,The path does not exist:{reconModel.ImagePath}");
				}
				break;
			default:
				scanReconModel.Image = FailedImage;
				break;
		}
		SetTooltipMessage(scanReconModel, reconModel);
	}

	private ScanReconModel GetRTDImage(ScanReconModel scanReconModel, ReconModel reconModel)
	{
		if (reconModel.IsRTD)
		{
			scanReconModel.Image = RTDImage;
		}
		else
		{
			scanReconModel.Image = null;
		}
		return scanReconModel;
	}

	private WriteableBitmap RTDImage
	{
		get => new(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/reconrt.png", UriKind.RelativeOrAbsolute)));
	}
	private WriteableBitmap WaitingImage
	{
		get => new(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/reconwaiting.png", UriKind.RelativeOrAbsolute)));
	}
	private WriteableBitmap FailedImage
	{
		get => new(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/reconfailed.png", UriKind.RelativeOrAbsolute)));
	}

	private WriteableBitmap PerformingImage
	{
		get => new(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/reconing.png", UriKind.RelativeOrAbsolute)));
	}

	private ReconRangeGroupModel ConstructReconRangeGroupModel(int index, string imagePath, string groupName, string scanId)
	{
		var tmp = new ReconRangeGroupModel();
		tmp.ScanIndex = index;
		tmp.ImagePath = imagePath;
		tmp.GroupName = groupName;
		tmp.ScanId = scanId;
		return tmp;
	}

	/// <summary>
	/// Recon Range外层对象
	/// </summary>
	private ReconRangeGroupModel? _selectedReconRangeModel = new();
	public ReconRangeGroupModel? SelectedReconRangeModel
	{
		get => _selectedReconRangeModel;
		set => SetProperty(ref _selectedReconRangeModel, value);
	}

	public Dictionary<string, string> ScanIdDictionary = new();

	/// <summary>
	/// Recon Ranges 列表
	/// </summary>
	private ObservableCollection<ReconRangeGroupModel> _reconRangeList = new();
	public ObservableCollection<ReconRangeGroupModel> ReconRangeList
	{
		get => _reconRangeList;
		set => SetProperty(ref _reconRangeList, value);
	}

	private bool _isReconEnable;
	public bool IsReconEnable
	{
		get => _isReconEnable;
		set => SetProperty(ref _isReconEnable, value);
	}

	private bool _isCancelEnable;
	public bool IsCancelEnable
	{
		get => _isCancelEnable;
		set => SetProperty(ref _isCancelEnable, value);
	}
	private WriteableBitmap? GetGenerateThumbImageByPath(string path)
	{
		WriteableBitmap? writeableBitmap = null;
		var fileName = Directory.GetFiles(path, "*.dcm").FirstOrDefault();
		if (!string.IsNullOrEmpty(fileName) && File.Exists(fileName))
		{
			try
			{
				_logger.LogInformation($"Start generate thumbnails:{fileName}");
				writeableBitmap = DicomImageHelper.Instance.GenerateThumbImage(fileName, 75, 75);
				_logger.LogInformation($"End generate thumbnails:{fileName}");
			}
			catch (Exception ex)
			{
				_logger.LogError("GenerateThumbImage_StackTrace:" + ex.StackTrace + ";Message:" + ex.Message);
			}
		}
		else
		{
			_logger.LogError($"Generate thumbnails,The file does not exist:{fileName}");
		}
		return writeableBitmap;
	}
}