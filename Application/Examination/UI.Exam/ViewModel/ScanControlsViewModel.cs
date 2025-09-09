//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.Alg.ScanReconCalculation.Scan.Gantry;
using NV.CT.Alg.ScanReconCalculation.Scan.Offset;
using NV.CT.Alg.ScanReconCalculation.Scan.Table;
using NV.CT.AppService.Contract;
using NV.CT.DatabaseService.Contract;
using NV.CT.Examination.ApplicationService.Contract.ScanControl;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SyncService.Contract;
using NV.CT.UI.Exam.Extensions;
using NV.MPS.Configuration;
using NV.MPS.Environment;
using NV.MPS.Exception;
using TubePos = NV.CT.FacadeProxy.Common.Enums.TubePosition;

namespace NV.CT.UI.Exam.ViewModel;

public class ScanControlsViewModel : BaseViewModel, IDisposable
{
	private readonly ILogger<ScanControlsViewModel> _logger;
	private readonly ISelectionManager _selectionManager;
	private readonly IScanControlService _scanControlService;
	private readonly IUIRelatedStatusService _uiRelatedStatusService;
	private readonly IStudyHostService _studyHostService;
	private readonly IProtocolHostService _protocolHostService;
	private readonly IOfflineReconService _offlineReconService;
	private readonly ILayoutManager _layoutManager;
	private readonly IDialogService _dialogService;
	private readonly ISystemReadyService _systemReadyService;
	private readonly IPageService _pageService;
	private readonly IUIControlStatusService _uiControlStatusService;
	private readonly IGoService _goService;
	private readonly IScreenManagement _screenManagement;
	private readonly IHeatCapacityService _heatCapacityService;
	private readonly ITablePositionService _tablePositionService;
	private readonly IWorkflow? _workflow;
	private readonly IDataSync? _dataSync;
	private readonly IScreenSync? _screenSync;
	private readonly IApplicationCommunicationService _applicationCommunicationService;
	private readonly IFrontRearCoverStatusService _frontRearCoverStatusService;
	private readonly IDetectorTemperatureService _detectorTemperatureService;
	private readonly IRealtimeStatusProxyService _realtimeStatusProxyService;
	private readonly IOfflineConnectionService _offlineConnectionService;
	private readonly IDoseEstimateService _doseEstimateService;
	private readonly IMapper _mapper;
	private readonly IVoiceService _voiceService;
	private readonly MessageTooltipViewModel _messageTooltipViewModel;
	private RealtimeStatus RealtimeStatus { get; set; }
	private bool _isDoorClosed { get; set; }
	private bool _doorAlertWindowShown { get; set; }
	private DoorAlertWindow? _doorAlertWindow;

	private bool IsStandBy { get; set; } = true;
	private bool IsScanning { get; set; } = false;
	private bool IsAuto { get; set; } = false;

	private bool _isDetectorTemperatureNormalStatus;

	#region 属性
	private bool _isConfirmEnable;
	public bool IsConfirmEnable
	{
		get => _isConfirmEnable;
		set => SetProperty(ref _isConfirmEnable, value);
	}

	private bool _isReconAllEnable = false;
	public bool IsReconAllEnable
	{
		get => _isReconAllEnable;
		set => SetProperty(ref _isReconAllEnable, value);
	}

	private bool _isReconAllVisible;
	public bool IsReconAllVisible
	{
		get => _isReconAllVisible;
		set => SetProperty(ref _isReconAllVisible, value);
	}
	private bool _isConfirmVisible;
	public bool IsConfirmVisible
	{
		get => _isConfirmVisible;
		set => SetProperty(ref _isConfirmVisible, value);
	}

	private bool _isCloseVisible;
	public bool IsCloseVisible
	{
		get => _isCloseVisible;
		set => SetProperty(ref _isCloseVisible, value);
	}

	private bool _isGoVisible = true;
	public bool IsGoVisible
	{
		get => _isGoVisible;
		set => SetProperty(ref _isGoVisible, value);
	}

	private string _confirmReconAllContent = LanguageResource.Content_Confirm;
	public string ConfirmReconAllContent
	{
		get => _confirmReconAllContent;
		set => SetProperty(ref _confirmReconAllContent, value);
	}

	private bool _isGoEnable;
	public bool IsGoEnable
	{
		get => _isGoEnable;
		set => SetProperty(ref _isGoEnable, value);
	}

	private bool _isCancelEnable;
	public bool IsCancelEnable
	{
		get => _isCancelEnable;
		set => SetProperty(ref _isCancelEnable, value);
	}

	private bool _isCloseEnable;
	public bool IsCloseEnable
	{
		get => _isCloseEnable;
		set => SetProperty(ref _isCloseEnable, value);
	}

	DoseNotificationWindow? _doseNotificationWindow;
	DoseAlertWindow? _doseAlertWindow;
	CommonNotificationWindow? commonNotificationWindow;
	CommonWarningWindow? commonWarningWindow;
	#endregion
		
	public ScanControlsViewModel(
		ILogger<ScanControlsViewModel> logger,
		ISelectionManager selectionManager,
		IScanControlService scanControlService,
		IUIRelatedStatusService uiRelatedStatusService,
		IStudyHostService studyHostService,
		IProtocolHostService protocolHostService,
		IOfflineReconService offlineReconService,
		IDialogService dialogService,
		ILayoutManager layoutManager,
		ISystemReadyService systemReadyService,
		IPageService pageService,
		IUIControlStatusService uiControlStatusService,
		IGoService goService,
		IScreenManagement screenManagement,
		IGoValidateDialogService goValidateDialogService,
		IApplicationCommunicationService applicationCommunicationService,
		IHeatCapacityService heatCapacityService,
		ITablePositionService tablePositionService,
		IFrontRearCoverStatusService frontRearCoverStatusService,
		IDetectorTemperatureService detectorTemperatureService,
		IRealtimeStatusProxyService realtimeStatusProxyService,
		IMapper mapper,
		IOfflineConnectionService offlineConnectionService,
		IDoseEstimateService doseEstimateService,
		IVoiceService voiceService)
	{
		_logger = logger;
		_mapper = mapper;
		_systemReadyService = systemReadyService;
		_dialogService = dialogService;
		_layoutManager = layoutManager;
		_selectionManager = selectionManager;
		_scanControlService = scanControlService;
		_uiRelatedStatusService = uiRelatedStatusService;
		_studyHostService = studyHostService;
		_protocolHostService = protocolHostService;
		_offlineReconService = offlineReconService;
		_uiControlStatusService = uiControlStatusService;
		_goService = goService;
		_screenManagement = screenManagement;
		_applicationCommunicationService = applicationCommunicationService;
		_heatCapacityService = heatCapacityService;
		_frontRearCoverStatusService = frontRearCoverStatusService;
		_workflow = Global.ServiceProvider.GetService<IWorkflow>();
		_realtimeStatusProxyService = realtimeStatusProxyService;
		_dataSync = Global.ServiceProvider.GetService<IDataSync>();
		_offlineConnectionService = offlineConnectionService;
		_doseEstimateService = doseEstimateService;
		if (_dataSync is not null)
		{
			_dataSync.ReplaceProtocolFinished -= DataSync_ReplaceProtocolFinished;
			_dataSync.ReplaceProtocolFinished += DataSync_ReplaceProtocolFinished;
		}
		_screenSync = Global.ServiceProvider.GetService<IScreenSync>();
		_messageTooltipViewModel = Global.ServiceProvider.GetService<MessageTooltipViewModel>();

		Commands.Add(CommandParameters.COMMAND_CONFIRM, new DelegateCommand(Confirm));
		Commands.Add(CommandParameters.COMMAND_RECON_ALL, new DelegateCommand(ReconAll));
		Commands.Add(CommandParameters.COMMAND_GO, new DelegateCommand(Go));
		Commands.Add(CommandParameters.COMMAND_CANCEL, new DelegateCommand(Cancel));
		Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand(Close));

