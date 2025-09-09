//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.CTS.Models;

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