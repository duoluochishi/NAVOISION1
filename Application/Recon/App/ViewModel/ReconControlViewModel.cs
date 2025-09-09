using NV.CT.Alg.ScanReconCalculation.Recon.FovMatrix;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.CTS.Extensions;
using NV.CT.Language;
using System.Collections.Generic;
using System.Globalization;
using ScanReconModel = NV.CT.Recon.Model.ScanReconModel;

namespace NV.CT.Recon.ViewModel;

public class ReconControlViewModel : BaseViewModel
{
	private readonly IReconTaskService _reconTaskService;
	private readonly ISeriesService _seriesService;
	private readonly IImageAnnotationService _imageAnnotationService;
	private readonly ILogger<ReconControlViewModel> _logger;
	private readonly IProtocolHostService _protocolHostService;
	private readonly IStudyHostService _studyHostService;
	private readonly IOfflineReconService _offlineReconService;
	private readonly IDialogService _dialogService;
	private readonly IOfflineConnectionService _offlineConnectionService;

	public GeneralImageViewer? AdvancedReconControl { get; set; }
	private ScanReconModel? _lastSelectedReconRange;

	public ReconControlViewModel(ILogger<ReconControlViewModel> logger, ISeriesService seriesService, IReconTaskService reconTaskService, IImageAnnotationService imageAnnotationService, IProtocolHostService protocolHostService, IStudyHostService studyHostService, IOfflineReconService offlineReconService, IDialogService dialogService, IOfflineConnectionService offlineConnectionService)
	{
		_logger = logger;
		_protocolHostService = protocolHostService;
		_reconTaskService = reconTaskService;
		_seriesService = seriesService;
		_studyHostService = studyHostService;
		_offlineReconService = offlineReconService;
		_imageAnnotationService = imageAnnotationService;
		_dialogService = dialogService;
		_offlineConnectionService = offlineConnectionService;
		Commands.Add("Recon", new DelegateCommand(Recon, ()=>true));
		Commands.Add("Cancel", new DelegateCommand(Cancel, ()=>true));

		AdvancedReconControl = new GeneralImageViewer(1250, 1022);
		//设置默认重建方向
		AdvancedReconControl.SetReconstructionOrientation();
		//TODO:设置模式暂时不用,图像控件已经默认设置了
		//AdvancedReconControl.SetReconMode();
		AdvancedReconControl.ReconBoxChangeNotify += ReconBoxChangeNotify;

		_protocolHostService.PerformStatusChanged += ProtocolPerformStatusService_PerformStatusChanged;
        _protocolHostService.VolumnChanged += ProtocolHostService_VolumnChanged;

        EventAggregator.Instance.GetEvent<SelectedPlanningBaseChangedEvent>().Subscribe(SelectedPbChanged);
		EventAggregator.Instance.GetEvent<SelectedReconRangeChangedEvent>().Subscribe(SelectedRrChanged);
		EventAggregator.Instance.GetEvent<ReconFinishedEvent>().Subscribe(HandleReconFinished);

		InitFourCornerInformation();
		_dialogService = dialogService;

		//重新校验按钮状态
		FlushedCommandStatus();
	}

    private void ProtocolHostService_VolumnChanged(object? sender, EventArgs<(BaseModel, List<string>)> e)
    {
		if(_lastSelectedReconRange is not null)
            AdvancedReconControl?.SetReconBox(_lastSelectedReconRange.ReconModel);
    }

    private void HandleReconFinished(ScanReconModel scanReconModel)
	{
		if (string.IsNullOrEmpty(scanReconModel.ImagePath))
			return;

		LoadReconBoxAndPreview(scanReconModel);
	}

