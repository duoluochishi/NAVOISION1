//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using CommunityToolkit.HighPerformance;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.PostProcessEnums;
using NV.CT.FacadeProxy.Common.Enums.ReconEnums;
using NV.CT.UI.Exam.Extensions;
using NV.MPS.Configuration;
using NV.MPS.Environment;
using Type = System.Type;
using NV.CT.Alg.ScanReconCalculation.Recon.FovMatrix;

namespace NV.CT.UI.Exam.ViewModel;

public class ReconParameterViewModel : BaseViewModel
{
	private readonly ILogger<ReconParameterViewModel> _logger;
	private readonly ISelectionManager _selectionManager;
	private readonly IProtocolHostService _protocolHostService;
	private readonly IUIRelatedStatusService _uIRelatedStatusService;
	/// <summary>
	/// 当前的ReconModel
	/// </summary>
	private ReconModel? CurrentReconModel { get; set; }
	private Dictionary<string, bool> ParameterDirty { get; set; } = new();
	List<WindowingInfo> WindowWidthLevelModels = new();
	private bool IsUIChange = true;
	private bool _reconPageEnable = true;

	/// <summary>
	/// Recon页面是否可用 单独为Recon页面定义的
	/// </summary>
	public bool ReconPageEnable
	{
		get => _reconPageEnable;
		set
		{
			if (SetProperty(ref _reconPageEnable, value))
			{
				if (AppDomain.CurrentDomain.FriendlyName.Contains(ApplicationParameterNames.PROCESSNAME_EXAMINATION))
				{
					IsAutoReconEnabled = value;
					if (value && CurrentReconModel is ReconModel recon)
					{
						IsAutoReconEnabled = recon.Parent.Status == PerformStatus.Unperform;
					}
				}
				else
				{
					IsAutoReconEnabled = false;
				}
			}
		}
	}

	public ReconParameterViewModel(ISelectionManager selectionManager,
		IProtocolHostService protocolHostService,
		IUIRelatedStatusService uiRelatedStatusService,
		ILogger<ReconParameterViewModel> logger)
	{
		_logger = logger;
		_selectionManager = selectionManager;
		_protocolHostService = protocolHostService;
		//不需要订阅SelectionChange事件，SelectionReconChanged事件即可收到所有信息
		_selectionManager.SelectionReconChanged -= SelectionManager_SelectionReconChanged;
		_selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;
		_selectionManager.SelectionScanChanged -= SelectScanChanged;
		_selectionManager.SelectionScanChanged += SelectScanChanged;

		_uIRelatedStatusService = uiRelatedStatusService;
		_uIRelatedStatusService.RealtimeStatusChanged -= RealtimeStatusChanged;
		_uIRelatedStatusService.RealtimeStatusChanged += RealtimeStatusChanged;

		_protocolHostService.PerformStatusChanged -= ProtocolPerformStatusService_PerformStatusChanged;
		_protocolHostService.PerformStatusChanged += ProtocolPerformStatusService_PerformStatusChanged;
		Commands.Add(CommandParameters.COMMAND_MARKABLE_TEXT_BOX_GOT_FOCUS, new DelegateCommand<string>(MarkableTextBoxGotFocus, _ => true));
		Commands.Add(CommandParameters.COMMAND_MARKABLE_TEXT_BOX_TEXT_CHANGED, new DelegateCommand<string>(MarkableTextBoxTextChanged, _ => true));

		Commands.Add(CommandParameters.COMMAND_ADD, new DelegateCommand<object>(PostProcessItemAdded, _ => true));
		Commands.Add(CommandParameters.COMMAND_REMOVE, new DelegateCommand<string>(PostProcessItemRemoved, _ => true));
		Commands.Add(CommandParameters.COMMAND_SELECT, new DelegateCommand<object>(OnPostProcessItemSelected, _ => true));
		Commands.Add(CommandParameters.COMMAND_SHOW, new DelegateCommand(ShowPostProcessSettingWindow, () => true));
		Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand<object>(PostProcessSettingWindowClosed, _ => true));
		Commands.Add(CommandParameters.COMMAND_CONFIRM_POSTPROCESS, new DelegateCommand<object>(PostProcessSettingConfirmed, _ => true));

