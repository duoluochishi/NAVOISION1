//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.DicomImageViewer;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.UI.Exam.Contract;
using NV.CT.UI.Exam.Extensions;
using NVCTImageViewerInterop;
using System.Windows.Forms.Integration;
using ParameterNames = NV.CT.Protocol.Models.ProtocolParameterNames;
using ColorConverter = System.Windows.Media.ColorConverter;
using NV.MPS.Environment;
using NV.CT.Alg.ScanReconCalculation.Recon.FovMatrix;
namespace NV.CT.UI.Exam.ViewModel;

public class DicomImageViewModel : BaseViewModel, IDicomImageViewModel
{
	private readonly ILogger<DicomImageViewModel> _logger;
	private readonly IRTDReconService _rtdControlService;
	private readonly ISelectionManager _selectionManager;
	private readonly IProtocolHostService _protocolHostService;
	private readonly IImageOperationService _imageOperationService;
	private readonly ITablePositionService _tablePositionService;
	private readonly IImageAnnotationService _imageAnnotationService;
	private bool _isTomoLoading;
	private bool isShowHU = true;
	private bool _isSwitchTopo;

	public bool IsSwitchTopo
	{
		get => _isSwitchTopo;
		set => _isSwitchTopo = value;
	}

	public bool IsShowTomo
	{
		get
		{
			return RightWindowsFormsHost == TomoImageViewer.WindowsFormsHost;
		}
		set
		{
			if (value)
			{
				if (RightWindowsFormsHost == TopoImageViewer.WindowsFormsHost)
				{
					TopoImageViewer.ClearLocalizer();
					TopoImageViewer.ClearView();
				}
				RightWindowsFormsHost = TomoImageViewer.WindowsFormsHost;
			}
			else
			{
				if (RightWindowsFormsHost == TomoImageViewer.WindowsFormsHost)
				{
					TomoImageViewer.ClearView();
				}
				RightWindowsFormsHost = RightTopoImageViewer.WindowsFormsHost;
			}
		}
	}

	private WindowsFormsHost _leftWindowsFormsHost = new WindowsFormsHost();
	public WindowsFormsHost LeftWindowsFormsHost
	{
		get { return _leftWindowsFormsHost; }
		set { SetProperty(ref _leftWindowsFormsHost, value); }
	}

	private WindowsFormsHost _rightwindowsFormsHost = new WindowsFormsHost();
	public WindowsFormsHost RightWindowsFormsHost
	{
		get { return _rightwindowsFormsHost; }
		set { SetProperty(ref _rightwindowsFormsHost, value); }
	}

	public TopoImageViewer TopoImageViewer { get; set; }
	/// <summary>
	/// 双定位像用到的侧位像控件（默认）
	/// </summary>
	public TopoImageViewer RightTopoImageViewer { get; set; }

	public TomoImageViewer TomoImageViewer { get; set; }

	public ReconModel? TopoImageReconModel { get; set; }

	public ReconModel? TomoImageReconModel { get; set; }
	private ScanModel? CurrentScanModel { get; set; }
	/// <summary>
	/// 双定位像的270度recon模型（默认）
	/// </summary>
	public ReconModel? RightTopoImageReconModel { get; set; }

	public string selectReconID = string.Empty;