	private void SelectedRrChanged(ScanReconModel scanReconModel)
	{
		//TODO: 这里面最好改异步
		//Task.Delay(1000).Wait();

		//判断当前选中的RR和上一个RR是否同一个group,如果是同一个group那么只需要切换recon box 和preview
		//如果不是同一个group 那么需要重新加载pb的图像 并且也要切换 recon box和preview

		//_logger.LogInformation($"selected rr changed {scanReconModel.ToJson()}");
		SelectedScanReconModel = scanReconModel;

		if (_lastSelectedReconRange is null || scanReconModel.ScanID != _lastSelectedReconRange.ScanID)
		{
			var pbList = CTS.Global.ServiceProvider.GetService<PlanningBaseViewModel>()?.PlanningBaseList;
			var firstPbRecon = pbList?.FirstOrDefault(n => n.ScanId == scanReconModel.ScanID)?.Recons.FirstOrDefault();
			if (firstPbRecon is not null)
			{
				AdvancedReconControl?.ClearView();
				AdvancedReconControl?.LoadReconImageWithDirectoryPath(firstPbRecon.ImagePath);

				////TODO: 严格来说 不允许使用Task.Delay maybe 李泽话的bug
				////等李泽华改完,移出这里
				//Task.Delay(1000).Wait();
			}
		}

		LoadReconBoxAndPreview(scanReconModel);

		//LoadReconImageAndBoxOfPreview();
		//_pbLoadCompleted = false;

		_lastSelectedReconRange = scanReconModel;

		//重新校验按钮状态
		FlushedCommandStatus();
	}

	private void LoadReconBoxAndPreview(ScanReconModel scanReconModel)
	{
		AdvancedReconControl?.SetReconBox(scanReconModel.ReconModel);
		AdvancedReconControl?.SetReconPreview(scanReconModel.ImagePath);
		AdvancedReconControl?.SetReconComplete(scanReconModel.ReconModel.Status == PerformStatus.Performed);
	}

	private void SelectedPbChanged(ScanReconModel scanReconModel)
	{
		//not used anymore

		//AdvancedReconControl?.ClearView();
		//AdvancedReconControl?.LoadReconImageWithDirectoryPath(scanReconModel.ImagePath);

		//_pbLoadCompleted = true;
	}

	/// <summary>
	/// 框线变化事件
	/// </summary>
	private void ReconBoxChangeNotify(object? sender, NVCTImageViewerInterop.LOCATION_RANGEPARAM e)
	{
		if (SelectedScanReconModel is null)
			return;

		var reconModel = SelectedScanReconModel.ReconModel;

		var fov = e.FOVLengthHor;
		var matrix = ReconFovMatrixHelper.GetSuitableMatrix(fov, reconModel.ImageMatrixHorizontal);

		var parameters = new List<ParameterModel> {
			new()
			{
				Name = ProtocolParameterNames.RECON_CENTER_FIRST_X,
				Value = ((int)(e.CenterFirstX)).ToString(CultureInfo.InvariantCulture)
			},
			new()
			{
				Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y,
				Value = ((int)(e.CenterFirstY)).ToString(CultureInfo.InvariantCulture)
			},
			new()
			{
				Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z,
				Value = ((int)(e.CenterFirstZ)).ToString(CultureInfo.InvariantCulture)
			},
			new()
			{
				Name = ProtocolParameterNames.RECON_CENTER_LAST_X,
				Value = ((int) (e.CenterLastX )).ToString(CultureInfo.InvariantCulture)
			},
			new()
			{
				Name = ProtocolParameterNames.RECON_CENTER_LAST_Y,
				Value = ((int)(e.CenterLastY )).ToString(CultureInfo.InvariantCulture)
			},
			new()
			{
				Name = ProtocolParameterNames.RECON_CENTER_LAST_Z,
				Value = ((int)(e.CenterLastZ)).ToString(CultureInfo.InvariantCulture)
			},
			new()
			{
				Name = ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL,
				Value = ((int)(e.FOVLengthHor)).ToString(CultureInfo.InvariantCulture)
			},
			new()
			{
				Name = ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL,
				Value =( (int) (e.FOVLengthVer)).ToString(CultureInfo.InvariantCulture)
			},

			new()
			{
				Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X,
				Value = e.FOVDirectionHorX.ToString(CultureInfo.InvariantCulture)
			},
			new()
			{
				Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y,
				Value = e.FOVDirectionHorY.ToString(CultureInfo.InvariantCulture)
			},
			new()
			{
				Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z,
				Value = e.FOVDirectionHorZ.ToString(CultureInfo.InvariantCulture)
			},

			new()
			{
				Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X,
				Value = e.FOVDirectionVerX.ToString(CultureInfo.InvariantCulture)
			},
			new()
			{
				Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y,
				Value = e.FOVDirectionVerY.ToString(CultureInfo.InvariantCulture)
			},
			new()
			{
				Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z,
				Value = e.FOVDirectionVerZ.ToString(CultureInfo.InvariantCulture)
			},
            new()
            {
                Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_HORIZONTAL,
                Value = matrix.ToString(CultureInfo.InvariantCulture)
            },
            new()
            {
                Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL,
                Value = matrix.ToString(CultureInfo.InvariantCulture)
            },
        };
		_protocolHostService.SetParameters(reconModel, parameters);

		//parm.IsSquareFixed = recon.FOVLengthHorizontal == recon.FOVLengthVertical;
	}

