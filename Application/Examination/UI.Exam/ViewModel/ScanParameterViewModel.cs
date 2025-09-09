//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.DatabaseService.Contract;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.UI.Exam.Extensions;
using NV.MPS.Configuration;
using NV.MPS.Environment;
using TriggerMode = NV.CT.FacadeProxy.Common.Enums.TriggerMode;
using TubePosition = NV.CT.FacadeProxy.Common.Enums.TubePosition;
using Window = System.Windows.Window;

namespace NV.CT.UI.Exam.ViewModel;

public class ScanParameterViewModel : BaseViewModel
{
	private readonly ILogger<ScanParameterViewModel> _logger;
	private readonly IProtocolHostService _protocolHostService;
	private readonly ISelectionManager _selectionManager;
	private readonly IUIRelatedStatusService _uIRelatedStatusService;

	private readonly IDialogService _dialogService;
	private readonly IVoiceService _voiceService;
	private readonly IImageOperationService _imageOperationService;
	private readonly ITablePositionService _tablePositionService;
	private readonly IDoseEstimateService _doseEstimateService;

	private bool IsDoseErrorNotice = false;
	private double SettingDLP = 0.0;
	private double SettingCTDI = 0.0;

	private double RealTimelTablePosition = 0.0;
	private bool IsOpenLaser = true;
	private bool IsUIOpenLaserChange = true;
	/// <summary>
	/// 当前的ScanModel
	/// </summary>
	private ScanModel? CurrentScanModel { get; set; }
	private Dictionary<string, bool> ParameterDirty { get; set; } = new();

	private bool IsUIChange = true;

	VoiceAPIConfigWindow? _voiceAPIConfigWindow;

	TubePositionWindow? _tubePositionWindow;

	public ScanParameterViewModel(IProtocolHostService protocolHostService,
		ISelectionManager selectionManager,
		ILogger<ScanParameterViewModel> logger,
		IUIRelatedStatusService uiRelatedStatusService,
		IDialogService dialogService,
		IVoiceService voiceService,
		IImageOperationService imageOperationService,
		ITablePositionService tablePositionService,
		IDoseEstimateService doseEstimateService)
	{
		_logger = logger;
		_protocolHostService = protocolHostService;
		_selectionManager = selectionManager;
		_uIRelatedStatusService = uiRelatedStatusService;
		_dialogService = dialogService;
		_voiceService = voiceService;
		_imageOperationService = imageOperationService;
		_tablePositionService = tablePositionService;
		_doseEstimateService = doseEstimateService;

		_selectionManager.SelectionScanChanged -= SelectScanChanged;
		_selectionManager.SelectionScanChanged += SelectScanChanged;
		_protocolHostService.PerformStatusChanged -= PerformStatusChanged;
		_protocolHostService.PerformStatusChanged += PerformStatusChanged;

		_uIRelatedStatusService.RealtimeStatusChanged -= RealtimeStatusChanged;
		_uIRelatedStatusService.RealtimeStatusChanged += RealtimeStatusChanged;
		_uIRelatedStatusService.EmergencyStopped -= UIRelatedStatusService_EmergencyStopped;
		_uIRelatedStatusService.EmergencyStopped += UIRelatedStatusService_EmergencyStopped;

		RealTimelTablePosition = _tablePositionService.CurrentTablePosition.HorizontalPosition;

		Commands.Add(CommandParameters.COMMAND_MARKABLE_TEXT_BOX_GOT_FOCUS, new DelegateCommand<string>(MarkableTextBoxGotFocus, _ => true));
		Commands.Add(CommandParameters.COMMAND_MARKABLE_TEXT_BOX_TEXT_CHANGED, new DelegateCommand<string>(MarkableTextBoxTextChanged, _ => true));
		Commands.Add(CommandParameters.COMMAND_MARKABLE_TEXT_BOX_LOST_FOCUS, new DelegateCommand<string>(MarkableTextBoxLostFocus, _ => true));
		Commands.Add(CommandParameters.COMMAND_APPLIEDTOALLPROTOCOLS, new DelegateCommand(AppliedToAllProtocols, () => true));
		Commands.Add(CommandParameters.COMMAND_SETTINGTUBEPOSITION, new DelegateCommand(SettingTubePosition, () => true));
		Commands.Add(CommandParameters.COMMAND_LOAD, new DelegateCommand<object>(Loaded, _ => true));
		Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand<object>(Closed, _ => true));
		_protocolHostService.ParameterChanged -= ProtocolModificationService_ParameterChanged;
		_protocolHostService.ParameterChanged += ProtocolModificationService_ParameterChanged;
		_imageOperationService.CenterPositionChanged -= ProtocolHostService_CenterPositionChanged;
		_imageOperationService.CenterPositionChanged += ProtocolHostService_CenterPositionChanged;

		_tablePositionService.TablePositionChanged -= TablePositionService_TablePositionChanged;
		_tablePositionService.TablePositionChanged += TablePositionService_TablePositionChanged;