	public DicomImageViewModel(IRTDReconService rtdReconService,
		ISelectionManager selectionManager,
		TopoImageViewer topoImageViewer,
		TomoImageViewer tomoImageViewer,
		IImageOperationService imageOperationService,
		ILogger<DicomImageViewModel> logger,
		IProtocolHostService protocolHostService,
		ITablePositionService tablePositionService,
		IImageAnnotationService imageAnnotationService,
		ILogger<TopoImageViewer> topoLogger)
	{
		_logger = logger;
		TopoImageViewer = topoImageViewer;
		TomoImageViewer = tomoImageViewer;
		RightTopoImageViewer = new TopoImageViewer(topoLogger);

		_protocolHostService = protocolHostService;
		_imageOperationService = imageOperationService;
		_rtdControlService = rtdReconService;
		_selectionManager = selectionManager;

		LeftWindowsFormsHost = TopoImageViewer.WindowsFormsHost;
		RightWindowsFormsHost = TomoImageViewer.WindowsFormsHost;

		_tablePositionService = tablePositionService;

		_rtdControlService.ImageReceived -= ReconControlService_ReconImageReceived;
		_rtdControlService.ImageReceived += ReconControlService_ReconImageReceived;

		_rtdControlService.ReconDone -= RtdControlService_ReconDone;
		_rtdControlService.ReconDone += RtdControlService_ReconDone;

		_imageOperationService.ClickToolButtonChanged -= ImageOperationService_ClickToolButtonChanged;
		_imageOperationService.ClickToolButtonChanged += ImageOperationService_ClickToolButtonChanged;

		_selectionManager.SelectionReconChanged -= SelectionManager_SelectionReconChanged;
		_selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;

		_selectionManager.SelectionScanChanged -= SelectionManager_SelectionScanChanged;
		_selectionManager.SelectionScanChanged += SelectionManager_SelectionScanChanged;

		_tablePositionService.TablePositionChanged -= TablePositionChanged;
		_tablePositionService.TablePositionChanged += TablePositionChanged;

		_protocolHostService.PerformStatusChanged -= ProtocolPerformStatusService_PerformStatusChanged;
		_protocolHostService.PerformStatusChanged += ProtocolPerformStatusService_PerformStatusChanged;

		_protocolHostService.VolumnChanged -= ModificationService_VolumnChanged;
		_protocolHostService.VolumnChanged += ModificationService_VolumnChanged;

		_protocolHostService.StructureChanged -= ProtocolHostService_StructureChanged;
		_protocolHostService.StructureChanged += ProtocolHostService_StructureChanged;

		_protocolHostService.ParameterChanged -= ProtocolHostService_ParameterChanged;
		_protocolHostService.ParameterChanged += ProtocolHostService_ParameterChanged;

		TopoImageViewer.OnLocationSeriesParamChanged -= TopoImageViewerWrapper_OnLocationSeriesParamChanged;
		TopoImageViewer.OnLocationSeriesParamChanged += TopoImageViewerWrapper_OnLocationSeriesParamChanged;
		TopoImageViewer.OnLocalizerSelectionChanged -= TopoImageViewer_OnLocalizerSelectionChanged;
		TopoImageViewer.OnLocalizerSelectionChanged += TopoImageViewer_OnLocalizerSelectionChanged;
		TopoImageViewer.WindowWidthLevelChanged -= TopoImageViewer_WindowWidthLevelChanged;
		TopoImageViewer.WindowWidthLevelChanged += TopoImageViewer_WindowWidthLevelChanged;

		RightTopoImageViewer.OnLocationSeriesParamChanged -= TopoImageViewerWrapper_OnLocationSeriesParamChanged;
		RightTopoImageViewer.OnLocationSeriesParamChanged += TopoImageViewerWrapper_OnLocationSeriesParamChanged;
		RightTopoImageViewer.OnLocalizerSelectionChanged -= TopoImageViewer_OnLocalizerSelectionChanged;
		RightTopoImageViewer.OnLocalizerSelectionChanged += TopoImageViewer_OnLocalizerSelectionChanged;
		RightTopoImageViewer.WindowWidthLevelChanged -= TopoImageViewer_WindowWidthLevelChanged;
		RightTopoImageViewer.WindowWidthLevelChanged += TopoImageViewer_WindowWidthLevelChanged;

		TomoImageViewer.SliceIndexChanged -= TomoImageViewer_SliceIndexChanged;
		TomoImageViewer.SliceIndexChanged += TomoImageViewer_SliceIndexChanged;
		TomoImageViewer.WindowWidthLevelChanged -= TomoImageViewer_WindowWidthLevelChanged;
		TomoImageViewer.WindowWidthLevelChanged += TomoImageViewer_WindowWidthLevelChanged;

		_imageOperationService.SetImageSliceLocationChanged -= ImageOperationService_SetImageSliceLocationChanged;
		_imageOperationService.SetImageSliceLocationChanged += ImageOperationService_SetImageSliceLocationChanged;

		_imageOperationService.SwitchViewsChanged -= ImageOperationService_SwitchViewsChanged;
		_imageOperationService.SwitchViewsChanged += ImageOperationService_SwitchViewsChanged;

		_imageAnnotationService = imageAnnotationService;
		InitImageViewrFourCornersInfo();

		TopoImageViewer.SerialLoaded -= TopoImageViewer_SerialLoaded;
		TopoImageViewer.SerialLoaded += TopoImageViewer_SerialLoaded;
		TomoImageViewer.SerialLoaded -= TomoImageViewer_SerialLoaded;
		TomoImageViewer.SerialLoaded += TomoImageViewer_SerialLoaded;
		RightTopoImageViewer.SerialLoaded -= RightTopoImageViewer_SerialLoaded;
		RightTopoImageViewer.SerialLoaded += RightTopoImageViewer_SerialLoaded;

		TomoImageViewer.TimeDensityInfoChangedNotify -= TomoImageViewer_TimeDensityInfoChangedNotify;
		TomoImageViewer.TimeDensityInfoChangedNotify += TomoImageViewer_TimeDensityInfoChangedNotify;

		_imageOperationService.CommondToTimeDensityEvent -= ImageOperationService_CommondToTimeDensityEvent;
		_imageOperationService.CommondToTimeDensityEvent += ImageOperationService_CommondToTimeDensityEvent;

		TomoImageViewer.TimeDensityRemoveRoiEvent -= TomoImageViewer_TimeDensityRemoveRoiEvent;
		TomoImageViewer.TimeDensityRemoveRoiEvent += TomoImageViewer_TimeDensityRemoveRoiEvent;
	}

