using NV.CT.CTS.Extensions;
using NV.CT.DicomUtility.DicomImage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NV.CT.Recon.ViewModel;

public class PlanningBaseViewModel : BaseViewModel
{
	private readonly IReconTaskService _reconTaskService;
	private readonly ISeriesService _seriesService;
	private readonly IImageAnnotationService _imageAnnotationService;
	private readonly ILogger<PlanningBaseViewModel> _logger;
	private readonly IProtocolHostService _protocolHostService;
	private readonly IDialogService _dialogService;
	private readonly ISelectionManager _selectionManager;
	private ParameterDetailWindow? _parameterDetailWindow;

	public PlanningBaseViewModel(ILogger<PlanningBaseViewModel> logger, ISeriesService seriesService, IReconTaskService reconTaskService, IImageAnnotationService imageAnnotationService, IProtocolHostService protocolHostService, IDialogService dialogService, ISelectionManager selectionManager)
	{
		_logger = logger;
		_protocolHostService = protocolHostService;
		_reconTaskService = reconTaskService;
		_seriesService = seriesService;
		_imageAnnotationService = imageAnnotationService;
		_dialogService = dialogService;
		_selectionManager = selectionManager;

		_selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;

		EventAggregator.Instance.GetEvent<CommandNameChangedEvent>().Subscribe(HandleUserCommand);
		EventAggregator.Instance.GetEvent<SelectedReconRangeChangedEvent>().Subscribe(SelectedRRChanged);
		EventAggregator.Instance.GetEvent<ReconRangeLoadCompletedEvent>().Subscribe(ReconRangeHandleLoadComplete);
		EventAggregator.Instance.GetEvent<PlanningBaseLoadCompletedEvent>().Subscribe(PlanningBaseLoadComplete);

		Init();
	}

	private void SelectionManager_SelectionReconChanged(object? sender, EventArgs<ReconModel> e)
	{
	}

	private void HandleUserCommand(CommandModel commandModel)
	{
		if (commandModel.CommandData?.ReconGroupType == ReconGroupType.ReconRanges)
			return;

		var reconItem = commandModel.CommandData;
		if (reconItem is null)
			return;

		switch (commandModel.CommandName)
		{
			case CommandName.ShowParameterCommand:
				//因为ParameterDetailWindow依赖SelectionManager的选择，所以下发给参数界面
				_selectionManager.SelectRecon(reconItem.ReconModel);
				ShowParameterDetailWindow();
				break;
			case CommandName.ReconRepeatCommand:
				ReconRepeat(reconItem);
				break;
		}
	}

