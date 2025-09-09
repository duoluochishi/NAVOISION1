//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.Protocol.Models;
using NV.MPS.Environment;
using NVCTImageViewerInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using MouseButton = NVCTImageViewerInterop.MouseButton;
using Point = System.Drawing.Point;
using LogLevel = NVCTImageViewerInterop.LogLevel;

namespace NV.CT.DicomImageViewer;

public class GeneralImageViewer
{
	#region 私有变量  
	private NvImageViewerWrapperCLI _cliWrapper;
	private IntPtr _winFormHandle;
	private int _viewHandle;
    private WindowsFormsHost _windowsFormsHost;
	private UserControl _userControlWinForm;
	private readonly ILogger<GeneralImageViewer>? _logger;
	private bool _axisVisibility = true;
	private bool _overlayVisibility = true;
	public const string DragObjectKey = "series_drag_object";
	private LookupTable _lookupTable = LookupTable.LUT_BW;
    private LookupTable _lookupTable3D = LookupTable.LUT_BW;

	private double MinReconFov = 84.48;
	private double MaxReconFov = 506.88;

    /// <summary>
    /// 文本获取焦点的坐标
    /// </summary>
    private Point TextInputFocusPoint = new();


	public event EventHandler<int>? OnCineFinished;
	public event EventHandler<(int, double, int)>? SliceIndexChanged;
	public event EventHandler<(double ww, double wl)>? WindowWidthLevelChanged;
	public event EventHandler<int>? OnViewLoadFinishedEvent;
	public event EventHandler<int>? OnCPRFinishedEvent;
	public event EventHandler<string>? OnAddPrintImageEvent;
	public event EventHandler<int>? OnROICreateSucceedEvent;

	public event EventHandler<Point>? TextInputFocusPositionChanged;
	public event EventHandler<string>? TextInputDoubleClickChanged;
	public event EventHandler<LOCATION_RANGEPARAM>? ReconBoxChangeNotify;
	public event EventHandler<(int arg1, MedROI arg2, int arg3)>? ROIModified;

    public event EventHandler<(int,string)>? OnTableRemoveFinishedEvent;

    //ROIModifiedNotify(int arg1, MedROI arg2, int arg3)
    //public event EventHandler<bool>? OnMouseForwardToBackwardEvent;
    //public event EventHandler<List<LOCATION_PARAM>>? LocationWideChangeEvent;



    public WindowsFormsHost WindowsFormsHost
	{
		get => _windowsFormsHost;
		set => _windowsFormsHost = value;
	}
	public int ViewHandle
	{
		get => _viewHandle;
		set => _viewHandle = value;
	}
    #endregion

    #region 构造函数

    public GeneralImageViewer(int width, int height, bool is3D)
	{
		_logger = CTS.Global.ServiceProvider.GetService<ILogger<GeneralImageViewer>>();

		if (is3D)
		{
			InitTomoImageViewerPre(width, height);
			ClearView();
			var myHandle = CreateVRMPRView(0, 0, width, height);
			SetBrowserViewMouseAction(myHandle, MouseButton.MouseWheelButtonForwardAndBackward, ViewMouseAction.BrowserMouseAction_Slicing, ROIType.ROI_None);
			InitTomoImageViewerPost(width, height);
		}
		else
		{
			InitTomoImageViewerPre(width, height);
			CreateImageBrowserView(width, height);
			InitTomoImageViewerPost(width, height);
		}
	}

	/// <summary>
	/// MPR 高级重建构造函数
	/// </summary>
	public GeneralImageViewer(int width, int height, int createType = 0)
	{
		_logger = CTS.Global.ServiceProvider.GetService<ILogger<GeneralImageViewer>>();

		InitTomoImageViewerPre(width, height);
		ClearView();
		var myHandle = CreateMPRReconView(0, 0, width, height);

		SetBrowserViewMouseAction(myHandle, MouseButton.MouseWheelButtonForwardAndBackward, ViewMouseAction.BrowserMouseAction_Slicing, ROIType.ROI_None);
		InitTomoImageViewerPost(width, height);
	}

	private void InitTomoImageViewerPre(int width, int height)
	{
		_logger?.LogDebug($"GeneralImageViewer action info:Start Initialize {width}*{height}");
		_cliWrapper = new NvImageViewerWrapperCLI();
		_windowsFormsHost = new WindowsFormsHost();
		_windowsFormsHost.AllowDrop = true;
		_windowsFormsHost.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
		_windowsFormsHost.IsHitTestVisible = false;

		_userControlWinForm = new UserControl();
		_userControlWinForm.AllowDrop = true;

        EnableDragAndDrop();

		_winFormHandle = _userControlWinForm.Handle;
		_windowsFormsHost.Child = _userControlWinForm;

        _cliWrapper.Initialize();
	}

	private void InitTomoImageViewerPost(int width, int height)
	{
		InitEventHandler();
		SetMouseActions();
		_logger?.LogDebug($"GeneralImageViewer action info:End Initialize TomoImageViewer {width}*{height}");
	}