		_uiRelatedStatusService.RealtimeStatusChanged -= RealtimeStatusChanged;
		_uiRelatedStatusService.RealtimeStatusChanged += RealtimeStatusChanged;

		_uiRelatedStatusService.DoorStatusChanged -= UIRelatedStatusService_DoorStatusChanged;
		_uiRelatedStatusService.DoorStatusChanged += UIRelatedStatusService_DoorStatusChanged;
		_systemReadyService.StatusChanged -= SystemReadyService_StatusChanged;
		_systemReadyService.StatusChanged += SystemReadyService_StatusChanged;

		_selectionManager.SelectionScanChanged -= SelectionScanChanged;
		_selectionManager.SelectionScanChanged += SelectionScanChanged;
		_protocolHostService.PerformStatusChanged -= PerformStatusChanged;
		_protocolHostService.PerformStatusChanged += PerformStatusChanged;
		_layoutManager.LayoutChanged -= LayoutChanged;
		_layoutManager.LayoutChanged += LayoutChanged;
		_protocolHostService.StructureChanged -= ProtocolHostService_StructureChanged;
		_protocolHostService.StructureChanged += ProtocolHostService_StructureChanged;
		_uiControlStatusService.GoButtonStatusChanged -= UIControlStatusService_GoButtonStatusChanged;
		_uiControlStatusService.GoButtonStatusChanged += UIControlStatusService_GoButtonStatusChanged;

		_detectorTemperatureService = detectorTemperatureService;
		_detectorTemperatureService.TemperatureStatusChanged -= DetectorTemperatureService_TemperatureStatusChanged;
		_detectorTemperatureService.TemperatureStatusChanged += DetectorTemperatureService_TemperatureStatusChanged;
		_isDetectorTemperatureNormalStatus = _detectorTemperatureService.IsTemperatureNormalStatus;

		IsConfirmVisible = true;
		_pageService = pageService;
		_isDoorClosed = _uiRelatedStatusService.IsDoorClosed;

		_goService.Validated -= GoService_Validated;
		_goService.Validated += GoService_Validated;

		_goService.ReconAllValidated -= GoService_ReconAllValidated;
		_goService.ReconAllValidated += GoService_ReconAllValidated;

		goValidateDialogService.PopValidateMessageWindow -= GoValidateRule_PopValidateMessageWindow;
		goValidateDialogService.PopValidateMessageWindow += GoValidateRule_PopValidateMessageWindow;
		_tablePositionService = tablePositionService;

		_goService.ParameterValidated -= GoService_ParameterValidated;
		_goService.ParameterValidated += GoService_ParameterValidated;

		_goService.ParameterLogicValidated -= GoService_ParameterLogicValidated;
		_goService.ParameterLogicValidated += GoService_ParameterLogicValidated;

		_realtimeStatusProxyService.ScanReconErrorOccurred -= RealtimeStatusProxyService_ScanReconErrorOccurred;
		_realtimeStatusProxyService.ScanReconErrorOccurred += RealtimeStatusProxyService_ScanReconErrorOccurred;

