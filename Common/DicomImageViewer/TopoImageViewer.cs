//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/10/26 15:52:00           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.Protocol.Models;
using NVCTImageViewerInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using TaskStatus = NVCTImageViewerInterop.TaskStatus;
using NV.MPS.Environment;
using LogLevel = NVCTImageViewerInterop.LogLevel;
using System.Windows.Threading;
using Microsoft.VisualBasic.Logging;

namespace NV.CT.DicomImageViewer;

public class TopoImageViewer
{
	#region 私有变量  
	private NvImageViewerWrapperCLI _cliWrapper;
	private IntPtr _winFormHandle;
	private int _viewHandle;
	private WindowsFormsHost _windowsFormsHost;
	private readonly ILogger<TopoImageViewer> _logger;
	private string SelectLocalizerID = string.Empty;

	public WindowsFormsHost WindowsFormsHost
	{
		get { return _windowsFormsHost; }
		set { _windowsFormsHost = value; }
	}

	/// <summary>
	/// 当前显示内容路径
	/// </summary>
	public string CurrentContentPath
	{
		get; private set;
	}

	public event EventHandler<List<LocationParam>>? OnLocationSeriesParamChanged;
	public event EventHandler<(double ww, double wl)>? WindowWidthLevelChanged;
	public event EventHandler<string>? OnLocalizerSelectionChanged;
	public event EventHandler<(int handle, int readerID, int imageTotal)>? SerialLoaded;
	public event EventHandler<int>? OnROICreateSucceedEvent;
	#endregion

