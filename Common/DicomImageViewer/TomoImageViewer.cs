//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NVCTImageViewerInterop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using MouseButton = NVCTImageViewerInterop.MouseButton;
using LogLevel = NVCTImageViewerInterop.LogLevel;
using Newtonsoft.Json;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace NV.CT.DicomImageViewer;

public class TomoImageViewer
{
	#region 私有变量  
	private NvImageViewerWrapperCLI _cliWrapper;
	private IntPtr _winFormHandle;
	private int _viewHandle;
	private WindowsFormsHost _windowsFormsHost;
	private UserControl _userControlWinForm;
	private readonly ILogger<TomoImageViewer> _logger;
	private bool _overlayVisibility = true;
	private LookupTable _lookupTable = LookupTable.LUT_BW;
	public event EventHandler<int>? OnCineFinished;
	public event EventHandler<(int index, double pos, int total)>? SliceIndexChanged;
	public event EventHandler<(double ww, double wl)>? WindowWidthLevelChanged;
	public event EventHandler<int>? OnViewLoadFinishedEvent;
	public event EventHandler<List<LOCATION_PARAM>>? LocationWideChangeEvent;
	public event EventHandler<int>? OnCPRFinishedEvent;
	public event EventHandler<bool>? OnMouseForwardToBackwardEvent;
	public event EventHandler<(string name, double[] pointStart, double[] pointEnd, ROI_Common_ViewStyle viewStyle)>? InterventionNeedleEvent;
	public event EventHandler<(int handle, int readerID, int imageTotal)>? SerialLoaded;

	public event EventHandler<TimeDensityInfo>? TimeDensityInfoChangedNotify;
	public event EventHandler<string>? TimeDensityRemoveRoiEvent;

	/// <summary>
	/// 介入针相关事件
	/// </summary>
	public event EventHandler<string>? InterventionSelectNeedleChanged;
	public event EventHandler<string>? InterventionAddNeedleEvent;
	public event EventHandler<string>? InterventionDelectNeedleEvent;
	public event EventHandler<int>? OnROICreateSucceedEvent;

	public WindowsFormsHost WindowsFormsHost
	{
		get => _windowsFormsHost;
		set => _windowsFormsHost = value;
	}

	public string CurrentContentPath
	{
		get; private set;
	}
	#endregion

	#region 构造函数
	public TomoImageViewer(ILogger<TomoImageViewer> logger)
	{
		_logger = logger;
		InitTomoImageViewerPre(725, 685);
		CreateBrowserView();
		InitTomoImageViewerPost(725, 685);
	}

	public TomoImageViewer(int width, int height)
	{
		_logger = CTS.Global.ServiceProvider?.GetService<ILogger<TomoImageViewer>>();
		InitTomoImageViewerPre(width, height);
		CreateBrowserView(width, height);
		InitTomoImageViewerPost(width, height);
	}

	public TomoImageViewer(int width, int height, bool isLoaut = false, ILogger<TomoImageViewer> logger = null)
	{
		_logger = logger;
		InitTomoImageViewerPre(width, height);
		if (isLoaut)
		{
			CreateImageBrowserView(width, height);
		}
		else
		{
			CreateBrowserView(width, height);
		}
		InitTomoImageViewerPost(width, height);
	}

	public TomoImageViewer(int width, int height, ILogger<TomoImageViewer> logger, bool isIntervention = false)
	{
		_logger = logger;
		InitTomoImageViewerPre(width, height);
		if (isIntervention)
		{
			CreateInterventionView(width, height);
		}
		else
		{
			CreateBrowserView(width, height);
		}
		InitTomoImageViewerPost(width, height);
	}