	private void ReconRepeat(ScanReconModel scanReconModel)
	{
		_protocolHostService.CopyRecon(scanReconModel.ScanID, scanReconModel.ID);
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

	private void SelectedRRChanged(ScanReconModel scanReconModel)
	{
		var pbGroup = PlanningBaseList.FirstOrDefault(n => n.ScanId == scanReconModel.ScanID);
		if (pbGroup is null)
			return;

		SelectedPlanningBaseModel = pbGroup;

		var firstPBReconItem = SelectedPlanningBaseModel.Recons.FirstOrDefault();
		if (firstPBReconItem is null)
			return;
		firstPBReconItem.IsSelected = true;
	}

	private bool _isReconRangeLoaded;
	private bool _isPlanningBaseLoaded;
	private void ReconRangeHandleLoadComplete()
	{
		_isReconRangeLoaded = true;
		HandleLoadComplete();
	}
	private void PlanningBaseLoadComplete()
	{
		_isPlanningBaseLoaded = true;
		HandleLoadComplete();
	}

	/// <summary>
	/// 默认都加载完成之后，选中第一个Recon Item模拟点击事件
	/// </summary>
	private void HandleLoadComplete()
	{
		if (_isReconRangeLoaded && _isPlanningBaseLoaded)
		{
			SelectedPlanningBaseModel = PlanningBaseList.FirstOrDefault();

			if (SelectedPlanningBaseModel is null)
				return;

			var firstPBReconItem = SelectedPlanningBaseModel.Recons.FirstOrDefault();
			if (firstPBReconItem is null)
				return;

			firstPBReconItem.IsSelected = true;
		}
	}

	private void Init()
	{

		PlanningBaseList.Clear();

		//Task.Run(() =>
		//{
			var index = 0;
			var rtdReconIds = _reconTaskService.GetAll(Global.Instance.StudyId).Where(n => n.IsRTD).Select(n => n.ReconId).ToList();
			var planningBaseSeries = _seriesService.GetSeriesByStudyId(Global.Instance.StudyId).Where(n => rtdReconIds.Contains(n.ReconId)).ToList();
			var scanList = ProtocolHelper.Expand(_protocolHostService.Instance);
			//为了保持协议项的顺序
			foreach (var itemTuple in scanList)
			{
				var scanModel = itemTuple.Scan;
				var scanId = scanModel.Descriptor.Id;
				var newIndex = ++index;

				//Recon Ranges和Planning Base都不需要 topo
				if (scanModel.ScanImageType == ScanImageType.Topo)
					continue;

				var scanRTDSeries = planningBaseSeries.FirstOrDefault(n => n.ScanId == scanModel.Descriptor.Id);

				if (scanRTDSeries is null)
					continue;

				var reconList = ReconModelConvertToViewModelList(scanModel, newIndex);
				var pbGroupModel = ConstructPlanningBaseGroupModel(newIndex, scanRTDSeries.SeriesPath ?? string.Empty
					, scanModel.Descriptor.Id, scanId);

				//构造PlanningBase列表
				var rtdReconList = reconList.Where(n => n.IsRTD).ToList();
				rtdReconList.ForEach(n => n.ReconGroupType = ReconGroupType.PlanningBase);
				pbGroupModel.Recons = rtdReconList.ToObservableCollection();
				PlanningBaseList.Add(pbGroupModel);

				ScanIdDictionary.Add(scanModel.Descriptor.Id, scanModel.Descriptor.Name);
			}

		//});
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
			var rtdReconModel = ConvertTo(scanModel, reconItem, i, scanIndex);
			list.Add(rtdReconModel);
			if (reconItem is not null && !reconItem.IsRTD)
			{
				i++;
			}
		}

		return list;
	}
	private ScanReconModel ConvertTo(ScanModel scanModel, ReconModel reconModel, int index, int scanIndex)
	{
		//currentScanModel = scanModel;

		ScanReconModel scanReconModel = new ScanReconModel();
		scanReconModel.ScanID = scanModel.Descriptor.Id;
		scanReconModel.ID = reconModel.Descriptor.Id;
		scanReconModel.ReconTaskID = reconModel.Descriptor.Id;
		if (!reconModel.IsRTD)
		{
			scanReconModel.Content = GetReconIndicator(index, scanIndex);
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
	private string GetReconIndicator(int index, int scanIndex)
	{
		return $"{scanIndex}-{index}";
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
	private PlanningBaseGroupModel ConstructPlanningBaseGroupModel(int index, string imagePath, string groupName, string scanId)
	{
		var tmp = new PlanningBaseGroupModel();
		tmp.ScanIndex = index;
		tmp.ImagePath = imagePath;
		tmp.GroupName = groupName;
		tmp.ScanId = scanId;
		return tmp;
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
	#region property

	public Dictionary<string, string> ScanIdDictionary = new();
	private PlanningBaseGroupModel? _selectedPlanningBaseModel;
	public PlanningBaseGroupModel? SelectedPlanningBaseModel
	{
		get => _selectedPlanningBaseModel;
		set => SetProperty(ref _selectedPlanningBaseModel, value);
	}
	/// <summary>
	/// Planning base 列表
	/// </summary>
	private ObservableCollection<PlanningBaseGroupModel> _planningBaseList = new();
	public ObservableCollection<PlanningBaseGroupModel> PlanningBaseList
	{
		get => _planningBaseList;
		set => SetProperty(ref _planningBaseList, value);
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
	#endregion
}