		WindowWidthLevelModels = UserConfig.WindowingConfig.Windowings;
		_protocolHostService.ParameterChanged -= ProtocolModificationService_ParameterChanged;
		_protocolHostService.ParameterChanged += ProtocolModificationService_ParameterChanged;
		IsUIChange = false;
		InitComboBoxItem();
		LoadSelectedParameters();
		IsUIChange = true;
	}

	/// <summary>
	/// 解决扫描和重建控件第一次加载，SelectionManager change事件已经发送完毕，收不到事件变更处理的问题
	/// </summary> 
	private void LoadSelectedParameters()
	{
		var selectedRecon = _selectionManager.CurrentSelectionRecon;
		if (selectedRecon is null)
			return;

		SelectReconChanged(selectedRecon);
	}

	[UIRoute]
	private void ProtocolModificationService_ParameterChanged(object? sender, EventArgs<(BaseModel, List<string>)> e)
	{
		if (e is null || e.Data.Item1 is null || e.Data.Item2 is null)
		{
			return;
		}
		IsUIChange = false;
		if (e.Data.Item1 is ReconModel reconModel
			&& CurrentReconModel is not null
			&& reconModel.Descriptor.Id == CurrentReconModel.Descriptor.Id
			&& !(reconModel.Parent.ScanOption == ScanOption.Surview || reconModel.Parent.ScanOption == ScanOption.DualScout))
		{
			CurrentReconModel = reconModel;
			SetParameter(reconModel);
		}

		if (e.Data.Item1 is ScanModel scanModel
			&& CurrentReconModel is not null
			&& CurrentReconModel.Parent.Descriptor.Id == scanModel.Descriptor.Id
			&& CurrentReconModel.Status == PerformStatus.Unperform
			&& (scanModel.ScanOption == ScanOption.NVTestBolus || scanModel.ScanOption == ScanOption.TestBolus))
		{
			ImagesCount = (int)scanModel.Loops;
		}
		IsUIChange = true;
	}

	[UIRoute]
	private void RealtimeStatusChanged(object? sender, EventArgs<RealtimeInfo> e)
	{
		PageEnable = _selectionManager.CurrentSelection.Scan?.Status == PerformStatus.Unperform;
		var reconModel = _selectionManager.CurrentSelectionRecon;
		if (reconModel is null)
		{
			return;
		}
		if (reconModel.IsRTD)
		{
			ReconPageEnable = false;     //RTD参数不让修改
		}
		else
		{
			ReconPageEnable = reconModel.Status == PerformStatus.Unperform;
		}
	}

	[UIRoute]
	private void SelectionManager_SelectionReconChanged(object? sender, EventArgs<ReconModel> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}

		SelectReconChanged(e.Data);
	}

	[UIRoute]
	private void SelectReconChanged(ReconModel reconModel)
	{
		IsUIChange = false;
		//比较是不是同一个Scan
		if ((CurrentReconModel is not null && CurrentReconModel.Descriptor.Id != reconModel.Descriptor.Id) || CurrentReconModel is null)
		{
			//不是同一个Recon的情况
			SetMarkStatusToDefault();
		}
		if (reconModel.IsRTD)
		{
			ReconPageEnable = false;      //RTD参数不让修改			
		}
		else
		{
			ReconPageEnable = reconModel.Status == PerformStatus.Unperform;	
		}
		CurrentReconModel = reconModel;
		SetParameter(reconModel);

		if (!reconModel.IsRTD && reconModel.Status == PerformStatus.Unperform)
		{
			FilterFovAndMatrix(SelectFov, IsHDRecon, reconModel);
		}
		IsUIChange = true;
	}

	[UIRoute]
	private void ProtocolPerformStatusService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		//不是Recon模型，不处理
		if (e.Data.Model.GetType() != typeof(ReconModel))
		{
			return;
		}
		switch (e.Data.NewStatus)
		{
			case PerformStatus.Waiting:
			case PerformStatus.Performing:
			case PerformStatus.Performed:
				ReconPageEnable = false;
				WindowTypeEnable = false;
				WwEnable = false;
				WlEnable = false;
				if (e.Data.Model is ReconModel reconModel
					&& reconModel.FailureReason == FailureReasonType.None
					&& Directory.Exists(reconModel.ImagePath))
				{
					ImagesCount = Directory.GetFiles(reconModel.ImagePath).Length;
				}
				break;
			case PerformStatus.Unperform:
				if (e.Data.Model is ReconModel recon)
				{
					ReconPageEnable = true & !recon.IsRTD;
					WindowTypeEnable = true & !recon.IsRTD;
				}
				break;
		}
	}

	[UIRoute]
	private void SelectScanChanged(object? sender, EventArgs<ScanModel> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		//只要不是未扫描，就是 扫描中，扫描完成都要禁止页面
		if (e.Data?.Status != PerformStatus.Unperform)
		{
			ReconPageEnable = false;
		}
	}

	public void CloseWindow(Type typeName)
	{
		var parameterDetailWindow = Application.Current.Windows.Cast<Window>().FirstOrDefault(n => n.GetType() == typeName);
		if (parameterDetailWindow is not null)
		{
			parameterDetailWindow.Close();
		}
	}

	private void SetParameter(ReconModel reconModel)
	{
		//TODO:set recon parameter 
		SetWindowType(reconModel);
		CenterX = Math.Round(UnitConvert.Micron2Millimeter((float)reconModel.CenterFirstX), 2);
		CenterY = Math.Round(UnitConvert.Micron2Millimeter((float)reconModel.CenterFirstY), 2);
		StartRange = Math.Round(UnitConvert.Micron2Millimeter((double)reconModel.CenterFirstZ), 2).ToString();
		EndRange = Math.Round(UnitConvert.Micron2Millimeter((double)reconModel.CenterLastZ), 2).ToString();

		PreDenoiseCoef = reconModel.PreDenoiseCoef;
		PostDenoiseCoef = reconModel.PostDenoiseCoef;
		SelectReconIncrement = ReconIncrementList.FirstOrDefault(t => t.Key == (int)(reconModel.ImageIncrement));
		SelectSliceThickness = SliceThicknessList.FirstOrDefault(t => t.Key == (int)(reconModel.SliceThickness));

		SeriesDescription = reconModel.SeriesDescription;
		if (string.IsNullOrEmpty(SeriesDescription))
		{
			SeriesDescription = reconModel.DefaultSeriesDescription;
			SetProperty(ref _seriesDescription, SeriesDescription, OnParameterChanged, ProtocolParameterNames.RECON_SERIES_DESCRIPTION);
		}
		SelectImageOrder = ImageOrderList.FirstOrDefault(t => t.Key == (int)reconModel.ImageOrder);
		SelectKernel = KernelList.FirstOrDefault(t => t.Key == (int)reconModel.FilterType);
		GetReconMethod(reconModel);
		SelectPreDenoiseType = PreDenoiseTypeList.FirstOrDefault(t => t.Key == (int)reconModel.PreDenoiseType);
		SelectPostDenoiseType = PostDenoiseTypeList.FirstOrDefault(t => t.Key == (int)reconModel.PostDenoiseType);
		SelectAirCorrectionMode = AirCorrectionModeList.FirstOrDefault(t => t.Key == (int)reconModel.AirCorrectionMode);

		IsMetalAritifactEnable = reconModel.MetalAritifactEnable;
		IsBoneAritifactEnable = reconModel.BoneAritifactEnable;
		IsTwoPassEnable = reconModel.TwoPassEnable;
		SelectFov = Math.Round(UnitConvert.Micron2Millimeter((double)reconModel.FOVLengthHorizontal), 2);

		FilterReconMatrix(reconModel.FOVLengthHorizontal, reconModel);
		var keyValuePair = ReconMatrixeList.FirstOrDefault(t => t.Key == reconModel.ImageMatrixHorizontal);
		if (keyValuePair.Value is not null)
		{
			SelectReconMatrix = keyValuePair;
		}
		else if (ReconMatrixeList.Count > 0)
		{
			SelectReconMatrix = ReconMatrixeList[0];
		}

		if (reconModel.Status != PerformStatus.Performed)
		{
			if (reconModel.Parent is ScanModel scan && (scan.ScanOption == ScanOption.TestBolus || scan.ScanOption == ScanOption.NVTestBolus))
			{
				ImagesCount = (int)scan.Loops;
			}
			else
			{
				ImagesCount = ReconImageNumHelper.GetReconImageNum(reconModel);
			}
		}
		else
		{
			if (reconModel.FailureReason == FailureReasonType.None
				&& Directory.Exists(reconModel.ImagePath))
			{
				ImagesCount = Directory.GetFiles(reconModel.ImagePath).Length;
			}
		}
		SelectBinning = BinningList.FirstOrDefault(t => t.Key == (int)reconModel.PreBinning);
		if (reconModel.IVRTVCoef != 0)
		{
			IVRTVCoef = (float)Math.Round(reconModel.IVRTVCoef, 2);
		}
		else
		{
			//todo: 读取配置
			var coefficent = SystemConfig.GetTVCoefficientInfo(reconModel.Parent.BodyPart.ToString(), reconModel.WindowType.ToString());
			IVRTVCoef = (float)Math.Round(coefficent.Factor, 2);
		}
		RingCorrectionCoef = reconModel.RingCorrectionCoef;
		IsAutoRecon = reconModel.IsAutoRecon;
		IsHDRecon = reconModel.IsHDRecon;
		SmoothZEnable = reconModel.SmoothZEnable;
		ReconBodyPart = reconModel.ReconBodyPart;
		ScatterAlgorithm = reconModel.ScatterAlgorithm.ToString();
		IsWindmillArtifact = reconModel.WindmillArtifactEnable;
		IsConeAngleArtifact = reconModel.ConeAngleArtifactEnable;
		// load post process items parameters		
		LoadPostProcessParameters(reconModel);
	}

	private void GetReconMethod(ReconModel reconModel)
	{
		if (reconModel is null)
		{
			return;
		}
		switch (reconModel.ReconType)
		{
			case ReconType.HCT:
			case ReconType.FDK:
			case ReconType.XFDK:
				SelectReconMethod = ReconMethodList.FirstOrDefault(t => t.Value.Equals("FBP"));
				break;
			case ReconType.IVR:
			case ReconType.IVR_TV_OLD:
			case ReconType.IVR_TV:
			default:
				SelectReconMethod = ReconMethodList.FirstOrDefault(t => t.Value.Equals("IVR"));
				break;
		}
	}

	private void SetReconMethod(ReconModel reconModel, string medthodType)
	{
		if (reconModel is null || string.IsNullOrEmpty(medthodType))
		{
			return;
		}
		if (reconModel.IsRTD)
		{
			_protocolHostService.SetParameter(reconModel, ProtocolParameterNames.RECON_RECON_TYPE, ReconType.HCT.ToString());
		}
		else if (reconModel.Parent is ScanModel scan)
		{
			if (scan.ScanOption == ScanOption.Surview || scan.ScanOption == ScanOption.DualScout)
			{
				_protocolHostService.SetParameter(reconModel, ProtocolParameterNames.RECON_RECON_TYPE, ReconType.HCT.ToString());
			}
			switch (medthodType)
			{
				case "FBP":
					if (scan.ScanOption == ScanOption.Axial
						|| scan.ScanOption == ScanOption.NVTestBolusBase
						|| scan.ScanOption == ScanOption.NVTestBolus
						|| scan.ScanOption == ScanOption.TestBolus)
					{
						_protocolHostService.SetParameter(reconModel, ProtocolParameterNames.RECON_RECON_TYPE, ReconType.XFDK.ToString());
					}
					if (scan.ScanOption == ScanOption.Helical)
					{
						_protocolHostService.SetParameter(reconModel, ProtocolParameterNames.RECON_RECON_TYPE, ReconType.FDK.ToString());
					}
					break;
				case "IVR":
				default:
					_protocolHostService.SetParameter(reconModel, ProtocolParameterNames.RECON_RECON_TYPE, ReconType.IVR_TV.ToString());
					break;
			}
		}
	}

	private void LoadPostProcessParameters(ReconModel reconModel)
	{
		if (reconModel is null)
		{
			return;
		}

		if (reconModel.PostProcesses.Count > 0)
		{
			CurrentPostProcessModelList = DeepCopyPostProcessList(reconModel.PostProcesses);
			ConfirmedPostProcessItems = ConvertPostProcessViewList(reconModel.PostProcesses);
			SelectedPostProcessItems = ConvertPostProcessViewList(reconModel.PostProcesses);
		}
		else
		{
			CurrentPostProcessModelList.Clear();
			ConfirmedPostProcessItems.Clear();
			SelectedPostProcessItems.Clear();
		}

		ResetPostProcessParameters();
	}


	private void ResetPostProcessParameters()
	{
		SelectPostProcessDenoiseType = PostDenoiseTypeList[0];
		DenoiseLevel = 1;
		SharpLevel = 1;

		MotionArtifactReduceLevel = 5;
		PitchArtifactReduceLevel = 5;
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

	private ObservableCollection<KeyValuePair<string, string>> ConvertPostProcessViewList(List<PostProcessModel> postProcessModels)
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

	/// <summary>
	/// 根据recon类型和扫描类型确认重建类型
	/// </summary>
	/// <param name="scanModel"></param>
	/// <param name="reconModel"></param>
	private void InitReconMethodList()
	{
		ReconMethodList = new ObservableCollection<KeyValuePair<int, string>>();
		ReconMethodList.Add(new KeyValuePair<int, string>(0, "FBP"));
		ReconMethodList.Add(new KeyValuePair<int, string>(1, "IVR"));
	}

	private void SetWindowType(ReconModel reconModel)
	{
		var sModel = WindowList.FirstOrDefault(t => t.Value.Equals(reconModel.WindowType));
		if (string.IsNullOrEmpty(sModel.Key))
		{
			return;
		}
		SelectWindow = sModel;
		if (!(reconModel.Status == PerformStatus.Unperform || reconModel.IsRTD))
		{
			WindowTypeEnable = false;
			WwEnable = false;
			WlEnable = false;
			Ww = reconModel.WindowWidth[0];
			Wl = reconModel.WindowCenter[0];
		}
		else
		{
			WindowTypeEnable = ReconPageEnable;
			if (reconModel.WindowType.Equals(ProtocolParameterNames.RECON_WINDOW_CUSTOM_TYPE))
			{
				WwEnable = ReconPageEnable;
				WlEnable = ReconPageEnable;
				Ww = reconModel.WindowWidth[0];
				Wl = reconModel.WindowCenter[0];
			}
			else
			{
				WwEnable = false;
				WlEnable = false;
				var model = WindowWidthLevelModels.FirstOrDefault(t => t.BodyPart.Equals(SelectWindow.Key));
				if (model is not null)
				{
					Ww = model.Width.Value;
					Wl = model.Level.Value;
				}
			}
		}
	}

	private void InitComboBoxItem()
	{
		ImageOrderList = Extensions.Extensions.EnumToItems(typeof(ImageOrders));
		InitKernelList();
		InitReconMethodList();
		ReconMethods = Extensions.Extensions.EnumToList(typeof(ReconType)).ToList();
		RemoveArtifactList = Extensions.Extensions.EnumToItems(typeof(RemoveArtifacts));

		PreDenoiseTypeList = Extensions.Extensions.EnumToList(typeof(PreDenoiseType));
		PostDenoiseTypeList = Extensions.Extensions.EnumToList(typeof(PostDenoiseType));

		AirCorrectionModeList = Extensions.Extensions.EnumToList(typeof(AirCorrectionMode));
		InitReconMatrix();
		InitSliceThickness();
		InitReconIncrement();
		WindowList = GetWindowList();

		SelectSliceThickness = SliceThicknessList[0];
		SelectReconIncrement = ReconIncrementList[0];

		if (ReconMatrixeList.Count > 0)
		{
			SelectReconMatrix = ReconMatrixeList[0];
		}
		SelectImageOrder = ImageOrderList[0];
		SelectRemoveArtifact = RemoveArtifactList[0];
		SelectReconMethod = ReconMethodList[0];
		SelectKernel = KernelList[0];
		SelectWindow = WindowList[0];
		SelectPreDenoiseType = PreDenoiseTypeList[0];
		SelectPostDenoiseType = PostDenoiseTypeList[0];
		SelectAirCorrectionMode = AirCorrectionModeList[0];
		BinningList = Extensions.Extensions.EnumToList(typeof(PreBinning));
		SelectBinning = BinningList[0];
	}

	private ObservableCollection<KeyValuePair<string, string>> GetWindowList()
	{
		ObservableCollection<KeyValuePair<string, string>> keyValuePairs = new ObservableCollection<KeyValuePair<string, string>>();
		foreach (var item in WindowWidthLevelModels)
		{
			keyValuePairs.Add(new KeyValuePair<string, string>(item.BodyPart, item.BodyPart));
		}
		keyValuePairs.Add(new KeyValuePair<string, string>(ProtocolParameterNames.RECON_WINDOW_CUSTOM_TYPE, ProtocolParameterNames.RECON_WINDOW_CUSTOM_TYPE));
		return keyValuePairs;
	}

	#region MarkStatus

	private void SetMarkStatusToDefault()
	{
		SelectSliceThicknessMarkStatus = MarkControlStatus.Default;
		ImagesCountMarkStatus = MarkControlStatus.Default;
		SelectRemoveArtifactMarkStatus = MarkControlStatus.Default;
		SelectImageOrderMarkStatus = MarkControlStatus.Default;
		SelectReconMatrixMarkStatus = MarkControlStatus.Default;
		SelectWindowMarkStatus = MarkControlStatus.Default;
		CenterXMarkStatus = MarkControlStatus.Default;
		SelectReconMethodMarkStatus = MarkControlStatus.Default;
		CenterYMarkStatus = MarkControlStatus.Default;
		TVDenoiseCoefMarkStatus = MarkControlStatus.Default;
		BM3DenoiseCoefMarkStatus = MarkControlStatus.Default;
		PreDenoiseCoefMarkStatus = MarkControlStatus.Default;
		PostDenoiseCoefMarkStatus = MarkControlStatus.Default;
		PreDenoiseTypeMarkStatus = MarkControlStatus.Default;
		PostDenoiseTypeMarkStatus = MarkControlStatus.Default;
		SelectKernelMarkStatus = MarkControlStatus.Default;
		WwMarkStatus = MarkControlStatus.Default;
		WlMarkStatus = MarkControlStatus.Default;
		ReconIncrementMarkStatus = MarkControlStatus.Default;
		SelectFovMarkStatus = MarkControlStatus.Default;
		SelectBinningMarkStatus = MarkControlStatus.Default;
		AirCorrectionModeMarkStatus = MarkControlStatus.Default;
		IVRTVCoefMarkStatus = MarkControlStatus.Default;
		RingCorrectionCoefMarkStatus = MarkControlStatus.Default;
		PostProcessMarkStatus = MarkControlStatus.Default;
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
		if (!(ParameterDirty.ContainsKey(propName) && ParameterDirty[propName]))
		{
			return;
		}
		switch (propName)
		{
			case nameof(SelectSliceThickness):
				SelectSliceThicknessMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(ImagesCount):
				ImagesCountMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectRemoveArtifact):
				SelectRemoveArtifactMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectImageOrder):
				SelectImageOrderMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectReconMatrix):
				SelectReconMatrixMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectWindow):
				SelectWindowMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(CenterX):
				CenterXMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectReconMethod):
				SelectReconMethodMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(Wl):
				WlMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(Ww):
				WwMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(CenterY):
				CenterYMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(TVDenoiseCoef):
				TVDenoiseCoefMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(BM3DenoiseCoef):
				BM3DenoiseCoefMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(PreDenoiseCoef):
				PreDenoiseCoefMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(PostDenoiseCoef):
				PostDenoiseCoefMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectPreDenoiseType):
				PreDenoiseTypeMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectPostDenoiseType):
				PostDenoiseTypeMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectKernel):
				SelectKernelMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectFov):
				SelectFovMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectReconIncrement):
				ReconIncrementMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectBinning):
				SelectBinningMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(SelectAirCorrectionMode):
				AirCorrectionModeMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(IVRTVCoef):
				IVRTVCoefMarkStatus = MarkControlStatus.Modified;
				break;
			case nameof(RingCorrectionCoef):
				RingCorrectionCoefMarkStatus = MarkControlStatus.Modified;
				break;
		}
	}

	private MarkControlStatus _selectKernelMarkStatus;
	public MarkControlStatus SelectKernelMarkStatus
	{
		get => _selectKernelMarkStatus;
		set => SetProperty(ref _selectKernelMarkStatus, value);
	}

	private MarkControlStatus _selectFovMarkStatus;
	public MarkControlStatus SelectFovMarkStatus
	{
		get => _selectFovMarkStatus;
		set => SetProperty(ref _selectFovMarkStatus, value);
	}

	private MarkControlStatus _tvDenoiseCoefMarkStatus;
	public MarkControlStatus TVDenoiseCoefMarkStatus
	{
		get => _tvDenoiseCoefMarkStatus;
		set => SetProperty(ref _tvDenoiseCoefMarkStatus, value);
	}

	private MarkControlStatus _bm3DenoiseCoefMarkStatus;
	public MarkControlStatus BM3DenoiseCoefMarkStatus
	{
		get => _bm3DenoiseCoefMarkStatus;
		set => SetProperty(ref _bm3DenoiseCoefMarkStatus, value);
	}

	private MarkControlStatus _preDenoiseCoefMarkStatus;
	public MarkControlStatus PreDenoiseCoefMarkStatus
	{
		get => _preDenoiseCoefMarkStatus;
		set => SetProperty(ref _preDenoiseCoefMarkStatus, value);
	}

	private MarkControlStatus _postDenoiseCoefMarkStatus;
	public MarkControlStatus PostDenoiseCoefMarkStatus
	{
		get => _postDenoiseCoefMarkStatus;
		set => SetProperty(ref _postDenoiseCoefMarkStatus, value);
	}

	private MarkControlStatus _preDenoiseTypeMarkStatus;
	public MarkControlStatus PreDenoiseTypeMarkStatus
	{
		get => _preDenoiseTypeMarkStatus;
		set => SetProperty(ref _preDenoiseTypeMarkStatus, value);
	}
	private MarkControlStatus _postDenoiseTypeMarkStatus;
	public MarkControlStatus PostDenoiseTypeMarkStatus
	{
		get => _postDenoiseTypeMarkStatus;
		set => SetProperty(ref _postDenoiseTypeMarkStatus, value);
	}

	private MarkControlStatus _reconIncrementMarkStatus;
	public MarkControlStatus ReconIncrementMarkStatus
	{
		get => _reconIncrementMarkStatus;
		set => SetProperty(ref _reconIncrementMarkStatus, value);
	}

	private MarkControlStatus _centerYMarkStatus;
	public MarkControlStatus CenterYMarkStatus
	{
		get => _centerYMarkStatus;
		set => SetProperty(ref _centerYMarkStatus, value);
	}

	private MarkControlStatus _wwMarkStatus;
	public MarkControlStatus WwMarkStatus
	{
		get => _wwMarkStatus;
		set => SetProperty(ref _wwMarkStatus, value);
	}
	private MarkControlStatus _wlMarkStatus;
	public MarkControlStatus WlMarkStatus
	{
		get => _wlMarkStatus;
		set => SetProperty(ref _wlMarkStatus, value);
	}

	private MarkControlStatus _selectReconMethodMarkStatus;
	public MarkControlStatus SelectReconMethodMarkStatus
	{
		get => _selectReconMethodMarkStatus;
		set => SetProperty(ref _selectReconMethodMarkStatus, value);
	}
	private MarkControlStatus _centerXMarkStatus;
	public MarkControlStatus CenterXMarkStatus
	{
		get => _centerXMarkStatus;
		set => SetProperty(ref _centerXMarkStatus, value);
	}

	private MarkControlStatus _selectWindowMarkStatus;
	public MarkControlStatus SelectWindowMarkStatus
	{
		get => _selectWindowMarkStatus;
		set => SetProperty(ref _selectWindowMarkStatus, value);
	}

	private MarkControlStatus _selectReconMatrixMarkStatus;
	public MarkControlStatus SelectReconMatrixMarkStatus
	{
		get => _selectReconMatrixMarkStatus;
		set => SetProperty(ref _selectReconMatrixMarkStatus, value);
	}

	private MarkControlStatus _selectImageOrderMarkStatus;
	public MarkControlStatus SelectImageOrderMarkStatus
	{
		get => _selectImageOrderMarkStatus;
		set => SetProperty(ref _selectImageOrderMarkStatus, value);
	}

	private MarkControlStatus _selectRemoveArtifactMarkStatus;
	public MarkControlStatus SelectRemoveArtifactMarkStatus
	{
		get => _selectRemoveArtifactMarkStatus;
		set => SetProperty(ref _selectRemoveArtifactMarkStatus, value);
	}

	private MarkControlStatus _selectSliceThicknessMarkStatus;
	public MarkControlStatus SelectSliceThicknessMarkStatus
	{
		get => _selectSliceThicknessMarkStatus;
		set => SetProperty(ref _selectSliceThicknessMarkStatus, value);
	}

	private MarkControlStatus _imagesCountMarkStatus;
	public MarkControlStatus ImagesCountMarkStatus
	{
		get => _imagesCountMarkStatus;
		set => SetProperty(ref _imagesCountMarkStatus, value);
	}

	private MarkControlStatus _selectBinningMarkStatus;
	public MarkControlStatus SelectBinningMarkStatus
	{
		get => _selectBinningMarkStatus;
		set => SetProperty(ref _selectBinningMarkStatus, value);
	}

	private MarkControlStatus _airCorrectionModeStatus;
	public MarkControlStatus AirCorrectionModeMarkStatus
	{
		get => _airCorrectionModeStatus;
		set => SetProperty(ref _airCorrectionModeStatus, value);
	}

	private MarkControlStatus _iVRTVCoefStatus;
	public MarkControlStatus IVRTVCoefMarkStatus
	{
		get => _iVRTVCoefStatus;
		set => SetProperty(ref _iVRTVCoefStatus, value);
	}

	private MarkControlStatus _ringCorrectionCoefStatus;
	public MarkControlStatus RingCorrectionCoefMarkStatus
	{
		get => _ringCorrectionCoefStatus;
		set => SetProperty(ref _ringCorrectionCoefStatus, value);
	}

	// TODO:
	private MarkControlStatus _postProcessStatus;
	public MarkControlStatus PostProcessMarkStatus
	{
		get => _postProcessStatus;
		set => SetProperty(ref _postProcessStatus, value);
	}
	#endregion

	private ObservableCollection<KeyValuePair<int, string>> _imageOrderList = new();
	public ObservableCollection<KeyValuePair<int, string>> ImageOrderList
	{
		get => _imageOrderList;
		set => SetProperty(ref _imageOrderList, value);
	}

	private ObservableCollection<KeyValuePair<int, string>> _kernelList = new();
	public ObservableCollection<KeyValuePair<int, string>> KernelList
	{
		get => _kernelList;
		set => SetProperty(ref _kernelList, value);
	}

	private ObservableCollection<KeyValuePair<int, string>> _reconMatrixList = new();
	public ObservableCollection<KeyValuePair<int, string>> ReconMatrixeList
	{
		get => _reconMatrixList;
		set => SetProperty(ref _reconMatrixList, value);
	}

	private ObservableCollection<KeyValuePair<int, string>> _reconMethodList = new();
	public ObservableCollection<KeyValuePair<int, string>> ReconMethodList
	{
		get => _reconMethodList;
		set => SetProperty(ref _reconMethodList, value);
	}

	private List<KeyValuePair<int, string>> _reconMethods = new();
	public List<KeyValuePair<int, string>> ReconMethods
	{
		get => _reconMethods;
		set => SetProperty(ref _reconMethods, value);
	}

	private ObservableCollection<KeyValuePair<int, string>> _removeArtifactList = new();
	public ObservableCollection<KeyValuePair<int, string>> RemoveArtifactList
	{
		get => _removeArtifactList;
		set => SetProperty(ref _removeArtifactList, value);
	}

	private ObservableCollection<KeyValuePair<int, string>> _sliceThicknessList = new();
	public ObservableCollection<KeyValuePair<int, string>> SliceThicknessList
	{
		get => _sliceThicknessList;
		set => SetProperty(ref _sliceThicknessList, value);
	}

	private ObservableCollection<KeyValuePair<int, string>> _reconIncrementList = new();
	public ObservableCollection<KeyValuePair<int, string>> ReconIncrementList
	{
		get => _reconIncrementList;
		set => SetProperty(ref _reconIncrementList, value);
	}

	private ObservableCollection<KeyValuePair<string, string>> _windowList = new();
	public ObservableCollection<KeyValuePair<string, string>> WindowList
	{
		get => _windowList;
		set => SetProperty(ref _windowList, value);
	}

	private ObservableCollection<KeyValuePair<int, string>> _preDenoiseTypeList = new();
	public ObservableCollection<KeyValuePair<int, string>> PreDenoiseTypeList
	{
		get => _preDenoiseTypeList;
		set => SetProperty(ref _preDenoiseTypeList, value);
	}

	private ObservableCollection<KeyValuePair<int, string>> _postDenoiseTypeList = new();
	public ObservableCollection<KeyValuePair<int, string>> PostDenoiseTypeList
	{
		get => _postDenoiseTypeList;
		set => SetProperty(ref _postDenoiseTypeList, value);
	}

	private ObservableCollection<KeyValuePair<int, string>> _airCorrectionModeList = new();
	public ObservableCollection<KeyValuePair<int, string>> AirCorrectionModeList
	{
		get => _airCorrectionModeList;
		set => SetProperty(ref _airCorrectionModeList, value);
	}

	private string _seriesDescription = string.Empty;
	public string SeriesDescription
	{
		get => _seriesDescription;
		set
		{
			if (SetProperty(ref _seriesDescription, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.RECON_SERIES_DESCRIPTION, value);
			}
		}
	}

	private KeyValuePair<int, string> _selectSliceThickness;
	public KeyValuePair<int, string> SelectSliceThickness
	{
		get => _selectSliceThickness;
		set
		{
			if (SetProperty(ref _selectSliceThickness, value) && IsUIChange && CurrentReconModel is ReconModel recon)
			{
				SelectReconIncrement = ReconIncrementList.FirstOrDefault(t => t.Key == (int)(value.Key));
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_SLICE_THICKNESS, UnitConvert.Millimeter2Micron(double.Parse(value.Value)));
				if (recon.Status == PerformStatus.Unperform)
				{
					SeriesDescription = recon.DefaultSeriesDescription;
				}
			}
		}
	}

	private KeyValuePair<int, string> _selectReconIncrement;
	public KeyValuePair<int, string> SelectReconIncrement
	{
		get => _selectReconIncrement;
		set
		{
			if (SetProperty(ref _selectReconIncrement, value) && IsUIChange && CurrentReconModel is ReconModel recon)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_IMAGE_INCREMENT, UnitConvert.Millimeter2Micron(double.Parse(value.Value)));
			}
		}
	}

	private int _imagesCount;
	public int ImagesCount
	{
		get => _imagesCount;
		set => SetProperty(ref _imagesCount, value);
	}

	private double _selectFov = 0;
	public double SelectFov
	{
		get => _selectFov;
		set
		{
			if (SetProperty(ref _selectFov, value)
				&& IsUIChange
				&& CurrentReconModel is ReconModel recon)
			{
				FilterFovAndMatrix(value, recon.IsHDRecon, recon);
			}
		}
	}

	private void FilterFovAndMatrix(double fov, bool isHDRecon, ReconModel recon)
	{
		var newFov = UnitConvert.Millimeter2Micron(fov);
		//临时方案
		//var minFov = ReconFovMatrixHelper.GetMinFov(recon.IsHDRecon);
		//            if (newFov < minFov)
		//            {
		//                newFov = minFov;
		//            }
		newFov = (int)ReconFovMatrixHelper.GetSuitableFOVTemp(newFov);
		FilterReconMatrix(newFov, recon);
		var oriMatrix = recon.ImageMatrixHorizontal;
		var newMatrix = ReconFovMatrixHelper.GetSuitableMatrix(newFov, oriMatrix, isHDRecon);

		List<ParameterModel> parameterModels = new List<ParameterModel>
			{
				new ParameterModel
				{
					Name = ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL,
					Value=newFov.ToString(CultureInfo.InvariantCulture)
				},
				new ParameterModel
				{
					Name = ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL,
					Value =newFov.ToString(CultureInfo.InvariantCulture)
				},
				new ParameterModel
				{
					Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_HORIZONTAL,
					Value=newMatrix.ToString(CultureInfo.InvariantCulture)
				},
				new ParameterModel
				{
					Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL,
					Value =newMatrix.ToString(CultureInfo.InvariantCulture)
				},
				new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_FIRST_X,Value = 0.ToString()},		//临时0724
                new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y,Value = 0.ToString()},		//临时0724
                new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_LAST_X,Value =  0.ToString()},		//临时0724
                new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_LAST_Y,Value = 0.ToString()},			//临时0724
            };
		_protocolHostService.SetParameters(recon, parameterModels);
	}

	private void FilterReconMatrix(int fov, ReconModel reconModel)
	{
		SelectReconMatrix = new KeyValuePair<int, string>();
		ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
		foreach (var item in ReconFovMatrixHelper.GetSuitableMatrixList(fov, reconModel.IsHDRecon))
		{
			list.Add(new KeyValuePair<int, string>(item, item.ToString()));
		}
		ReconMatrixeList = list;
	}

	private void InitKernelList()
	{
		ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
		foreach (var item in Extensions.Extensions.EnumToList(typeof(FilterType)))
		{
			list.Add(new KeyValuePair<int, string>(item.Key, item.Value.Replace("Plus", "+")));
		}
		KernelList = list;
	}

	private void InitReconMatrix()
	{
		SelectReconMatrix = new KeyValuePair<int, string>();
		ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
		foreach (var item in SystemConfig.OfflineReconParamConfig.OfflineReconParam.ReconMatrix.Ranges)
		{
			list.Add(new KeyValuePair<int, string>(item, item.ToString()));
		}
		ReconMatrixeList = list;
	}

	private void InitSliceThickness()
	{
		SelectReconMatrix = new KeyValuePair<int, string>();
		ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
		foreach (var item in SystemConfig.OfflineReconParamConfig.OfflineReconParam.SliceThickness.Ranges)
		{
			list.Add(new KeyValuePair<int, string>(item, UnitConvert.ReduceThousand((float)item).ToString()));
		}
		SliceThicknessList = list;
	}

	private void InitReconIncrement()
	{
		SelectReconMatrix = new KeyValuePair<int, string>();
		ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
		foreach (var item in SystemConfig.OfflineReconParamConfig.OfflineReconParam.ImageIncrement.Ranges)
		{
			list.Add(new KeyValuePair<int, string>(item, UnitConvert.ReduceThousand((float)item).ToString()));
		}
		ReconIncrementList = list;
	}

	private KeyValuePair<int, string> _selectReconMatrix;
	public KeyValuePair<int, string> SelectReconMatrix
	{
		get => _selectReconMatrix;
		set
		{
			if (value.Value is not null
				&& SetProperty(ref _selectReconMatrix, value)
				&& IsUIChange
				&& CurrentReconModel is ReconModel recon)
			{
				List<ParameterModel> parameterModels = new List<ParameterModel> {
					new ParameterModel
					{
						Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_HORIZONTAL,
						Value=value.Value.ToString(CultureInfo.InvariantCulture)
					},
					new ParameterModel
					{
						Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL,
						Value =value.Value.ToString(CultureInfo.InvariantCulture)
					},
				};
				_protocolHostService.SetParameters(recon, parameterModels);
			}
		}
	}

	private KeyValuePair<int, string> _selectImageOrder;
	public KeyValuePair<int, string> SelectImageOrder
	{
		get => _selectImageOrder;
		set
		{
			if (SetProperty(ref _selectImageOrder, value)
				&& IsUIChange
				&& CurrentReconModel is not null
				&& CurrentReconModel.Parent is not null
				&& !(CurrentReconModel.Parent.ScanOption == ScanOption.Surview || CurrentReconModel.Parent.ScanOption == ScanOption.DualScout))
			{
				KeyValuePair<int, string> keyValuePair = new KeyValuePair<int, string>(value.Key, ((ImageOrders)value.Key).ToString());
				OnParameterChanged(ProtocolParameterNames.RECON_IMAGE_ORDER, keyValuePair);
			}
		}
	}

	private string _comments = string.Empty;
	public string Comments
	{
		get => _comments;
		set => SetProperty(ref _comments, value);
	}

	private KeyValuePair<int, string> _selectRemoveArtifact;
	public KeyValuePair<int, string> SelectRemoveArtifact
	{
		get => _selectRemoveArtifact;
		set => SetProperty(ref _selectRemoveArtifact, value);
	}

	private KeyValuePair<int, string> _selectReconMethod;
	public KeyValuePair<int, string> SelectReconMethod
	{
		get => _selectReconMethod;
		set
		{
			if (SetProperty(ref _selectReconMethod, value) && IsUIChange && CurrentReconModel is ReconModel recon)
			{
				SetReconMethod(recon, value.Value);
			}
			//设置IVRTVCoef是否可见
			if (!string.IsNullOrEmpty(value.Value) && value.Value.Equals("IVR"))
			{
				IsIVRTVCoefEnable = true;
			}
			else
			{
				IsIVRTVCoefEnable = false;
			}
			if (value.Key >= 0)
			{
				ReconMethodShow = value.Value;
			}
		}
	}

	private string _reconMethod = string.Empty;
	public string ReconMethodShow
	{
		get => _reconMethod;
		set => SetProperty(ref _reconMethod, value);
	}

	private KeyValuePair<int, string> _selectKernel;
	public KeyValuePair<int, string> SelectKernel
	{
		get => _selectKernel;
		set
		{
			if (SetProperty(ref _selectKernel, value) && IsUIChange && _selectionManager.CurrentSelectionRecon is ReconModel recon)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_FILTER_TYPE, value.Value.Replace("+", "Plus"));
				if (recon.Status == PerformStatus.Unperform)
				{
					SeriesDescription = recon.DefaultSeriesDescription;
				}
			}
		}
	}

	private bool _windowTypeEnable = true;
	public bool WindowTypeEnable
	{
		get => _windowTypeEnable & ReconPageEnable;
		set
		{
			SetProperty(ref _windowTypeEnable, value);
		}
	}

	private bool _wwEnable = true;
	public bool WwEnable
	{
		get => _wwEnable;
		set
		{
			SetProperty(ref _wwEnable, value);
		}
	}

	private bool _wlEnable = true;
	public bool WlEnable
	{
		get => _wlEnable;
		set
		{
			SetProperty(ref _wlEnable, value);
		}
	}

	private KeyValuePair<string, string> _selectWindow;
	public KeyValuePair<string, string> SelectWindow
	{
		get => _selectWindow;
		set
		{
			if (SetProperty(ref _selectWindow, value) && IsUIChange)
			{
				if (value.Key.Equals(ProtocolParameterNames.RECON_WINDOW_CUSTOM_TYPE))
				{
					WwEnable = ReconPageEnable;
					WlEnable = ReconPageEnable;
				}
				else
				{
					WwEnable = false;
					WlEnable = false;
					var model = WindowWidthLevelModels.FirstOrDefault(t => t.BodyPart.Equals(value.Key));
					if (model is not null)
					{
						Ww = model.Width.Value;
						Wl = model.Level.Value;
					}
				}
				OnParameterChanged(ProtocolParameterNames.RECON_WINDOW_TYPE, value.Value);
				//TODO:原厂协议中窗宽窗位的值配置少了一个值，先这么处理
				var modelw = WindowWidthLevelModels.FirstOrDefault(t => t.BodyPart.Equals(value.Value));
				if (modelw is not null && !value.Key.Equals(ProtocolParameterNames.RECON_WINDOW_CUSTOM_TYPE))
				{
					OnParameterChanged(ProtocolParameterNames.RECON_WINDOW_CENTER, modelw.Level.Value);
					OnParameterChanged(ProtocolParameterNames.RECON_WINDOW_WIDTH, modelw.Width.Value);
				}
				if (CurrentReconModel is ReconModel recon && recon.Status == PerformStatus.Unperform)
				{
					SeriesDescription = recon.DefaultSeriesDescription;
				}
			}
		}
	}

	private double _ww = 20;
	public double Ww
	{
		get => _ww;
		set
		{
			if (SetProperty(ref _ww, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.RECON_WINDOW_WIDTH, value);
			}
		}
	}

	private double _wl = 350;
	public double Wl
	{
		get => _wl;
		set
		{
			if (SetProperty(ref _wl, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.RECON_WINDOW_CENTER, value);
			}
		}
	}

	private bool _isAutoStorage;
	public bool IsAutoStorage
	{
		get => _isAutoStorage;
		set => SetProperty(ref _isAutoStorage, value);
	}

	private bool _isAutoFilm;
	public bool IsAutoFilm
	{
		get => _isAutoFilm;
		set => SetProperty(ref _isAutoFilm, value);
	}

	private bool _isAutoReconEnabled = true;
	public bool IsAutoReconEnabled
	{
		get => _isAutoReconEnabled;
		set
		{
			SetProperty(ref _isAutoReconEnabled, value);
		}
	}

	private bool _isAutoRecon;
	public bool IsAutoRecon
	{
		get => _isAutoRecon;
		set
		{
			if (SetProperty(ref _isAutoRecon, value) && CurrentReconModel is ReconModel recon && IsUIChange)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_IS_AUTO_RECON, value);
			}
		}
	}

	private bool _isHDRecon;
	public bool IsHDRecon
	{
		get => _isHDRecon;
		set
		{
			if (SetProperty(ref _isHDRecon, value) && CurrentReconModel is ReconModel recon && IsUIChange)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_IS_HD_RECON, value);
				FilterFovAndMatrix(UnitConvert.Micron2Millimeter(recon.FOVLengthHorizontal), value, recon);

				if (recon.Status == PerformStatus.Unperform)
				{
					SeriesDescription = recon.DefaultSeriesDescription;
				}
			}
		}
	}

	private bool _isSmoothZEnable;
	public bool SmoothZEnable
	{
		get => _isSmoothZEnable;
		set
		{
			if (SetProperty(ref _isSmoothZEnable, value) && CurrentReconModel is ReconModel recon && IsUIChange)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_SMOOTH_Z_ENABLE, value);
			}
		}
	}

	private string _scatterAlgorithm = string.Empty;
	public string ScatterAlgorithm
	{
		get => _scatterAlgorithm;
		set => SetProperty(ref _scatterAlgorithm, value);
	}


	private bool _isWindmillArtifact;
	public bool IsWindmillArtifact
	{
		get => _isWindmillArtifact;
		set
		{
			if (SetProperty(ref _isWindmillArtifact, value) && CurrentReconModel is ReconModel recon && IsUIChange)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_WINDMILL_ARTIFACT_ENABLE, value);
			}
		}
	}

	private bool _isConeAngleArtifact;
	public bool IsConeAngleArtifact
	{
		get => _isConeAngleArtifact;
		set
		{
			if (SetProperty(ref _isConeAngleArtifact, value) && CurrentReconModel is ReconModel recon && IsUIChange)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_CONE_ANGLE_ARTIFACT_ENABLE, value);
			}
		}
	}

	private double _centerX = 0;
	public double CenterX
	{
		get => _centerX;
		set
		{
			if (SetProperty(ref _centerX, value) && IsUIChange && CurrentReconModel is ReconModel recon)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_CENTER_FIRST_X, UnitConvert.Millimeter2Micron(value));
			}
		}
	}

	private double _centerY = 0;
	public double CenterY
	{
		get => _centerY;
		set
		{
			if (SetProperty(ref _centerY, value) && IsUIChange && CurrentReconModel is ReconModel recon)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_CENTER_FIRST_Y, UnitConvert.Millimeter2Micron(value));
			}
		}
	}

	private string _startRange = string.Empty;
	public string StartRange
	{
		get => _startRange;
		set => SetProperty(ref _startRange, value);
	}

	private string _endRange = string.Empty;
	public string EndRange
	{
		get => _endRange;
		set => SetProperty(ref _endRange, value);
	}

	private double _tvDenoiseCoef = 20;
	public double TVDenoiseCoef
	{
		get => _tvDenoiseCoef;
		set
		{
			//if (SetProperty(ref _tvDenoiseCoef, value)&&IsUIChange)
			//{
			//	 OnParameterChanged( ProtocolParameterNames.RECON_TV_DENOISE_COEF,value);
			//}
		}
	}

	private double _bm3DenoiseCoef = 20;
	public double BM3DenoiseCoef
	{
		get => _bm3DenoiseCoef;
		set
		{
			//if (SetProperty(ref _bm3DenoiseCoef, value)&&IsUIChange)
			//{
			//	 OnParameterChanged(ProtocolParameterNames.RECON_BM3D_DENOISE_COEF,value);
			//}
		}
	}

	public void OnParameterChanged<T>(string parameterName, T value)
	{
		if (_selectionManager.CurrentSelectionRecon is ReconModel recon)
		{
			if (parameterName.Equals(ProtocolParameterNames.RECON_WINDOW_CENTER) || parameterName.Equals(ProtocolParameterNames.RECON_WINDOW_WIDTH))
			{
				var list = new List<int> { int.Parse(value?.ToString() ?? string.Empty), 0, };
				_protocolHostService.SetParameter(recon, parameterName, list);
			}
			else if (parameterName.Equals(ProtocolParameterNames.RECON_RECON_TYPE) || parameterName.Equals(ProtocolParameterNames.RECON_IMAGE_ORDER))
			{
				//处理枚举类型
				if (value is KeyValuePair<int, string>)
				{
					var pair = value as KeyValuePair<int, string>?;
					_protocolHostService.SetParameter(recon, parameterName, pair?.Value);
				}
			}
			else
			{
				_protocolHostService.SetParameter(recon, parameterName, value);
			}
		}
	}

	public void Closed(object parameter)
	{
		if (parameter is Window window)
		{
			window.Hide();
		}
	}

	private double _preDenoiseCoef = 0;
	public double PreDenoiseCoef
	{
		get => _preDenoiseCoef;
		set
		{
			if (SetProperty(ref _preDenoiseCoef, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.RECON_PRE_DENOISE_COEF, (int)value);
			}
		}
	}

	private double _postDenoiseCoef = 0;
	public double PostDenoiseCoef
	{
		get => _postDenoiseCoef;
		set
		{
			if (SetProperty(ref _postDenoiseCoef, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.RECON_POST_DENOISE_COEF, (int)value);
			}
		}
	}

	private string _reconBodyPart = string.Empty;
	public string ReconBodyPart
	{
		get => _reconBodyPart;
		set => SetProperty(ref _reconBodyPart, value);
	}

	private KeyValuePair<int, string> _selectPreDenoiseType;
	public KeyValuePair<int, string> SelectPreDenoiseType
	{
		get => _selectPreDenoiseType;
		set
		{
			if (SetProperty(ref _selectPreDenoiseType, value) && IsUIChange && _selectionManager.CurrentSelectionRecon is ReconModel recon)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_PRE_DENOISE_TYPE, value.Value);
			}
		}
	}

	private KeyValuePair<int, string> _selectPostDenoiseType;
	public KeyValuePair<int, string> SelectPostDenoiseType
	{
		get => _selectPostDenoiseType;
		set
		{
			if (SetProperty(ref _selectPostDenoiseType, value) && IsUIChange && _selectionManager.CurrentSelectionRecon is ReconModel recon)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_POST_DENOISE_TYPE, value.Value);
			}
		}
	}

	private ObservableCollection<KeyValuePair<int, string>> _binningList = new();
	public ObservableCollection<KeyValuePair<int, string>> BinningList
	{
		get => _binningList;
		set => SetProperty(ref _binningList, value);
	}

	private KeyValuePair<int, string> _selectBinning;
	public KeyValuePair<int, string> SelectBinning
	{
		get => _selectBinning;
		set
		{
			if (SetProperty(ref _selectBinning, value) && IsUIChange && CurrentReconModel is ReconModel recon)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_PRE_BINNING, value.Value);
			}
		}
	}

	private KeyValuePair<int, string> _selectAirCorrectionMode;
	public KeyValuePair<int, string> SelectAirCorrectionMode
	{
		get => _selectAirCorrectionMode;
		set
		{
			if (SetProperty(ref _selectAirCorrectionMode, value) && IsUIChange && _selectionManager.CurrentSelectionRecon is ReconModel recon)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_AIR_CORRECTION_MODE, value.Value);
			}
		}
	}

	private bool _isTwoPassEnable;
	public bool IsTwoPassEnable
	{
		get => _isTwoPassEnable;
		set
		{
			if (SetProperty(ref _isTwoPassEnable, value) && IsUIChange && _selectionManager.CurrentSelectionRecon is ReconModel recon)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_TWO_PASS_ENABLE, value);
			}
		}
	}

	private bool _isBoneAritifactEnable;
	public bool IsBoneAritifactEnable
	{
		get => _isBoneAritifactEnable;
		set
		{
			if (SetProperty(ref _isBoneAritifactEnable, value) && IsUIChange && _selectionManager.CurrentSelectionRecon is ReconModel recon)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_BONE_ARITIFACT_ENABALE, value);
			}
		}
	}

	private bool _isMetalAritifactEnable;
	public bool IsMetalAritifactEnable
	{
		get => _isMetalAritifactEnable;
		set
		{
			if (SetProperty(ref _isMetalAritifactEnable, value) && IsUIChange && _selectionManager.CurrentSelectionRecon is ReconModel recon)
			{
				_protocolHostService.SetParameter(recon, ProtocolParameterNames.RECON_METAL_ARITIFACT_ENABLE, value);
			}
		}
	}

	private bool _isIVRTVCoefEnable = false;
	public bool IsIVRTVCoefEnable
	{
		get => _isIVRTVCoefEnable;
		set
		{
			SetProperty(ref _isIVRTVCoefEnable, value);
		}
	}

	private float _iVRTVCoef = 0.02f;
	public float IVRTVCoef
	{
		get => _iVRTVCoef;
		set
		{
			if (SetProperty(ref _iVRTVCoef, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.RECON_IVR_TV_COEF, value);
			}
		}
	}

	private double _ringCorrectionCoef = 0;
	public double RingCorrectionCoef
	{
		get => _ringCorrectionCoef;
		set
		{
			if (SetProperty(ref _ringCorrectionCoef, value) && IsUIChange)
			{
				OnParameterChanged(ProtocolParameterNames.RECON_RING_CORRECTION_COEF, (int)value);
			}
		}
	}

	private ObservableCollection<KeyValuePair<string, string>> _selectedPostProcessItems = new();
	public ObservableCollection<KeyValuePair<string, string>> SelectedPostProcessItems
	{
		get => _selectedPostProcessItems;
		set => SetProperty(ref _selectedPostProcessItems, value);
	}

	private void OnPostProcessItemSelected(object parameter)
	{
		if (parameter is KeyValuePair<string, string> item)
		{

			SelectedItem = item;
		}
		else
		{
			_logger.LogError($"[PostProcess]Fail to set selected item");
		}
	}

	private void PostProcessItemRemoved(string methodName)
	{
		if (SelectedPostProcessItems.Count <= 0 || CurrentPostProcessModelList.Count <= 0) return;

		var itemToRemove = SelectedPostProcessItems.FirstOrDefault(item => item.Key == methodName);
		if (!itemToRemove.Equals(default(KeyValuePair<string, string>)))
		{
			SelectedPostProcessItems.Remove(itemToRemove);
		}

		var modelToRemove = CurrentPostProcessModelList.FirstOrDefault(model => model.Type.ToString() == methodName);
		var modifiedList = CurrentPostProcessModelList;
		if (modelToRemove != null)
		{
			modifiedList.Remove(modelToRemove);
		}
		CurrentPostProcessModelList = modifiedList;
	}

	private Dictionary<string, List<PostProcessModel>> _postProcessModelDict = new();
	public Dictionary<string, List<PostProcessModel>> PostProcessModelDict
	{
		get => _postProcessModelDict;
		set => SetProperty(ref _postProcessModelDict, value);
	}

	private List<PostProcessModel> _currentPostProcessModelList = new();
	public List<PostProcessModel> CurrentPostProcessModelList
	{
		get => _currentPostProcessModelList;
		set => SetProperty(ref _currentPostProcessModelList, value);
	}

	private ObservableCollection<KeyValuePair<string, string>> _confirmedPostProcessItems = new();
	public ObservableCollection<KeyValuePair<string, string>> ConfirmedPostProcessItems
	{
		get => _confirmedPostProcessItems;
		set => SetProperty(ref _confirmedPostProcessItems, value);
	}

	private void PostProcessItemAdded(object type)
	{
		if (type is not PostProcessType)
		{
			return;
		}
		var postProcessType = (PostProcessType)type;

		if (CurrentPostProcessModelList.Count >= MaxPostProcessCount) return;
		if (CurrentPostProcessModelList is null || CurrentPostProcessModelList.Count == 0)
		{
			CurrentPostProcessModelList = new List<PostProcessModel>();
		}
		var modifiedList = CurrentPostProcessModelList;
		var filterType = SelectKernel.Key.ToString();
		switch (postProcessType)
		{
			case PostProcessType.MotionArtifactReduce:
				modifiedList.Add(new PostProcessModel
				{
					Index = modifiedList.Count(),
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
					Index = modifiedList.Count(),
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
					Index = modifiedList.Count(),
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
					Index = modifiedList.Count(),
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
					Index = modifiedList.Count(),
					Type = postProcessType
				});
				break;

			case PostProcessType.Denoise:
				modifiedList.Add(new PostProcessModel
				{
					Index = modifiedList.Count(),
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
		CurrentPostProcessModelList = modifiedList;
		SelectedPostProcessItems = ConvertPostProcessViewList(CurrentPostProcessModelList);
	}

	public void ConfirmReconPostProcessSettings()
	{
		if (CurrentReconModel is null)
		{
			_logger.LogError("[PostProcess] CurrentReconModel is null. Cannot save PostProcessList.");
			return;
		}
		CurrentReconModel.PostProcesses = DeepCopyPostProcessList(CurrentPostProcessModelList);
		SelectedPostProcessItems = ConvertPostProcessViewList(CurrentPostProcessModelList);
		ConfirmedPostProcessItems = ConvertPostProcessViewList(CurrentPostProcessModelList);
		LogAllPostProcesses();
	}

	// post process parameters
	private int _motionArtifactReduceLevel = 5;
	public int MotionArtifactReduceLevel
	{
		get => _motionArtifactReduceLevel;
		set
		{
			SetProperty(ref _motionArtifactReduceLevel, value);
		}
	}

	// alg not supported yet
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


	private KeyValuePair<int, string> _selectPostProcessDenoiseType;
	public KeyValuePair<int, string> SelectPostProcessDenoiseType
	{
		get => _selectPostProcessDenoiseType;
		set
		{
			SetProperty(ref _selectPostProcessDenoiseType, value);
		}
	}

	private const int MaxPostProcessCount = 8;
	private PostProcessSettingWindow? _postProcessSettingWindow;

	public void ShowPostProcessSettingWindow()
	{
		if (_postProcessSettingWindow is null)
		{
			_postProcessSettingWindow = CTS.Global.ServiceProvider?.GetRequiredService<PostProcessSettingWindow>();
		}
		if (_postProcessSettingWindow is not null)
		{
			LoadPostProcessParameters(CurrentReconModel);

			WindowDialogShow.DialogShow(_postProcessSettingWindow);
		}
	}

	public void PostProcessSettingConfirmed(object parameter)
	{
		if (parameter is not Window window)
			return;

		ConfirmReconPostProcessSettings();
		window.Hide();
	}

	public void PostProcessSettingWindowClosed(object parameter)
	{
		if (parameter is Window window)
		{
			window.Hide();
		}
	}

	/// <summary>
	/// UI上选中后处理列表中的后处理Item
	/// </summary>
	private KeyValuePair<string, string>? _selectedItem;
	public KeyValuePair<string, string>? SelectedItem
	{
		get => _selectedItem;
		set
		{
			SetProperty(ref _selectedItem, value);
		}
	}

	private bool _isConfirmButtonEnable = true;

	public bool IsConfirmButtonEnable
	{
		get => _isConfirmButtonEnable;
		set
		{
			SetProperty(ref _isConfirmButtonEnable, value);
		}
	}

	private void LogAllPostProcesses()
	{
		if (CurrentReconModel?.PostProcesses == null || !CurrentReconModel.PostProcesses.Any())
		{
			_logger.LogInformation("[PostProcess] No post processes available for the current recon model.");
			return;
		}

		var allPostProcessesInfo = CurrentReconModel.PostProcesses.Select(postProcess =>
		{
			var parameters = postProcess.Parameters != null && postProcess.Parameters.Any()
				? string.Join(", ", postProcess.Parameters.Select(p => $"{p.Name}: {p.Value}"))
				: "No parameters";

			return $"Index: {postProcess.Index}, Type: {postProcess.Type}, Parameters: {parameters}";
		});

		var info = string.Join(" ; ", allPostProcessesInfo);
		_logger.LogInformation($"[PostProcess] All PostProcesses for ReconModel ID: {CurrentReconModel.Descriptor.Id} - {info}");
	}
}