	private void SetMouseActions()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.MouseWheelButtonForwardAndBackward, ViewMouseAction.BrowserMouseAction_Slicing, ROIType.ROI_None);
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.RightMouseButton, ViewMouseAction.BrowserMouseAction_Zoom, ROIType.ROI_None);
    }

	/// <summary>
	/// 可以切换布局的多序列用的
	/// </summary>
	public void CreateImageBrowserView(int width, int height)
	{
		_viewHandle = _cliWrapper.CreateImageBrowserView(this._winFormHandle, 0, 0, width, height);
	}

	public void ClearView()
	{
		WindowsFormsHost.Dispatcher.Invoke(() =>
		{
			_cliWrapper.ClearView(_viewHandle);
            _cliWrapper.ClearViewData(_viewHandle);

        });
		//CurrentContentPath = string.Empty;
	}

	private void InitEventHandler()
	{
		_cliWrapper.OnCineFinishedNotify -= CliWrapper_OnCineFinishedNotify;
		_cliWrapper.ImageLogerNotify -= CliWrapper_ImageLogerNotify;
		_cliWrapper.SliceChangedNotify -= CliWrapper_SliceChangedNotify;
		_cliWrapper.WindowLevelChangedNotify -= CliWrapper_WindowLevelChangedNotify;
		_cliWrapper.OnCprModifiedNotify -= CliWrapper_OnCprModifiedNotify;
		_cliWrapper.OnMprVrViewLoadFinishNotify -= CliWrapper_OnMprVrViewLoadFinishNotify;

		_cliWrapper.OnCineFinishedNotify += CliWrapper_OnCineFinishedNotify;
		_cliWrapper.ImageLogerNotify += CliWrapper_ImageLogerNotify;
		_cliWrapper.SliceChangedNotify += CliWrapper_SliceChangedNotify;
		_cliWrapper.WindowLevelChangedNotify += CliWrapper_WindowLevelChangedNotify;
		_cliWrapper.OnCprModifiedNotify += CliWrapper_OnCprModifiedNotify;
		_cliWrapper.OnMprVrViewLoadFinishNotify += CliWrapper_OnMprVrViewLoadFinishNotify;

		_cliWrapper.CreateTextNotify -= CliWrapper_CreateTextNotify;
		_cliWrapper.ChangeTextNotify -= CliWrapper_ChangeTextNotify;
		_cliWrapper.ROICreateSucceedNotify -= CliWrapper_OnROICreateSuccceedNotify;
		_cliWrapper.AddPrintImageNotify -= CliWrapper_AddPrintImageNotify;

		_cliWrapper.CreateTextNotify += CliWrapper_CreateTextNotify;
		_cliWrapper.ChangeTextNotify += CliWrapper_ChangeTextNotify;
		_cliWrapper.ROICreateSucceedNotify += CliWrapper_OnROICreateSuccceedNotify;
		_cliWrapper.AddPrintImageNotify += CliWrapper_AddPrintImageNotify;

		_cliWrapper.ReconBoxChangeNotify -= CliWrapper_ReconBoxChangeNotify;
		_cliWrapper.ROIModifiedNotify -= CliWrapper_ROIModifiedNotify;

		_cliWrapper.ReconBoxChangeNotify += CliWrapper_ReconBoxChangeNotify;
		_cliWrapper.ROIModifiedNotify += CliWrapper_ROIModifiedNotify;

        _cliWrapper.ProcessFinishedNotify += _cliWrapper_ProcessFinishedNotify;
        _cliWrapper.ImageData3dBoundingBoxNotify += _cliWrapper_ImageData3dBoundingBoxNotify;

    }

    private void _cliWrapper_ImageData3dBoundingBoxNotify(int arg1, LOCATION_RANGEPARAM arg2,bool isReconDataBox)
    {
		if(isReconDataBox)
		{
			return;
		}
		var limit = new LOCATION_LIMIT_PARAM();
		var minZ = arg2.CenterLastZ > arg2.CenterFirstZ? arg2.CenterFirstZ : arg2.CenterLastZ;
        var maxZ = arg2.CenterLastZ > arg2.CenterFirstZ ? arg2.CenterLastZ : arg2.CenterFirstZ;

		//不是正规地取值，可能存在风险
		var x1 = arg2.CenterFirstX - arg2.FOVLengthHor /2;
        var x2 = arg2.CenterFirstX + arg2.FOVLengthHor /2;
        var y1 = arg2.CenterFirstY - arg2.FOVLengthVer /2;
        var y2 = arg2.CenterFirstY + arg2.FOVLengthVer /2;

        limit.MinLocationPostion = minZ;
        limit.MaxLocationPostion = maxZ;
		limit.MinScanFov = MinReconFov;
		limit.MaxScanFov = MaxReconFov;
		limit.MinScanLength = 10;
		limit.MaxScanLength = maxZ - minZ;
		limit.MaxXPostion = x2;
		limit.MinXPostion = x1;
		limit.MaxYPostion = y2;
		limit.MinYPostion = y1;

        _cliWrapper.SetReconBoxLimit(_viewHandle,limit);
    }

    private void _cliWrapper_ProcessFinishedNotify(int arg1, string arg2)
    {
		OnTableRemoveFinishedEvent?.Invoke(this,(arg1,arg2));
    }

    private void CliWrapper_ROIModifiedNotify(int _viewHandle, MedROI roi, int action)
	{
		var state = action == 2;
		_cliWrapper.SetSyncRoi(_viewHandle, roi, state);
	}

	private void CliWrapper_AddPrintImageNotify(int _viewHandle, List<MedImageProperty> list)
	{
		string medImageList = JsonConvert.SerializeObject(list);
		_logger?.LogDebug($"CliWrapper_AddPrintImageNotify:{medImageList}");
		OnAddPrintImageEvent?.Invoke(this, medImageList);
	}

	private void CliWrapper_ReconBoxChangeNotify(int arg1, LOCATION_RANGEPARAM arg2)
	{
		var convertedRangeParam = new LOCATION_RANGEPARAM();
		convertedRangeParam.CenterFirstX = UnitConvert.ExpandThousand(arg2.CenterFirstX);
        convertedRangeParam.CenterFirstY = UnitConvert.ExpandThousand(arg2.CenterFirstY);
        convertedRangeParam.CenterFirstZ = UnitConvert.ExpandThousand(arg2.CenterFirstZ);
        convertedRangeParam.CenterLastX = UnitConvert.ExpandThousand(arg2.CenterLastX);
        convertedRangeParam.CenterLastY = UnitConvert.ExpandThousand(arg2.CenterLastY);
        convertedRangeParam.CenterLastZ = UnitConvert.ExpandThousand(arg2.CenterLastZ);
        convertedRangeParam.FOVDirectionHorX = arg2.FOVDirectionHorX;
        convertedRangeParam.FOVDirectionHorY = arg2.FOVDirectionHorY;
        convertedRangeParam.FOVDirectionHorZ = arg2.FOVDirectionHorZ;
        convertedRangeParam.FOVDirectionVerX = arg2.FOVDirectionVerX;
        convertedRangeParam.FOVDirectionVerY = arg2.FOVDirectionVerY;
        convertedRangeParam.FOVDirectionVerZ = arg2.FOVDirectionVerZ;
        convertedRangeParam.FOVLengthHor = UnitConvert.ExpandThousand(arg2.FOVLengthHor);
        convertedRangeParam.FOVLengthVer = UnitConvert.ExpandThousand(arg2.FOVLengthVer);
        convertedRangeParam.IsSquareFixed = arg2.IsSquareFixed;
        ReconBoxChangeNotify?.Invoke(this, convertedRangeParam);
	}

	/// <summary>
	/// 双击，ROI修改文本信息
	/// </summary>
	private void CliWrapper_ChangeTextNotify(int _viewHandle, string content)
	{
		TextInputDoubleClickChanged?.Invoke(this, content);
	}

	/// <summary>
	/// 单击，ROI文本创建位置
	/// </summary>
	private void CliWrapper_CreateTextNotify(int _viewHandle, int x, int y)
	{
		TextInputFocusPoint = new Point(x, y);
		TextInputFocusPositionChanged?.Invoke(this, TextInputFocusPoint);
	}

	private void CliWrapper_OnMprVrViewLoadFinishNotify(int obj)
	{
		OnViewLoadFinishedEvent?.Invoke(this, obj);
	}

	private void CliWrapper_OnCprModifiedNotify(int obj)
	{
		OnCPRFinishedEvent?.Invoke(this, obj);
	}

	private void CliWrapper_WindowLevelChangedNotify(int viewHandle, double ww, double wl)
	{
		WindowWidthLevelChanged?.Invoke(this, (ww, wl));
	}

	private void CliWrapper_SliceChangedNotify(int viewHandle, int index, double pos, int total)
	{
		SliceIndexChanged?.Invoke(this, (index, pos, total));
	}

	private void CliWrapper_OnCineFinishedNotify(int obj)
	{
		OnCineFinished?.Invoke(this, _viewHandle);
	}
	private void CliWrapper_OnROICreateSuccceedNotify(int obj)
	{
		OnROICreateSucceedEvent?.Invoke(this, _viewHandle);
	}

	private void CliWrapper_ImageLogerNotify(LogLevel logLevel, string message)
	{
		switch (logLevel)
		{
			case LogLevel.Debug:
				_logger?.LogDebug($"NvImageViewerWrapperCLI:{message}");
				break;
			case LogLevel.Critical:
				_logger?.LogCritical($"NvImageViewerWrapperCLI:{message}");
				break;
			case LogLevel.Warning:
				_logger?.LogWarning($"NvImageViewerWrapperCLI:{message}");
				break;
			case LogLevel.Error:
				_logger?.LogError($"NvImageViewerWrapperCLI:{message}");
				break;
			case LogLevel.Trace:
				_logger?.LogTrace($"NvImageViewerWrapperCLI:{message}");
				break;
			case LogLevel.Information:
			case LogLevel.None:
			default:
				_logger?.LogInformation($"NvImageViewerWrapperCLI:{message}");
				break;
		}
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

	public void SetMaxMinReconFov(double minFov,double maxFov)
	{
		MinReconFov = UnitConvert.ReduceThousand(minFov);
		MaxReconFov = UnitConvert.ReduceThousand(maxFov);
	}

	public void SetWindowWidthLevel(double windowWidth, double windowLevel)
	{
		if (windowWidth <= 0 && windowLevel <= 0)
		{
			return;
		}
		_cliWrapper.SetWWWL(_viewHandle, windowWidth, windowLevel);
	}

	public void GetWWWL(ref double ww, ref double wl)
	{
		_cliWrapper.GetWWWL(_viewHandle, ref ww, ref wl);
	}

	/// <summary>
	/// 设置重建预览
	/// </summary>
	/// <param name="viewPath">完成后是序列路径，没完成不用传</param>
	public void SetReconPreview(string viewPath = "")
	{
		_cliWrapper.SetReconViewData(_viewHandle, viewPath);
	}

	/// <summary>
	/// 默认重建方向
	/// </summary>
	public void SetReconstructionOrientation()
	{
		_cliWrapper.SetBatchOrientation(_viewHandle, ViewOrientation.Axial);
	}

	public void SetReconMode()
	{
		_cliWrapper.SetBatchMode(_viewHandle, BatchMode.RectBatch);
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
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
		_cliWrapper.SetWWWL(_viewHandle, ww, wl);
	}
	public void Move()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Move, ROIType.ROI_None);
	}

	/// <summary>
	/// 缩放
	/// </summary>
	public void Zoom()
	{
		_logger?.LogDebug($"TomoImageViewer action info:Start TomoImageViewer Zoom");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Zoom, ROIType.ROI_None);
		_logger?.LogDebug($"TomoImageViewer action info:End TomoImageViewer Zoom");
	}

	public void Scroll()
	{
		_logger?.LogDebug($"TomoImageViewer action info:Start TomoImageViewer Scroll");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Slicing, ROIType.ROI_None);
		_logger?.LogDebug($"TomoImageViewer action info:End TomoImageViewer Scroll");
	}

	public void Rotate()
	{
		_logger?.LogDebug($"TomoImageViewer action info:Start TomoImageViewer Rotate");
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Rotate, ROIType.ROI_None);
		_logger?.LogDebug($"TomoImageViewer action info:End TomoImageViewer Rotate");

	}

	/// <summary>
	/// 2D角度旋转
	/// </summary>
	public void Rotate(int degree)
	{
		//_cliWrapper.SetViewMouseAction(_viewHandle,MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Rotate);
		//if (degree is not null)
		//{
		//    _cliWrapper.Rotate(degree);
		//}

		WindowsFormsHost.Dispatcher.Invoke(() =>
		{
			_cliWrapper.Rotate2D(_viewHandle, degree);
		});

	}

	/// <summary>
	/// 2D ROI重置
	/// </summary>
	public void Reset()
	{
		_cliWrapper.ResetView(_viewHandle);

		ShowOverlay(true);
		SetMouseActions();
	}

	/// <summary>
	/// 布局 2d/3d 通用
	/// </summary>
	public void Layout(ViewLayout viewLayout)
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
    public void Invert3D()
    {
        if (_lookupTable3D == LookupTable.LUT_BW)
        {
            _cliWrapper.SetLookupTable(_viewHandle, LookupTable.LUT_BWInverse);
            _lookupTable3D = LookupTable.LUT_BWInverse;
        }
        else
        {
            _cliWrapper.SetLookupTable(_viewHandle, LookupTable.LUT_BW);
            _lookupTable3D = LookupTable.LUT_BW;
        }
    }
    public void InitInvert()
	{
		_lookupTable = LookupTable.LUT_BW;
	}
    public void InitInvert3D()
    {
        _lookupTable3D = LookupTable.LUT_BW;
    }
    public void Kernel(string kernelType)
	{
		switch (kernelType)
		{
			case "None":
				_cliWrapper.SetImageFilter(_viewHandle, ImageFilter.ImageFilter_None);
				break;
			case "Sharp":
				_cliWrapper.SetImageFilter(_viewHandle, ImageFilter.ImageFilter_Sharpen);
				break;
			case "Sharp+":
				_cliWrapper.SetImageFilter(_viewHandle, ImageFilter.ImageFilter_Very_Sharpen);
				break;
			case "Smooth":
				_cliWrapper.SetImageFilter(_viewHandle, ImageFilter.ImageFilter_Smooth);
				break;
			case "Smooth+":
				_cliWrapper.SetImageFilter(_viewHandle, ImageFilter.ImageFilter_Very_Smooth);
				break;
		}
	}
	public void PrintSelectedImages()
	{
		_cliWrapper.PrintSelectedRenderView(_viewHandle);
    }

	/// <summary>
	/// 同步设置参数
	/// </summary>
	public void SetSyncParameters(int syncParameter)
	{
		_cliWrapper.SetSynchronousMode(_viewHandle, syncParameter);
	}

	/// <summary>
	/// 同步状态开关
	/// </summary>
	public void SetSyncBinding(bool syncOrNot)
	{
		_cliWrapper.SetSelectedRenderViewSynchronous(_viewHandle, syncOrNot);
	}

	public void FlipVertical()
	{
		_cliWrapper.FlipY(_viewHandle);
	}

	public void FlipHorizontal()
	{
		_cliWrapper.FlipX(_viewHandle);
	}


	public void ShowCursorRelativeValue(bool flag)
	{
		_cliWrapper.ShowCursorRelativeValue(_viewHandle, flag);
	}

	public string GetMultiSeriesDicomFilePath()
	{
		return _cliWrapper.GetSelectedRenderViewDataPath(_viewHandle);
    }

	public void LoadImageWithFilePath(string imageFile)
	{
		if (!string.IsNullOrEmpty(imageFile) && File.Exists(imageFile))
		{
			WindowsFormsHost.Dispatcher.Invoke(() =>
			{
				_logger?.LogDebug($"TomoImageViewer action info:Start TomoImageViewer SetViewDataFile:{imageFile}");
				//ShowOverlay(false);
				_cliWrapper.SetViewDataFile(_viewHandle, imageFile);
				_logger?.LogDebug($"TomoImageViewer action info:End TomoImageViewer SetViewDataFile");
			});
			//CurrentContentPath = "";
		}
	}
	public void LoadImageWithDirectoryPath(string imagePath)
	{
		if (!string.IsNullOrEmpty(imagePath) && Directory.Exists(imagePath))
		{
			WindowsFormsHost.Dispatcher.Invoke(() =>
			{
				//TODO:真正选中的窗口索引，放开这里 bug 1950
				var index = _cliWrapper.GetMultipleViewSelectedIndex(_viewHandle);
                //var index = 0;

                _cliWrapper.SetMultipleViewDataFloder(_viewHandle, new List<string> { imagePath }, index);

                ShowOverlay(true);
			});
			//CurrentContentPath = imagePath;
		}
	}

	/// <summary>
	/// 加载 recon mpr图像
	/// </summary>
	/// <param name="imagePath"></param>
	public void LoadReconImageWithDirectoryPath(string imagePath)
	{
		if (!string.IsNullOrEmpty(imagePath) && Directory.Exists(imagePath))
		{
			WindowsFormsHost.Dispatcher.Invoke(() =>
			{
				_cliWrapper.SetViewDataFloder3D(_viewHandle, imagePath);
			});
		}
	}

	/// <summary>
	/// 设置重建框是否可以调整
	/// </summary>
	/// <param name="canModify">是否可以调整</param>
	public void SetReconComplete(bool canModify)
	{
		_cliWrapper.SetReconCompleted(_viewHandle, canModify);
	}

	public void SetReconBox(ReconModel recon)
	{
		if (recon is null)
			return;

		var divider = 1000.0;

		LOCATION_RANGEPARAM parm = new LOCATION_RANGEPARAM();
		parm.CenterFirstX = recon.CenterFirstX / divider;
		parm.CenterFirstY = recon.CenterFirstY / divider;
		parm.CenterFirstZ = recon.CenterFirstZ / divider;
		parm.CenterLastX = recon.CenterLastX / divider;
		parm.CenterLastY = recon.CenterLastY / divider;
		parm.CenterLastZ = recon.CenterLastZ / divider;
		parm.FOVLengthHor = recon.FOVLengthHorizontal / divider;
		parm.FOVLengthVer = recon.FOVLengthVertical / divider;
		parm.FOVDirectionHorX = recon.FOVDirectionHorizontalX;
		parm.FOVDirectionHorY = recon.FOVDirectionHorizontalY;
		parm.FOVDirectionHorZ = recon.FOVDirectionHorizontalZ;
		parm.FOVDirectionVerX = recon.FOVDirectionVerticalX;
		parm.FOVDirectionVerY = recon.FOVDirectionVerticalY;
		parm.FOVDirectionVerZ = recon.FOVDirectionVerticalZ;
		parm.IsSquareFixed = recon.FOVLengthHorizontal == recon.FOVLengthVertical;

		_cliWrapper.SetReconBox(_viewHandle, parm);
	}

	public void SetFourCornersMessage(OverlayTextStyle overlayTextStyle, List<OverlayText> overlayTexts)
	{
		if (overlayTexts.Count > 0)
		{
			WindowsFormsHost.Dispatcher.Invoke(() =>
			{
				_cliWrapper.InitOverlayText(_viewHandle, overlayTextStyle, overlayTexts, 0);
			});
		}
	}

	#endregion

	#region not used
	public void SetMultipleViewDataFolder(List<string> folderList)
	{
		_cliWrapper.SetMultipleViewDataFloder(_viewHandle, folderList, 0);
	}
	/// <summary>
	/// 设置为选中状态
	/// </summary>
	public void SetSelector()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, ROIType.ROI_None);
	}

	public void LoadDicomSeriesReverseOrder()
	{
		_cliWrapper.Inverse(_viewHandle);
	}

	public void SetSliceIndex(int currentIndex)
	{
		if (currentIndex < 0)
		{
			return;
		}
		_cliWrapper.SetSliceIndex(_viewHandle, currentIndex);
	}

	public void LoadImageWithDirectoryPath(string imagePath, int matrixHeight, int direction)
	{
		if (!string.IsNullOrEmpty(imagePath) && Directory.Exists(imagePath))
		{
			WindowsFormsHost.Dispatcher.Invoke(() =>
			{
				_cliWrapper.SetViewDataFloder(_viewHandle, imagePath, matrixHeight, direction);
				ShowOverlay(true);
			});
		}
	}

	#endregion

	#region ROI
	public void CreateROI(ROIType roiType)
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, roiType);

	}

	/// <summary>
	/// Grid ROI
	/// </summary>
	public void CreateGridRoi(int? width)
	{
		_cliWrapper.DeleteGrid(_viewHandle);

		//如果传递了width值
		if (width is not null)
		{
			double spacing=width.Value;
            _cliWrapper.CreateGrid(_viewHandle, spacing);
        }
			
	}

	/// <summary>
	/// 写入文本
	/// </summary>
	public void WriteTextInput(string content)
	{
		_cliWrapper.InputTextContent(_viewHandle, content);
	}

	/// <summary>
	/// 双击改变文本的时候调用的方法
	/// </summary>
	public void WriteChangedText(string content)
	{
		_cliWrapper.InputTextContent(_viewHandle, content);
	}

	#endregion

	#region 电影播放相关
	public void GotoBegin()
	{
		_cliWrapper.CineGoBegin(_viewHandle);
	}

	public void CineStop()
	{
		_cliWrapper.CineStop(_viewHandle);
	}
	public void SetCineParam(int fps)
	{
		int num = _cliWrapper.GetSliceNumber(_viewHandle);
		_cliWrapper.CineRange(_viewHandle, 0, num - 1, 1);
		AdjustFrameRateCine(fps);
	}
	public void CineStart(int initialFPS)
	{
		SetCineParam(initialFPS);
		_cliWrapper.CineStart(_viewHandle);
	}
	public void CinePause()
	{
		_cliWrapper.CinePause(_viewHandle);
	}
	public void CineCyclePlay()
	{
		_cliWrapper.CineEnableCyclePlay(_viewHandle, true);

	}
	public void CinePauseResume()
	{
		_cliWrapper.CinePauseResume(_viewHandle);

	}
	public void MoveToNextSlice()
	{
		_cliWrapper.CineGoEnd(_viewHandle);
	}

	public void MoveToPriorSlice()
	{
		_cliWrapper.CineGoBegin(_viewHandle);
	}
	/// <summary>
	/// 向前一帧
	/// </summary>
	public void CineForward()
	{
		_cliWrapper.CineForward(_viewHandle);
	}
	/// <summary>
	/// 向后一帧
	/// </summary>
	public void CineBackward()
	{
		_cliWrapper.CineBackward(_viewHandle);
	}
	public void AdjustFrameRateCine(int fps)
	{
		_cliWrapper.AdjustFrameRateCine(_viewHandle, fps);
	}

	#endregion

	#region 3D
	public double GetAutoRemoveTableProcess()
	{
	   return	_cliWrapper.GetProgress(_viewHandle);

    }
	public void LoadImageWithDirectoryPath3D(string imagePath)
	{
		if (!string.IsNullOrEmpty(imagePath) && Directory.Exists(imagePath))
		{
			WindowsFormsHost.Dispatcher.Invoke(() =>
			{
				_logger?.LogDebug($"TomoImageViewer action info:Start TomoImageViewer SetViewDataFolder3D:{imagePath}");
				_cliWrapper.SetViewDataFloder3D(_viewHandle, imagePath);
				ShowOverlay(true);
				_logger?.LogDebug($"TomoImageViewer action info:End TomoImageViewer SetViewDataFolder3D");
			});
		}
	}

	/// <summary>
	/// 3D mpr view
	/// </summary>
	public int CreateVRMPRView(int left, int right, int width, int height)
	{
		var handle = _cliWrapper.CreateVRMPRView(_winFormHandle, left, right, width, height);
		_viewHandle = handle;
		return handle;
	}

	public int CreateMPRReconView(int left, int right, int width, int height)
	{
		var handle = _cliWrapper.CreateVRMPRView(_winFormHandle, left, right, width, height);
		_viewHandle = handle;
		_cliWrapper.SetMprWorkMode(_viewHandle, MPRWorkMode.ReconMode);
		_cliWrapper.SetLayout(_viewHandle,ViewLayout.CPR2x2);
		_cliWrapper.SetBatchOrientation(_viewHandle,ViewOrientation.Axial);
		return handle;
	}

	public void SetWWWL2D()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
	}
	public void SetWWWL3D()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
	}
	public void SetWWWL3D(double ww, double wl)
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
		_cliWrapper.SetWWWL(_viewHandle, ww, wl);
	}

	public void Move3D()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Move, ROIType.ROI_None);
	}

	public void Zoom3D()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Zoom, ROIType.ROI_None);
	}

	public void SetBrowserViewMouseAction(int viewHandle, MouseButton type, ViewMouseAction action, ROIType rOIType)
	{
		_cliWrapper.SetViewMouseAction(viewHandle, type, action, rOIType);
	}

	/// <summary>
	/// 3D控件里面的 2D旋转
	/// </summary>
	public void SetRotate2D()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Rotate, ROIType.ROI_None);
	}

	/// <summary>
	/// 3D控件里面的 3D旋转
	/// </summary>
	public void SetRotate3D()
	{
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MPRVRMouseAction_Rotate3D, ROIType.ROI_None);
	}

	/// <summary>
	/// 隐藏或显示 坐标轴
	/// </summary>
	private void SetMprResliceAxisVisibility(bool showAxis)
	{
		_cliWrapper.SetMprResliceAxisVisibility(_viewHandle, showAxis);
	}

	/// <summary>
	/// Toggle 坐标轴
	/// </summary>
	public void SetAxis3D()
	{
		SetMprResliceAxisVisibility(!_axisVisibility);
	}

	public void ToggleAxis3D()
	{
		_axisVisibility = !_axisVisibility;
		_cliWrapper.SetMprResliceAxisVisibility(_viewHandle, _axisVisibility);
	}

	public void Reset3D()
	{
		_overlayVisibility = true;
		_axisVisibility = true;
		ShowOverlay(true);
		//_cliWrapper.RemoveSeriesAllROI(_viewHandle);
		_cliWrapper.ResetView(_viewHandle);
		_cliWrapper.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);

		SetMprResliceAxisVisibility(true);
	}
	public void RemoveAllROI2D()
	{
		_cliWrapper.RemoveAllROI(_viewHandle);
		_cliWrapper.SetVolumePolyDataCut(_viewHandle, false, true);
		//SetWWWL2D();
	}
	/// <summary>
	/// 移除3D控件的 ROI
	/// </summary>
	public void RemoveAllROI3D()
	{
		_cliWrapper.RemoveAllROI(_viewHandle);
		_cliWrapper.SetVolumePolyDataCut(_viewHandle, false, true);
		//SetWWWL3D();
	}
	public void SetCustomLayout2D(int row, int column)
	{
		_cliWrapper.SetLayout(_viewHandle, column, row);
	}
	public void SetCustomLayout3D(int row, int column)
	{
	}
	public void SetMPR(ImageResliceMode imageResliceMode)
	{
		_cliWrapper.SetImageResliceMode(_viewHandle, imageResliceMode);
		_cliWrapper.SetLayout(_viewHandle, ViewLayout.OrthoMPRL2R1);
	}

	public void SetGlobalSliceThickness(int sliceThickness)
	{
		_cliWrapper.SetSlabLineThickness(_viewHandle, sliceThickness);
	}

	public void SetVR()
	{
		_cliWrapper.SetLayout(_viewHandle, ViewLayout.VRSSD1x1);
		_cliWrapper.Set3dMode(_viewHandle, View3dMode.VR);
		_overlayVisibility = true;
		ShowOverlay(true);
	}

	public void SetCPR()
	{
		_cliWrapper.SetBatchMode(_viewHandle, BatchMode.CprBatch);
		_cliWrapper.SetLayout(_viewHandle, ViewLayout.CPR2x2);
	}

	public void SetSSD()
	{
		_cliWrapper.SetLayout(_viewHandle, ViewLayout.OrthoMPR2x2);
		_cliWrapper.Set3dMode(_viewHandle, View3dMode.SSD);
	}

	/// <summary>
	/// VRT 重置
	/// </summary>
	public void SetVRTReset()
	{
		_cliWrapper.SetBoxWidgetOn(_viewHandle, false);
		_cliWrapper.SetBatchMode(_viewHandle, BatchMode.None);
		_cliWrapper.SetVolumePolyDataCut(_viewHandle, false, false);

		//TODO:2.0从配置里面读取
		//_cliWrapper.Set3DPresetsXml(_viewHandle, fileNameList.First(r => r.Name == "CT-AAA.plist").FullName);
		//_cliWrapper.SetViewMouseAction(_viewHandle, NVCTImageViewerInterop.MouseButton.LeftMouseButton, NVCTImageViewerInterop.ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_NULL);
		_cliWrapper.ResetView(_viewHandle);
	}
	public void CloseCPRMode()
	{
		_cliWrapper.SetBatchMode(_viewHandle, BatchMode.None);
	}
	public void InitVRorSSDViewMode()
	{
		_cliWrapper.SetBoxWidgetOn(_viewHandle, false);
		_cliWrapper.SetBatchMode(_viewHandle, BatchMode.None);
		_cliWrapper.SetVolumePolyDataCut(_viewHandle, false, false);
	}
	public void SetClipBox(bool clipboxState)
	{
		_cliWrapper.SetBoxWidgetOn(_viewHandle, clipboxState);
	}
	public void SetCutState(bool cutState)
	{
		_cliWrapper.SetVolumePolyDataCut(_viewHandle, cutState, true);
	}
	public void SetSelectedCut()
	{
		_cliWrapper.SetVolumePolyDataCut(_viewHandle, true, true);
	}
	public void SetUnselectedCut()
	{
		_cliWrapper.SetVolumePolyDataCut(_viewHandle, true, false);
	}
	public void UndoVolumeCut()
	{
		_cliWrapper.UndoVolumeCut(_viewHandle);
	}
	public void RedoVolumeCut()
	{
		_cliWrapper.RedoVolumeCut(_viewHandle);
	}
    public void AutoRemoveTable(bool removeState)
    {
        _cliWrapper.RemoveBedboard(_viewHandle, removeState);
    }
    public void SetAdvancedPreset(string presetName)
	{
		var finalPresetFileName = "";
		switch (presetName)
		{
			case "Airways":
				finalPresetFileName = "Airways II.plist";
				break;
			case "X-ray":
				finalPresetFileName = "CT-X-ray.plist";
				break;
			case "Muscle":
				finalPresetFileName = "CT-Muscle.plist";
				break;
			case "MIP":
				finalPresetFileName = "CT-MIP.plist";
				break;
			case "Air":
				finalPresetFileName = "CT-AIR.plist";
				break;
			case "Default":
				finalPresetFileName = "CT-AAA.plist";
				break;
		}

		var filePath = Path.Combine(RuntimeConfig.Console.MCSConfig.Path, $"PresetList\\{finalPresetFileName}");
		if (File.Exists(filePath))
		{
			_cliWrapper.Set3DPresetsXml(_viewHandle, filePath);
		}
	}

	public void MPRRecon(double thickness, double fov, ViewOrientation viewOrientation, BatchMode reconMode)
	{
		LOCATION_RANGEPARAM paramFov = new LOCATION_RANGEPARAM();
		paramFov.CenterFirstX = 11;              //起始位置中心点
		paramFov.CenterFirstY = 17;
		paramFov.CenterFirstZ = 38.4;
		paramFov.CenterLastX = 11;               //中止位置中心点
		paramFov.CenterLastY = 17;
		paramFov.CenterLastZ = -281;
		paramFov.FOVLengthHor = fov;                //横向FOV长度
		paramFov.FOVLengthVer = fov;                //纵向FOV长度
		paramFov.FOVDirectionHorX = 1;          //横向FOV方向
		paramFov.FOVDirectionHorY = 0;
		paramFov.FOVDirectionHorZ = 0;
		paramFov.FOVDirectionVerX = 0;      //纵向FOV方向
		paramFov.FOVDirectionVerY = 1;
		paramFov.FOVDirectionVerZ = 0;
		paramFov.IsSquareFixed = true;      //是否固定为正方形FOV区域

		LOCATION_LIMIT_PARAM limitParam = new LOCATION_LIMIT_PARAM();
		limitParam.MinLocationPostion = 0;
		limitParam.MaxLocationPostion = 0;
		limitParam.MaxScanLength = 400;
		limitParam.MinScanLength = 100;
		limitParam.MaxScanFov = 400;
		limitParam.MinScanFov = 100;

		_cliWrapper.SetReconSegmentThickness(_viewHandle, thickness);
		_cliWrapper.SetReconBox(_viewHandle, paramFov);
		_cliWrapper.SetReconBoxLimit(_viewHandle, limitParam);
		_cliWrapper.SetBatchMode(_viewHandle, reconMode);
		_cliWrapper.SetBatchOrientation(_viewHandle, viewOrientation);
	}
	#endregion

	#region DnD

	private void EnableDragAndDrop()
	{
		_userControlWinForm.DragEnter += UserControl_DragEnter;
		_userControlWinForm.DragDrop += UserControl_DragDrop;
	}

	private void UserControl_DragEnter(object? sender, DragEventArgs e)
	{
		e.Effect = DragDropEffects.Move;
	}

	private void UserControl_DragDrop(object? sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DragObjectKey))
		{
			var passedData = e.Data.GetData(DragObjectKey) as string;
            if (string.IsNullOrEmpty(passedData))
				return;
            if (passedData.Contains(".dcm"))
            {
                passedData = Directory.GetParent(passedData)?.FullName;
            }

            var clientPoint = _userControlWinForm.PointToClient(new Point(e.X, e.Y));

			var x = clientPoint.X;
			var y = _userControlWinForm.ClientSize.Height - clientPoint.Y;
			_logger?.LogInformation($"GeneralImageViewer drag and drop at point [{x},{y},{passedData}]");
			if (!string.IsNullOrEmpty(passedData))
			{
                SetMultipleViewDataFolder(x, y, new List<string> { passedData });
            }
        }
	}
	#endregion

	#region ScreenshotSave


	public bool SaveScreenshotToSeires(ScreenshotProperties screenshotProperties)
	{
		return _cliWrapper.SaveScreenshotToSeries(_viewHandle, screenshotProperties);
	}
	#endregion

}