	[UIRoute]
	private void ProtocolPerformStatusService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		if (e.Data.Model.GetType() != typeof(ReconModel))
		{
			return;
		}

		ReconModel reconModel = (ReconModel)e.Data.Model;
		if (SelectedScanReconModel?.ReconTaskID != reconModel.Descriptor.Id)
			return;

		SelectedScanReconModel.IsReconProcessing = e.Data.NewStatus == PerformStatus.Performing || e.Data.NewStatus == PerformStatus.Waiting;
		SelectedScanReconModel.IsCutEnable = !(e.Data.NewStatus == PerformStatus.Performing || e.Data.NewStatus == PerformStatus.Waiting);
		SelectedScanReconModel.ReconModel.Status = e.Data.NewStatus;
		SelectedScanReconModel.PerformStatus = e.Data.NewStatus;

		FlushedCommandStatus();
	}

	private void FlushedCommandStatus()
	{
		ValidateRecon();
		ValidateCancel();
	}

	[UIRoute]
	private void ValidateRecon()
	{
		if (SelectedScanReconModel is null)
			return ;

		switch (SelectedScanReconModel.ReconModel.Status)
		{
			case PerformStatus.Performed:
			case PerformStatus.Waiting:
			case PerformStatus.Performing:
				IsReconEnable = false;
				break;
			case PerformStatus.Unperform:
				IsReconEnable = true;
				break;
		}
	}

	[UIRoute]
	private void ValidateCancel()
	{
		if (SelectedScanReconModel is null)
			return ;

		IsCancelEnable = SelectedScanReconModel.ReconModel.Status == PerformStatus.Waiting ||
		                 SelectedScanReconModel.ReconModel.Status == PerformStatus.Performing;
	}

	private void Recon()
	{
		if (SelectedScanReconModel is null)
			return;

		if (!_offlineConnectionService.IsConnected)
		{
			_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, LanguageResource.Message_Warring_OfflineConnectFailed, null, ConsoleSystemHelper.WindowHwnd);
			return;
		}
		var reconItem = SelectedScanReconModel;
		var selectedRecon = ProtocolHelper.GetRecon(_protocolHostService.Instance, reconItem.ScanID, reconItem.ID);
		string mesage = string.Empty;
		if (selectedRecon is ReconModel && !ReconCalculateExtension.ReconParamCalculate(selectedRecon, out mesage))
		{
			_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, mesage, null, ConsoleSystemHelper.WindowHwnd);
			return;
		}
		ProtocolHelper.SetNextSeriesNumber(_protocolHostService.Instance, selectedRecon);
		_studyHostService.UpdateProtocol(_studyHostService.Instance, _protocolHostService.Instance);
		_offlineReconService.CreateReconTask(_studyHostService.StudyId, reconItem.ScanID, reconItem.ID);
	}

	private void Cancel()
	{
		if (SelectedScanReconModel is null)
			return;

		var reconItem = SelectedScanReconModel;
		_offlineReconService.CloseReconTask(_studyHostService.StudyId, reconItem.ScanID, reconItem.ID);

		Application.Current?.Dispatcher?.Invoke(() =>
		{
			IsCancelEnable = false;
		});
	}

	public void InitFourCornerInformation()
	{
		var (topoTextStyle, topoTexts) = ImageSettingToOverlayText.Get(_imageAnnotationService.GetConfigs().ViewSettings);
		if (topoTexts.Count > 0)
		{
			AdvancedReconControl?.ShowCursorRelativeValue(true);
			AdvancedReconControl?.SetFourCornersMessage(topoTextStyle, topoTexts);
		}
	}

	private ScanReconModel? _selectedScanReconModel;
	public ScanReconModel? SelectedScanReconModel
	{
		get => _selectedScanReconModel;
		set => SetProperty(ref _selectedScanReconModel, value);
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
}