		_realtimeStatusProxyService.EmergencyStopped -= RealtimeStatusProxyService_EmergencyStopped;
		_realtimeStatusProxyService.EmergencyStopped += RealtimeStatusProxyService_EmergencyStopped;
		_realtimeStatusProxyService.ErrorStopped -= RealtimeStatusProxyService_EmergencyStopped;
		_realtimeStatusProxyService.ErrorStopped += RealtimeStatusProxyService_EmergencyStopped;
		_voiceService = voiceService;
	}

	private void RealtimeStatusProxyService_EmergencyStopped(object? sender, EventArgs<RealtimeInfo> e)
	{
		Global.IsGoing = false;
	}

	private void RealtimeStatusProxyService_ScanReconErrorOccurred(object? sender, EventArgs<List<string>> e)
	{
		if (e.Data is not null && e.Data.Count > 0)
		{
			_dialogService.ShowDialog(true, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, String.Join(",", e.Data), arg =>
			{
				if (arg.Result == ButtonResult.Close || arg.Result == ButtonResult.Cancel)
				{
					return;
				}
			}, ConsoleSystemHelper.WindowHwnd);
		}
	}

	private void DetectorTemperatureService_TemperatureStatusChanged(object? sender, bool e)
	{
		_isDetectorTemperatureNormalStatus = e;
	}

	private void GoService_ParameterLogicValidated(object? sender, EventArgs<bool> e)
	{
		if (e is null || !e.Data)
		{
			return;
		}
		if (e.Data)
		{
			ParamsLogicValidated();
		}
	}

	private void GoService_ParameterValidated(object? sender, EventArgs<bool> e)
	{
		if (e is null || !e.Data)
		{
			return;
		}
		if (e.Data)
		{
			ParamsValidated();
		}
	}

	/// <summary>
	/// 替换协议完成,模拟点击confirm按钮
	/// </summary>
	[UIRoute]
	private void DataSync_ReplaceProtocolFinished(object? sender, EventArgs e)
	{
		Confirm();
	}

	[UIRoute]
	private void GoService_ReconAllValidated(object? sender, EventArgs<bool> e)
	{
		if (e is null || !e.Data)
		{
			return;
		}
		ReconAllHandler();
	}

	private void GoValidateRule_PopValidateMessageWindow(object? sender, EventArgs<RuleDialogType> e)
	{
		if (e is null)
		{
			return;
		}
		switch (e.Data)
		{
			case RuleDialogType.DoseNotificationDialog:
				PopDoseNotificationDialog();
				break;
			case RuleDialogType.DoseAlertDialog:
				PopDoseAlertDialog();
				break;
			case RuleDialogType.CommonNotificationDialog:
				CommonNotificationDialog();
				break;
			case RuleDialogType.WarningDialog:
				CommonWarningDialog();
				break;
		}
	}

	private void GoService_Validated(object? sender, EventArgs<bool> e)
	{
		if (e is null || !e.Data)
		{
			return;
		}
		if (e.Data)
		{
			GoHandler();
		}
	}

	private void CommonWarningDialog()
	{
		if (commonWarningWindow is null)
		{
			commonWarningWindow = new();
		}
		WindowDialogShow.DialogShow(commonWarningWindow);
	}

	private void CommonNotificationDialog()
	{
		if (commonNotificationWindow is null)
		{
			commonNotificationWindow = new();
		}
		WindowDialogShow.DialogShow(commonNotificationWindow);
	}

	private void PopDoseNotificationDialog()
	{
		if (_doseNotificationWindow is null)
		{
			_doseNotificationWindow = new();
		}
		WindowDialogShow.DialogShow(_doseNotificationWindow);
	}

	private void PopDoseAlertDialog()
	{
		if (_doseAlertWindow is null)
		{
			_doseAlertWindow = new();
		}
		if (_doseAlertWindow is not null && _doseAlertWindow.DoseAlertPasswordBox is not null)
		{
			_doseAlertWindow.DoseAlertPasswordBox.Password = string.Empty;
			WindowDialogShow.DialogShow(_doseAlertWindow);
		}
	}

	[UIRoute]
	private void UIControlStatusService_GoButtonStatusChanged(object? sender, EventArgs e)
	{
		ValidateScanControl(nameof(UIControlStatusService_GoButtonStatusChanged));
	}

	[UIRoute]
	private void SystemReadyService_StatusChanged(object? sender, EventArgs<(bool status, bool isSyatemStatus)> e)
	{
		ValidateScanControl(nameof(SystemReadyService_StatusChanged));
	}

	[UIRoute]
	public void ProtocolHostService_StructureChanged(object? sender, EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)> e)
	{
		if (e is null)
		{
			return;
		}
		if (e.Data.Parent is ScanModel && e.Data.Current is ReconModel)
		{
			switch (e.Data.ChangeType)
			{
				case StructureChangeType.Add:
				case StructureChangeType.Delete:
					ValidateScanControl();
					break;
			}
		}

		if (e.Data.Current is ProtocolModel protocolModel)
		{
			ProtocolStructureChanged(protocolModel);
		}
	}

	[UIRoute]
	private void SetDoorState(bool isDoorClosed, RealtimeStatus realtimeStatus)
	{
		_logger.LogDebug($"[SetDoorState] current door is closed : {isDoorClosed} , realtimestatus is {realtimeStatus}");

		if (isDoorClosed == false && (RealtimeStatus == RealtimeStatus.MovingPartEnable || RealtimeStatus == RealtimeStatus.ExposureEnable))
		{
			//门打开了 ，只在这两种情况下提示
			_doorAlertWindow = CTS.Global.ServiceProvider?.GetService<DoorAlertWindow>();
			if (_doorAlertWindow is null)
				return;

			if (!_doorAlertWindowShown)
			{
				_doorAlertWindowShown = true;
				_doorAlertWindow.Show();
			}
		}
		else
		{
			_doorAlertWindow?.Hide();
			_doorAlertWindowShown = false;
		}
	}

	[UIRoute]
	private void UIRelatedStatusService_DoorStatusChanged(object? sender, bool e)
	{
		_isDoorClosed = e;
		SetDoorState(e, RealtimeStatus);
	}

	[UIRoute]
	private void SetSystemAbnormal()
	{
		IsGoEnable = false && _systemReadyService.Status;
		IsCancelEnable = false;
		IsReconAllEnable = false;
	}

	[UIRoute]
	private void LayoutChanged(object? sender, EventArgs<ScanTaskAvailableLayout> e)
	{
		_pageService.SetCurrentPage(e.Data);
		if (e.Data == ScanTaskAvailableLayout.ProtocolSelection)
		{
			IsConfirmVisible = true;

			ConfirmReconAllContent = LanguageResource.Content_Confirm;
			IsReconAllVisible = false;

			IsConfirmEnable = _protocolHostService.Models.Any(t => t.Scan is not null);
			IsReconAllEnable = false;
			IsGoVisible = true;
			IsGoEnable = false && _systemReadyService.Status;
			IsCloseVisible = false;
			IsCancelEnable = false;
		}
		else if (e.Data == ScanTaskAvailableLayout.Recon)
		{
			IsReconAllEnable = false;
			IsConfirmEnable = false;

			IsGoVisible = false;

			IsCloseVisible = true;
			IsCloseEnable = true;

			IsCancelEnable = false;
		}
		else if (e.Data == ScanTaskAvailableLayout.ScanDefault)
		{
			IsReconAllVisible = true;
			InitReconAllEnable();
			IsCloseVisible = false;

			IsGoVisible = true;

			IsGoEnable = true && _systemReadyService.Status;
			IsCancelEnable = false;
		}

		//页面跳转之后，重新校验按钮状态
		ValidateScanControl();
	}

	[UIRoute]
	private void PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		_logger.LogInformation($"[ScanControlViewModel] {e.Data.Model.GetType().Name}({e.Data.Model.Descriptor.Id}) PerformStatusChanged finished, old status is {e.Data.OldStatus}, new status is {e.Data.NewStatus}");

		if (e.Data.Model is MeasurementModel && e.Data.NewStatus != e.Data.OldStatus)
		{
			//TODO:临时处理
			if (e.Data.NewStatus == PerformStatus.Performed)
			{
				IsScanning = false;
			}

			ValidateScanControl(nameof(PerformStatusChanged));
		}
	}

	[UIRoute]
	private void SelectionScanChanged(object? sender, EventArgs<ScanModel> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		ValidateScanControl(nameof(SelectionScanChanged));
		//通知RGT那边
		NotifyRgtSelectionChanged(e.Data);
	}

	private void NotifyRgtSelectionChanged(ScanModel scanModel)
	{
		//通知RGT那边 Scan变化了
		var rgtModel = new RgtScanModel();
		rgtModel.Protocol = scanModel.Parent.Parent.Parent.Descriptor.Name;
		rgtModel.ScanType = scanModel.Descriptor.Name;
		rgtModel.Kv = scanModel.Kilovolt[0];
		rgtModel.Ma = scanModel.Milliampere[0];
		rgtModel.PatientPosition = scanModel.PatientPosition.ToString() ?? "";
		rgtModel.ScanLength = scanModel.ScanLength;
		rgtModel.CTDlvol = scanModel.DoseEstimatedCTDI;
		rgtModel.DLP = scanModel.DoseEstimatedDLP;
		rgtModel.DelayTime = scanModel.ExposureDelayTime;
		rgtModel.ExposureTime = scanModel.ExposureTime;

		//_workflow?.SelectionScan(rgtModel);
	}

	private void ProtocolStructureChanged(ProtocolModel protocolModel)
	{
		IsConfirmEnable = protocolModel.Children.SelectMany(n => n.Children.SelectMany(m => m.Children)).Any();
		var protocolMainViewModel = Global.ServiceProvider.GetService<ProtocolSelectMainViewModel>();
		if (IsConfirmEnable && protocolMainViewModel is not null && protocolMainViewModel.IsProtocolChangeFromRgt)
		{
			//如果Confirm可用，处理RGT
			NotifyReplaceProtocol();
		}
		//TODO:
		//IsMRSReady = true;

		if (!IsAuto)
		{
			if (protocolModel.IsEmergency)
			{
				//如果是急诊，直接进入扫描页面
				Confirm();
			}
			else
			{
				RecoverPatient();
			}
		}
		IsAuto = true;

		ValidateScanControl(nameof(ProtocolStructureChanged));

		SetMessageTip();
	}

	/// <summary>
	/// 恢复非急诊协议的病人数据
	/// </summary>
	private void RecoverPatient()
	{
		var _workflow = CTS.Global.ServiceProvider.GetRequiredService<IWorkflow>();
		var studyId = _workflow.GetCurrentStudy();
		var studyService = CTS.Global.ServiceProvider?.GetRequiredService<IStudyService>();
		if (studyService is not null)
		{
			var (studyModel, _) = studyService.Get(studyId);
			if (studyModel is not null && !string.IsNullOrEmpty(studyModel.Protocol) && studyModel.StudyStatus == WorkflowStatus.Examinating.ToString())
			{
				Confirm();
			}
		}
	}

	/// <summary>
	/// 通知RGT,替换协议完成
	/// </summary>
	private void NotifyReplaceProtocol()
	{
		_dataSync?.NotifyReplaceProtocol();
	}

	/// <summary>
	/// 通知RGT,实时状态
	/// </summary>
	private void NotifyRealtimeStatus(RealtimeInfo info)
	{
		_logger.LogInformation($"notify realtime status {info.Status}");
		_dataSync?.NotifyRealtimeStatus(info);
	}

	[UIRoute]
	private void RealtimeStatusChanged(object? sender, EventArgs<RealtimeInfo> e)
	{
		NotifyRealtimeStatus(e.Data);

		RealtimeStatus = e.Data.Status;

		switch (e.Data.Status)
		{
			case RealtimeStatus.Init:
			case RealtimeStatus.Standby:
				_workflow?.Unlocking();
				IsStandBy = true;
				IsScanning = false;
				SetCancelState(false);
				if (_layoutManager.CurrentLayout == ScanTaskAvailableLayout.ProtocolSelection)
				{
					//协议选择页面
					IsConfirmVisible = true;
					IsConfirmEnable = _protocolHostService.Models.Any(n => n.Scan is not null);
					IsReconAllVisible = false;
					IsReconAllEnable = false;
				}
				else if (_layoutManager.CurrentLayout == ScanTaskAvailableLayout.ScanDefault)
				{
					//扫描页面
					IsConfirmVisible = false;
					IsReconAllVisible = true;
				}
				ValidateScanControl($"{nameof(RealtimeStatusChanged)} - [init,standby]");
				break;
			case RealtimeStatus.Validated:
			case RealtimeStatus.ParamConfig:
			case RealtimeStatus.MovingPartEnable:
			case RealtimeStatus.MovingPartEnabling:
			case RealtimeStatus.MovingPartEnabled:
			case RealtimeStatus.ExposureEnable:
			case RealtimeStatus.ExposureStarted:
			case RealtimeStatus.ExposureSpoting:
			case RealtimeStatus.ExposureFinished:
			case RealtimeStatus.ScanStopping:
				IsStandBy = false;
				ValidateScanControl($"{nameof(RealtimeStatusChanged)} - [MovePart,Exposure]");
				SetCancelState(true);
				break;
			case RealtimeStatus.NormalScanStopped:
				IsStandBy = false;
				ValidateScanControl($"{nameof(RealtimeStatusChanged)} - [NormalScanStopped]");
				_logger.LogInformation($"RealtimeStatusChanged NormalScanStopped SetCancelState={IsCancelEnable}");
				SetCancelState(false);
				break;
			case RealtimeStatus.EmergencyScanStopped:
			case RealtimeStatus.Error:
				IsStandBy = false;
				ValidateScanControl($"{nameof(RealtimeStatusChanged)} - [EmergencyStopped,Error]");
				_logger.LogInformation($"RealtimeStatusChanged Error SetCancelState");
				SetCancelState(false);
				break;
			default:
				IsStandBy = false;
				ValidateScanControl($"{nameof(RealtimeStatusChanged)} - [default]");
				break;
		}

		//设置门控状态
		SetDoorState(_isDoorClosed, RealtimeStatus);
	}

	public void Confirm()
	{
		try
		{
			_layoutManager.SwitchToView(ScanTaskAvailableLayout.ScanDefault);

			ConfirmReconAllContent = LanguageResource.Content_ReconAll;
			IsConfirmEnable = false;

			InitReconAllEnable();

			IsReconAllVisible = true;
			IsGoEnable = true && _systemReadyService.Status;
			_logger.LogInformation($"UIControlConfirmStatus IsGoEnable={_systemReadyService.Status}");
			IsCancelEnable = false;
			IsConfirmVisible = false;

			_selectionManager.SelectScan();
			ValidateScanControl();

			var changingList = ScanReconDirectionHelper.AdjustAllScanReconDirections(_protocolHostService);
			_protocolHostService.SetParameters(changingList);
			//RGT
			_screenSync?.Go();

			StartIntervention();
		}
		catch (NanoException ex)
		{
			ShowErrorCode(ex);
		}
	}

	private void InitReconAllEnable()
	{
		if (_selectionManager.LastSelectionTomoScan is not null
			&& !string.IsNullOrEmpty(_selectionManager.LastSelectionTomoScan.Descriptor.Id)
			&& !(_selectionManager.LastSelectionTomoScan.ScanOption == ScanOption.DualScout || _selectionManager.LastSelectionTomoScan.ScanOption == ScanOption.Surview))
		{
			var rtd = _selectionManager.LastSelectionTomoScan.Children.FirstOrDefault(t => t.IsRTD && t.Status == PerformStatus.Performed);
			var nrtd = _selectionManager.LastSelectionTomoScan.Children.FirstOrDefault(t => !t.IsRTD && t.Status == PerformStatus.Unperform);
			if (rtd is not null && nrtd is not null)
			{
				IsReconAllEnable = true;
			}
		}
	}

	private void ReconAll()
	{
		if (!RuntimeConfig.IsDevelopment || _uiRelatedStatusService.IsValidated)
		{
			_goService.ReconAllValidate();
		}
		else
		{
			ReconAllHandler();
		}
	}

	private void ReconAllHandler()
	{
		try
		{
			if (!_offlineConnectionService.IsConnected)
			{
				_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, LanguageResource.Message_Warring_OfflineConnectFailed, null, ConsoleSystemHelper.WindowHwnd);
				return;
			}
			string mesage = string.Empty;
			var procotolList = _protocolHostService.Models.Where(t => t.Scan.Children.FirstOrDefault(y => y.IsRTD is false && y.Status == PerformStatus.Unperform) is not null).ToList();
			if (procotolList.Count == 0 || procotolList[0].Scan is null)
			{
				return;
			}
			IsReconAllEnable = false;
			//重建参数验证
			foreach (var models in procotolList)
			{
				foreach (var recon in models.Scan.Children)
				{
					if (recon is not null
						&& recon.IsRTD is false
						&& recon.Status == PerformStatus.Unperform
						&& !ReconCalculateExtension.ReconParamCalculate(recon, out mesage))
					{
						_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, mesage, null, ConsoleSystemHelper.WindowHwnd);
						return;
					}
				}
			}
			_offlineReconService.StartAllReconTasks(_studyHostService.StudyId);
		}
		catch (NanoException ex)
		{
			ShowErrorCode(ex);
		}
	}

	private void Go()
	{
		if (!CheckXRayFocusType())
		{
			return;
		}
		//保证扫描参数的预计值的写入
		var activeItem = _protocolHostService.Models.FirstOrDefault(item => item.Measurement.Status == PerformStatus.Unperform);
		if (activeItem.Measurement is MeasurementModel measurement)
		{
			ScanDoseCheckHelper.GetDoseEstimatedInfoByUnperformMeasurement(_doseEstimateService, _protocolHostService, measurement);
		}
		if (!RuntimeConfig.IsDevelopment || _uiRelatedStatusService.IsValidated)
		{
			_goService.GoValidate();
		}
		else
		{
			GoHandler();
		}		
	}

	private void GoHandler()
	{
		Global.IsGoing = true;
		bool goContinue = true;
		////判断X轴是否在ISO中心
		//if (!_tablePositionService.CheckISOCenterWithAxisX())
		//{
		//	_dialogService?.ShowDialog(true, MessageLeveles.Info, LanguageResource.Message_Warning_GoWarningTitle
		//	  , "Make sure to move axis X to the ISO centre? ", arg =>
		//	  {
		//		  if (arg.Result == ButtonResult.Close || arg.Result == ButtonResult.Cancel)
		//		  {
		//			  goContinue = false;
		//		  }
		//		  else
		//		  { 
		//			  _tablePositionService.ResetAxisX();
		//		  }
		//	  }, ConsoleSystemHelper.WindowHwnd);
		//}
		//if (!goContinue)
		//{
		//	return;
		//}
		//goContinue = true;
		//前后罩判断
		if (!_frontRearCoverStatusService.IsClosed)
		{
			_dialogService?.ShowDialog(true, MessageLeveles.Info, LanguageResource.Message_Warning_GoWarningTitle
			  , "The cover is open. Are you sure to keep scanning? ", arg =>
			  {
				  if (arg.Result == ButtonResult.Close || arg.Result == ButtonResult.Cancel)
				  {
					  goContinue = false;
				  }
			  }, ConsoleSystemHelper.WindowHwnd);
		}
		if (!goContinue)
		{
			Global.IsGoing = false;
			return;
		}

		//探测器温度异常
		if (!_isDetectorTemperatureNormalStatus)
		{
			_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, $"Detector temperature is abnormal.", null, ConsoleSystemHelper.WindowHwnd);
			Global.IsGoing = false;
			return;
		}

		//TODO: 开发阶段暂时这样处理
		if (!IsStandBy)
		{
			_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, $"Realtime status is not ready,current status {RealtimeStatus}", null, ConsoleSystemHelper.WindowHwnd);
			Global.IsGoing = false;
			return;
		}

		if (!_systemReadyService.Status)
		{
			//_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, _systemReadyService.LatestFailReason, null, ConsoleSystemHelper.WindowHwnd);
			Global.IsGoing = false;
			return;
		}

		GoProcess();

		GoToScan();
	}

	private void GoProcess()
	{
		ValidateScanControl("GoHanlder");

		//通过RGT同步界面
		var screenSync = CTS.Global.ServiceProvider.GetService<IScreenSync>();
		screenSync?.Go();

		_workflow?.Locking();

		_selectionManager.SelectScan();
	}

	private void GoToScan()
	{
		if (_selectionManager.CurrentSelection.Scan is not null
			&& _selectionManager.CurrentSelection.Scan.Descriptor is not null
			&& !string.IsNullOrEmpty(_selectionManager.CurrentSelection.Scan.Descriptor.Id))
		{
			var scan = _selectionManager.CurrentSelection.Scan;
			//校正扫描长度
			var activeItem = _protocolHostService.Models.FirstOrDefault(item => item.Measurement.Descriptor.Id == scan.Parent.Descriptor.Id);

			ScanLengthHelper.GetCorrectedScanLength(_protocolHostService, activeItem.Measurement);

			var _currentActiveScan = activeItem.Measurement.Children.FirstOrDefault();
			try
			{
				if (activeItem.Measurement is not null && _currentActiveScan is not null) //设置修改MeasurementModel下的参数
				{
					//下参判断TestBolus/NVTestBolus扫描任务的ROIs参数是否存在
					bool isNoCycleROIs = false;
					if (activeItem.Measurement.Children.Count > 0
						&& activeItem.Measurement.Children.FirstOrDefault(t => t.ScanOption == ScanOption.NVTestBolus || t.ScanOption == ScanOption.TestBolus) is ScanModel scanModel)
					{
						foreach (var recon in scanModel.Children)
						{
							if (recon.IsRTD && (recon.CycleROIs is null || recon.CycleROIs.Count == 0))
							{
								isNoCycleROIs = true;
								break;
							}
						}
					}
					if (isNoCycleROIs)
					{
						_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, $"The ROIs of the TestBolus scan cannot be null.", null, ConsoleSystemHelper.WindowHwnd);
						Global.IsGoing = false;
						return;
					}
					//判断语音的ID是否设置正确，以及最小的扫描延迟时间是否满足要求
					if (!ExposureDelayTimeHelper.CalcDelayTimeInMeasurement(_protocolHostService, _voiceService, activeItem.Measurement))
					{
						_dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_Title, $"Scanning delay time parameter  or exposure interval time parameter setting error!", arg => { }, ConsoleSystemHelper.WindowHwnd);
						Global.IsGoing = false;
						return;
					}
					//判断螺旋扫描、轴扫扫描长度是否满足最大最小扫描长度
					if (!ScanLengthHelper.GetCorrectedScanLengthMaxMin(_heatCapacityService, _mapper, activeItem.Measurement))
					{
						_dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_Title, ScanLengthHelper.errorMessage, arg => { }, ConsoleSystemHelper.WindowHwnd);
						ScanLengthHelper.errorMessage = string.Empty;
						Global.IsGoing = false;
						return;
					}
					SetMeasurementParams(activeItem.Measurement);
				}
				//下参前的参数验证
				_protocolHostService.SetCurrentMeasurementID(scan.Parent.Descriptor.Id);
				_goService.ParamsValidate();
			}
			catch (NanoException ex)
			{
				Global.IsGoing = false;
				ShowErrorCode(ex);
			}
			catch (AggregateException aex)
			{
				Global.IsGoing = false;
				_logger.LogError($"Go handler catch aggregated exception : {aex.Flatten().Message}");
			}
		}
	}

	private void SetMeasurementParams(MeasurementModel measurement)
	{
		SetNVTestBolusBaseImagePath(measurement);
		//SetScanReconParam(measurement);
		AdaptScanParamForDataAndTableControlInput(measurement);
		SetScanDelayParam(measurement);
		SetOffsetFrames(measurement);
		Global.IsGoing = false;
	}

	private void ParamsValidated()
	{
		_goService.ParamsLogicValidate();
	}

	private void ParamsLogicValidated()
	{
		Task.Run(() =>
		{
			try
			{
				IsScanning = true;
				var result = _scanControlService.StartMeasurement(_protocolHostService.CurrentMeasurementID);
				if ((result is null || result.Status != CommandExecutionStatus.Success) &&
					(result is not null && result.Details.Count > 0))
				{
					ShowErrorDialog(result.Details[0].Code);
				}
			}
			catch (NanoException ex)
			{
				ShowErrorCode(ex);
			}
			catch (Exception ex)
			{
				_logger.LogError($"Go handler catch exception : {ex.Message}-{ex.StackTrace}");
			}
			finally
			{
				IsScanning = false;

				ValidateScanControl(nameof(ParamsLogicValidated));
			}
		});
	}

	[UIRoute]
	private void ShowErrorDialog(string code)
	{
		_dialogService.ShowErrorDialog(code, ConsoleSystemHelper.WindowHwnd);
	}

	[UIRoute]
	private void ShowErrorCode(NanoException ex)
	{
		_dialogService.ShowDialog(false, MessageLeveles.Warning, LanguageResource.Message_Warning_Title, $"{ex.ErrorCode}{System.Environment.NewLine}{ex.Message}", _ => { }, ConsoleSystemHelper.WindowHwnd);
	}

	private void SetNVTestBolusBaseImagePath(MeasurementModel measurement)
	{
		var scan = measurement.Children.FirstOrDefault(t => (t.ScanOption == ScanOption.NVTestBolus || t.ScanOption == ScanOption.TestBolus) && t.Status == PerformStatus.Unperform);
		if (scan is ScanModel scanModel && scanModel.Children.FirstOrDefault(t => t.IsRTD && t.Status == PerformStatus.Unperform) is ReconModel tagetReconModel)
		{
			var item = _protocolHostService.Models.LastOrDefault(t => t.Scan.ScanOption == ScanOption.NVTestBolusBase && t.Scan.Status == PerformStatus.Performed);
			if (item.Scan is ScanModel baseModel && baseModel.Children.FirstOrDefault(t => t.IsRTD && t.Status == PerformStatus.Performed) is ReconModel reconModel)
			{
				var fileName = Directory.GetFiles(reconModel.ImagePath, "*.dcm").FirstOrDefault();

				var parameters = new List<ParameterModel>
				{
					new ParameterModel
					{
						Name = ProtocolParameterNames.RECON_TESTBOLUS_BASE_IMAGE_PATH,
						Value = fileName
					},
				};
				_protocolHostService.SetParameters(tagetReconModel, parameters);
			}
		}
	}

	private void SetOffsetFrames(MeasurementModel measurement)
	{
		foreach (var scanModel in measurement.Children)
		{
			var offsetPreview = OffsetCalculator.GetPreOffset(scanModel.ScanOption, scanModel.ScanMode);
			var offsetPost = OffsetCalculator.GetPostOffset(scanModel.ScanOption, (int)scanModel.FrameTime);
			var parameters = new List<ParameterModel> {
				new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_PRE_OFFSET_FRAMES,
					Value = offsetPreview.ToString(CultureInfo.InvariantCulture)
				},
				new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_POST_OFFSET_FRAMES,
					Value = offsetPost.ToString(CultureInfo.InvariantCulture)
				}
			};
			_protocolHostService.SetParameters(scanModel, parameters);
		}
	}

	private void SetScanDelayParam(MeasurementModel measurementModel)
	{
		foreach (var scanModel in measurementModel.Children)
		{
			var delayInfo = SystemConfig.ScanDelayConfig.ScanDelays.FirstOrDefault(d => d.ScanOption == scanModel.ToString() && d.FrameTime == scanModel.FrameTime);
			if (delayInfo is null)
			{
				delayInfo = SystemConfig.ScanDelayConfig.ScanDelays.FirstOrDefault(d => d.ScanOption == scanModel.ToString() && d.FrameTime == -1);
			}
			var parameters = new List<ParameterModel> {
				new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_R_DELAY,
					Value = delayInfo is null ? "300": delayInfo.RDelay.ToString(CultureInfo.InvariantCulture)
				},
				new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_T_DELAY,
					Value = delayInfo is null ? "300": delayInfo.TDelay.ToString(CultureInfo.InvariantCulture)
				},
				new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_SPOT_DELAY,
					Value = delayInfo is null ? "0": delayInfo.SpotDelay.ToString(CultureInfo.InvariantCulture)
				}
			};
			_protocolHostService.SetParameters(scanModel, parameters);
		}
	}

	//private void SetScanReconParam(MeasurementModel measurementModel)
	//{
	//	foreach (var scan in measurementModel.Children)
	//	{
	//		if (scan.Status == PerformStatus.Unperform && (scan.ScanOption == ScanOption.Surview || scan.ScanOption == ScanOption.DualScout))
	//		{
	//			foreach (var recon in scan.Children)
	//			{
	//				var dir = ScanReconCoordinateHelper.GetDefaultTopoReconOrientation(scan.Parent.Parent.PatientPosition, (TubePos)scan.TubePositions[0]);
	//				var centerP = ScanReconCoordinateHelper.GetTopoReconParamByScanRange(scan.Parent.Parent.PatientPosition, scan.ReconVolumeStartPosition, scan.ScanLength, recon.ImageOrder);
	//				List<ParameterModel> parameterModels = new List<ParameterModel> {
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X,
	//						Value=dir[0].ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y,
	//						Value = dir[1].ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel()
	//					{
	//						Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z,
	//						Value = dir[2].ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel()
	//					{
	//						Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X,
	//						Value = dir[3].ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel()
	//					{
	//						Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y,
	//						Value = dir[4].ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel()
	//					{
	//						Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z,
	//						Value = dir[5].ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_CENTER_FIRST_X,
	//						Value = ((int)centerP[0]).ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y,
	//						Value = ((int)centerP[1]).ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z,
	//						Value = ((int)centerP[2]).ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_CENTER_LAST_X,
	//						Value = ((int)centerP[0]).ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_CENTER_LAST_Y,
	//						Value = ((int)centerP[1]).ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_CENTER_LAST_Z,
	//						Value = ((int)centerP[2]).ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL,
	//						Value = (recon.FOVLengthHorizontal).ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL,
	//						Value = (scan.ScanLength).ToString(CultureInfo.InvariantCulture)
	//					},
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_HORIZONTAL,
	//						Value = (recon.ImageMatrixHorizontal).ToString()
	//					},
	//					new ParameterModel
	//					{
	//						Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL,
	//						Value = ((int)UnitConvert.Micron2Millimeter(scan.ScanLength /recon.SliceThickness) ).ToString()
	//					},
	//					new ParameterModel()
	//					{
	//						Name = ProtocolParameterNames.RECON_IMAGE_INCREMENT,
	//						Value = (recon.SliceThickness).ToString()
	//					}
	//				};
	//				_protocolHostService.SetParameters(recon, parameterModels);
	//			}
	//		}
	//	}
	//}

	public virtual void Close()
	{
		_layoutManager.Back();
	}

	[UIRoute]
	public void SetMessageTip()
	{
		//TODO:临时
		if (!IsStandBy)
		{
			_messageTooltipViewModel.ScanMessage = $"System is not standby!";
		}
		else
		{
			_messageTooltipViewModel.ScanMessage = string.Empty;
		}
	}

	[UIRoute]
	public virtual void ValidateScanControl(string sourceMethod = "")
	{
		//TODO:暂时这样处理,Go按钮状态加上 StandBy控制，默认是UIControlStatusService来一次性给定所有条件
		IsGoEnable = _uiControlStatusService.IsGoButtonEnable && !IsScanning && _systemReadyService.Status;

		IsReconAllEnable = _uiControlStatusService.IsReconAllButtonEnable && IsStandBy;
		_logger.LogInformation($"UIControlStatusService IsGoEnable={IsGoEnable},IsReconAllEnable={IsReconAllEnable},SystemReadyService.Status={_systemReadyService.Status},IsScanning={IsScanning},RealtimeStatusChanged={sourceMethod}");
	}

	private void SetCancelState(bool isEnable)
	{
		IsCancelEnable = _layoutManager.CurrentLayout == ScanTaskAvailableLayout.ScanDefault && isEnable;

		_logger.LogInformation($"SetCancelState IsCancelEnable={IsCancelEnable} - systemReadyStatus:{_systemReadyService.Status}");
	}

	public void Cancel()
	{
		try
		{
			_scanControlService.CancelMeasurement();
		}
		catch (NanoException ex)
		{
			ShowErrorCode(ex);
		}
	}

	public void Dispose()
	{
		_uiRelatedStatusService.RealtimeStatusChanged -= RealtimeStatusChanged;
		_protocolHostService.StructureChanged -= ProtocolHostService_StructureChanged;
	}

	private void StartIntervention()
	{
		if (Process.GetCurrentProcess().ProcessName != ApplicationParameterNames.PROCESSNAME_EXAMINATION)
		{
			return;
		}
		Task.Run(() =>
		{
			bool IsIntervention = _protocolHostService.Models.Any(t => t.Scan.IsIntervention);
			if (IsIntervention
				&& _applicationCommunicationService.IsExistsProcess(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_INTERVENTIONSCAN, "Intervention")))
			{
				_applicationCommunicationService.Start(new ApplicationRequest(ApplicationParameterNames.APPLICATIONNAME_INTERVENTIONSCAN, "Intervention"));
			}
		});
	}

	/// <summary>
	/// go下参设置床跟机架的参数
	/// </summary>
	/// <param name=""></param>
	private void AdaptScanParamForDataAndTableControlInput(MeasurementModel measurementModel)
	{
		_logger.LogDebug($"CalculatePositon: current table position => {_tablePositionService.CurrentTablePosition.HorizontalPosition}, {_tablePositionService.CurrentTablePosition.VerticalPosition}, current gantry position => {_tablePositionService.CurrentGantryPosition.Position}");
		ScanModel preScan = null;
		foreach (var scan in measurementModel.Children)
		{
			List<ParameterModel> parameterModels = GetTableGantryControlParameterModels(scan, preScan);
			if (parameterModels.Count > 0)
			{
				_protocolHostService.SetParameters(scan, parameterModels);
			}
			preScan = scan;
		}
	}

	/// <summary>
	/// 根据扫描参数获取床参跟机架参数列表
	/// </summary>
	/// <param name="scan">扫描参数</param>
	/// <returns>机架参数列表跟床参数列表</returns>
	private List<ParameterModel> GetTableGantryControlParameterModels(ScanModel scan, ScanModel preScan)
	{
		List<ParameterModel> parameterModels = new List<ParameterModel>();
		TableControlInput tableControlInput = new TableControlInput();
		tableControlInput.ScanMode = scan.ScanMode;
		tableControlInput.ScanOption = scan.ScanOption;
		tableControlInput.Pitch = UnitConvert.ReduceHundred(scan.Pitch * 1.0);
		tableControlInput.FramesPerCycle = (int)scan.FramesPerCycle;
		tableControlInput.CollimatedSliceWidth = scan.CollimatorSliceWidth;
		tableControlInput.ExposureTime = scan.ExposureTime;
		tableControlInput.ExpSourceCount = (int)scan.ExposureMode;
		tableControlInput.FrameTime = scan.FrameTime;
		tableControlInput.PreIgnoredFrames = (int)scan.AutoDeleteNum;
		tableControlInput.ReconVolumeBeginPos = scan.ReconVolumeStartPosition;
		tableControlInput.ReconVolumeEndPos = scan.ReconVolumeEndPosition;
		tableControlInput.TableAcc = scan.TableAcceleration;
		tableControlInput.TableFeed = scan.TableFeed;
		tableControlInput.TableDirection = scan.TableDirection;
		tableControlInput.TotalSourceCount = _heatCapacityService.Current.Count;
		tableControlInput.Loops = scan.Loops;
		tableControlInput.PreDeleteRatio = 1;
		tableControlInput.PostDeleteLegnth = scan.PostDeleteLength;
		tableControlInput.ObjectFov = scan.ObjectFOV;
		tableControlInput.BodyPart = _mapper.Map<FacadeProxy.Common.Enums.BodyPart>(scan.BodyPart);
		tableControlInput.CollimatorZ = (int)scan.CollimatorZ;
		_logger.LogDebug($"The input parameters of TablePosition Calculator: {JsonConvert.SerializeObject(tableControlInput)}");
		TableControlOutput tableControlOutput = ScanTableCalculator.Instance.CalculateTableControlInfo(tableControlInput);
		_logger.LogDebug($"The result of TablePosition Calculator: {JsonConvert.SerializeObject(tableControlOutput)}");
		if (tableControlOutput is TableControlOutput table)
		{
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION,
				Value = ((int)Math.Round(table.ReconVolumeBeginPos, 0)).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION,
				Value = ((int)Math.Round(table.ReconVolumeEndPos, 0)).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_TABLE_START_POSITION,
				Value = ((int)Math.Round(table.TableBeginPos, 0)).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_TABLE_END_POSITION,
				Value = ((int)Math.Round(table.TableEndPos, 0)).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_TABLE_ACCELERATION_TIME,
				Value = ((uint)Math.Round(table.TableAccTime, 0)).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_TABLE_SPEED,
				Value = ((uint)Math.Round(table.TableSpeed, 0)).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_EXPOSURE_START_POSITION,
				Value = ((int)Math.Round(table.DataBeginPos, 0)).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_EXPOSURE_END_POSITION,
				Value = ((int)Math.Round(table.DataEndPos, 0)).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_TOTAL_FRAMES,
				Value = table.TotalFrames.ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_SMALL_ANGLE_DELETE_LENGTH,
				Value = ((int)table.SmallAngleDeleteLength).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_LARGE_ANGLE_DELETE_LENGTH,
				Value = ((int)table.LargeAngleDeleteLength).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_TABLE_HEIGHT,
				Value = ((int)_tablePositionService.CurrentTablePosition.VerticalPosition).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.AddRange(GetGantryControlParameterModels(scan, tableControlOutput, preScan));
		}
		return parameterModels;
	}

	/// <summary>
	/// 根据扫描参数跟床参数获取机架参数列表
	/// </summary>
	/// <param name="scan">扫描参数</param>
	/// <param name="tableControlOutput">床参数</param>
	/// <returns>机架参数列表</returns>
	private List<ParameterModel> GetGantryControlParameterModels(ScanModel scan, TableControlOutput tableControlOutput, ScanModel preScan)
	{
		List<ParameterModel> parameterModels = new List<ParameterModel>();
		GantryControlInput gantryControlInput = new GantryControlInput();
		gantryControlInput.NumOfScan = tableControlOutput.NumOfScan;
		gantryControlInput.FramesPerCycle = scan.FramesPerCycle;
		gantryControlInput.CurrentGantryPos = preScan is null ? _tablePositionService.CurrentGantryPosition.Position : preScan.GantryEndPosition;
		gantryControlInput.DataBeginPos = tableControlOutput.DataBeginPos;
		gantryControlInput.DataEndPos = tableControlOutput.DataEndPos;
		gantryControlInput.ExpSourceCount = (int)scan.ExposureMode;
		gantryControlInput.FramesPerCycle = scan.FramesPerCycle;
		gantryControlInput.FrameTime = scan.FrameTime;
		gantryControlInput.GantryAcc = scan.GantryAcceleration;
		gantryControlInput.TableSpeed = tableControlOutput.TableSpeed;
		gantryControlInput.TableFeed = scan.TableFeed;
		gantryControlInput.TableAcc = scan.TableAcceleration;
		List<double> hc = new List<double>();
		List<double> ot = new List<double>();
		foreach (var tube in _heatCapacityService.Current)
		{
			hc.Add(tube.RaySource.HeatCapacity);
			ot.Add(tube.RaySource.OilTemperature);
		}
		gantryControlInput.HeatCaps = hc.ToArray();
		gantryControlInput.OilTem = ot.ToArray();

		gantryControlInput.NumOfScan = tableControlOutput.NumOfScan;
		gantryControlInput.PreIgnoredN = (int)scan.AutoDeleteNum;
		gantryControlInput.ScanMode = scan.ScanMode;
		gantryControlInput.ScanOption = scan.ScanOption;
		gantryControlInput.TotalSourceCount = _heatCapacityService.Current.Count;
		gantryControlInput.TubePositions = scan.TubePositions;
		gantryControlInput.Loops = scan.Loops;
		gantryControlInput.LoopTime = scan.LoopTime;

		_logger.LogDebug($"The input parameters of GantryPosition Calculator: {JsonConvert.SerializeObject(gantryControlInput)}");
		var gantryControlOutput = ScanGantryCalculator.Instance.GetGantryControlOutput(gantryControlInput);
		if (gantryControlOutput is not null)
		{
			_logger.LogDebug($"The result of GantryPosition Calculator: {JsonConvert.SerializeObject(gantryControlOutput)}");
			_protocolHostService.SetParameter(scan, ProtocolParameterNames.SCAN_TUBE_NUMBERS, gantryControlOutput.SelectedTube);
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_GANTRY_DIRECTION,
				Value = gantryControlOutput.GantryDirection.ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_GANTRY_START_POSITION,
				Value = ((uint)Math.Round(gantryControlOutput.GantryStartPos, 0)).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_GANTRY_END_POSITION,
				Value = ((uint)Math.Round(gantryControlOutput.GantryEndPos, 0)).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_GANTRY_ACCELERATION_TIME,
				Value = ((uint)Math.Round(gantryControlOutput.GantryAccTime, 0)).ToString(CultureInfo.InvariantCulture)
			});
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_GANTRY_SPEED,
				Value = ((uint)Math.Round(gantryControlOutput.GantrySpeed, 0)).ToString(CultureInfo.InvariantCulture)
			});
		}
		return parameterModels;
	}

	private bool CheckXRayFocusType()
	{
		bool flag = true;
		if (_selectionManager.CurrentSelection.Scan is not null
			&& _selectionManager.CurrentSelection.Scan.Parent is MeasurementModel measurementModel)
		{
			ScanningParamInfo scanningParamInfo = SystemConfig.ScanningParamConfig.ScanningParam;
			foreach (var scan in measurementModel.Children)
			{
				if (scan.Status != PerformStatus.Unperform) continue;
				//计算方式是：（KV*1000*mA/1000）/1000=KW
				double kw = UnitConvert.ReduceThousand(scan.Kilovolt[0] * scan.Milliampere[0]);

				if (scan.FocalType == FocalType.Small && kw > scanningParamInfo.SmallFocal.Max)
				{
					flag = false;
					_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, $"Small focus energy cannot be greater than {SystemConfig.ScanningParamConfig.ScanningParam.SmallFocal.Max}kw;", null, ConsoleSystemHelper.WindowHwnd);
					break;
				}
				if (scan.FocalType == FocalType.Large && kw > scanningParamInfo.LargeFocal.Max)
				{
					flag = false;
					_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, $"Large focus energy cannot be greater than {SystemConfig.ScanningParamConfig.ScanningParam.LargeFocal.Max}kw;", null, ConsoleSystemHelper.WindowHwnd);
					break;
				}
				if (scan.ScanOption == ScanOption.DualScout)
				{
					//计算方式是：（KV*1000*mA/1000）/1000=KW
					var kw1 = UnitConvert.ReduceThousand(scan.Kilovolt[1] * scan.Milliampere[1]);
					if (scan.FocalType == FocalType.Small && kw1 > scanningParamInfo.SmallFocal.Max)
					{
						flag = false;
						_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, $"Small focus energy cannot be greater than {SystemConfig.ScanningParamConfig.ScanningParam.SmallFocal.Max}kw;", null, ConsoleSystemHelper.WindowHwnd);
						break;
					}
					if (scan.FocalType == FocalType.Large && kw1 > scanningParamInfo.LargeFocal.Max)
					{
						flag = false;
						_dialogService.Show(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, $"Large focus energy cannot be greater than {SystemConfig.ScanningParamConfig.ScanningParam.LargeFocal.Max}kw;", null, ConsoleSystemHelper.WindowHwnd);
						break;
					}
				}
			}
		}
		return flag;
	}
}