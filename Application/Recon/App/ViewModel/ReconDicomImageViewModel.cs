//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.UI.Exam.Model;
using NV.MPS.Environment;
using NVCTImageViewerInterop;
using System.Collections.Generic;
using System.Windows.Media;

namespace NV.CT.Recon.ViewModel;

public class ReconDicomImageViewModel : DicomImageViewModel
{
	private readonly IImageOperationService _imageOperationService;
	private readonly IProtocolHostService _protocolHostService;
	private readonly ISelectionManager _selectionManager;
	private readonly ITablePositionService _tablePositionService;

	public ReconDicomImageViewModel(IRTDReconService rtdReconService, ISelectionManager selectionManager, TopoImageViewer topoImageViewer, TomoImageViewer tomoImageViewer, IImageOperationService imageOperationService, ILogger<DicomImageViewModel> logger, IProtocolHostService protocolHostService, ITablePositionService tablePositionService, IImageAnnotationService imageAnnotationService, ILogger<TopoImageViewer> topoLogger) : base(rtdReconService, selectionManager, topoImageViewer, tomoImageViewer, imageOperationService, logger, protocolHostService, tablePositionService, imageAnnotationService, topoLogger)
	{
		_imageOperationService = imageOperationService;
		_protocolHostService = protocolHostService;
		_selectionManager = selectionManager;
		_tablePositionService = tablePositionService;

		int width = 726, height = 812;
		//改变容器大小
		TopoImageViewer = new TopoImageViewer(width, height);
		RightTopoImageViewer = new TopoImageViewer(width, height);
		TomoImageViewer = new TomoImageViewer(width, height);

		TomoImageViewer.SliceIndexChanged += TomoImageViewer_SliceIndexChanged;

		LeftWindowsFormsHost = TopoImageViewer.WindowsFormsHost;
		RightWindowsFormsHost = TomoImageViewer.WindowsFormsHost;

		_imageOperationService.SetImageSliceLocationChanged += ImageOperationService_SetImageSliceLocationChanged;
		_selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;

		_tablePositionService.TablePositionChanged -= TablePositionChanged;
		_tablePositionService.TablePositionChanged += TablePositionChanged;

		TopoImageViewer.OnLocationSeriesParamChanged -= TopoImageViewerWrapper_OnLocationSeriesParamChanged;
		TopoImageViewer.OnLocationSeriesParamChanged += TopoImageViewerWrapper_OnLocationSeriesParamChanged;
		TopoImageViewer.OnLocalizerSelectionChanged -= TopoImageViewer_OnLocalizerSelectionChanged;
		TopoImageViewer.OnLocalizerSelectionChanged += TopoImageViewer_OnLocalizerSelectionChanged;
		InitImageViewrFourCornersInfo();

		TopoImageViewer.SerialLoaded -= TopoImageViewer_SerialLoaded;
		TopoImageViewer.SerialLoaded += TopoImageViewer_SerialLoaded;
		RightTopoImageViewer.SerialLoaded -= RightTopoImageViewer_SerialLoaded;
		RightTopoImageViewer.SerialLoaded += RightTopoImageViewer_SerialLoaded;
		RightTopoImageViewer.OnLocalizerSelectionChanged -= TopoImageViewer_OnLocalizerSelectionChanged;
		RightTopoImageViewer.OnLocalizerSelectionChanged += TopoImageViewer_OnLocalizerSelectionChanged;
		TomoImageViewer.SerialLoaded -= TomoImageViewer_SerialLoaded;
		TomoImageViewer.SerialLoaded += TomoImageViewer_SerialLoaded;

		TomoImageViewer.TimeDensityInfoChangedNotify -= TomoImageViewer_TimeDensityInfoChangedNotify;
		TomoImageViewer.TimeDensityInfoChangedNotify += TomoImageViewer_TimeDensityInfoChangedNotify;

		TomoImageViewer.TimeDensityRemoveRoiEvent -= TomoImageViewer_TimeDensityRemoveRoiEvent;
		TomoImageViewer.TimeDensityRemoveRoiEvent += TomoImageViewer_TimeDensityRemoveRoiEvent;
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
	private void TomoImageViewer_TimeDensityInfoChangedNotify(object? sender, TimeDensityInfo e)
	{
		_imageOperationService.SetTimeDensityInfoChanged(JsonConvert.SerializeObject(e));
	}

	[UIRoute]
	private void TomoImageViewer_SerialLoaded(object? sender, (int handle, int readerID, int imageTotal) e)
	{
		TomoImageViewer.SetZoomRatio(0.82);
	}

	[UIRoute]
	private void TopoImageViewer_SerialLoaded(object? sender, (int handle, int readerID, int imageTotal) e)
	{
		TopoImageViewer.SetZoomRatio(0.82);
	}

	/// <summary>
	/// 床位服务变化事件响应代码
	/// </summary>
	public override void TablePositionChanged(object? sender, EventArgs<TablePositionInfo> e)
	{
		//do nothing
	}

	[UIRoute]
	private void SelectionManager_SelectionReconChanged(object? sender, EventArgs<ReconModel> e)
	{
		if (e is null || e.Data is null)
		{
			return;
		}

		var selectedRecon = _selectionManager.CurrentSelectionRecon;
		if (selectedRecon is null)
			return;

		if (string.IsNullOrEmpty(selectedRecon.ImagePath))
		{
			_imageOperationService.SetImageCount(0);
		}
		else
		{
			var imageCount = 0;
			if (Directory.Exists(selectedRecon.ImagePath))
			{
				imageCount = Directory.GetFiles(selectedRecon.ImagePath, "*.dcm").Count();
			}

			_imageOperationService.SetImageCount(imageCount);
		}
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

			//高级重建模块不允操作这个ROI图
			timeDensityPARAM.IsDynamic = false;

			timeDensityInfo.RoiParam = timeDensityPARAM;
			timeDensityInfos.Add(timeDensityInfo);
		}
		if (timeDensityInfos.Any())
		{
			TomoImageViewer.SetTimeDensityROI(timeDensityInfos);
		}
	}

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

	[UIRoute]
	private void ImageOperationService_SetImageSliceLocationChanged(object? sender, EventArgs<int> e)
	{
		if (e is null)
		{
			return;
		}
		TomoImageViewer.SetSliceIndex(e.Data);
	}
}