	[UIRoute]
	private void ProtocolHostService_ParameterChanged(object? sender, EventArgs<(BaseModel baseModel, List<string> list)> e)
	{
		if (e is null || e.Data.baseModel is null || e.Data.list is null)
		{
			return;
		}
		if (e.Data.baseModel is ScanModel scanModel
			&& CurrentScanModel is not null
			&& scanModel.Descriptor.Id == CurrentScanModel.Descriptor.Id
			&& scanModel.Status == PerformStatus.Unperform
			&& e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_OPTION)) is not null)
		{
			HandleImageContentDisplay();
		}
	}

	[UIRoute]
	private void RtdControlService_ReconDone(object? sender, EventArgs<RealtimeReconInfo> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}

		//_logger.LogInformation($"RealtimeReconInfo from RtdControlService_ReconDone:{JsonConvert.SerializeObject(e.Data)};Select TomoImageReconModel:{JsonConvert.SerializeObject(TomoImageReconModel)}");
		if (TomoImageReconModel is not null && TomoImageReconModel.Descriptor.Id.Equals(e.Data.ReconId))
		{
			var lastImage = e.Data.LastImage;
			string filePath = lastImage;
			if (string.IsNullOrEmpty(lastImage))
			{
				filePath = e.Data.ImagePath;
			}
			else
			{
				if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
				{
					filePath = Path.GetDirectoryName(filePath);
				}
			}
			TomoImageViewer.LoadImageWithFilePath(filePath);
			HandleUnperformROIInfo(TomoImageReconModel);
		}
	}

	[UIRoute]
	private void TomoImageViewer_TimeDensityRemoveRoiEvent(object? sender, string e)
	{
		if (e is null || string.IsNullOrEmpty(e))
		{
			return;
		}
		_imageOperationService.SetTimeDensityRoiRemoved(e);
	}

	[UIRoute]
	private void ImageOperationService_CommondToTimeDensityEvent(object? sender, EventArgs<(string commandStr, string param)> e)
	{
		if (e is null)
		{
			return;
		}
		switch (e.Data.commandStr)
		{
			case CommandParameters.COMMAND_ADD:
				if (!string.IsNullOrEmpty(e.Data.param) && e.Data.param.IndexOf(',') > 0)
				{
					var pl = e.Data.param.Split(',');
					TomoImageViewer.CreateTimeDensityROI(pl[0], pl[1]);
				}
				break;
			case CommandParameters.COMMAND_REMOVE:
				if (!string.IsNullOrEmpty(e.Data.param))
				{
					TomoImageViewer.RemoveTimeDensityROI(e.Data.param);
				}
				break;
			default:
				break;
		}
	}

	[UIRoute]
	private void TomoImageViewer_TimeDensityInfoChangedNotify(object? sender, TimeDensityInfo e)
	{
		_imageOperationService.SetTimeDensityInfoChanged(JsonConvert.SerializeObject(e));
	}

	[UIRoute]
	public void RightTopoImageViewer_SerialLoaded(object? sender, (int handle, int readerID, int imageTotal) e)
	{
		RightTopoImageViewer.SetZoomRatio(0.75);
	}

	[UIRoute]
	private void TomoImageViewer_SerialLoaded(object? sender, (int handle, int readerID, int imageTotal) e)
	{
		TomoImageViewer.SetZoomRatio(0.8);
	}

	[UIRoute]
	private void TopoImageViewer_SerialLoaded(object? sender, (int handle, int readerID, int imageTotal) e)
	{
		TopoImageViewer.SetZoomRatio(0.75);
	}

	[UIRoute]
	private void ProtocolHostService_StructureChanged(object? sender, EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)> e)
	{
		//添加删除重建需要刷新计划框
		if (e is not null
			&& e.Data.Current is ReconModel
			&& e.Data.ChangeType is StructureChangeType.Add or StructureChangeType.Delete)
		{
			//todo:这里的逻辑后续可能需要重新确认一下。先解决exception
			var recon = e.Data.Current as ReconModel;
			if (recon?.Parent.ScanImageType is ScanImageType.Tomo)
			{
				HandleImageContentDisplay();
			}
			//新增recon的时候，默认选中新增的线框
			if (e.Data.ChangeType is StructureChangeType.Add)
			{
				var reconAdd = e.Data.Current as ReconModel;
				if (reconAdd?.Parent.ScanImageType is ScanImageType.Tomo)
				{
					HandleLocalizerSelection(reconAdd.Descriptor.Id);
				}
			}
		}
	}

	[UIRoute]
	private void SelectionManager_SelectionScanChanged(object? sender, EventArgs<ScanModel> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		CurrentScanModel = e.Data;
		HandleImageContentDisplay();
	}

	[UIRoute]
	public virtual void TopoImageViewer_OnLocalizerSelectionChanged(object? sender, string e)
	{
		if (!selectReconID.Equals(e))
		{
			_imageOperationService.SetSelectionReconID(e);
			selectReconID = e;
		}
		HandleLocalizerSelection(e);
	}

	/// <summary>
	/// 切换双定位像的视图位置
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	/// <exception cref="NotImplementedException"></exception>
	[UIRoute]
	private void ImageOperationService_SwitchViewsChanged(object? sender, EventArgs<bool> e)
	{
		IsSwitchTopo = !IsSwitchTopo;
		HandleImageContentDisplay();
	}

	/// <summary>
	/// 初始化四角配置信息
	/// </summary>
	public void InitImageViewrFourCornersInfo()
	{
		var (topoTextStyle, topoTexts) = ImageSettingToOverlayText.Get(_imageAnnotationService.GetConfigs().ScanTopoSettings);
		if (topoTexts.Count > 0)
		{
			TopoImageViewer.SetFourCornersMessage(topoTextStyle, topoTexts);
			RightTopoImageViewer.SetFourCornersMessage(topoTextStyle, topoTexts);
		}
		var (tomoTextStyle, tomoTexts) = ImageSettingToOverlayText.Get(_imageAnnotationService.GetConfigs().ScanTomoSettings);
		if (tomoTexts.Count > 0)
		{
			TomoImageViewer.SetFourCornersMessage(tomoTextStyle, tomoTexts);
		}
	}

	/// <summary>
	/// 设置断层图像的显示索引
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[UIRoute]
	private void ImageOperationService_SetImageSliceLocationChanged(object? sender, EventArgs<int> e)
	{
		if (e is null)
		{
			return;
		}
		TomoImageViewer.SetSliceIndex(e.Data);
	}

	/// <summary>
	/// 断层图像的窗宽窗位返回来保存进协议
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[UIRoute]
	private void TomoImageViewer_WindowWidthLevelChanged(object? sender, (double ww, double wl) e)
	{
		//TODO:暂时给他去掉
		//DicomImageExtension.HandleWindowWidthLevelChanged(_protocolHostService, TomoImageReconModel, e.wl, e.ww);
	}

	/// <summary>
	/// 定位像返回来的窗宽窗位值保存进协议
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[UIRoute]
	private void TopoImageViewer_WindowWidthLevelChanged(object? sender, (double ww, double wl) e)
	{
		//TODO:暂时给他去掉
		//TopoImageViewer.SetWindowWidthLevel(e.ww, e.wl);
		//RightTopoImageViewer.SetWindowWidthLevel(e.ww, e.wl);
		//DicomImageExtension.HandleWindowWidthLevelChanged(_protocolHostService, TopoImageReconModel, e.wl, e.ww);
		//if (TopoImageReconModel is not null && TopoImageReconModel.Parent is not null && TopoImageReconModel.Parent.ScanOption == ScanOption.DualScout)
		//{
		//	var anotherTopoRtd = TopoImageReconModel.Parent.Children.SingleOrDefault(x => x != TopoImageReconModel);
		//	if (anotherTopoRtd is not null)
		//	{
		//		DicomImageExtension.HandleWindowWidthLevelChanged(_protocolHostService, anotherTopoRtd, e.wl, e.ww);
		//	}
		//}
	}

	/// <summary>
	/// recon选中变更事件响应代码
	/// </summary>
	[UIRoute]
	private void SelectionManager_SelectionReconChanged(object? sender, EventArgs<ReconModel> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		HandleImageContentDisplay(false);
		//在定位像上选中指定Recon
		var reconId = e.Data.Descriptor.Id;
		HandleLocalizerSelection(reconId);
		if (e.Data is not null
			&& e.Data.Parent is ScanModel currentScan
			&& currentScan.Status != PerformStatus.Performing
			&& (currentScan.ScanOption == ScanOption.NVTestBolus
				|| currentScan.ScanOption == ScanOption.TestBolus
				|| currentScan.ScanOption == ScanOption.NVTestBolusBase))
		{
			_imageOperationService.DeleteAllTimeDensityRoi();
			if (e.Data is ReconModel recon
				&& (currentScan.ScanOption == ScanOption.NVTestBolus
				|| currentScan.ScanOption == ScanOption.TestBolus))
			{
				DicomImageExtension.SetTestBolusCycleROIsByBase(_protocolHostService, recon);
			}
		}
		HandleROIInfo(e.Data);
	}

	private void HandleROIInfo(ReconModel reconModel)
	{
		if (reconModel is not null
			&& reconModel.Parent is ScanModel currentScan
			&& reconModel.Status == PerformStatus.Performed
			&& reconModel.CycleROIs is not null
			&& reconModel.CycleROIs.Count > 0)
		{
			bool isTestBolus = false;
			if (currentScan.ScanOption == ScanOption.NVTestBolus
				|| currentScan.ScanOption == ScanOption.TestBolus)
			{
				isTestBolus = true;
			}
			SetTimeDensityROIByCycleROIs(reconModel.CycleROIs, isTestBolus);
		}
	}

	private void SetTimeDensityROIByCycleROIs(List<CycleROIModel> list, bool isTestBolus = false)
	{
		List<TimeDensityInfo> timeDensityInfos = new List<TimeDensityInfo>();
		for (int i = 0; i < list.Count; i++)
		{
			TimeDensityInfo timeDensityInfo = new TimeDensityInfo();
			NVCTImageViewerInterop.MedROI timeDensityPARAM = new NVCTImageViewerInterop.MedROI();
			timeDensityPARAM.Id = "1";
			var colorModel = ColorConverter.ConvertFromString("#1E90FF");
			if (i == 1)
			{
				timeDensityPARAM.Id = "2";
				colorModel = ColorConverter.ConvertFromString("#5DE2E7");
			}
			if (i == 2)
			{
				timeDensityPARAM.Id = "3";
				colorModel = ColorConverter.ConvertFromString("#EEEE00");
			}
			if (colorModel is System.Windows.Media.Color color)
			{
				ROI_Common_ViewStyle rOI_Common_ViewStyle = new ROI_Common_ViewStyle();
				rOI_Common_ViewStyle.LabelColorB = (float)(color.B / 255.0);
				rOI_Common_ViewStyle.LabelColorR = (float)(color.R / 255.0);
				rOI_Common_ViewStyle.LabelColorG = (float)(color.G / 255.0);
				rOI_Common_ViewStyle.ShapeColorG = (float)(color.G / 255.0);
				rOI_Common_ViewStyle.ShapeColorR = (float)(color.R / 255.0);
				rOI_Common_ViewStyle.ShapeColorB = (float)(color.B / 255.0);
				timeDensityPARAM.Style = rOI_Common_ViewStyle;
			}
			timeDensityPARAM.Points.Add(new NVCTImageViewerInterop.NVPoint()
			{
				x = UnitConvert.Micron2Millimeter((double)list[i].CenterX),
				y = UnitConvert.Micron2Millimeter((double)list[i].CenterY),
				z = UnitConvert.Micron2Millimeter((double)list[i].CenterZ)
			});

			timeDensityPARAM.InfoDictionary = new Dictionary<string, string>();
			timeDensityPARAM.InfoDictionary.Add("Radius", "10");
			timeDensityPARAM.RoiTyppe = (ROIType)9;

			timeDensityPARAM.IsDynamic = !isTestBolus;

			timeDensityInfo.RoiParam = timeDensityPARAM;
			timeDensityInfos.Add(timeDensityInfo);
		}
		if (timeDensityInfos.Any())
		{
			TomoImageViewer.SetTimeDensityROI(timeDensityInfos);
		}
	}

	public void HandleLocalizerSelection(string reconId)
	{
		TopoImageViewer.SetLocationSelected(reconId);
		if (!IsShowTomo)
		{
			RightTopoImageViewer.SetLocationSelected(reconId);
		}
	}

	[UIRoute]
	private void ModificationService_VolumnChanged(object? sender, EventArgs<(BaseModel baseModel, List<string>)> e)
	{
		if (e is null || e.Data.baseModel is null)
		{
			return;
		}
		if (e.Data.baseModel is ScanModel scanModel
			&& _selectionManager.LastSelectionTopoScan is not null
			//&& _selectionManager.LastSelectionTopoScan.Status == PerformStatus.Performed
			&& scanModel == _selectionManager.LastSelectionTomoScan)
		{
			var selectedID = TopoImageViewer.GetLocationSelected();
			DicomImageExtension.LoadPlanBox(TopoImageViewer, scanModel, TopoImageReconModel);
			TopoImageViewer.SetLocationSelected(selectedID);
			TopoImageViewer.ShowScanLine(true);

			if (!IsShowTomo)
			{
				DicomImageExtension.LoadPlanBox(RightTopoImageViewer, scanModel, RightTopoImageReconModel);
				RightTopoImageViewer.SetLocationSelected(selectedID);
				RightTopoImageViewer.ShowScanLine(true);
			}
		}
	}

	/// <summary>
	/// 协议状态变更事件响应代码
	/// </summary>
	[UIRoute]
	private void ProtocolPerformStatusService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		if (e is null || e.Data.Model is null)
		{
			return;
		}
		//扫描状态发生变化时的显示切换
		if (e.Data.Model is ScanModel sm && sm.ScanImageType is ScanImageType.Tomo && e.Data.NewStatus is PerformStatus.Unperform or PerformStatus.Performing)
		{
			HandleImageContentDisplay();
		}

		//重建状态发生变化时的显示切换
		if (e.Data.Model is ReconModel && e.Data.NewStatus == PerformStatus.Performed)
		{
			HandleImageContentDisplay();
		}

		//根据重建状态，设置计划框颜色。
		if (e.Data.Model is ReconModel reconModel && reconModel.Parent.ScanOption is not ScanOption.Surview or ScanOption.DualScout)
		{
			TopoImageViewer.SetScanPlanBoxColor(reconModel, e.Data.NewStatus);
			if (!IsShowTomo)
			{
				RightTopoImageViewer.SetScanPlanBoxColor(reconModel, e.Data.NewStatus);
			}
		}
		//根据扫描状态，设置计划框颜色。
		if (e.Data.Model is ScanModel scanModel
			&& !(scanModel.ScanOption == ScanOption.Surview || scanModel.ScanOption == ScanOption.DualScout))
		{
			TopoImageViewer.SetScanPlanBoxColor(scanModel, e.Data.NewStatus);
			if (!IsShowTomo)
			{
				RightTopoImageViewer.SetScanPlanBoxColor(scanModel, e.Data.NewStatus);
			}
		}
		//if (e.Data.Model is ReconModel recon
		//	&& recon.Parent is ScanModel scan
		//	&& !(scan.ScanOption is ScanOption.Surview or ScanOption.DualScout))
		//{
		//	if (e.Data.NewStatus is PerformStatus.Performing)
		//	{

		//	}
		//	if (e.Data.NewStatus is PerformStatus.Performed)
		//	{

		//	}
		//}
	}

	[UIRoute]
	public void TopoImageViewerWrapper_OnLocationSeriesParamChanged(object? sender, List<LocationParam> locationParams)
	{
		if (locationParams is null || locationParams.Count == 0)
		{
			return;
		}
		var list = ProtocolHelper.Expand(_protocolHostService.Instance);
		var model = list.FirstOrDefault(t => t.Scan.Descriptor.Id == locationParams[0].ScanID);
		if (model.Scan is null)
		{
			return;
		}
		var resultDic = new Dictionary<BaseModel, List<ParameterModel>>();
		var rtdLP = locationParams.FirstOrDefault(t => t.IsChild == false);

		List<ParameterModel> sacnParameterModels = new List<ParameterModel>();
		if (rtdLP is not null)
		{
			////这里要注意！！！扫描参数中的所有内容都是设备坐标系下的，重加参数中的所有内容都是患者坐标系下的。拿到的参数要经过适当转换！！！
			var cfz_p = rtdLP.CenterFirstZ;
			var clz_p = rtdLP.CenterLastZ;
			var cfz_d = CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(model.Scan.PatientPosition.Value, cfz_p);
			var clz_d = CoordinateConverter.Instance.TransformSliceLocationPatientToDevice(model.Scan.PatientPosition.Value, clz_p);

			sacnParameterModels.AddRange(new List<ParameterModel> {
				new ParameterModel{Name = ParameterNames.SCAN_RECON_VOLUME_START_POSITION,Value= ((int)cfz_d).ToString()},
				new ParameterModel{Name = ParameterNames.SCAN_RECON_VOLUME_END_POSITION,Value = ((int)clz_d).ToString()},
				new ParameterModel{Name = ParameterNames.SCAN_LENGTH,Value =((uint)Math.Abs(rtdLP.CenterLastZ - rtdLP.CenterFirstZ)).ToString()},
			});
		}
		resultDic.Add(model.Scan, sacnParameterModels);
		foreach (var param in locationParams)
		{
			var recon = model.Scan.Children.FirstOrDefault(t => t.Descriptor.Id == param.LocationSeriesUID);
			if (recon is not null)
			{
				/*
				//FOV发生变化后，有可能造成matrix重算。 
				//当前策略最小像素0.1mm，若低于0.1需要找最近的。
				var minFov = ReconFovMatrixHelper.GetMinFov();
				var newFOV = param.FoVLengthHor > minFov ? param.FoVLengthHor : minFov;
				var newMatrix = ReconFovMatrixHelper.GetSuitableMatrix(newFOV, recon.ImageMatrixHorizontal);

				List<ParameterModel> parameterModels = new List<ParameterModel> {
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X,Value= param.FOVDirectionHorX.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y,Value = param.FOVDirectionHorY.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z,Value = param.FOVDirectionHorZ.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X,Value = param.FOVDirectionVerX.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y,Value = param.FOVDirectionVerY.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z,Value = param.FOVDirectionVerZ.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_FIRST_X,Value =((int) param.CenterFirstX).ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y,Value = ((int)param.CenterFirstY).ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z,Value =((int) param.CenterFirstZ).ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_LAST_X,Value = ((int)param.CenterLastX).ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_LAST_Y,Value =((int) param.CenterLastY).ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_LAST_Z,Value = ((int)param.CenterLastZ).ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL,Value =((int) newFOV).ToString()},   //配合算法这两个字段暂时不能联动修改
                    new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL,Value =((int) newFOV).ToString()},      //配合算法这两个字段暂时不能联动修改
					new ParameterModel{Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_HORIZONTAL,Value = newMatrix.ToString()},      //配合算法这两个字段暂时不能联动修改
					new ParameterModel{Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL,Value = newMatrix.ToString()},      //配合算法这两个字段暂时不能联动修改

                };
				*/

				//20250723 临时方案代码，fov固定337.92和506.88，center固定0，matrix调整增加4500,4。
				var newFov = ReconFovMatrixHelper.GetSuitableFOVTemp(param.FoVLengthHor);
				var newMatrix = ReconFovMatrixHelper.GetSuitableMatrix(newFov, recon.ImageMatrixHorizontal, recon.IsHDRecon);

				List<ParameterModel> parameterModels = new List<ParameterModel> {
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X,Value= param.FOVDirectionHorX.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y,Value = param.FOVDirectionHorY.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z,Value = param.FOVDirectionHorZ.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X,Value = param.FOVDirectionVerX.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y,Value = param.FOVDirectionVerY.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z,Value = param.FOVDirectionVerZ.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_FIRST_X,Value = 0.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_FIRST_Y,Value = 0.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_FIRST_Z,Value = ((int) param.CenterFirstZ).ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_LAST_X,Value =  0.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_LAST_Y,Value = 0.ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_CENTER_LAST_Z,Value = ((int) param.CenterLastZ).ToString()},
					new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL,Value =((int) newFov).ToString()},   //配合算法这两个字段暂时不能联动修改
                    new ParameterModel{Name = ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL,Value =((int) newFov).ToString()},      //配合算法这两个字段暂时不能联动修改
					new ParameterModel{Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_HORIZONTAL,Value = newMatrix.ToString()},      //配合算法这两个字段暂时不能联动修改
					new ParameterModel{Name = ProtocolParameterNames.RECON_IMAGE_MATRIX_VERTICAL,Value = newMatrix.ToString()},      //配合算法这两个字段暂时不能联动修改
					
                };
				resultDic.Add(recon, parameterModels);
			}
		}
		_protocolHostService.SetParameters(resultDic);
		ScanLengthHelper.GetCorrectedScanLength(_protocolHostService, model.Scan);  //参数设置后校准扫描长度
	}

	/// <summary>
	/// 断层图像索引变更事件响应代码
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[UIRoute]
	private void TomoImageViewer_SliceIndexChanged(object? sender, (int index, double position, int total) e)
	{
		if (TomoImageReconModel is not null && TomoImageReconModel.Status == PerformStatus.Performed)
		{
			TopoImageViewer.SetScanLinePosition(e.position);
		}
		_imageOperationService.SetImageSliceIndex(e.index);
		_imageOperationService.SetCenterPositon(e.position);
	}

	/// <summary>
	/// ReconImageReceiced事件响应代码
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>	
	private void ReconControlService_ReconImageReceived(object? sender, EventArgs<RealtimeReconInfo> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		//_logger.LogInformation($"RealtimeReconInfo from ReconControlService_ReconImageReceived:{JsonConvert.SerializeObject(e.Data)},Select Topo: {JsonConvert.SerializeObject(TopoImageReconModel)}");
		var lastImage = e.Data.LastImage;
		if (TopoImageReconModel is not null && TopoImageReconModel.Descriptor.Id.Equals(e.Data.ReconId) && TopoImageReconModel.IsRTD)
		{
			DicomImageExtension.LoadTopoImage(TopoImageViewer, TopoImageReconModel, lastImage, true);
		}
		if (RightTopoImageReconModel is not null && RightTopoImageReconModel.Descriptor.Id.Equals(e.Data.ReconId) && RightTopoImageReconModel.IsRTD)
		{
			DicomImageExtension.LoadTopoImage(RightTopoImageViewer, RightTopoImageReconModel, lastImage, true);
		}
		//_logger.LogInformation($"RealtimeReconInfo from ReconControlService_ReconImageReceived:{JsonConvert.SerializeObject(e.Data)},Select Tomo: {JsonConvert.SerializeObject(TomoImageReconModel)}");
		if (TomoImageReconModel is not null && TomoImageReconModel.Descriptor.Id.Equals(e.Data.ReconId))
		{
			TomoImageViewer.LoadImageWithFilePath(lastImage);
			HandleUnperformROIInfo(TomoImageReconModel);
		}
	}

	/// <summary>
	/// 扫描页面右侧工具栏的按钮按下事件响应代码
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	[UIRoute]
	private void ImageOperationService_ClickToolButtonChanged(object? sender, EventArgs<string> e)
	{
		if (e is null || e.Data is null || string.IsNullOrEmpty(e.Data))
		{
			return;
		}
		switch (e.Data)
		{
			case CommandParameters.IMAGE_OPERATE_SELECT:
				TopoImageViewer.SetSelector();
				TomoImageViewer.SetSelector();

				RightTopoImageViewer.SetSelector();
				break;
			case CommandParameters.IMAGE_OPERATE_HU:
				TomoImageViewer.ShowCursorRelativeValue(isShowHU);
				isShowHU = !isShowHU;
				break;
			case CommandParameters.IMAGE_OPERATE_ZOOM:
				TopoImageViewer.Zoom();
				TomoImageViewer.Zoom();

				RightTopoImageViewer.Zoom();
				break;
			case CommandParameters.IMAGE_OPERATE_MOVE:
				TopoImageViewer.Move();
				TomoImageViewer.Move();

				RightTopoImageViewer.Move();
				break;
			case CommandParameters.IMAGE_OPERATE_WL:
				TopoImageViewer.SetWWWL();
				TomoImageViewer.SetWWWL();

				RightTopoImageViewer.SetWWWL();
				break;
			case CommandParameters.IMAGE_OPERATE_ROI:
				TopoImageViewer.CreateCircleRoi();
				TomoImageViewer.CreateCircleRoi();

				RightTopoImageViewer.CreateCircleRoi();
				break;
			case CommandParameters.IMAGE_OPERATE_LENGTH:
				TopoImageViewer.CreateLengthRoi();
				TomoImageViewer.CreateLengthRoi();

				RightTopoImageViewer.CreateLengthRoi();
				break;
			case CommandParameters.IMAGE_OPERATE_ANGLE:
				TopoImageViewer.CreateAngleRoi();
				TomoImageViewer.CreateAngleRoi();

				RightTopoImageViewer.CreateAngleRoi();
				break;
			case CommandParameters.IMAGE_OPERATE_ARROW:
				TopoImageViewer.CreateArrowRoi();
				TomoImageViewer.CreateArrowRoi();

				RightTopoImageViewer.CreateArrowRoi();
				break;
			case CommandParameters.IMAGE_OPERATE_REVERSE:
				TomoImageViewer.LoadDicomSeriesReverseOrder();
				break;
			case CommandParameters.IMAGE_OPERATE_REWORK:
				TopoImageViewer.Reset();
				TomoImageViewer.Reset();
				RightTopoImageViewer.Reset();

				TopoImageViewer.RemoveAllROI();
				TomoImageViewer.RemoveAllROI();
				RightTopoImageViewer.RemoveAllROI();
				//设置缩放比例，定位像设置成0.75倍，断层图像设置成0.8倍
				TopoImageViewer.SetZoomRatio(0.75);
				TomoImageViewer.SetZoomRatio(0.8);
				RightTopoImageViewer.SetZoomRatio(0.75);
				break;
			case CommandParameters.IMAGE_OPERATE_CROP:
				break;
			default:
				TopoImageViewer.SetSelector();
				TomoImageViewer.SetSelector();

				RightTopoImageViewer.SetSelector();
				break;
		}
	}

	/// <summary>
	/// 处理非扫描过程中图像控件的内容与显示。
	/// </summary>
	private void HandleImageContentDisplay(bool clearTopo = true)
	{
		var selectedTopo = _selectionManager.LastSelectionTopoScan;
		var selectedTomoScan = _selectionManager.LastSelectionTomoScan;
		var currentSelection = _selectionManager.CurrentSelection;
		_logger.LogInformation($"current Selection: topo={selectedTopo?.Descriptor.Id}/{selectedTopo?.Status},tomo={selectedTomoScan?.Descriptor.Id}/{selectedTomoScan?.Status}");
		if (selectedTopo is null)
		{
			//无定位
			IsShowTomo = true;
			TopoImageReconModel = null;
			RightTopoImageReconModel = null;
			TomoImageReconModel = _selectionManager.CurrentSelectionRecon;
			_logger.LogInformation("HandleImageContentDisplay 无选中定位像");
		}
		else if (selectedTopo.ScanOption is ScanOption.Surview)
		{
			//单定位
			IsShowTomo = true;
			TopoImageReconModel = selectedTopo.Children[0];
			RightTopoImageReconModel = null;
			if (selectedTopo.Status is PerformStatus.Unperform)
			{
				if (currentSelection.Scan == selectedTopo)
				{
					TomoImageReconModel = null;
					_logger.LogInformation("HandleImageContentDisplay 单定位像，当前点击Topo序列，定位像未完成，定位像、断层显示为空。");
				}
				else if (currentSelection.Scan.Status is PerformStatus.Unperform)
				{
					TomoImageReconModel = null;
					_logger.LogInformation("HandleImageContentDisplay 单定位像，当前点击Tomo序列，Topo、tomo未完成，定位像、断层显示为空。");
				}
				else
				{
					TomoImageReconModel = _selectionManager.CurrentSelectionRecon;
					_logger.LogInformation("HandleImageContentDisplay 单定位像，当前点击Tomo序列，Topo未完成，tomo已完成，定位像显示为空，断层应该显示RTD序列。");
				}
			}
			else
			{
				if (_selectionManager.CurrentSelectionRecon is null)
				{
					TomoImageReconModel = null;
					_logger.LogInformation("HandleImageContentDisplay 单定位像，选中当前断层重建图像,极可能是当前recon被删除的时候。");
				}
				else if (_selectionManager.CurrentSelectionRecon.Parent.ScanImageType is not ScanImageType.Topo)
				{
					TomoImageReconModel = _selectionManager.CurrentSelectionRecon;
					_logger.LogInformation("HandleImageContentDisplay 单定位像，选中当前断层重建图像");
				}
				else
				{
					_logger.LogInformation("HandleImageContentDisplay 单定位像，维持选中的断层重建图像");
				}
			}
		}
		else if (selectedTopo.ScanOption is ScanOption.DualScout && selectedTopo.Status is not PerformStatus.Performed)
		{
			//双定位，定位像未完成
			IsShowTomo = false;
			if (selectedTopo.Children.Count > 1)
			{
				TopoImageReconModel = IsSwitchTopo ? selectedTopo.Children[1] : selectedTopo.Children[0];
				RightTopoImageReconModel = IsSwitchTopo ? selectedTopo.Children[0] : selectedTopo.Children[1];
			}
			TomoImageReconModel = null;
			_logger.LogInformation("HandleImageContentDisplay 双定位像，定位像未完成");
		}
		else
		{
			//双定位，定位像已完成。
			if (selectedTomoScan is null)
			{
				//双定位，无断层选中
				IsShowTomo = false;
				if (selectedTopo is not null && selectedTopo.Children.Count > 1)
				{
					TopoImageReconModel = IsSwitchTopo ? selectedTopo.Children[1] : selectedTopo.Children[0];
					RightTopoImageReconModel = IsSwitchTopo ? selectedTopo.Children[0] : selectedTopo.Children[1];
				}
				TomoImageReconModel = null;
				_logger.LogInformation("HandleImageContentDisplay 双定位像，定位像已完成，无断层重建选中");
			}
			else if (selectedTomoScan.Status is PerformStatus.Unperform)
			{
				//双定位，断层计划阶段
				IsShowTomo = false;
				if (selectedTopo is not null && selectedTopo.Children.Count > 1)
				{
					TopoImageReconModel = IsSwitchTopo ? selectedTopo.Children[1] : selectedTopo.Children[0];
					RightTopoImageReconModel = IsSwitchTopo ? selectedTopo.Children[0] : selectedTopo.Children[1];
				}
				TomoImageReconModel = null;
				_logger.LogInformation("HandleImageContentDisplay 双定位像，定位像已完成，断层计划阶段");
			}
			else if (selectedTomoScan.Status is PerformStatus.Performing)
			{
				//双定位，断层开始，右边切换到tomo，对应recon切换为断层的rtd
				IsShowTomo = true;
				if (selectedTopo is not null && selectedTopo.Children.Count > 1)
				{
					TopoImageReconModel = IsSwitchTopo ? selectedTopo.Children[1] : selectedTopo.Children[0];
				}
				RightTopoImageReconModel = null;
				TomoImageReconModel = selectedTomoScan.Children.FirstOrDefault(x => x.IsRTD);
				_logger.LogInformation("HandleImageContentDisplay 双定位像，定位像已完成，断层已加载。");
			}
			else if (selectedTomoScan.Status is PerformStatus.Performed)
			{
				//双定位，断层扫描结束
				IsShowTomo = true;
				if (selectedTopo is not null && selectedTopo.Children.Count > 1)
				{
					TopoImageReconModel = IsSwitchTopo ? selectedTopo.Children[1] : selectedTopo.Children[0];
				}
				RightTopoImageReconModel = null;
				if (_selectionManager.CurrentSelectionRecon.Parent.ScanImageType is not ScanImageType.Topo)
				{
					TomoImageReconModel = _selectionManager.CurrentSelectionRecon;
					_logger.LogInformation("HandleImageContentDisplay 双定位像，选中当前断层重建图像");
				}
				else
				{
					_logger.LogInformation("HandleImageContentDisplay 双定位像，维持选中的断层重建图像");
				}
			}
		}

		if (TopoImageReconModel is null || string.IsNullOrEmpty(TopoImageReconModel.ImagePath))
		{
			DicomImageExtension.HandleUnPerformedTopoSelection(TopoImageViewer);
		}
		else
		{
			if (clearTopo)
			{
				DicomImageExtension.HandlePerformedTopoSelection(TopoImageViewer, selectedTomoScan, TopoImageReconModel, _tablePositionService.CurrentTablePosition.HorizontalPosition);
			}
		}

		if (!IsShowTomo)
		{
			if (RightTopoImageReconModel is null || string.IsNullOrEmpty(RightTopoImageReconModel.ImagePath))
			{
				DicomImageExtension.HandleUnPerformedTopoSelection(RightTopoImageViewer);
			}
			else
			{
				DicomImageExtension.HandlePerformedTopoSelection(RightTopoImageViewer, selectedTomoScan, RightTopoImageReconModel, _tablePositionService.CurrentTablePosition.HorizontalPosition);
			}
		}
		else
		{
			if (TomoImageReconModel is null || string.IsNullOrEmpty(TomoImageReconModel.ImagePath))
			{
				DicomImageExtension.HandleUnperformedTomoSelection(TomoImageViewer, TopoImageViewer);
			}
			else
			{
				DicomImageExtension.LoadTomoImage(_imageOperationService, TomoImageViewer, TopoImageViewer, TomoImageReconModel, TomoImageReconModel.ImagePath, ref _isTomoLoading);
			}
		}
	}

	/// <summary>
	/// 床位服务变化事件响应代码
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>	
	public virtual void TablePositionChanged(object? sender, EventArgs<TablePositionInfo> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}
		var horizontalPosition = e.Data.HorizontalPosition;
		_logger.LogDebug($"DicomImageViewModel action SetTablePosition:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + ":" + e.Data.HorizontalPosition}");
		DicomImageExtension.SetTablePositionOnCurrentSelectedTopo(TopoImageViewer, TopoImageReconModel, horizontalPosition);
		if (!IsShowTomo)
		{
			DicomImageExtension.SetTablePositionOnCurrentSelectedTopo(RightTopoImageViewer, RightTopoImageReconModel, horizontalPosition);
		}
	}

	private void HandleUnperformROIInfo(ReconModel reconModel)
	{
		if (reconModel is not null
			&& reconModel.Status != PerformStatus.Unperform
			&& reconModel.Parent is ScanModel currentScan
			&& (currentScan.ScanOption == ScanOption.NVTestBolus
				|| currentScan.ScanOption == ScanOption.TestBolus)
			&& reconModel.CycleROIs is not null
			&& reconModel.CycleROIs.Count > 0)
		{
			SetTimeDensityROIByCycleROIs(reconModel.CycleROIs, true);
		}
	}
}