	#region 构造函数
	public TopoImageViewer(ILogger<TopoImageViewer> logger)
	{
		_logger = logger;
		//_logger.LogDebug($"TopoImageViewer action info:Start Initialize TopoImageViewer");
		_cliWrapper = new NvImageViewerWrapperCLI();
		_windowsFormsHost = new WindowsFormsHost();
		_windowsFormsHost.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
		UserControl _userControlWinForm = new UserControl();
		_winFormHandle = _userControlWinForm.Handle;
		_windowsFormsHost.Child = _userControlWinForm;

		_cliWrapper.Initialize();
		CreateLocatorView();
		InitEventHandler();
		InitScoutImageViewerStyle();
		_cliWrapper.ImageLogerNotify += CliWrapper_ImageLogerNotify;

		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.MouseWheelButtonForwardAndBackward, ViewMouseAction.BrowserMouseAction_Zoom, ROIType.ROI_None);
		SetLocationLimtParams(51.2, 10);
		//_logger.LogDebug($"TopoImageViewer action info:End Initialize TopoImageViewer");
	}

	public TopoImageViewer(int width, int height)
	{
		_logger = CTS.Global.ServiceProvider.GetService<ILogger<TopoImageViewer>>();
		//_logger.LogDebug($"TopoImageViewer action info:Start Initialize TopoImageViewer");
		_cliWrapper = new NvImageViewerWrapperCLI();
		_windowsFormsHost = new WindowsFormsHost();
		_windowsFormsHost.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
		UserControl _userControlWinForm = new UserControl();
		_winFormHandle = _userControlWinForm.Handle;
		_windowsFormsHost.Child = _userControlWinForm;

		_cliWrapper.Initialize();
		CreateLocatorView(width, height);
		InitEventHandler();
		InitScoutImageViewerStyle();
		_cliWrapper.ImageLogerNotify += CliWrapper_ImageLogerNotify;

		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.MouseWheelButtonForwardAndBackward, ViewMouseAction.BrowserMouseAction_Zoom, ROIType.ROI_None);
		SetLocationLimtParams(51.2, 10);
		//_logger.LogDebug($"TopoImageViewer action info:End Initialize TopoImageViewer");		
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
				_logger.LogInformation($"NvImageViewerWrapperCLI:{messageStr}");
				break;
		}
	}

	/// <summary>
	/// 线框参数改变
	/// </summary>
	/// <param name="arg"></param>
	/// <param name="list"></param>
	private void ImageViewerWrapperScout_LocationWideChangeEvent(int arg, List<LOCATION_PARAM> list)
	{
		List<LocationParam> lpList = new List<LocationParam>();
		foreach (var item in list)
		{
			lpList.Add(ConvertFromCliLocationParam(item));
		}
		if (lpList.Count > 0)
		{
			OnLocationSeriesParamChanged?.Invoke(this, lpList);
		}
	}

	#endregion

	#region Method  
	public void CreateLocatorView()
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer CreateLocatorView");
		_viewHandle = _cliWrapper.CreateLocatorView(this._winFormHandle, 0, 0, 725, 685);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer CreateLocatorView");
	}

	public void CreateLocatorView(int Width, int Height)
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer CreateLocatorView");
		_viewHandle = _cliWrapper.CreateLocatorView(this._winFormHandle, 0, 0, Width, Height);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer CreateLocatorView");
	}

	private void SetLocationLimtParams(double minFov = 21.5, double minLength = 10)
	{
		LOCATION_LIMIT_PARAM limitParam = new LOCATION_LIMIT_PARAM();
		limitParam.MinScanLength = minLength;
		limitParam.MinScanFov = minFov;
		limitParam.MaxScanFov = 200000;
		limitParam.MaxScanLength = 200000;
		limitParam.MinLocationPostion = -2200000;
		limitParam.MaxLocationPostion = 200000;

		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetLocationLimtParam");
		_cliWrapper.SetLocationLimtParam(this._viewHandle, limitParam);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetLocationLimtParam");
	}

	public void ClearView()
	{
		this.WindowsFormsHost.Dispatcher.Invoke(() =>
		{
			//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer ClearView");
			_cliWrapper.ClearView(_viewHandle);
			//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer ClearView");
		});
		CurrentContentPath = string.Empty;
	}

	public void ClearLocalizer()
	{
		this.WindowsFormsHost.Dispatcher.Invoke(() =>
		{
			//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer ClearLocalizer");
			_cliWrapper.ClearLocalizer(_viewHandle);
			//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer ClearLocalizer");
		});
	}

	private void SetLocationLimtParam(LOCATION_LIMIT_PARAM limitParam)
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetLocationLimtParam");
		_cliWrapper.SetLocationLimtParam(_viewHandle, limitParam);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetLocationLimtParam");
	}

	private void AddLocationReconParam(LOCATION_PARAM param)
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer AddLocationReconParam");
		_cliWrapper.AddLocationReconParam(_viewHandle, param);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer AddLocationReconParam");
	}

	private void RemoveLocationSerieParam(string uid)
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer RemoveLocationReconParam:{uid}");
		_cliWrapper.RemoveLocationReconParam(_viewHandle, uid);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer RemoveLocationReconParam");
	}

	private void SetLocationDisplayStatus(LOCATION_DISPLAYSTATUS status)
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetLocationDisplayStatus");
		_cliWrapper.SetLocationDisplayStatus(_viewHandle, status);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetLocationDisplayStatus");
	}

	private void SetMouseActions()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.MouseWheelButtonForwardAndBackward, ViewMouseAction.BrowserMouseAction_Slicing, ROIType.ROI_None);
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.RightMouseButton, ViewMouseAction.BrowserMouseAction_Zoom, ROIType.ROI_None);
	}

	private void InitEventHandler()
	{
		_cliWrapper.LocationWideChangeNotify -= ImageViewerWrapperScout_LocationWideChangeEvent;
		_cliWrapper.LocationSelectedChangedNotify -= CliWrapper_LocationSelectedChangedNotify;
		_cliWrapper.WindowLevelChangedNotify -= ImageViewerWrapperScout_WindowLevelChangedNotify;
		_cliWrapper.DataReaderFinishedNotify -= CliWrapper_DataReaderFinishedNotify;
		_cliWrapper.ROICreateSucceedNotify -= CliWrapper_OnROICreateSuccceedNotify;

		_cliWrapper.LocationWideChangeNotify += ImageViewerWrapperScout_LocationWideChangeEvent;
		_cliWrapper.LocationSelectedChangedNotify += CliWrapper_LocationSelectedChangedNotify;
		_cliWrapper.WindowLevelChangedNotify += ImageViewerWrapperScout_WindowLevelChangedNotify;
		_cliWrapper.DataReaderFinishedNotify += CliWrapper_DataReaderFinishedNotify;
		_cliWrapper.ROICreateSucceedNotify += CliWrapper_OnROICreateSuccceedNotify;
	}

	private void CliWrapper_DataReaderFinishedNotify(int handle, int readerID, int imageTotal)
	{
		SerialLoaded?.Invoke(this, (handle, readerID, imageTotal));
	}

	private void ImageViewerWrapperScout_WindowLevelChangedNotify(int handle, double ww, double wl)
	{
		//TODO:暂时给他去掉
		//WindowWidthLevelChanged?.Invoke(this, (ww, wl));
	}

	private void CliWrapper_LocationSelectedChangedNotify(int handle, string localizerID)
	{
		if (!SelectLocalizerID.Equals(localizerID))
		{
			OnLocalizerSelectionChanged?.Invoke(this, localizerID);
			SelectLocalizerID = localizerID;
		}
	}

	private void InitScoutImageViewerStyle()
	{
		Dictionary<LOCATION_VIEWSTATUS, LOCATION_VIEWSTYLE> dic = new Dictionary<LOCATION_VIEWSTATUS, LOCATION_VIEWSTYLE>();

		LOCATION_VIEWSTYLE loc = GetLOCATION_VIEWSTYLE();
		loc.lineColorB = 0.4627f;
		loc.lineColorG = 0.4627f;
		loc.lineColorR = 0.4627f;
		loc.fontColorB = 0.0f;
		loc.fontColorG = 0.38f;
		loc.fontColorR = 1.00f;
		loc.reconAreaHightLightColorB = 0.75f;
		loc.reconAreaHightLightColorG = 0.84f;
		loc.reconAreaHightLightColorR = 0.25f;
		dic.Add(LOCATION_VIEWSTATUS.UnPerformedStyle, loc);

		loc = GetLOCATION_VIEWSTYLE();
		loc.lineColorB = 0.25f;
		loc.lineColorG = 0.65f;
		loc.lineColorR = 0.99f;
		loc.fontColorB = 0.0f;
		loc.fontColorG = 0.38f;
		loc.fontColorR = 1.00f;
		loc.reconAreaHightLightColorB = 0.25f;
		loc.reconAreaHightLightColorG = 0.65f;
		loc.reconAreaHightLightColorR = 0.99f;
		dic.Add(LOCATION_VIEWSTATUS.PerformingStyle, loc);

		loc = GetLOCATION_VIEWSTYLE(); ;
		loc.lineColorB = 0.82f;
		loc.lineColorG = 0.43f;
		loc.lineColorR = 0.00f;
		loc.fontColorB = 0.0f;
		loc.fontColorG = 0.38f;
		loc.fontColorR = 1.00f;
		loc.reconAreaHightLightColorB = 0.82f;
		loc.reconAreaHightLightColorG = 0.43f;
		loc.reconAreaHightLightColorR = 0.00f;

		dic.Add(LOCATION_VIEWSTATUS.PerformSucceedStyle, loc);

		loc = GetLOCATION_VIEWSTYLE();
		loc.lineColorB = 0.31f;
		loc.lineColorG = 0.30f;
		loc.lineColorR = 0.99f;
		loc.fontColorB = 0.0f;
		loc.fontColorG = 0.38f;
		loc.fontColorR = 1.00f;
		loc.reconAreaHightLightColorB = 0.31f;
		loc.reconAreaHightLightColorG = 0.30f;
		loc.reconAreaHightLightColorR = 0.99f;

		dic.Add(LOCATION_VIEWSTATUS.PerformFaildStyle, loc);
		this.WindowsFormsHost.Dispatcher.Invoke(() =>
		{
			_cliWrapper.SetLocationSeriesViewStyle(_viewHandle, dic);
		});
	}

	private LOCATION_VIEWSTYLE GetLOCATION_VIEWSTYLE()
	{
		return new LOCATION_VIEWSTYLE
		{
			HighlightLineColorR = 0.25f,
			HighlightLineColorG = 0.84f,
			HighlightLineColorB = 0.75f,
			HighlightLineThickness = 1.3f,
			reconAreaOpacticy = 0.2f,
			fontSize = 10,
			lineThickness = 1.3f,
			fontStyle = 0
		};
	}

	private void CliWrapper_OnROICreateSuccceedNotify(int obj)
	{
		OnROICreateSucceedEvent?.Invoke(this, _viewHandle);
	}
	#endregion

	#region 方法
	public void SetWindowWidthLevel(double windowWidth, double windowLevel)
	{
		if (windowWidth <= 0 && windowLevel <= 0)
		{
			return;
		}
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetWWWL");
		_cliWrapper.SetWWWL(_viewHandle, windowWidth, windowLevel);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetWWWL");
	}

	/// <summary>
	/// 加载线框参数
	/// </summary>
	/// <param name="list"></param>
	public void LoadPlanBox(List<LocationParam> list)
	{
		ClearLocalizer();
		List<LOCATION_PARAM> lists = new List<LOCATION_PARAM>();
		foreach (var recon in list)
		{
			lists.Add(ConvertToCliLocationParam(recon));
		}
		var selectedId = GetLocationSelected();
		if (!string.IsNullOrWhiteSpace(selectedId)
			&& lists.FirstOrDefault(x => x.taskStatus.locationSeriesUID == selectedId) is LOCATION_PARAM lOCATION_PARAM)
		{
			lOCATION_PARAM.taskStatus.IsSelected = true;
		}
		if (lists.Count > 0)
		{
			this.WindowsFormsHost.Dispatcher.Invoke(() =>
			{
				//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetLocationSeriesParam");
				_cliWrapper.SetLocationSeriesParam(_viewHandle, lists);
				//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetLocationSeriesParam");
			});
		}
	}

	public void SetLocationSelected(string uid, bool selected = true)
	{
		if (_cliWrapper.GetLocationSelectedUID(_viewHandle).Equals(uid))
		{
			return;
		}
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetLocationSelected {uid}");
		_cliWrapper.SetLocationSelected(_viewHandle, uid, selected);
		SelectLocalizerID = uid;
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetLocationSelected");
	}

	public string GetLocationSelected()
	{
		return _cliWrapper.GetLocationSelectedUID(_viewHandle);
	}

	public void ShowScanLine(bool isShown)
	{
		_cliWrapper.ShowScanLine(_viewHandle, isShown);
	}

	public void SetScanLinePosition(double pos)
	{
		this.WindowsFormsHost.Dispatcher.Invoke(DispatcherPriority.Background, () =>
		{
			_cliWrapper.SetScanLinePosition(_viewHandle, (float)pos);
			_logger.LogDebug($"TopoImageViewer action SetScanLinePosition:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + ":" + pos}");
		});
	}

	/// <summary>
	/// 设置线框状态从而设置线框颜色
	/// </summary>
	/// <param name="st"></param>
	/// <param name="performStatus"></param>
	public void SetScanPlanBoxColor(ReconModel reconModel, PerformStatus performStatus)
	{
		LOCATION_TASKSTATUS locationStatus = new LOCATION_TASKSTATUS();
		locationStatus.locationSeriesUID = reconModel.Descriptor.Id;
		locationStatus = GetLocationStatus(locationStatus, performStatus, reconModel.FailureReason);
		this.WindowsFormsHost.Dispatcher.Invoke(() =>
		{
			//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetLocationTask");
			_cliWrapper.SetLocationTask(_viewHandle, locationStatus.locationSeriesUID, locationStatus.taskStatus);
			//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetLocationTask");
		});
	}

	/// <summary>
	/// 根据扫描参数更新整个重建线框的状态
	/// </summary>
	/// <param name="scanModel">扫描模型</param>
	/// <param name="performStatus">线框的状态</param>
	public void SetScanPlanBoxColor(ScanModel scanModel, PerformStatus performStatus)
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetLocationTask");
		var rtdRecon = scanModel.Children.FirstOrDefault(x => x.IsRTD);
		var displayPerformStatus = performStatus;
		//有可能出现扫描完成，但RTD未完成的情况。此时框线应仍处于扫描过程中状态。
		if (rtdRecon is not null
			&& rtdRecon.Status is not PerformStatus.Performed
			&& performStatus is PerformStatus.Performed)
		{
			displayPerformStatus = PerformStatus.Performing;
		}
		foreach (var recon in scanModel.Children)
		{
			LOCATION_TASKSTATUS locationStatus = new LOCATION_TASKSTATUS();
			locationStatus.locationSeriesUID = recon.Descriptor.Id;
			if (displayPerformStatus == PerformStatus.Performed)
			{
				locationStatus = GetLocationStatus(locationStatus, recon.Status, recon.FailureReason);
			}
			else
			{
				locationStatus = GetLocationStatus(locationStatus, displayPerformStatus, recon.FailureReason);
			}
			this.WindowsFormsHost.Dispatcher.Invoke(() =>
			{
				_cliWrapper.SetLocationTask(_viewHandle, locationStatus.locationSeriesUID, locationStatus.taskStatus);
			});
		}
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetLocationTask");
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="locationStatus"></param>
	/// <param name="performStatus"></param>
	/// <param name="failureReasonType"></param>
	/// <returns></returns>
	private LOCATION_TASKSTATUS GetLocationStatus(LOCATION_TASKSTATUS locationStatus, PerformStatus performStatus, FailureReasonType failureReasonType)
	{
		switch (performStatus)
		{
			case PerformStatus.Unperform:
				locationStatus.taskStatus = TaskStatus.UnPerformed;
				break;
			case PerformStatus.Waiting:
				locationStatus.taskStatus = TaskStatus.Performing;
				break;
			case PerformStatus.Performed:
				if (failureReasonType == FailureReasonType.None)
				{
					locationStatus.taskStatus = TaskStatus.PerformSucceed;
				}
				else
				{
					locationStatus.taskStatus = TaskStatus.PerformFaild;
				}
				break;
			case PerformStatus.Performing:
				locationStatus.taskStatus = TaskStatus.Performing;
				break;
			default:
				locationStatus.taskStatus = TaskStatus.PerformFaild;
				break;
		}
		return locationStatus;
	}

	public void ShowOverlay(bool isShow)
	{
		_cliWrapper.ShowOverlay(_viewHandle, isShow);
	}

	public void ShowRuler(bool bShow)
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer ShowRuler");
		_cliWrapper.ShowRuler(_viewHandle, bShow);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer ShowRuler");
	}

	/// <summary>
	/// 设置窗宽窗位
	/// </summary>  
	public void SetWWWL()
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetWWWL");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetWWWL");
	}

	/// <summary>
	/// 缩放
	/// </summary>
	public void Zoom()
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer Zoom");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Zoom, ROIType.ROI_None);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer Zoom");
	}

	public void SetZoomRatio(double zoom)
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetZoomRatio");
		_cliWrapper.SetZoomRatio(_viewHandle, zoom);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetZoomRatio");
	}

	/// <summary>
	/// 创建圆形ROI
	/// </summary>   
	public void CreateCircleRoi()
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer CreateCircleRoi");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Circle);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer CreateCircleRoi");
	}

	/// <summary>
	/// 创建线段
	/// </summary>   
	public void CreateLengthRoi()
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer CreateLengthRoi");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Length);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer CreateLengthRoi");
	}

	/// <summary>
	/// 创建箭头
	/// </summary>  
	public void CreateArrowRoi()
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer CreateArrowRoi");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Arrow);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer CreateArrowRoi");
	}

	/// <summary>
	/// 创建角度
	/// </summary>   
	public void CreateAngleRoi()
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer CreateAngleRoi");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, ROIType.ROI_Angle);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer CreateAngleRoi");
	}

	/// <summary>
	/// 重置
	/// </summary>
	public void Reset()
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer Reset");
		_cliWrapper.ResetView(_viewHandle);
		ShowOverlay(true);
		SetMouseActions();
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer Reset");
	}

	public void RemoveAllROI()
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer RemoveAllROI");
		_cliWrapper.RemoveAllROI(_viewHandle);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer RemoveAllROI");
	}

	/// <summary>
	/// 设置为选中状态
	/// </summary>
	public void SetSelector()
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetSelector");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, ROIType.ROI_None);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetSelector");
	}

	/// <summary>
	/// 移动 
	/// </summary>    
	public void Move()
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer Move");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Move, ROIType.ROI_None);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer Move");
	}

	public void SetTablePosition(double tablePositionInPatient)
	{
		this.WindowsFormsHost.Dispatcher.Invoke(DispatcherPriority.Background, () =>
		{
			_cliWrapper.SetTablePosition(_viewHandle, (float)tablePositionInPatient);
			_logger.LogInformation($"TopoImageViewer action SetTablePosition:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + ":" + tablePositionInPatient}");
		});
	}

	public void SetTableShow(bool isShown)
	{
		//this.WindowsFormsHost.Dispatcher.Invoke(DispatcherPriority.Background, () =>
		//{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer ShowTabled");
		_cliWrapper.ShowTable(_viewHandle, isShown);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer ShowTable");
		//});
	}

	public void LoadImageWithFilePath(string imagePath)
	{
		if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
		{
			this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
			{
				//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetViewDataFile:{imagePath}");
				_cliWrapper.SetViewDataFile(_viewHandle, imagePath);
				//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetViewDataFile");
			});
		}
		CurrentContentPath = imagePath;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="imagePath"></param>
	/// <param name="matrixHeight"></param>
	/// <param name="direction">1:头到脚； -1:脚到头</param>
	public void LoadImageWithFilePath(string imagePath, int matrixHeight, int direction)
	{
		if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
		{
			this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
			{
				//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetViewDataFile:{imagePath}");
				_cliWrapper.SetViewDataFile(_viewHandle, imagePath, matrixHeight, direction);
				//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetViewDataFile");
			});
		}
		CurrentContentPath = imagePath;
	}

	private LOCATION_PARAM ConvertToCliLocationParam(LocationParam lp)
	{
		var result = new LOCATION_PARAM();

		result.taskStatus.locationScanID = lp.ScanID;
		result.taskStatus.locationSeriesUID = lp.LocationSeriesUID;
		result.taskStatus.locationSeriesName = lp.LocationSeriesName;
		result.rangeParam.CenterFirstX = UnitConvert.Micron2Millimeter(lp.CenterFirstX);
		result.rangeParam.CenterFirstY = UnitConvert.Micron2Millimeter(lp.CenterFirstY);
		result.rangeParam.CenterFirstZ = UnitConvert.Micron2Millimeter(lp.CenterFirstZ);
		result.rangeParam.CenterLastX = UnitConvert.Micron2Millimeter(lp.CenterLastX);
		result.rangeParam.CenterLastY = UnitConvert.Micron2Millimeter(lp.CenterLastY);
		result.rangeParam.CenterLastZ = UnitConvert.Micron2Millimeter(lp.CenterLastZ);
		result.rangeParam.FOVLengthHor = UnitConvert.Micron2Millimeter(lp.FoVLengthHor);
		result.rangeParam.FOVLengthVer = UnitConvert.Micron2Millimeter(lp.FoVLengthVer);
		result.rangeParam.FOVDirectionHorX = lp.FOVDirectionHorX;
		result.rangeParam.FOVDirectionHorY = lp.FOVDirectionHorY;
		result.rangeParam.FOVDirectionHorZ = lp.FOVDirectionHorZ;
		result.rangeParam.FOVDirectionVerX = lp.FOVDirectionVerX;
		result.rangeParam.FOVDirectionVerY = lp.FOVDirectionVerY;
		result.rangeParam.FOVDirectionVerZ = lp.FOVDirectionVerZ;
		result.rangeParam.IsSquareFixed = lp.IsSquareFixed;
		result.taskStatus.IsChild = lp.IsChild;
		result.taskStatus.IsSelected = false;
		switch (lp.Status)
		{
			case PerformStatus.Unperform:
				result.taskStatus.taskStatus = TaskStatus.UnPerformed;
				break;
			case PerformStatus.Waiting:
				result.taskStatus.taskStatus = TaskStatus.Performing;
				break;
			case PerformStatus.Performed:
				if (lp.FailureReasonType == FailureReasonType.None)
				{
					result.taskStatus.taskStatus = TaskStatus.PerformSucceed;
				}
				else
				{
					result.taskStatus.taskStatus = TaskStatus.PerformFaild;
				}
				break;
			case PerformStatus.Performing:
				result.taskStatus.taskStatus = TaskStatus.Performing;
				break;
			default:
				result.taskStatus.taskStatus = TaskStatus.PerformFaild;
				break;
		}
		return result;
	}

	private LocationParam ConvertFromCliLocationParam(LOCATION_PARAM lp)
	{
		LocationParam result = new LocationParam();
		result.ScanID = lp.taskStatus.locationScanID;
		result.LocationSeriesUID = lp.taskStatus.locationSeriesUID;
		result.LocationSeriesName = lp.taskStatus.locationSeriesName;
		result.CenterFirstX = UnitConvert.Millimeter2Micron(lp.rangeParam.CenterFirstX);
		result.CenterFirstY = UnitConvert.Millimeter2Micron(lp.rangeParam.CenterFirstY);
		result.CenterFirstZ = UnitConvert.Millimeter2Micron(lp.rangeParam.CenterFirstZ);
		result.CenterLastX = UnitConvert.Millimeter2Micron(lp.rangeParam.CenterLastX);
		result.CenterLastY = UnitConvert.Millimeter2Micron(lp.rangeParam.CenterLastY);
		result.CenterLastZ = UnitConvert.Millimeter2Micron(lp.rangeParam.CenterLastZ);
		result.FoVLengthHor = UnitConvert.Millimeter2Micron(lp.rangeParam.FOVLengthHor);
		result.FoVLengthVer = UnitConvert.Millimeter2Micron(lp.rangeParam.FOVLengthVer);
		result.FOVDirectionHorX = lp.rangeParam.FOVDirectionHorX;
		result.FOVDirectionHorY = lp.rangeParam.FOVDirectionHorY;
		result.FOVDirectionHorZ = lp.rangeParam.FOVDirectionHorZ;
		result.FOVDirectionVerX = lp.rangeParam.FOVDirectionVerX;
		result.FOVDirectionVerY = lp.rangeParam.FOVDirectionVerY;
		result.FOVDirectionVerZ = lp.rangeParam.FOVDirectionVerZ;
		result.IsSquareFixed = lp.rangeParam.IsSquareFixed;
		result.IsChild = lp.taskStatus.IsChild;
		result.IsSelected = lp.taskStatus.IsSelected;

		return result;
	}

	public void SetFourCornersMessage(OverlayTextStyle overlayTextStyle, List<OverlayText> overlayTexts)
	{
		this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
		{
			//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer InitOverlayText");
			_cliWrapper.InitOverlayText(_viewHandle, overlayTextStyle, overlayTexts, 0);
			//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer InitOverlayText");
		});
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="scanModel"></param>
	/// <param name="weight">高度的一半</param>
	public void SetLocationScanMode(LOCATION_SCAN_MODE scanModel, int weight)
	{
		//_logger.LogDebug($"TopoImageViewer action info:Start TopoImageViewer SetLocationScanMode");
		_cliWrapper.SetLocationScanMode(_viewHandle, scanModel, weight);
		//_logger.LogDebug($"TopoImageViewer action info:End TopoImageViewer SetLocationScanMode");
	}

	public void SetLocalizerLocked(bool isLocaked = false)
	{
		this.WindowsFormsHost.Dispatcher.BeginInvoke(DispatcherPriority.Background, () =>
		{
			//_logger.LogDebug($"TopoImageViewer action info:Start SetLocalizerLocked");
			_cliWrapper.SetLocalizerLocked(this._viewHandle, isLocaked);
			//_logger.LogDebug($"TopoImageViewer action info:End SetLocalizerLocked");
		});
	}
	#endregion
}