	private void InitTomoImageViewerPre(int width, int height)
	{
		//_logger?.LogDebug($"TomoImageViewer action info:Start Initialize TomoImageViewer {width}*{height}");
		_cliWrapper = new NvImageViewerWrapperCLI();
		_windowsFormsHost = new WindowsFormsHost();
		_windowsFormsHost.AllowDrop = true;
		_windowsFormsHost.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));

		_userControlWinForm = new UserControl();
		_winFormHandle = _userControlWinForm.Handle;
		_windowsFormsHost.Child = _userControlWinForm;
		_cliWrapper.Initialize();

		InitEventHandler();
	}

	private void InitTomoImageViewerPost(int width, int height)
	{
		SetMouseActions();
		//_logger?.LogDebug($"TomoImageViewer action info:End Initialize TomoImageViewer {width}*{height}");
	}

	private void SetMouseActions()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.MouseWheelButtonForwardAndBackward, ViewMouseAction.BrowserMouseAction_Slicing, ROIType.ROI_None);
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.RightMouseButton, ViewMouseAction.BrowserMouseAction_Zoom, ROIType.ROI_None);
	}

	private void CliWrapper_OnCineFinishedNotify(int obj)
	{
		OnCineFinished?.Invoke(this, _viewHandle);
	}

	private void CliWrapper_ImageLogerNotify(LogLevel logLevel, string messageStr)
	{
		switch (logLevel)
		{
			case LogLevel.Debug:
				_logger.LogDebug($"NvImageViewerWrapperCLI:{messageStr}");
				break;
			case LogLevel.Critical:
				_logger.LogCritical($"NvImageViewerWrapperCLI:{messageStr}");
				break;
			case LogLevel.Warning:
				_logger.LogWarning($"NvImageViewerWrapperCLI:{messageStr}");
				break;
			case LogLevel.Error:
				_logger.LogError($"NvImageViewerWrapperCLI:{messageStr}");
				break;
			case LogLevel.Trace:
				_logger.LogTrace($"NvImageViewerWrapperCLI:{messageStr}");
				break;
			case LogLevel.Information:
			case LogLevel.None:
			default:
				_logger?.LogInformation($"NvImageViewerWrapperCLI:{messageStr}");
				break;
		}
	}
	#endregion

	#region Method                                                                                                                                                                                 
	public void CreateBrowserView()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer CreateBrowserView");
		_viewHandle = _cliWrapper.CreateSliceBrowserView(this._winFormHandle, 0, 0, 725, 685);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer CreateBrowserView");
	}

	public void CreateBrowserView(int width, int height)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer CreateBrowserView");
		_viewHandle = _cliWrapper.CreateSliceBrowserView(this._winFormHandle, 0, 0, width, height);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer CreateBrowserView");
	}

	/// <summary>
	/// 可以切换布局的多序列用的
	/// </summary>
	public void CreateImageBrowserView(int width, int height)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer CreateImageBrowserView");
		_viewHandle = _cliWrapper.CreateImageBrowserView(this._winFormHandle, 0, 0, width, height);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer CreateImageBrowserView");
	}

	public void CreateInterventionView(int width, int height)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer CreateInterventionView");
		_viewHandle = _cliWrapper.CreateInterventionView(this._winFormHandle, 0, 0, width, height);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer CreateInterventionView");
	}

	public void ClearView()
	{
		this.WindowsFormsHost.Dispatcher.Invoke(() =>
		{
			//_logger?.LogDebug($"TomoImageViewer action info:Start TomoImageViewer ClearView");
			_cliWrapper.ClearView(_viewHandle);
			//_logger?.LogDebug($"TomoImageViewer action info:End TomoImageViewer ClearView");
		});
		CurrentContentPath = string.Empty;
	}

	public void ClearLocalizer()
	{
		this.WindowsFormsHost.Dispatcher.Invoke(() =>
		{
			//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer ClearLocalizer");
			_cliWrapper.ClearLocalizer(_viewHandle);
			//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer ClearLocalizer");
		});
	}

	private void InitEventHandler()
	{
		_cliWrapper.OnCineFinishedNotify -= CliWrapper_OnCineFinishedNotify;
		_cliWrapper.ImageLogerNotify -= CliWrapper_ImageLogerNotify;
		_cliWrapper.SliceChangedNotify -= CliWrapper_SliceChangedNotify;
		_cliWrapper.WindowLevelChangedNotify += CliWrapper_WindowLevelChangedNotify;
		_cliWrapper.OnCprModifiedNotify -= CliWrapper_OnCprModifiedNotify;
		_cliWrapper.OnMprVrViewLoadFinishNotify -= CliWrapper_OnMprVrViewLoadFinishNotify;
		_cliWrapper.InterventionNeedleNotify -= CliWrapper_InterventionNeedleNotify;
		_cliWrapper.DataReaderFinishedNotify -= CliWrapper_DataReaderFinishedNotify;

		_cliWrapper.OnCineFinishedNotify += CliWrapper_OnCineFinishedNotify;
		_cliWrapper.ImageLogerNotify += CliWrapper_ImageLogerNotify;
		_cliWrapper.SliceChangedNotify += CliWrapper_SliceChangedNotify;
		_cliWrapper.WindowLevelChangedNotify += CliWrapper_WindowLevelChangedNotify;
		_cliWrapper.OnCprModifiedNotify += CliWrapper_OnCprModifiedNotify;
		_cliWrapper.OnMprVrViewLoadFinishNotify += CliWrapper_OnMprVrViewLoadFinishNotify;
		_cliWrapper.InterventionNeedleNotify += CliWrapper_InterventionNeedleNotify;
		_cliWrapper.DataReaderFinishedNotify += CliWrapper_DataReaderFinishedNotify;

		_cliWrapper.InterventionNeedleAddedNotify -= CliWrapper_InterventionNeedleAddedNotify;
		_cliWrapper.InterventionNeedleDeletedNotify -= CiWrapper_InterventionNeedleDeletedNotify;
		_cliWrapper.InterventionNeedleSelectedNotify -= CWrapper_InterventionNeedleSelectedNotify;
		_cliWrapper.ROICreateSucceedNotify -= CliWrapper_OnROICreateSuccceedNotify;

		_cliWrapper.InterventionNeedleAddedNotify += CliWrapper_InterventionNeedleAddedNotify;
		_cliWrapper.InterventionNeedleDeletedNotify += CiWrapper_InterventionNeedleDeletedNotify;
		_cliWrapper.InterventionNeedleSelectedNotify += CWrapper_InterventionNeedleSelectedNotify;
		_cliWrapper.ROICreateSucceedNotify += CliWrapper_OnROICreateSuccceedNotify;

		_cliWrapper.TimeDensityInfoChangedNotify -= CliWrapper_TimeDensityInfoChangedNotify;
		_cliWrapper.TimeDensityInfoChangedNotify += CliWrapper_TimeDensityInfoChangedNotify;

		_cliWrapper.TimeDensityROIDeletedNotify -= CliWrapper_TimeDensityROIDeletedNotify;
		_cliWrapper.TimeDensityROIDeletedNotify += CliWrapper_TimeDensityROIDeletedNotify;
	}

	private void CliWrapper_TimeDensityROIDeletedNotify(int handle, string id)
	{
		TimeDensityRemoveRoiEvent?.Invoke(this, id);
	}

	private void CliWrapper_TimeDensityInfoChangedNotify(int handle, TimeDensityInfo timeDensityInfo)
	{
		TimeDensityInfoChangedNotify?.Invoke(this, timeDensityInfo);
	}

	private void CliWrapper_InterventionNeedleAddedNotify(int handle, string needleName)
	{
		InterventionAddNeedleEvent?.Invoke(this, needleName);
	}

	private void CWrapper_InterventionNeedleSelectedNotify(int handle, string needleName)
	{
		InterventionSelectNeedleChanged?.Invoke(this, needleName);
	}

	private void CiWrapper_InterventionNeedleDeletedNotify(int handle, string needleName)
	{
		InterventionDelectNeedleEvent?.Invoke(this, needleName);
	}

	private void CliWrapper_DataReaderFinishedNotify(int handle, int readerID, int imageTotal)
	{
		SerialLoaded?.Invoke(this, (handle, readerID, imageTotal));
	}

	private void CliWrapper_OnMprVrViewLoadFinishNotify(int obj)
	{
		OnViewLoadFinishedEvent?.Invoke(this, obj);
	}

	private void CliWrapper_OnCprModifiedNotify(int obj)
	{
		OnCPRFinishedEvent?.Invoke(this, obj);
	}

	private void CliWrapper_WindowLevelChangedNotify(int handle, double ww, double wl)
	{
		//TODO:暂时给他去掉
		//WindowWidthLevelChanged?.Invoke(this, (ww, wl));
	}

	private void CliWrapper_SliceChangedNotify(int viewHandle, int index, double pos, int total)
	{
		Task.Run(() =>
		{
			SliceIndexChanged?.Invoke(this, (index, pos, total));
		});
	}

	private void CliWrapper_InterventionNeedleNotify(int viewHandle, NeedlePARAM needlePARAM)
	{
		if (needlePARAM is null)
		{
			return;
		}
		InterventionNeedleEvent?.Invoke(this, (needlePARAM.Name, needlePARAM.PointStart, needlePARAM.PointEnd, needlePARAM.Style));
	}

	private void CliWrapper_OnROICreateSuccceedNotify(int obj)
	{
		OnROICreateSucceedEvent?.Invoke(this, _viewHandle);
	}
	#endregion

	#region 方法   

	/// <summary>
	/// 显示/隐藏 附加层
	/// </summary>
	public void ShowOverlay(bool isShow)
	{
		_cliWrapper.ShowOverlay(_viewHandle, isShow);
	}

	public void ToggleOverlay()
	{
		_overlayVisibility = !_overlayVisibility;
		_cliWrapper.ShowOverlay(_viewHandle, _overlayVisibility);
	}

	/// <summary>
	/// 隐藏 四角信息
	/// </summary>
	public void HiddenText()
	{
		ShowOverlay(false);
	}

	public void SetWindowWidthLevel(double windowWidth, double windowLevel)
	{
		if (windowWidth <= 0 && windowLevel <= 0)
		{
			return;
		}
		_cliWrapper.SetWWWL(_viewHandle, windowWidth, windowLevel);
	}

	public void SetWWWL()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
	}
	/// <summary>
	/// 设置窗宽窗位
	/// </summary>   
	public void SetWWWL(double ww, double wl)
	{
		SetWWWL();
		_cliWrapper.SetWWWL(_viewHandle, ww, wl);
	}

	/// <summary>
	/// 缩放
	/// </summary>
	public void Zoom()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer Zoom");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Zoom, ROIType.ROI_None);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer Zoom");
	}

	public void SetZoomRatio(double zoom)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer SetZoomRatio");
		_cliWrapper.SetZoomRatio(_viewHandle, zoom);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer SetZoomRatio");
	}

	/// <summary>
	/// 2D旋转
	/// </summary>
	public void SetRotate(int degree)
	{
		WindowsFormsHost.Dispatcher.Invoke(() =>
		{
			_cliWrapper.Rotate2D(_viewHandle, degree);
		});
	}

	/// <summary>
	/// 创建圆形ROI
	/// </summary>  
	public void CreateCircleRoi()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Circle);
	}

	public void CreateFreeHandRoi()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_PolygonClosed);
	}

	/// <summary>
	/// 创建矩形 ROI
	/// </summary>
	public void CreateRectangleRoi()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Rect);
	}

	public void CreateSignRoi()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Point);
	}

	/// <summary>
	/// 创建线段
	/// </summary>   
	public void CreateLengthRoi()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Length);
	}

	/// <summary>
	/// 创建箭头
	/// </summary>   
	public void CreateArrowRoi()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Arrow);
	}

	public void CreateTextRoi()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Text);
	}

	/// <summary>
	/// 创建角度
	/// </summary>    
	public void CreateAngleRoi()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer CreateAngleRoi");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Angle);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer CreateAngleRoi");
	}

	/// <summary>
	/// 重置
	/// </summary>
	public void Reset()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer ResetView");
		_cliWrapper.ResetView(_viewHandle);
		ShowOverlay(true);
		SetMouseActions();
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer ResetView");
	}

	public void RemoveAllROI()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TopoImageViewer RemoveAllROI");
		_cliWrapper.RemoveAllROI(_viewHandle);
		//_logger.LogDebug($"TomoImageViewer action info:End TopoImageViewer RemoveAllROI");
	}

	/// <summary>
	/// 设置布局
	/// </summary>
	public void SetLayout(ViewLayout viewLayout)
	{
		_cliWrapper.SetLayout(_viewHandle, viewLayout);
	}

	/// <summary>
	/// 设置布局模式
	/// </summary>
	public void SetImageBrowserMode(BrowseMode browseMode)
	{
		_cliWrapper.SetImageBrowserMode(_viewHandle, browseMode);
	}

	public void SetMultipleViewDataFolder(List<string> folderList)
	{
		_cliWrapper.SetMultipleViewDataFloder(_viewHandle, folderList, 0);
	}

	public void SetMultipleViewDataFolder(int x, int y, List<string> folderList)
	{
		_cliWrapper.SetDropFilesEvent(_viewHandle, x, y, folderList);
	}

	public void ReverseSequence()
	{
		_cliWrapper.Inverse(_viewHandle);
	}

	public void Invert()
	{
		if (_lookupTable == LookupTable.LUT_BW)
		{
			_cliWrapper.SetLookupTable(_viewHandle, LookupTable.LUT_BWInverse);
			_lookupTable = LookupTable.LUT_BWInverse;
		}
		else
		{
			_cliWrapper.SetLookupTable(_viewHandle, LookupTable.LUT_BW);
			_lookupTable = LookupTable.LUT_BW;
		}
	}

	/// <summary>
	/// TODO:暂时没有 SIEImageFilter枚举
	/// </summary>
	/// <param name="imageFilter"></param>
	public void SetKernel(/*SIEImageFilter imageFilter*/)
	{
		//_cliWrapper.SetImageFilter(SIEImageFilter.SIEImageFilterNone);
	}

	public void FlipVertical()
	{
		//TODO:后期鼠标操作统一调整
		//_cliWrapper.SetViewMouseAction(SIEMouseButton.SIELeftMouseButton, SIEBrowserMouseAction.SIEBrowserMouseActionSelector);
		_cliWrapper.FlipY(_viewHandle);
	}

	public void FlipHorizontal()
	{
		//_cliWrapper.SetViewMouseAction(SIEMouseButton.SIELeftMouseButton, SIEBrowserMouseAction.SIEBrowserMouseActionSelector);
		_cliWrapper.FlipX(_viewHandle);
	}

	/// <summary>
	/// 设置为选中状态
	/// </summary>
	public void SetSelector()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer SetSelector");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, ROIType.ROI_None);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer SetSelector");
	}

	/// <summary>
	/// 移动 
	/// </summary>    
	public void Move()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer Move");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Move, ROIType.ROI_None);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer Move");
	}

	public void LoadDicomSeriesReverseOrder()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer CineStartInverse");
		_cliWrapper.Inverse(_viewHandle);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer CineStartInverse");
	}

	public void ShowCursorRelativeValue(bool flag)
	{
		//_logger?.LogDebug($"TomoImageViewer action info:Start TomoImageViewer ShowCursorRelativeValue-{flag}");
		_cliWrapper.ShowCursorRelativeValue(_viewHandle, flag);
		//_logger?.LogDebug($"TomoImageViewer action info:End TomoImageViewer ShowCursorRelativeValue");
	}

	public void SetSliceIndex(int currentIndex)
	{
		if (currentIndex < 0)
		{
			return;
		}
		this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
		{
			_cliWrapper.SetSliceIndex(_viewHandle, currentIndex);
		});
	}

	public void SetInterventionViewSliceIndex(int currentIndex)
	{
		if (currentIndex < 0)
		{
			return;
		}
		_cliWrapper.SetInterventionViewSliceIndex(_viewHandle, currentIndex);
	}

	public void LoadImageWithFilePath(string imageFile, bool isShowRuler = true)
	{
		//_logger.LogDebug($"LoadImageWithFilePath:{imageFile}");
		if (!string.IsNullOrEmpty(imageFile) && File.Exists(imageFile))
		{
			this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
			{
				//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer SetViewDataFile:{imageFile}");
				_cliWrapper.ShowRuler(_viewHandle, isShowRuler);
				ShowOverlay(false);
				_cliWrapper.SetViewDataFile(_viewHandle, imageFile);
				//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer SetViewDataFile");
			});
			CurrentContentPath = "";//不处理单幅图像
		}
	}

	public void LoadImageWithDirectoryPath(string imagePath, bool isShowRuler = true)
	{
		if (!string.IsNullOrEmpty(imagePath) && Directory.Exists(imagePath))
		{
			this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
			{
				//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer SetViewDataFolder:{imagePath}");
				_cliWrapper.ShowRuler(_viewHandle, isShowRuler);
				_cliWrapper.SetViewDataFloder(_viewHandle, imagePath);
				ShowOverlay(true);
				//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer SetViewDataFolder");
			});
			CurrentContentPath = imagePath;
		}
	}

	public void LoadImageWithDirectoryPath3D(string imagePath)
	{
		if (!string.IsNullOrEmpty(imagePath) && Directory.Exists(imagePath))
		{
			this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
			{
				//_logger?.LogDebug($"TomoImageViewer action info:Start TomoImageViewer SetViewDataFolder3D:{imagePath}");
				_cliWrapper.SetViewDataFloder3D(_viewHandle, imagePath);
				ShowOverlay(true);
				//_logger?.LogDebug($"TomoImageViewer action info:End TomoImageViewer SetViewDataFolder3D");
			});
			CurrentContentPath = imagePath;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="imagePath"></param>
	/// <param name="matrixHeight"></param>
	/// <param name="direction">1:头到脚； -1:脚到头 </param>
	public void LoadImageWithDirectoryPath(string imagePath, int matrixHeight, int direction)
	{
		if (!string.IsNullOrEmpty(imagePath) && Directory.Exists(imagePath))
		{
			this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
			{
				//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer SetViewDataFloder:{imagePath}");
				_cliWrapper.SetViewDataFloder(_viewHandle, imagePath, matrixHeight, direction);
				ShowOverlay(true);
				//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer SetViewDataFloder");
			});
		}
	}

	public void SetFourCornersMessage(OverlayTextStyle overlayTextStyle, List<OverlayText> overlayTexts)
	{
		if (overlayTexts.Count > 0)
		{
			this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
			{
				//_logger?.LogDebug($"TomoImageViewer action info:Start TomoImageViewer InitOverlayText");
				_cliWrapper.InitOverlayText(_viewHandle, overlayTextStyle, overlayTexts, 0);
				//_logger?.LogDebug($"TomoImageViewer action info:End TomoImageViewer InitOverlayText");
			});
		}
	}

	/// <summary>
	/// 3D
	/// </summary>
	public int CreateVRMPRView(int left, int right, int width, int height)
	{
		var handle = _cliWrapper.CreateVRMPRView(_winFormHandle, left, right, width, height);
		_viewHandle = handle;
		return handle;
	}
	#endregion

	#region 电影播放相关
	public void SetCineParam(int fps)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer SetCineParam:{fps}");
		int num = _cliWrapper.GetSliceNumber(_viewHandle);
		_cliWrapper.CineRange(_viewHandle, 0, num - 1, 1);
		AdjustFrameRateCine(fps);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer SetCineParam");
	}
	public void CineStart()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer CineStart");
		_cliWrapper.CineStart(_viewHandle);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer CineStart");
	}
	public void CinePause()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer CinePause");
		_cliWrapper.CinePause(_viewHandle);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer CinePause");
	}
	public void CinePauseResume()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer CinePauseResume");
		_cliWrapper.CinePauseResume(_viewHandle);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer CinePauseResume");
	}
	public void MoveToNextSlice()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer MoveToNextSlice");
		_cliWrapper.MoveToNextSlice(_viewHandle);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer MoveToNextSlice");
	}
	public void MoveToPriorSlice()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer MoveToPriorSlice");
		_cliWrapper.MoveToPriorSlice(_viewHandle);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer MoveToPriorSlice");
	}
	public void CineForward()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer CineForward");
		_cliWrapper.CineForward(_viewHandle);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer CineForward");
	}
	public void CineBackward()
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer CineBackward");
		_cliWrapper.CineBackward(_viewHandle);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer CineBackward");
	}
	public void AdjustFrameRateCine(int fps)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start TomoImageViewer AdjustFrameRateCine");
		_cliWrapper.AdjustFrameRateCine(_viewHandle, 1100 / fps);
		//_logger.LogDebug($"TomoImageViewer action info:End TomoImageViewer AdjustFrameRateCine");
	}
	#endregion

	#region Intervention
	public void CreateInterventionNeedle(string needleName, string colorString)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start CreateInterventionNeedle");
		var colorModel = ColorConverter.ConvertFromString(colorString);
		if (colorModel is Color color)
		{
			ROI_Common_ViewStyle rOI_Common_ViewStyle = new ROI_Common_ViewStyle();
			rOI_Common_ViewStyle.LabelColorB = (float)(color.B / 255.0);
			rOI_Common_ViewStyle.LabelColorR = (float)(color.R / 255.0);
			rOI_Common_ViewStyle.LabelColorG = (float)(color.G / 255.0);
			rOI_Common_ViewStyle.ShapeColorG = (float)(color.G / 255.0);
			rOI_Common_ViewStyle.ShapeColorR = (float)(color.R / 255.0);
			rOI_Common_ViewStyle.ShapeColorB = (float)(color.B / 255.0);
			_cliWrapper.CreateInterventionNeedle(this._viewHandle, needleName, rOI_Common_ViewStyle);
		}
		//_logger.LogDebug($"TomoImageViewer action info:End CreateInterventionNeedle");
	}

	public void CreateInterventionNeedle(string needleName, ROI_Common_ViewStyle colorString)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start CreateInterventionNeedle");
		_cliWrapper.CreateInterventionNeedle(this._viewHandle, needleName, colorString);
		//_logger.LogDebug($"TomoImageViewer action info:End CreateInterventionNeedle");
	}

	public void SetInterventionNeedle(string needleName, double[] pointStart, double[] pointEnd, ROI_Common_ViewStyle colorString)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start SetInterventionNeedle");
		_cliWrapper.SetInterventionNeedle(this._viewHandle, new List<NeedlePARAM> { new NeedlePARAM { Name = needleName, PointStart = pointStart, PointEnd = pointEnd, Style = colorString } });
		//_logger.LogDebug($"TomoImageViewer action info:End SetInterventionNeedle");
	}

	public void SetInterventionNeedle(List<NeedlePARAM> needlePARAMs)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start SetInterventionNeedle");
		_cliWrapper.SetInterventionNeedle(this._viewHandle, needlePARAMs);
		//_logger.LogDebug($"TomoImageViewer action info:End SetInterventionNeedle");
	}

	public void DeleteNeedle(string needleName)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start DeleteNeedle");
		_cliWrapper.DeleteNeedle(this._viewHandle, needleName);
		//_logger.LogDebug($"TomoImageViewer action info:End DeleteNeedle");
	}

	public void SelectNeedle(string needleName)
	{
		//_logger.LogDebug($"TomoImageViewer action info:Start SelectNeedle");
		_cliWrapper.SelectNeedle(this._viewHandle, needleName);
		//_logger.LogDebug($"TomoImageViewer action info:End SelectNeedle");
	}

	public void SetInterventionNeedle(ObservableCollection<(string name, double[] pointStart, double[] pointEnd, ROI_Common_ViewStyle rOI_Common_ViewStyle)> needles)
	{
		List<NeedlePARAM> list = new List<NeedlePARAM>();
		foreach (var needle in needles)
		{
			list.Add(new NeedlePARAM
			{
				Name = needle.name,
				PointStart = needle.pointStart,
				PointEnd = needle.pointEnd,
				Style = needle.rOI_Common_ViewStyle
			});
		}
		if (list.Count > 0)
		{
			//_logger.LogDebug($"TomoImageViewer action info:Start SetInterventionNeedle");
			_cliWrapper.SetInterventionNeedle(this._viewHandle, list);
			//_logger.LogDebug($"TomoImageViewer action info:End SetInterventionNeedle");
		}
	}
	#endregion

	#region 时间密度曲线相关
	public void CreateTimeDensityROI(string rioName, string colorString)
	{
		if (!string.IsNullOrEmpty(colorString) && !colorString.Contains("#"))
		{
			colorString = "#" + colorString;
		}
		var colorModel = ColorConverter.ConvertFromString(colorString);
		if (colorModel is Color color)
		{
			ROI_Common_ViewStyle rOI_Common_ViewStyle = new ROI_Common_ViewStyle();
			rOI_Common_ViewStyle.LabelColorB = (float)(color.B / 255.0);
			rOI_Common_ViewStyle.LabelColorR = (float)(color.R / 255.0);
			rOI_Common_ViewStyle.LabelColorG = (float)(color.G / 255.0);
			rOI_Common_ViewStyle.ShapeColorG = (float)(color.G / 255.0);
			rOI_Common_ViewStyle.ShapeColorR = (float)(color.R / 255.0);
			rOI_Common_ViewStyle.ShapeColorB = (float)(color.B / 255.0);
			this.WindowsFormsHost.Dispatcher.Invoke(() =>
			{
				//_logger.LogDebug($"TomoImageViewer action info:Start CreateTimeDensityROI");
				_cliWrapper.CreateTimeDensityROI(this._viewHandle, rioName, rOI_Common_ViewStyle, 10);
				//_logger.LogDebug($"TomoImageViewer action info:End CreateTimeDensityROI");
			});
		}
	}

	public void RemoveTimeDensityROI(string rioName)
	{
		if (!string.IsNullOrEmpty(rioName))
		{
			this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
			{
				//_logger.LogDebug($"TomoImageViewer action info:Start DeleteTimeDensityROI");
				_cliWrapper.DeleteTimeDensityROI(this._viewHandle, rioName);
				//_logger.LogDebug($"TomoImageViewer action info:End DeleteTimeDensityROI");
			});
		}
	}

	public void SetTimeDensityROI(List<TimeDensityInfo> timeDensityInfos)
	{
		if (timeDensityInfos.Count > 0)
		{
			List<MedROI> list = new List<MedROI>();
			foreach (TimeDensityInfo info in timeDensityInfos)
			{
				list.Add(info.RoiParam);
			}			
			this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
			{				
				_cliWrapper.SetTimeDensityROI(this._viewHandle, list);
			});
		}
	}
	#endregion
}