		IsUIChange = false;
		InitComboBoxItem();
		InitDoseConfig();
		LoadSelectedParameters();
		IsUIChange = true;
	}

	[UIRoute]
	private void TablePositionService_TablePositionChanged(object? sender, EventArgs<TablePositionInfo> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		RealTimelTablePosition = e.Data.HorizontalPosition;
		SetBaginRangeByLaser();
	}

	private void SetBaginRangeByLaser()
	{
		if (IsLaserChecked && CurrentScanModel is ScanModel scan && scan.Status == PerformStatus.Unperform && (scan.ScanOption == ScanOption.Surview || scan.ScanOption == ScanOption.DualScout))
		{
			SetRangeByLaserChecked();
		}
	}

	/// <summary>
	/// 解决扫描和重建控件第一次加载，SelectionManager change事件已经发送完毕，收不到事件变更处理的问题
	/// </summary>
	private void LoadSelectedParameters()
	{
		var selectedScan = _selectionManager.CurrentSelection.Scan;
		if (selectedScan is null)
			return;

		SelectScanChanged(selectedScan);
	}

	[UIRoute]
	private void ProtocolHostService_CenterPositionChanged(object? sender, EventArgs<double> e)
	{
		_dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_Title, "Setup successful!", arg => { }, ConsoleSystemHelper.WindowHwnd);
	}

	private void InitDoseConfig()
	{
		IsDoseErrorNotice = UserConfig.DoseSettingConfig.DoseSetting.NotificationEnabled.Value;
		switch (_protocolHostService.Instance.BodySize)
		{
			case BodySize.Child:
				SettingDLP = UserConfig.DoseSettingConfig.DoseSetting.ChildAlertDLPThreshold.Value;
				SettingCTDI = UserConfig.DoseSettingConfig.DoseSetting.ChildAlertCTDIThreshold.Value;
				break;
			case BodySize.Adult:
			default:
				SettingDLP = UserConfig.DoseSettingConfig.DoseSetting.AdultAlertDLPThreshold.Value;
				SettingCTDI = UserConfig.DoseSettingConfig.DoseSetting.AdultAlertCTDIThreshold.Value;
				break;
		}
	}

	[UIRoute]
	private void UIRelatedStatusService_EmergencyStopped(object? sender, EventArgs e)
	{
		_logger.LogInformation($"Emergency Stopped!");
	}

	[UIRoute]
	private void RealtimeStatusChanged(object? sender, EventArgs<RealtimeInfo> e)
	{
		switch (e.Data.Status)
		{
			case RealtimeStatus.ExposureStarted:
				if (CurrentScanModel is ScanModel scanModel && scanModel.Descriptor.Id.Equals(e.Data.ScanId) && scanModel.Status == PerformStatus.Performing)
				{
					_protocolHostService.SetParameter(scanModel, ProtocolParameterNames.SCAN_START_EXPOSURE_TIME, DateTime.Now);
				}
				break;
			default:
				break;
		}
	}

	[UIRoute]
	private void PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		if (e is null) return;

		var (model, _, newStatus) = e.Data;
		IsUIChange = false;
		if (model is not null && model is ScanModel scanModel && newStatus == PerformStatus.Performed && (model.Descriptor.Id.Equals(CurrentScanModel?.Descriptor.Id)))
		{
			HandlerDoseInfo(scanModel);
		}
		if (model is not null && model is ScanModel scan && (model.Descriptor.Id.Equals(CurrentScanModel?.Descriptor.Id)))
		{
			IsEffectiveKVPMaShow = e.Data.NewStatus == PerformStatus.Performed;
			if (scan.ScanOption == ScanOption.NVTestBolusBase)
			{
				IsScanNumEnable = false;
				IsBolusRangeStartEnable = true & newStatus == PerformStatus.Unperform;
			}
			else
			{
				IsBolusRangeStartEnable = false;
				IsScanNumEnable = true & newStatus == PerformStatus.Unperform;
			}
			SetPerformedParameters(scan, e.Data.NewStatus);

			IsScanDirectionEnable = e.Data.NewStatus == PerformStatus.Unperform & !(scan.ScanOption == ScanOption.NVTestBolus
			|| scan.ScanOption == ScanOption.TestBolus
			|| scan.ScanOption == ScanOption.NVTestBolusBase);
		}
		if (model is not null && model is ScanModel scanm
			&& CurrentScanModel is not null
			&& scanm.Descriptor.Id.Equals(scanm.Descriptor.Id))
		{
			PageEnable = e.Data.NewStatus == PerformStatus.Unperform;
			scanm.Status = newStatus;
			SetBeginRangeEnable(scanm);
		}
		IsUIChange = true;
	}

	[UIRoute]
	private void ProtocolModificationService_ParameterChanged(object? sender, EventArgs<(BaseModel baseModel, List<string> list)> e)
	{
		if (e is null || e.Data.baseModel is null || e.Data.list is null)
		{
			return;
		}
		IsUIChange = false;
		if (e.Data.baseModel is ScanModel scanModel && CurrentScanModel is not null && scanModel.Descriptor.Id == CurrentScanModel.Descriptor.Id)
		{
			CurrentScanModel = scanModel;
			if (e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION)) is not null)
			{
				BeginRange = Math.Round(UnitConvert.Micron2Millimeter((double)scanModel.ReconVolumeStartPosition), 2);
			}
			if (e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION)) is not null)
			{
				EndRange = Math.Round(UnitConvert.Micron2Millimeter((double)scanModel.ReconVolumeEndPosition), 2);
			}
			if (e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_LENGTH)) is not null)
			{
				ScanLength = Math.Round(UnitConvert.Micron2Millimeter((double)(scanModel.ScanLength)), 2);
			}
			if (e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_EXPOSURE_DELAY_TIME)) is not null)
			{
				DelayTime = Math.Round(UnitConvert.Microsecond2Second((double)(scanModel.ExposureDelayTime)), 2);
			}
			if (e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_EXPOSURE_INTERVAL_TIME)) is not null)
			{
				ExposureIntervalTime = Math.Round(UnitConvert.Microsecond2Second((double)scanModel.ExposureIntervalTime), 2);
			}
			//int preScanTime = (int)Math.Round(ScanTime, 2) * 100;
			ScanTime = Math.Round(ScanTimeHelper.GetScanTime(scanModel), 2);
			//int postScanTime = (int)Math.Round(ScanTime, 2) * 100;
			//if (preScanTime != postScanTime)
			//{
			//	SetDelayTimeByDefault(DelayTime);
			//}
			if (e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_FOV)) is not null)
			{
				ScanFov = Math.Round(UnitConvert.ReduceThousand((float)scanModel.ScanFOV), 2);
			}
			if (scanModel.Status == PerformStatus.Unperform &&
				!(e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_DOSE_ESTIMATED_DLP)) is not null
					|| e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_DOSE_ESTIMATED_CTDI)) is not null
					|| e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_DOSE_ACCUMULATED_ESTIMATED_DLP)) is not null
					|| e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_DOSE_ACCUMULATED_ESTIMATED_CTDI)) is not null))
			{
				HandlerDoseInfo(scanModel);
			}

			if ((e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION)) is not null
				|| e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_LENGTH)) is not null)
				&& !(scanModel.ScanOption == ScanOption.NVTestBolus || scanModel.ScanOption == ScanOption.TestBolus || scanModel.ScanOption == ScanOption.NVTestBolusBase))
			{
				GetCenterPosition(scanModel);
			}
			if ((scanModel.ScanOption == ScanOption.TestBolus || scanModel.ScanOption == ScanOption.NVTestBolus) && ScanTime > 120)
			{
				_dialogService.ShowDialog(false, MessageLeveles.Info, LanguageResource.Message_Info_Title, "TestBolus maximum scanning time cannot be greater than 120s!", arg => { }, ConsoleSystemHelper.WindowHwnd);
			}
		}
		IsUIChange = true;
	}

	private void InitComboBoxItem()
	{
		int kvMin = SystemConfig.ScanningParamConfig.ScanningParam.AvailableVoltages.Min;
		int kvMax = SystemConfig.ScanningParamConfig.ScanningParam.AvailableVoltages.Max;
		KvValueList = new ObservableCollection<KeyValuePair<int, string>>();
		for (int i = kvMin; i <= kvMax; i = i + 10)
		{
			KvValueList.Add(new KeyValuePair<int, string>(i, i.ToString()));
		}
		ApiStatusList = Controls.Extensions.EnumExtension.EnumToItems(Controls.Extensions.EnumExtension.GetValues(typeof(ApiStatus)));
		TriggerModeList = Controls.Extensions.EnumExtension.EnumToList(typeof(TriggerMode));
		ScanDirectionList = Controls.Extensions.EnumExtension.EnumToList(typeof(TableDirection));
		XRayFocusTypeList = Controls.Extensions.EnumExtension.EnumToList(typeof(FocalType));
		SelectedApi = ApiStatusList[0];
		SelectedScanDirection = ScanDirectionList[0];

		SelectedXRayFocusType = XRayFocusTypeList[0];
		LoopTimeList = GetLoopTimeList();
		SelectedLoopTime = LoopTimeList[0];
	}

	private ObservableCollection<KeyValuePair<double, double>> GetLoopTimeList()
	{
		ObservableCollection<KeyValuePair<double, double>> list = new ObservableCollection<KeyValuePair<double, double>>();
		list.Add(new KeyValuePair<double, double>(0.5, 0.5));
		list.Add(new KeyValuePair<double, double>(1, 1));
		list.Add(new KeyValuePair<double, double>(1.5, 1.5));
		list.Add(new KeyValuePair<double, double>(2, 2));
		list.Add(new KeyValuePair<double, double>(3, 3));
		return list;
	}

	[UIRoute]
	private void SelectScanChanged(object? sender, EventArgs<ScanModel> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		SelectScanChanged(e.Data);
	}

	private void SelectScanChanged(ScanModel scanModel)
	{
		IsUIChange = false;
		CurrentScanModel = scanModel;
		////比较是不是同一个Scan
		if ((CurrentScanModel is not null && !CurrentScanModel.Descriptor.Id.Equals(scanModel.Descriptor.Id)) || CurrentScanModel is null)
		{
			//不是同一个Scan的情况
			SetMarkStatusToDefault();
		}
		SetParameter(scanModel);
		//只要不是未扫描，就是 扫描中，扫描完成都要禁止页面
		PageEnable = scanModel.Status == PerformStatus.Unperform;
		SetLaserStatus(scanModel);
		SetBeginRangeEnable(scanModel);

		int index = scanModel.Parent.Children.FindIndex(t => !string.IsNullOrEmpty(t.Descriptor.Id) && t.Descriptor.Id.Equals(scanModel.Descriptor.Id));
		if (index > 0)
		{
			IsAutoScanNotFirst = true;
		}
		else
		{
			IsAutoScanNotFirst = false;
		}
		IsUIChange = true;
	}

	private void SetBeginRangeEnable(ScanModel scanModel)
	{
		if (scanModel is null)
		{
			return;
		}
		if (scanModel.Status != PerformStatus.Unperform)
		{
			IsBeginRangeEnable = false;
		}
		if (scanModel.Status == PerformStatus.Unperform)
		{
			if (scanModel.ScanOption == ScanOption.Surview || scanModel.ScanOption == ScanOption.Surview)
			{
				if (IsLaserChecked)
				{
					IsBeginRangeEnable = false;
				}
				else
				{
					IsBeginRangeEnable = true;
				}
			}
			else if (scanModel.ScanOption == ScanOption.NVTestBolus)
			{
				IsBeginRangeEnable = false;
			}
			else
			{
				IsBeginRangeEnable = true;
			}
		}
	}

	private void SetLaserStatus(ScanModel scanModel)
	{
		//判断激光定位功能开启逻辑		
		if (scanModel.Status == PerformStatus.Unperform && !(scanModel.ScanOption == ScanOption.Surview || scanModel.ScanOption == ScanOption.DualScout))
		{
			SetLaserChecked(false);
		}
		else if (scanModel.Status != PerformStatus.Unperform && (scanModel.ScanOption == ScanOption.Surview || scanModel.ScanOption == ScanOption.DualScout))
		{
			SetLaserChecked(IsOpenLaser);
		}
		else if (IsOpenLaser && scanModel.Status == PerformStatus.Unperform && (scanModel.ScanOption == ScanOption.Surview || scanModel.ScanOption == ScanOption.DualScout))
		{
			SetLaserChecked(true);
		}
	}

	private void SetLaserChecked(bool isLaser)
	{
		IsUIOpenLaserChange = false;
		IsLaserChecked = isLaser;
		IsUIOpenLaserChange = true;
	}

	private void SetParameter(ScanModel scanModel)
	{
		//TODO:TablePosition没有返回回来赋值
		TextLabel = scanModel.Descriptor.Name;
		ScanLength = Math.Round(UnitConvert.Micron2Millimeter((double)scanModel.ScanLength), 2);
		BeginRange = Math.Round(UnitConvert.Micron2Millimeter((double)scanModel.ReconVolumeStartPosition), 2);
		EndRange = Math.Round(UnitConvert.Micron2Millimeter((double)scanModel.ReconVolumeEndPosition), 2);
		SetBaginRangeByLaser();
		IsUIChange = false;
		Milliampere = scanModel.Milliampere.FirstOrDefault();

		Kilovolt = (int)scanModel.Kilovolt.FirstOrDefault();
		SelectedKv = KvValueList.FirstOrDefault(n => n.Key == Kilovolt);

		ScanOptionString = scanModel.ScanOption.ToString();
		TriggerMode = $"{scanModel.TriggerMode}";
		SelectedTriggerMode = TriggerModeList.FirstOrDefault(n => n.Value == TriggerMode);
		SelectedXRayFocusType = XRayFocusTypeList.FirstOrDefault(t => t.Value.Equals(scanModel.FocalType.ToString()));
		switch (scanModel.TubePositionType)
		{
			case TubePositionType.Lateral:
				LateralPosition = true;
				break;
			case TubePositionType.DualScout:
				DualScoutPosition = true;
				break;
			case TubePositionType.Normal:
				NormalPosition = true;
				break;
			default:
				break;
		}
		SetTubePosition(scanModel);
		Pitch = Math.Round(UnitConvert.ReduceHundred((float)scanModel.Pitch), 2);
		IsVoiceSupported = scanModel.IsVoiceSupported;

		IsPichEnable = scanModel.ScanOption == ScanOption.Helical;

		IsTableFeedEnable = scanModel.ScanOption == ScanOption.Axial
			|| scanModel.ScanOption == ScanOption.NVTestBolus
			|| scanModel.ScanOption == ScanOption.TestBolus
			|| scanModel.ScanOption == ScanOption.NVTestBolusBase
			|| scanModel.ScanOption == ScanOption.BolusTracking;

		if (scanModel.ScanOption == ScanOption.NVTestBolusBase)
		{
			IsIntervelEnable = false;
			IsLoopsEnable = false;
			IsScanNumEnable = false;
			IsBolusRangeStartEnable = true & scanModel.Status == PerformStatus.Unperform;
		}
		else
		{
			IsLoopsEnable = true & PageEnable;
			IsIntervelEnable = true;
			IsScanNumEnable = true & scanModel.Status == PerformStatus.Unperform;
			IsBolusRangeStartEnable = false;
		}

		DelayTime = Math.Round(UnitConvert.Microsecond2Second((double)scanModel.ExposureDelayTime), 2);
		ExposureIntervalTime = Math.Round(UnitConvert.Microsecond2Second((double)scanModel.ExposureIntervalTime), 2);
		TableFeed = Math.Round(UnitConvert.Micron2Millimeter((double)scanModel.TableFeed), 2);
		Loops = (int)scanModel.Loops;
		SelectedScanDirection = ScanDirectionList.FirstOrDefault(n => n.Value == scanModel.TableDirection.ToString());
		ScanTime = Math.Round(ScanTimeHelper.GetScanTime(scanModel), 2);
		ScanFov = Math.Round(UnitConvert.ReduceThousand((float)scanModel.ScanFOV), 2);
		SelectedLoopTime = LoopTimeList.FirstOrDefault(t => t.Key == UnitConvert.Microsecond2Second((int)scanModel.LoopTime));
		RotTime = UnitConvert.Microsecond2Second((double)scanModel.FrameTime) + "*" + scanModel.FramesPerCycle;

		DoseNotificationCTDI = Math.Round(scanModel.DoseNotificationCTDI, 2);
		DoseNotificationDLP = Math.Round(scanModel.DoseNotificationDLP, 2);
		IsEffectiveKVPMaShow = scanModel.Status == PerformStatus.Performed;
		HandlerDoseInfo(scanModel);
		if (!(scanModel.ScanOption == ScanOption.NVTestBolus || scanModel.ScanOption == ScanOption.TestBolus || scanModel.ScanOption == ScanOption.NVTestBolusBase))
		{
			GetCenterPosition(scanModel);
		}

		IsScanDirectionEnable = scanModel.Status == PerformStatus.Unperform & !(scanModel.ScanOption == ScanOption.NVTestBolus
			|| scanModel.ScanOption == ScanOption.TestBolus
			|| scanModel.ScanOption == ScanOption.NVTestBolusBase);

		ExposureTime = scanModel.ExposureTime;
		FrameTime = scanModel.FrameTime;
		FramesPerCycle = scanModel.FramesPerCycle;
		ExposureMode = scanModel.ExposureMode.ToString();
		IsBowtieEnable = scanModel.BowtieEnable;
		Gain = scanModel.Gain.ToString();
		CollimatorSliceWidth = Math.Round(UnitConvert.Micron2Millimeter(scanModel.CollimatorSliceWidth), 2);
		ObjectFOV = Math.Round(UnitConvert.ReduceThousand((float)scanModel.ObjectFOV), 2);
		PostDeleteLength = Math.Round(UnitConvert.Micron2Millimeter((float)scanModel.PostDeleteLength), 2);
		SetPerformedParameters(scanModel, scanModel.Status);
	}

	private void SetPerformedParameters(ScanModel scanModel, PerformStatus performStatus)
	{
		if (performStatus == PerformStatus.Performed)
		{
			EffectiveKv = Math.Round(scanModel.DoseEffectiveKVP, 2).ToString();
			Milliampere = (uint)scanModel.DoseEffectiveMeanMA;
			ScanLength = Math.Round(UnitConvert.Micron2Millimeter((float)scanModel.ActualScanLength), 2);
		}
	}

	private void SetTubePosition(ScanModel scanModel)
	{
		if (scanModel.TubePositions.Count() > 1)
		{
			switch (scanModel.TubePositions[0])
			{
				case TubePosition.Angle0:
					IsTop = true;
					IsBottom = false;
					break;
				case TubePosition.Angle90:
					IsRight = true;
					IsLeft = false;
					break;
				case TubePosition.Angle180:
					IsBottom = true;
					IsTop = false;
					break;
				case TubePosition.Angle270:
					IsLeft = true;
					IsRight = false;
					break;
			}
			switch (scanModel.TubePositions[1])
			{
				case TubePosition.Angle0:
					IsTop = true;
					IsBottom = false;
					break;
				case TubePosition.Angle90:
					IsRight = true;
					IsLeft = false;
					break;
				case TubePosition.Angle180:
					IsBottom = true;
					IsTop = false;
					break;
				case TubePosition.Angle270:
					IsLeft = true;
					IsRight = false;
					break;
			}
		}
	}

	public void OnParameterChanged<T>(string parameterName, T value)
	{
		var vm = _selectionManager.CurrentSelection.Scan;
		if (vm is null || string.IsNullOrEmpty(vm.Descriptor.Id))
		{
			return;
		}
		if (_protocolHostService is null)
		{
			return;
		}
		var pairs = _protocolHostService.Models.FirstOrDefault(s => s.Scan.Descriptor.Id == vm.Descriptor.Id);//vm.ScanId);

		//这几个特殊的值，下参过程中 必须 * 10
		var valueList = new List<string>
			{
				ProtocolParameterNames.SCAN_TABLE_HEIGHT,
				ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION,
				ProtocolParameterNames.SCAN_LENGTH,
				ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION
			};
		if (valueList.Contains(parameterName) && value is not null)
		{
			_protocolHostService.SetParameter(pairs.Scan, parameterName, UnitConvert.Millimeter2Micron(Convert.ToDouble(value)));
		}
		else
		{
			if (parameterName.Equals(ProtocolParameterNames.SCAN_PITCH))
			{
				_protocolHostService.SetParameter(pairs.Scan, parameterName, UnitConvert.ExpandHundred(Convert.ToDouble(value)).ToString(CultureInfo.CurrentCulture));
			}
			else if (parameterName.Equals(ProtocolParameterNames.SCAN_MILLIAMPERE))
			{
				var list = new List<int> { int.Parse(value?.ToString() ?? string.Empty), 0, 0, 0, 0, 0, 0, 0 };
				if (pairs.Scan.ScanOption == ScanOption.DualScout)
				{
					list = new List<int> { int.Parse(value?.ToString() ?? string.Empty), int.Parse(value?.ToString() ?? string.Empty), 0, 0, 0, 0, 0, 0 };
				}
				_protocolHostService.SetParameter(pairs.Scan, parameterName, list);
			}
			else if (parameterName.Equals(ProtocolParameterNames.SCAN_KILOVOLT))
			{
				var list = new List<int> { int.Parse(value?.ToString() ?? string.Empty), 0, 0, 0, 0, 0, 0, 0 };
				if (pairs.Scan.ScanOption == ScanOption.DualScout)
				{
					list = new List<int> { int.Parse(value?.ToString() ?? string.Empty), int.Parse(value?.ToString() ?? string.Empty), 0, 0, 0, 0, 0, 0 };
				}
				_protocolHostService.SetParameter(pairs.Scan, parameterName, list);
			}
			else if (parameterName.Equals(ProtocolParameterNames.SCAN_TUBE_POSITION_TYPE))
			{
				//设置 TubePosition
				_protocolHostService.SetParameter(pairs.Scan, ProtocolParameterNames.SCAN_TUBE_POSITIONS, TubePositions);
				//设置 TubePositionType
				_protocolHostService.SetParameter(pairs.Scan, ProtocolParameterNames.SCAN_TUBE_POSITION_TYPE, value);
			}
			else if (parameterName.Equals(ProtocolParameterNames.SCAN_TABLE_DIRECTION) || parameterName.Equals(ProtocolParameterNames.SCAN_FOCAL_TYPE))
			{
				if (value is KeyValuePair<int, string> item)
				{
					_protocolHostService.SetParameter(pairs.Scan, parameterName, item.Value);
				}
			}
			else
			{
				_protocolHostService.SetParameter(pairs.Scan, parameterName, value);
			}
		}
	}

	private void AppliedToAllProtocols()
	{
		if (IsVoiceSupported && CurrentScanModel is ScanModel scanModel && scanModel.Status == PerformStatus.Unperform)
		{
			if (_voiceAPIConfigWindow is null)
			{
				_voiceAPIConfigWindow = new();
			}

			WindowDialogShow.DialogShow(_voiceAPIConfigWindow);
		}
	}

	private void SettingTubePosition()
	{
		if (DualScoutPosition)
		{
			if (_tubePositionWindow is null)
			{
				_tubePositionWindow = new();
			}
			WindowDialogShow.DialogShow(_tubePositionWindow);
		}
	}

	private void Loaded(object parameter)
	{
		if (CurrentScanModel is ScanModel scan
			&& scan.ScanOption == ScanOption.DualScout)
		{
			OnParameterChanged(ProtocolParameterNames.SCAN_TUBE_POSITIONS, TubePositions);
		}

		if (parameter is Window window)
		{
			window.Hide();
		}
	}

	public void Closed(object parameter)
	{
		if (parameter is Window window)
		{
			window.Hide();
		}
	}

	#region MarkStatus 

	private MarkControlStatus _tablePositionMarkStatus;
	public MarkControlStatus TablePositionMarkStatus
	{
		get => _tablePositionMarkStatus;
		set => SetProperty(ref _tablePositionMarkStatus, value);
	}

	private MarkControlStatus _milliampereMarkStatus;
	public MarkControlStatus MilliampereMarkStatus
	{
		get => _milliampereMarkStatus;
		set => SetProperty(ref _milliampereMarkStatus, value);
	}

	private MarkControlStatus _tableHeightMarkStatus;
	public MarkControlStatus TableHeightMarkStatus
	{
		get => _tableHeightMarkStatus;
		set => SetProperty(ref _tableHeightMarkStatus, value);
	}

	private MarkControlStatus _beginRangeMarkStatus;
	public MarkControlStatus BeginRangeMarkStatus
	{
		get => _beginRangeMarkStatus;
		set => SetProperty(ref _beginRangeMarkStatus, value);
	}

	private MarkControlStatus _scanLengthMarkStatus;
	public MarkControlStatus ScanLengthMarkStatus
	{
		get => _scanLengthMarkStatus;
		set => SetProperty(ref _scanLengthMarkStatus, value);
	}

	private MarkControlStatus _selectedKvMarkStatus;
	public MarkControlStatus SelectedKvMarkStatus
	{
		get => _selectedKvMarkStatus;
		set => SetProperty(ref _selectedKvMarkStatus, value);
	}

	private MarkControlStatus _selectedXRayFocusTypeMarkStatus;
	public MarkControlStatus SelectedXRayFocusTypeMarkStatus
	{
		get => _selectedXRayFocusTypeMarkStatus;
		set => SetProperty(ref _selectedXRayFocusTypeMarkStatus, value);
	}

	private MarkControlStatus _textLabelMarkStatus;
	public MarkControlStatus TextLabelMarkStatus
	{
		get => _textLabelMarkStatus;
		set => SetProperty(ref _textLabelMarkStatus, value);
	}

	private MarkControlStatus _selectedApiMarkStatus;
	public MarkControlStatus SelectedApiMarkStatus
	{
		get => _selectedApiMarkStatus;
		set => SetProperty(ref _selectedApiMarkStatus, value);
	}

	private MarkControlStatus _pitchMarkStatus;
	public MarkControlStatus PitchMarkStatus
	{
		get => _pitchMarkStatus;
		set => SetProperty(ref _pitchMarkStatus, value);
	}

	private MarkControlStatus _selectedTriggerModeMarkStatus;
	public MarkControlStatus SelectedTriggerModeMarkStatus
	{
		get => _selectedTriggerModeMarkStatus;
		set => SetProperty(ref _selectedTriggerModeMarkStatus, value);
	}

	private MarkControlStatus _delayTimeMarkStatus;
	public MarkControlStatus DelayTimeMarkStatus
	{
		get => _delayTimeMarkStatus;
		set => SetProperty(ref _delayTimeMarkStatus, value);
	}

	private MarkControlStatus _exposureIntervalTimeMarkStatus;
	public MarkControlStatus ExposureIntervalTimeMarkStatus
	{
		get => _exposureIntervalTimeMarkStatus;
		set => SetProperty(ref _exposureIntervalTimeMarkStatus, value);
	}

	private MarkControlStatus _selectedScanDirectionMarkStatus;
	public MarkControlStatus SelectedScanDirectionMarkStatus
	{
		get => _selectedScanDirectionMarkStatus;
		set => SetProperty(ref _selectedScanDirectionMarkStatus, value);
	}

	private MarkControlStatus _realBeginRangeMarkStatus;
	public MarkControlStatus RealBeginRangeMarkStatus
	{
		get => _realBeginRangeMarkStatus;
		set => SetProperty(ref _realBeginRangeMarkStatus, value);
	}

	private MarkControlStatus _realEndRangeMarkStatus;
	public MarkControlStatus RealEndRangeMarkStatus
	{
		get => _realEndRangeMarkStatus;
		set => SetProperty(ref _realEndRangeMarkStatus, value);
	}

	private MarkControlStatus _selectedLayerFramesMarkStatus;
	public MarkControlStatus SelectedLayerFramesMarkStatus
	{
		get => _selectedLayerFramesMarkStatus;
		set => SetProperty(ref _selectedLayerFramesMarkStatus, value);
	}

	private MarkControlStatus _tableFeedMarkStatus;
	public MarkControlStatus TableFeedMarkStatus
	{
		get => _tableFeedMarkStatus;
		set => SetProperty(ref _tableFeedMarkStatus, value);
	}

	private MarkControlStatus _selectedScanOptionMarkStatus;
	public MarkControlStatus SelectedScanOptionMarkStatus
	{
		get => _selectedScanOptionMarkStatus;
		set => SetProperty(ref _selectedScanOptionMarkStatus, value);
	}

	private MarkControlStatus _loopsMarkStatus;
	public MarkControlStatus LoopsMarkStatus
	{
		get => _loopsMarkStatus;
		set => SetProperty(ref _loopsMarkStatus, value);
	}

	private MarkControlStatus _loopTimeMarkStatus;
	public MarkControlStatus LoopTimeMarkStatus
	{
		get => _loopTimeMarkStatus;
		set => SetProperty(ref _loopTimeMarkStatus, value);
	}

	private MarkControlStatus _rotTimeMarkStatus;
	public MarkControlStatus RotTimeMarkStatus
	{
		get => _rotTimeMarkStatus;
		set => SetProperty(ref _rotTimeMarkStatus, value);
	}

	private MarkControlStatus _doseNotificationCTDIMarkStatus;
	public MarkControlStatus DoseNotificationCTDIMarkStatus
	{
		get => _doseNotificationCTDIMarkStatus;
		set => SetProperty(ref _doseNotificationCTDIMarkStatus, value);
	}

	private MarkControlStatus _doseNotificationDLPMarkStatus;
	public MarkControlStatus DoseNotificationDLPMarkStatus
	{
		get => _doseNotificationDLPMarkStatus;
		set => SetProperty(ref _doseNotificationDLPMarkStatus, value);
	}
	#endregion

	#region Handle MarkStatus
	private void SetMarkStatusToDefault()
	{
		TablePositionMarkStatus = MarkControlStatus.Default;
		MilliampereMarkStatus = MarkControlStatus.Default;
		TableHeightMarkStatus = MarkControlStatus.Default;
		BeginRangeMarkStatus = MarkControlStatus.Default;
		ScanLengthMarkStatus = MarkControlStatus.Default;
		SelectedKvMarkStatus = MarkControlStatus.Default;
		TextLabelMarkStatus = MarkControlStatus.Default;
		SelectedApiMarkStatus = MarkControlStatus.Default;
		PitchMarkStatus = MarkControlStatus.Default;
		SelectedTriggerModeMarkStatus = MarkControlStatus.Default;
		DelayTimeMarkStatus = MarkControlStatus.Default;
		SelectedScanDirectionMarkStatus = MarkControlStatus.Default;
		RealBeginRangeMarkStatus = MarkControlStatus.Default;
		RealEndRangeMarkStatus = MarkControlStatus.Default;
		SelectedLayerFramesMarkStatus = MarkControlStatus.Default;
		TableFeedMarkStatus = MarkControlStatus.Default;
		SelectedScanOptionMarkStatus = MarkControlStatus.Default;
		SelectedXRayFocusTypeMarkStatus = MarkControlStatus.Default;
		LoopsMarkStatus = MarkControlStatus.Default;
		LoopTimeMarkStatus = MarkControlStatus.Default;
		RotTimeMarkStatus = MarkControlStatus.Default;
		DoseNotificationCTDIMarkStatus = MarkControlStatus.Default;
		DoseNotificationDLPMarkStatus = MarkControlStatus.Default;
		ExposureIntervalTimeMarkStatus = MarkControlStatus.Default;
		ParameterDirty.Keys?.ForEach(item =>
		{
			ParameterDirty[item] = false;
		});
	}

	private void MarkableTextBoxGotFocus(string propName)
	{
		ParameterDirty[propName] = true;
	}
	private void MarkableTextBoxTextChanged(string propName)
	{
		if (ParameterDirty.ContainsKey(propName) && ParameterDirty[propName])
		{
			switch (propName)
			{
				case nameof(Milliampere):
					MilliampereMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(BeginRange):
					BeginRangeMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(ScanLength):
					ScanLengthMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(SelectedKv):
					SelectedKvMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(TextLabel):
					TextLabelMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(SelectedApi):
					SelectedApiMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(Pitch):
					PitchMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(SelectedTriggerMode):
					SelectedTriggerModeMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(DelayTime):
					DelayTimeMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(ExposureIntervalTime):
					ExposureIntervalTimeMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(SelectedScanDirection):
					SelectedScanDirectionMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(SelectedLayerFrames):
					SelectedLayerFramesMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(TableFeed):
					TableFeedMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(SelectedXRayFocusType):
					SelectedXRayFocusTypeMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(Loops):
					LoopsMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(SelectedLoopTime):
					LoopTimeMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(RotTime):
					RotTimeMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(DoseNotificationCTDI):
					DoseNotificationCTDIMarkStatus = MarkControlStatus.Modified;
					break;
				case nameof(DoseNotificationDLP):
					DoseNotificationDLPMarkStatus = MarkControlStatus.Modified;
					break;
			}
		}
	}

	private void MarkableTextBoxLostFocus(string propName)
	{
		if (CurrentScanModel is null)
		{
			return;
		}
		switch (propName)
		{
			case ProtocolParameterNames.SCAN_LENGTH:
				ScanLengthHelper.GetCorrectedScanLength(_protocolHostService, CurrentScanModel, ScanLength);
				break;
			case "Pitch":
				OnParameterChanged(ProtocolParameterNames.SCAN_PITCH, Pitch);
				break;
		}
	}

	#endregion

	#region 属性 
	private bool _isLaserChecked = true;
	public bool IsLaserChecked
	{
		get => _isLaserChecked;
		set
		{
			SetProperty(ref _isLaserChecked, value);
			if (CurrentScanModel is ScanModel scan)
			{
				SetBeginRangeEnable(scan);
				if (value && scan.Status == PerformStatus.Unperform && !(scan.ScanOption == ScanOption.NVTestBolus || scan.ScanOption == ScanOption.TestBolus))
				{
					SetRangeByLaserChecked();
				}
			}
			if (IsUIOpenLaserChange)
			{
				IsOpenLaser = value;
			}
		}
	}

	private void SetRangeByLaserChecked()
	{
		LaserInfo node = SystemConfig.LaserConfig.Laser;
		OnParameterChanged(ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, UnitConvert.Micron2Millimeter((double)(RealTimelTablePosition - node.Offset.Value)));
		CalculateEndRange();
	}

	private bool _isBeginRangeEnable = true;
	public bool IsBeginRangeEnable
	{
		get => _isBeginRangeEnable;
		set => SetProperty(ref _isBeginRangeEnable, value);
	}

	private bool _normal = false;
	public bool NormalPosition
	{
		get => _normal;
		set
		{
			if (SetProperty(ref _normal, value) && IsUIChange)
			{
				TubePositionType = TubePositionType.Normal;
			}
		}
	}

	private bool _lateral = false;
	public bool LateralPosition
	{
		get => _lateral;
		set
		{
			if (SetProperty(ref _lateral, value) && IsUIChange)
			{
				TubePositionType = TubePositionType.Lateral;
			}
		}
	}

	private bool _dualScout = false;
	public bool DualScoutPosition
	{
		get => _dualScout;
		set
		{
			if (SetProperty(ref _dualScout, value) && IsUIChange)
			{
				TubePositionType = TubePositionType.DualScout;
			}
		}
	}

	private TubePositionType _tubePositionType = TubePositionType.Normal;

	public TubePositionType TubePositionType
	{
		get => _tubePositionType;
		set
		{
			if (SetProperty(ref _tubePositionType, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.SCAN_TUBE_POSITION_TYPE, value);
			}
		}
	}

	#region TubePositions
	private bool _isTop = true;
	public bool IsTop
	{
		get => _isTop;
		set
		{
			if (SetProperty(ref _isTop, value) && IsUIChange && value)
			{
				TubePositions[0] = TubePosition.Angle0;
			}
		}
	}

	private bool _isBottom = false;
	public bool IsBottom
	{
		get => _isBottom;
		set
		{
			if (SetProperty(ref _isBottom, value) && IsUIChange && value)
			{
				TubePositions[0] = TubePosition.Angle180;
			}
		}
	}

	private bool _isRight = false;
	public bool IsRight
	{
		get => _isRight;
		set
		{
			if (SetProperty(ref _isRight, value) && IsUIChange && value)
			{
				TubePositions[1] = TubePosition.Angle90;
			}
		}
	}

	private bool _isLeft = true;
	public bool IsLeft
	{
		get => _isLeft;
		set
		{
			if (SetProperty(ref _isLeft, value) && IsUIChange && value)
			{
				TubePositions[1] = TubePosition.Angle270;
			}
		}
	}

	private TubePosition[] _tubePositions = new TubePosition[2] { TubePosition.Angle0, TubePosition.Angle270 };

	public TubePosition[] TubePositions
	{
		get => _tubePositions;
		set
		{
			SetProperty(ref _tubePositions, value);
		}
	}
	#endregion

	private double _milliampere;
	public double Milliampere
	{
		get => _milliampere;
		set
		{
			if (SetProperty(ref _milliampere, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.SCAN_MILLIAMPERE, (uint)value);
			}
		}
	}

	#region Kv

	/// <summary>
	/// 绑定列表
	/// </summary>
	private ObservableCollection<KeyValuePair<int, string>> _kvValueList = new();
	public ObservableCollection<KeyValuePair<int, string>> KvValueList
	{
		get => _kvValueList;
		set => SetProperty(ref _kvValueList, value);
	}

	/// <summary>
	/// 选中属性
	/// </summary>
	private KeyValuePair<int, string> _selectedKv;
	public KeyValuePair<int, string> SelectedKv
	{
		get => _selectedKv;
		set
		{
			Kilovolt = value.Key;
			SetProperty(ref _selectedKv, value);

			EffectiveKv = value.Value;
		}
	}

	private bool _isEffectiveKVPMaShow = false;
	public bool IsEffectiveKVPMaShow
	{
		get => _isEffectiveKVPMaShow;
		set
		{
			SetProperty(ref _isEffectiveKVPMaShow, value);
		}
	}

	private string _effectiveKv = string.Empty;
	public string EffectiveKv
	{
		get => _effectiveKv;
		set => SetProperty(ref _effectiveKv, value);
	}

	/// <summary>
	/// 下参属性
	/// </summary>
	private int _kilovolt;
	public int Kilovolt
	{
		get => _kilovolt;
		set
		{
			if (SetProperty(ref _kilovolt, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.SCAN_KILOVOLT, value);
			}
		}
	}
	#endregion

	private ObservableCollection<KeyValuePair<int, string>> _xRayFocusTypeList = new();
	public ObservableCollection<KeyValuePair<int, string>> XRayFocusTypeList
	{
		get => _xRayFocusTypeList;
		set => SetProperty(ref _xRayFocusTypeList, value);
	}

	/// <summary>
	/// 选中属性
	/// </summary>
	private KeyValuePair<int, string> _selectedXRayFocusType;
	public KeyValuePair<int, string> SelectedXRayFocusType
	{
		get => _selectedXRayFocusType;
		set
		{
			if (SetProperty(ref _selectedXRayFocusType, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.SCAN_FOCAL_TYPE, value);
			}
		}
	}

	/// <summary>
	/// 下参属性
	/// </summary>
	private string _scanOptionString = string.Empty;
	public string ScanOptionString
	{
		get => _scanOptionString;
		set => SetProperty(ref _scanOptionString, value);
	}

	private double _beginRange;
	public double BeginRange
	{
		get => _beginRange;
		set
		{
			if (SetProperty(ref _beginRange, value) && IsUIChange && CurrentScanModel is ScanModel scan)
			{
				OnParameterChanged(ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION, value);
				switch (scan.ScanOption)     //增强扫描的上面长度等于0，起点位置等于结束点位置
				{
					case ScanOption.NVTestBolus:
					case ScanOption.NVTestBolusBase:
					case ScanOption.TestBolus:
					case ScanOption.BolusTracking:
						CalculateBolusEndRange();
						break;
					default:
						CalculateEndRange();
						break;
				}
			}
		}
	}

	private double _endRange;
	public double EndRange
	{
		get => _endRange;
		set
		{
			if (SetProperty(ref _endRange, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION, value);
			}
		}
	}

	private double _scanLength;
	/// <summary>
	/// 根据采集卡图像携带的床码信息计算 单位：毫米（mm）
	/// </summary>
	public double ScanLength
	{
		get => _scanLength;
		set
		{
			if (SetProperty(ref _scanLength, value) && IsUIChange && CurrentScanModel is ScanModel scan)
			{
				ScanLengthHelper.GetCorrectedScanLength(_protocolHostService, scan, value);
			}
		}
	}

	private double _scanTime;
	/// <summary>
	/// 曝光时长 us 微秒
	/// </summary>
	public double ScanTime
	{
		get => _scanTime;
		set
		{
			SetProperty(ref _scanTime, value);
		}
	}

	private double _scanFov;
	/// <summary>
	/// ScanFov
	/// </summary>
	public double ScanFov
	{
		get => _scanFov;
		set
		{
			SetProperty(ref _scanFov, value);
		}
	}

	public void CalculateEndRange()
	{
		if (SelectedScanDirection.Key == (int)TableDirection.In)
		{
			EndRange = BeginRange - ScanLength;
		}
		else
		{
			EndRange = BeginRange + ScanLength;
		}
	}

	public void CalculateBolusEndRange()
	{
		EndRange = BeginRange;
		if (CurrentScanModel is not null && CurrentScanModel.Status == PerformStatus.Unperform)
		{
			_protocolHostService.SetParameter(CurrentScanModel, ProtocolParameterNames.SCAN_LENGTH, 0);
		}
	}

	public void GetCenterPosition(ScanModel scanModel)
	{
		var start = scanModel.ReconVolumeStartPosition;
		var length = (int)scanModel.ScanLength;
		var center = scanModel.TableDirection == TableDirection.In ? start - length / 2 : start + length / 2;
		CenterPosition = UnitConvert.Micron2Millimeter((double)center).ToString();
	}

	#region General
	private string _textLabel = string.Empty;
	public string TextLabel
	{
		get => _textLabel;
		set
		{
			if (SetProperty(ref _textLabel, value)
				&& IsUIChange
				&& CurrentScanModel is not null
				&& CurrentScanModel.Status == PerformStatus.Unperform)
			{
				_protocolHostService.SetModelName(CurrentScanModel, value);
			}
		}
	}

	private ObservableCollection<KeyValuePair<int, string>> _apiStatusList = new();
	public ObservableCollection<KeyValuePair<int, string>> ApiStatusList
	{
		get => _apiStatusList;
		set => SetProperty(ref _apiStatusList, value);
	}

	private KeyValuePair<int, string> _selectedApi;
	public KeyValuePair<int, string> SelectedApi
	{
		get => _selectedApi;
		set => SetProperty(ref _selectedApi, value);
	}

	private ApiStatus _api = ApiStatus.No;
	public ApiStatus Api
	{
		get => (ApiStatus)SelectedApi.Key;
		set
		{
			SelectedApi = ApiStatusList.FirstOrDefault(t => t.Key == (int)value);
			SetProperty(ref _api, value);
		}
	}

	private bool _isPichEnable = false;
	public bool IsPichEnable
	{
		get => _isPichEnable;
		set => SetProperty(ref _isPichEnable, value);
	}

	private bool _isTableFeedEnable = false;
	public bool IsTableFeedEnable
	{
		get => _isTableFeedEnable;
		set => SetProperty(ref _isTableFeedEnable, value);
	}

	private double _pitch;
	public double Pitch
	{
		get => _pitch;
		set
		{
			if (SetProperty(ref _pitch, value) && IsUIChange && CurrentScanModel is ScanModel scan)
			{
				_protocolHostService.SetParameter(scan, ProtocolParameterNames.SCAN_PITCH, UnitConvert.ExpandHundred(Convert.ToDouble(value)).ToString(CultureInfo.CurrentCulture));
				if (scan.ScanOption == ScanOption.Helical)  //仅当扫描类型为螺旋扫描任务时，扫描长度才有用
				{
					ScanLengthHelper.GetCorrectedScanLength(_protocolHostService, scan, UnitConvert.Micron2Millimeter(scan.ScanLength));
				}
			}
		}
	}

	private bool _isVoiceSupported = false;

	public bool IsVoiceSupported
	{
		get => _isVoiceSupported;
		set
		{
			if (SetProperty(ref _isVoiceSupported, value) && IsUIChange)
			{
				SetIsVoiceSupportedProperty(value);
			}
		}
	}

	private void SetIsVoiceSupportedProperty(bool IsVoiceSupported)
	{
		if (CurrentScanModel is not ScanModel || (CurrentScanModel is ScanModel scan && scan.Status != PerformStatus.Unperform))
		{
			return;
		}
		List<ParameterModel> parameterModels = new List<ParameterModel>();
		parameterModels.Add(new ParameterModel
		{
			Name = ProtocolParameterNames.SCAN_IS_VOICE_SUPPORTED,
			Value = IsVoiceSupported.ToString(CultureInfo.InvariantCulture)
		});
		if (IsVoiceSupported)
		{
			parameterModels.AddRange(GetIsVoiceSupportedParters());
		}
		else
		{
			parameterModels.AddRange(GetNotVoiceSupportedParters());
		}
		if (CurrentScanModel is ScanModel scanModel && scanModel.Status == PerformStatus.Unperform && parameterModels.Count > 0)
		{
			_protocolHostService.SetParameters(scanModel, parameterModels);

			ExposureDelayTimeHelper.CorrectDelayTimeMeasurement(_protocolHostService, _voiceService, scanModel.Descriptor.Id);
		}
	}

	private List<ParameterModel> GetIsVoiceSupportedParters()
	{
		List<ParameterModel> parameterModels = new List<ParameterModel>();
		var list = _voiceService.GetDefaultList();
		if (list is not null && list.Count > 0 && CurrentScanModel is ScanModel scan)
		{
			var preD = list.FirstOrDefault(t => t.IsFront);
			if (preD is not null)
			{
				parameterModels.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_PRE_VOICE_DELAY_TIME,
					Value = (0).ToString(CultureInfo.InvariantCulture)
				});
				parameterModels.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_PRE_VOICE_PLAY_TIME,
					Value = UnitConvert.Second2Microsecond((int)preD.VoiceLength).ToString(CultureInfo.InvariantCulture)
				});
				parameterModels.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_PRE_VOICE_ID,
					Value = preD.InternalId.ToString(CultureInfo.InvariantCulture)
				});
			}
			var postD = list.FirstOrDefault(t => !t.IsFront);
			if (postD is not null)
			{
				parameterModels.Add(new ParameterModel
				{
					Name = ProtocolParameterNames.SCAN_POST_VOICE_ID,
					Value = postD.InternalId.ToString(CultureInfo.InvariantCulture)
				});
			}
		}
		return parameterModels;
	}

	private List<ParameterModel> GetNotVoiceSupportedParters()
	{
		List<ParameterModel> parameterModels = new List<ParameterModel>();
		parameterModels.Add(new ParameterModel
		{
			Name = ProtocolParameterNames.SCAN_PRE_VOICE_DELAY_TIME,
			Value = 0.ToString(CultureInfo.InvariantCulture)
		});
		parameterModels.Add(new ParameterModel
		{
			Name = ProtocolParameterNames.SCAN_PRE_VOICE_PLAY_TIME,
			Value = 0.ToString(CultureInfo.InvariantCulture)
		});
		parameterModels.Add(new ParameterModel
		{
			Name = ProtocolParameterNames.SCAN_POST_VOICE_ID,
			Value = 0.ToString(CultureInfo.InvariantCulture)
		});
		parameterModels.Add(new ParameterModel
		{
			Name = ProtocolParameterNames.SCAN_PRE_VOICE_ID,
			Value = 0.ToString(CultureInfo.InvariantCulture)
		});
		return parameterModels;
	}

	#region Dose info
	private double _doseNotificationCTDI;
	public double DoseNotificationCTDI
	{
		get => _doseNotificationCTDI;
		set
		{
			if (SetProperty(ref _doseNotificationCTDI, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_CTDI, value);
			}
		}
	}

	private double _doseNotificationDLP;
	public double DoseNotificationDLP
	{
		get => _doseNotificationDLP;
		set
		{
			if (SetProperty(ref _doseNotificationDLP, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.SCAN_DOSE_NOTIFICATION_DLP, value);
			}
		}
	}

	private bool _isCTDIEnabled = true;
	public bool IsCTDIEnabled
	{
		get => _isCTDIEnabled;
		set
		{
			SetProperty(ref _isCTDIEnabled, value);
		}
	}

	private bool _isCTDIWarning = true;
	public bool IsCTDIWarning
	{
		get => _isCTDIWarning;
		set
		{
			SetProperty(ref _isCTDIWarning, value);
		}
	}

	public BitmapImage CTDIImage
	{
		get
		{
			if (IsCTDIWarning)
			{
				return DoseWarningImage;
			}
			else
			{
				return DoseErrorImage;
			}
		}
	}

	private string _cTDI = "0.00";
	public string CTDI
	{
		get => _cTDI;
		set => SetProperty(ref _cTDI, value);
	}

	private string _cTDIs = "0.00";
	public string CTDIs
	{
		get => _cTDIs;
		set => SetProperty(ref _cTDIs, value);
	}

	private bool _isDLPEnabled = true;
	public bool IsDLPEnabled
	{
		get => _isDLPEnabled;
		set => SetProperty(ref _isDLPEnabled, value);
	}

	private bool _isDLPWarning = true;
	public bool IsDLPWarning
	{
		get => _isDLPWarning;
		set => SetProperty(ref _isDLPWarning, value);
	}

	public BitmapImage DLPImage
	{
		get
		{
			if (IsDLPWarning)
			{
				return DoseWarningImage;
			}
			else
			{
				return DoseErrorImage;
			}
		}
	}

	private BitmapImage DoseErrorImage
	{
		get => new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/doseerror.png", UriKind.RelativeOrAbsolute));
	}

	private BitmapImage DoseWarningImage
	{
		get => new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/dosewarning.png", UriKind.RelativeOrAbsolute));
	}

	private string _dLP = "0.00";
	/// <summary>
	/// 描述：DLP 来源：CTDI* 扫描长度
	/// </summary>
	public string DLP
	{
		get => _dLP;
		set => SetProperty(ref _dLP, value);
	}

	private string _dLPs = "0.00";
	/// <summary>
	/// 描述：DLP 来源：CTDI* 扫描长度
	/// </summary>
	public string DLPs
	{
		get => _dLPs;
		set => SetProperty(ref _dLPs, value);
	}

	private void HandlerDoseInfo(ScanModel scanModel)
	{
		if (scanModel is null)
		{
			return;
		}
		if (scanModel.Status == PerformStatus.Unperform || scanModel.Status == PerformStatus.Performed)
		{
			InitDoseInfo();
		}
		if (scanModel.Status == PerformStatus.Unperform)
		{
			ScanDoseCheckHelper.GetDoseEstimatedInfoByUnperformScan(_doseEstimateService, _protocolHostService, scanModel);
			HandlerDoseInfoUnperform(scanModel);
		}
		if (scanModel.Status == PerformStatus.Performed)
		{
			HandlerDoseInfoPerformed(scanModel);
		}
	}

	private void InitDoseInfo()
	{
		string str = 0.ToString();
		CTDI = str;
		CTDIs = str;
		DLP = str;
		DLPs = str;

		IsCTDIEnabled = true;
		IsCTDIWarning = false;

		IsDLPEnabled = true;
		IsDLPWarning = false;
	}

	private void HandlerDoseInfoUnperform(ScanModel scanModel)
	{
		if (scanModel.Status != PerformStatus.Unperform)
		{
			return;
		}
		CTDI = Extensions.Extensions.GetMaxFixedLengthStrings(Math.Round(scanModel.DoseEstimatedCTDI, 2).ToString(), 6);
		CTDIs = Extensions.Extensions.GetMaxFixedLengthStrings(Math.Round(scanModel.AccumulatedDoseEstimatedCTDI, 2).ToString(), 6);

		DLP = Extensions.Extensions.GetMaxFixedLengthStrings(Math.Round(scanModel.DoseEstimatedDLP, 2).ToString(), 6);
		DLPs = Extensions.Extensions.GetMaxFixedLengthStrings(Math.Round(scanModel.AccumulatedDoseEstimatedDLP, 2).ToString(), 6);
		HandlerDoseImage(scanModel, scanModel.DoseEstimatedCTDI, scanModel.AccumulatedDoseEstimatedCTDI, scanModel.DoseEstimatedDLP, scanModel.AccumulatedDoseEstimatedDLP);
	}

	private void HandlerDoseInfoPerformed(ScanModel scanModel)
	{
		if (scanModel.Status != PerformStatus.Performed)
		{
			return;
		}
		CTDI = Extensions.Extensions.GetMaxFixedLengthStrings(Math.Round(scanModel.DoseEffectiveCTDI, 2).ToString(), 6);
		float cdtis = ScanDoseCheckHelper.GetEffectiveCTDIsByScan(_protocolHostService, scanModel);
		CTDIs = Extensions.Extensions.GetMaxFixedLengthStrings(Math.Round(cdtis, 2).ToString(), 6);

		DLP = Extensions.Extensions.GetMaxFixedLengthStrings(Math.Round(scanModel.DoseEffectiveDLP, 2).ToString(), 6);
		float dlps = ScanDoseCheckHelper.GetEffectiveDLPsByScan(_protocolHostService, scanModel);
		DLPs = Extensions.Extensions.GetMaxFixedLengthStrings(Math.Round(dlps, 2).ToString(), 6);
		HandlerDoseImage(scanModel, scanModel.DoseEffectiveCTDI, cdtis, scanModel.DoseEffectiveDLP, dlps);
	}

	private void HandlerDoseImage(ScanModel scanModel, float ctdi, float ctdis, float dlp, float dlps)
	{
		if (scanModel.ScanOption == ScanOption.Surview || scanModel.ScanOption == ScanOption.DualScout)
		{
			return;
		}
		if (IsDoseErrorNotice
			&& scanModel.DoseNotificationCTDI > 0
			&& ctdi > scanModel.DoseNotificationCTDI)
		{
			IsCTDIEnabled = false;
			IsCTDIWarning = true;
		}
		if (IsDoseErrorNotice
			&& scanModel.DoseNotificationDLP > 0
			&& dlp > scanModel.DoseNotificationDLP)
		{
			IsDLPEnabled = false;
			IsDLPWarning = true;
		}
		if (SettingCTDI > 0
			&& ctdis > SettingCTDI)
		{
			IsCTDIEnabled = false;
			IsCTDIWarning = false;
		}
		if (SettingDLP > 0
			&& dlps > SettingDLP)
		{
			IsDLPEnabled = false;
			IsDLPWarning = false;
		}
	}
	#endregion

	private string _centerPosition = string.Empty;
	public string CenterPosition
	{
		get => _centerPosition;
		set => SetProperty(ref _centerPosition, value);
	}

	/// <summary>
	/// 绑定列表
	/// </summary>
	private ObservableCollection<KeyValuePair<int, string>> _layerFramesValueList = new();
	public ObservableCollection<KeyValuePair<int, string>> LayerFramesValueList
	{
		get => _layerFramesValueList;
		set => SetProperty(ref _layerFramesValueList, value);
	}

	/// <summary>
	/// 选中属性
	/// </summary>
	private KeyValuePair<int, string> _selectedLayerFrames;
	public KeyValuePair<int, string> SelectedLayerFrames
	{
		get => _selectedLayerFrames;
		set
		{
			SetProperty(ref _selectedLayerFrames, value);
		}
	}

	private bool _isLoopsEnable = true;
	public bool IsLoopsEnable
	{
		get => _isLoopsEnable;
		set
		{
			SetProperty(ref _isLoopsEnable, value);
		}
	}

	private bool _isBolusRangeStartEnable = true;
	public bool IsBolusRangeStartEnable
	{
		get => _isBolusRangeStartEnable;
		set
		{
			SetProperty(ref _isBolusRangeStartEnable, value);
		}
	}

	private bool _isScanNumEnable = true;
	public bool IsScanNumEnable
	{
		get => _isScanNumEnable;
		set
		{
			SetProperty(ref _isScanNumEnable, value);
		}
	}

	private double _loops = 0;
	public double Loops
	{
		get => _loops;
		set
		{
			if (SetProperty(ref _loops, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.SCAN_LOOPS, (int)value);
			}
		}
	}

	private ObservableCollection<KeyValuePair<double, double>> _loopTimeList = new();
	public ObservableCollection<KeyValuePair<double, double>> LoopTimeList
	{
		get => _loopTimeList;
		set => SetProperty(ref _loopTimeList, value);
	}

	/// <summary>
	/// 选中属性
	/// </summary>
	private KeyValuePair<double, double> _selectedLoopTime;
	public KeyValuePair<double, double> SelectedLoopTime
	{
		get => _selectedLoopTime;
		set
		{
			if (SetProperty(ref _selectedLoopTime, value) && IsUIChange && CurrentScanModel is not null)
			{
				_protocolHostService.SetParameter(CurrentScanModel, ProtocolParameterNames.SCAN_LOOP_TIME, UnitConvert.Second2Microsecond(value.Value));
			}
		}
	}

	/// <summary>
	/// 选中属性
	/// </summary>
	private string _rotTime = string.Empty;
	public string RotTime
	{
		get => _rotTime;
		set => SetProperty(ref _rotTime, value);
	}
	#endregion

	#region TriggerMode

	private ObservableCollection<KeyValuePair<int, string>> _triggerModeList = new();
	public ObservableCollection<KeyValuePair<int, string>> TriggerModeList
	{
		get => _triggerModeList;
		set => SetProperty(ref _triggerModeList, value);
	}

	private KeyValuePair<int, string> _selectedTriggerMode;
	public KeyValuePair<int, string> SelectedTriggerMode
	{
		get => _selectedTriggerMode;
		set
		{
			TriggerMode = value.Value;
			SetProperty(ref _selectedTriggerMode, value);
		}
	}

	private string _triggerMode = string.Empty;
	public string TriggerMode
	{
		get
		{
			return _triggerMode;
		}
		set
		{
			if (SetProperty(ref _triggerMode, value) && IsUIChange && CurrentScanModel is not null)
			{
				_protocolHostService.SetParameter(CurrentScanModel, ProtocolParameterNames.SCAN_TRIGGER_MODE, value);
			}
		}
	}

	#endregion

	#region ScanDirection 

	private ObservableCollection<KeyValuePair<int, string>> _scanDirectionList = new();
	public ObservableCollection<KeyValuePair<int, string>> ScanDirectionList
	{
		get => _scanDirectionList;
		set => SetProperty(ref _scanDirectionList, value);
	}

	private bool _isScanDirectionEnable = false;
	public bool IsScanDirectionEnable
	{
		get => _isScanDirectionEnable;
		set => SetProperty(ref _isScanDirectionEnable, value);
	}

	private KeyValuePair<int, string> _selectedScanDirection;
	public KeyValuePair<int, string> SelectedScanDirection
	{
		get => _selectedScanDirection;
		set
		{
			if (SetProperty(ref _selectedScanDirection, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.SCAN_TABLE_DIRECTION, value);
			}
		}
	}
	#endregion

	#region Currently Not Used	
	private uint _exposureTime;
	public uint ExposureTime
	{
		get => _exposureTime;
		set => SetProperty(ref _exposureTime, value);
	}

	private uint _frameTime;
	public uint FrameTime
	{
		get => _frameTime;
		set => SetProperty(ref _frameTime, value);
	}

	private uint _framesPerCycle;
	public uint FramesPerCycle
	{
		get => _framesPerCycle;
		set => SetProperty(ref _framesPerCycle, value);
	}

	private uint _cycles;
	public uint Cycles
	{
		get => _cycles;
		set => SetProperty(ref _cycles, value);
	}

	private double _delayTime;
	public double DelayTime
	{
		get => _delayTime;
		set
		{
			if (SetProperty(ref _delayTime, value) && IsUIChange)
			{
				SetDelayTimeByDefault(value);
			}
		}
	}

	private void SetDelayTimeByDefault(double time)
	{
		if (CurrentScanModel is not ScanModel || (CurrentScanModel is ScanModel cuuentScan && cuuentScan.Status != PerformStatus.Unperform))
		{
			return;
		}
		int delayTime = UnitConvert.Second2Microsecond(time);
		if (CurrentScanModel is ScanModel scanModel && CurrentScanModel.Status == PerformStatus.Unperform)
		{
			List<ParameterModel> parameterModels = new List<ParameterModel>();
			parameterModels.Add(new ParameterModel
			{
				Name = ProtocolParameterNames.SCAN_EXPOSURE_DELAY_TIME,
				Value = (delayTime).ToString(CultureInfo.InvariantCulture)
			});
			_protocolHostService.SetParameters(scanModel, parameterModels);

			ExposureDelayTimeHelper.CorrectDelayTimeMeasurement(_protocolHostService, _voiceService, scanModel.Descriptor.Id);
		}
	}

	private bool _isAutoScanNotFirst = false;
	public bool IsAutoScanNotFirst
	{
		get => _isAutoScanNotFirst;
		set
		{
			SetProperty(ref _isAutoScanNotFirst, value);
		}
	}

	private double _exposureIntervalTime;
	public double ExposureIntervalTime
	{
		get => _exposureIntervalTime;
		set
		{
			if (SetProperty(ref _exposureIntervalTime, value) && IsUIChange && CurrentScanModel is ScanModel scan)
			{
				_protocolHostService.SetParameter(scan, ProtocolParameterNames.SCAN_EXPOSURE_INTERVAL_TIME, UnitConvert.Second2Microsecond(value));

				ExposureDelayTimeHelper.CorrectDelayTimeMeasurement(_protocolHostService, _voiceService, scan.Descriptor.Id);
			}
		}
	}

	private double _tableFeed;
	public double TableFeed
	{
		get => _tableFeed;
		set => SetProperty(ref _tableFeed, value);
	}

	private string _exposureMode;
	public string ExposureMode
	{
		get => _exposureMode;
		set => SetProperty(ref _exposureMode, value);
	}

	private bool _isBowtieEnable = true;
	public bool IsBowtieEnable
	{
		get => _isBowtieEnable;
		set
		{
			if (SetProperty(ref _isBowtieEnable, value) && IsUIChange && CurrentScanModel is ScanModel scan)
			{
				_protocolHostService.SetParameter(scan, ProtocolParameterNames.SCAN_BOWTIE_ENABLE, value);
			}
		}
	}

	private string _gain;
	public string Gain
	{
		get => _gain;
		set => SetProperty(ref _gain, value);
	}

	private double _collimatorSliceWidth;
	public double CollimatorSliceWidth
	{
		get => _collimatorSliceWidth;
		set => SetProperty(ref _collimatorSliceWidth, value);
	}

	private double _objectFOV;
	public double ObjectFOV
	{
		get => _objectFOV;
		set => SetProperty(ref _objectFOV, value);
	}

	private double _postDeleteLength;
	public double PostDeleteLength
	{
		get => _postDeleteLength;
		set => SetProperty(ref _postDeleteLength, value);
	}

	#endregion

	#endregion
	private bool _isIntervelEnable = false;
	public bool IsIntervelEnable
	{
		get => _isIntervelEnable;
		set => SetProperty(ref _isIntervelEnable, value);
	}
}