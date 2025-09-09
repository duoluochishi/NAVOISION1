//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Extensions;
using NVCTImageViewerInterop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using LogLevel = NVCTImageViewerInterop.LogLevel;
using Color = System.Windows.Media.Color;
using MouseButton = NVCTImageViewerInterop.MouseButton;

namespace NV.CT.DicomImageViewer;

public class PrintImageViewer
{
    #region 私有变量  
    private NvImageViewerWrapperCLI? _cliWrapper;
    private IntPtr _winFormHandle;
    private int _viewHandle;
    private WindowsFormsHost? _windowsFormsHost;
    private UserControl? _userControlWinForm;
    private readonly ILogger<PrintImageViewer>? _logger;
    private bool _overlayVisibility = true;
    public const string DragObjectKey = "series_drag_object";
    private LookupTable _lookupTable = LookupTable.LUT_BW;

    public event EventHandler<int>? OnCineFinished;
    public event EventHandler<(int, double, int)>? SliceIndexChanged;
    public event EventHandler<(double ww, double wl)>? WindowWidthLevelChanged;
    public event EventHandler<int>? OnViewLoadFinishedEvent;
    public event EventHandler<int>? OnCPRFinishedEvent;

    public event EventHandler<(int totalNumber, int pageNumber)>? NotifyPrintPageChanged;
    public event EventHandler<(int pageNumber, Bitmap value)>? NotifyReadyPage;
    public event EventHandler<bool>? NotifyAllPagesDone;

    public event EventHandler<(NVCTImageViewerInterop.LogLevel logLevel, string message)>? NotifyImageLog;

    public event EventHandler<string>? NotifyPrintImageModified;
    public event EventHandler<string>? NotifyPrintLayoutModified;

    public WindowsFormsHost? WindowsFormsHost
    {
        get => _windowsFormsHost;
        set => _windowsFormsHost = value;
    }

    #endregion

    #region 构造函数

    public PrintImageViewer(int width, int height)
    {
        _logger = CTS.Global.ServiceProvider.GetService<ILogger<PrintImageViewer>>();

        InitTomoImageViewerPre(width, height);
        CreatePrintView(width, height);
        InitTomoImageViewerPost(width, height);
    }

    private void InitTomoImageViewerPre(int width, int height)
    {
        _logger?.LogDebug($"PrintImageViewer action info:Start Initialize {width}*{height}");
        _cliWrapper = new NvImageViewerWrapperCLI();
        _windowsFormsHost = new WindowsFormsHost();
        _windowsFormsHost.AllowDrop = true;
        _windowsFormsHost.Background = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        _windowsFormsHost.IsHitTestVisible = false;

        _userControlWinForm = new UserControl();
        _userControlWinForm.AllowDrop = true;

        _winFormHandle = _userControlWinForm.Handle;
        _windowsFormsHost.Child = _userControlWinForm;

        _cliWrapper.Initialize();
    }

    private void InitTomoImageViewerPost(int width, int height)
    {
        InitEventHandler();
        SetMouseActions();
        _logger?.LogDebug($"PrintImageViewer action info:End Initialize TomoImageViewer {width}*{height}");
    }

    private void SetMouseActions()
    {
        _cliWrapper?.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
        _cliWrapper?.SetViewMouseAction(_viewHandle, MouseButton.MouseWheelButtonForwardAndBackward, ViewMouseAction.BrowserMouseAction_Slicing, ROIType.ROI_None);
        _cliWrapper?.SetViewMouseAction(_viewHandle, MouseButton.RightMouseButton, ViewMouseAction.BrowserMouseAction_Zoom, ROIType.ROI_None);
    }

    /// <summary>
    /// 可以切换布局的多序列用的
    /// </summary>
    public void CreatePrintView(int width, int height)
    {
        _viewHandle = _cliWrapper.CreatePrintView(this._winFormHandle, 0, 0, width, height);
    }

    public void ClearView()
    {
        WindowsFormsHost?.Dispatcher.Invoke(() =>
        {
            _cliWrapper?.ClearView(_viewHandle);
        });
    }

    public void ViewRender()
    {
        WindowsFormsHost?.Dispatcher.Invoke(() =>
        {
           // _cliWrapper?.ViewRender(_viewHandle);
        });
    }

    public void MoveView(int width, int height)
    {
        WindowsFormsHost?.Dispatcher.Invoke(() =>
        {
            _cliWrapper?.MoveView(_viewHandle,0,0, width, height);
        });
    }

    private void InitEventHandler()
    {
        if (_cliWrapper is null)
        {
            return;
        }
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

        //Print callback
        _cliWrapper.PageWindowDataNotify -= CliWrapper_OnPageWindowDataNotify;
        _cliWrapper.GetPageWindowDataSucceedNotify -= CliWrapper_OnPageWindowDataSucceedNotify;
        _cliWrapper.PrintPageChangedNotify -= CliWrapper_OnPrintPageChangedNotify;
        _cliWrapper.PrintImageModifiedNotify -= CliWrapper_OnPrintImageModifiedNotify;
        _cliWrapper.PrintLayoutModifiedNotify -= CliWrapper_OnPrintLayoutModifiedNotify;
        _cliWrapper.ImageLogerNotify -= CliWrapper_OnImageLogerNotify;

        _cliWrapper.PageWindowDataNotify += CliWrapper_OnPageWindowDataNotify;
        _cliWrapper.GetPageWindowDataSucceedNotify += CliWrapper_OnPageWindowDataSucceedNotify;
        _cliWrapper.PrintPageChangedNotify += CliWrapper_OnPrintPageChangedNotify;
        _cliWrapper.PrintImageModifiedNotify += CliWrapper_OnPrintImageModifiedNotify;
        _cliWrapper.PrintLayoutModifiedNotify += CliWrapper_OnPrintLayoutModifiedNotify;
        _cliWrapper.ImageLogerNotify += CliWrapper_OnImageLogerNotify;
    }

    public void InsertLocationImage(string filePath)
    {
        WindowsFormsHost?.Dispatcher.Invoke(() =>
        {
            _cliWrapper?.InsertLocationImage(_viewHandle, filePath);
        });
    }

    public void SetInsertLocationRenderViewPosition(OverlayTextPosition position)
    {
        WindowsFormsHost?.Dispatcher.Invoke(() =>
        {
            _cliWrapper?.SetInsertLocationRenderViewPosition(_viewHandle, position);
        });
    }

    private void CliWrapper_OnImageLogerNotify(NVCTImageViewerInterop.LogLevel logLevel, string message)
    {
        NotifyImageLog?.Invoke(this, (logLevel, message));
    }

    private void CliWrapper_OnPrintImageModifiedNotify(int viewHandle, List<MedImageProperty> imageList)
    {
        NotifyPrintImageModified?.Invoke(this, imageList.ToJson());
    }

    private void CliWrapper_OnPrintLayoutModifiedNotify(int viewHandle, List<ItemRect> layoutItemList)
    {
        NotifyPrintLayoutModified?.Invoke(this, layoutItemList.ToJson());
    }

    private void CliWrapper_OnPageWindowDataSucceedNotify(int viewHandle, bool isAllDone)
    {
        NotifyAllPagesDone?.Invoke(this, isAllDone);
    }

    private void CliWrapper_OnPageWindowDataNotify(int viewHandle, int pageNumber, Bitmap bitmap)
    {
        NotifyReadyPage?.Invoke(this, (pageNumber, bitmap));
    }

    private void CliWrapper_OnPrintPageChangedNotify(int viewHandle, int totalNumberOfPages, int pageNumber)
    {
        NotifyPrintPageChanged?.Invoke(this, (totalNumberOfPages, pageNumber));
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

    private void CliWrapper_ImageLogerNotify(LogLevel logLevel, string message)
    {
        //sllin CliWrapper_ImageLogerNotify
        switch (logLevel)
        {
            case LogLevel.Debug:
                //_logger?.LogDebug($"NvImageViewerWrapperCLI:{message}");
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
                //_logger?.LogTrace($"NvImageViewerWrapperCLI:{message}");
                break;
            case LogLevel.Information:
            case LogLevel.None:
            default:
                //_logger?.LogInformation($"NvImageViewerWrapperCLI:{message}");
                break;
        }
    }
    #endregion

    #region 方法   

    /// <summary>
    /// 设置页眉页脚
    /// </summary>
    /// <param name="headerHeight">页眉归一化高度</param>
    /// <param name="headerItems">页眉字段列表</param>
    /// <param name="footerHeighter">页脚归一化高度</param>
    /// <param name="footerItems">页脚字段列表</param>
    public void SetFilmHeaderAndFooter(float headerHeight, List<FilmFieldItem> headerItems, float footerHeighter, List<FilmFieldItem> footerItems)
    {
        _cliWrapper?.SetFilmHeaderAndFooter(_viewHandle, headerHeight, headerItems, footerHeighter, footerItems);
    }

    /// <summary>
    /// 设置页眉页脚Logo
    /// </summary>
    /// <param name="headerLogo">页眉Logo</param>
    /// <param name="footerLogo">页脚Logo</param>
    public void SetLogo(FilmLogoItem headerLogo, FilmLogoItem footerLogo)
    {
        _cliWrapper?.SetLogo(_viewHandle, headerLogo, footerLogo);
    }

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
        _cliWrapper?.ShowOverlay(_viewHandle, _overlayVisibility);
    }

    /// <summary>
    /// 隐藏 四角信息
    /// </summary>
    public void HiddenText()
    {
        ShowOverlay(false);
    }

    public void SetVisibilityOfPageHeader(bool showHeader)
    {
        _cliWrapper?.SetPageHeaderRenderShow(_viewHandle, showHeader);
    }
    public void SetVisibilityOfPageFooter(bool showFooter)
    {
        _cliWrapper?.SetPageFooterRenderShow(_viewHandle, showFooter);
    }


    public void SetWindowWidthLevel(double windowWidth, double windowLevel)
    {
        if (windowWidth <= 0 && windowLevel <= 0)
        {
            return;
        }
        _cliWrapper?.SetWWWL(_viewHandle, windowWidth, windowLevel);
    }

    public void SetWWWL()
    {
        _cliWrapper?.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
    }
    /// <summary>
    /// 设置窗宽窗位
    /// </summary>   
    public void SetWWWL(double ww, double wl)
    {
        _cliWrapper?.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
        _cliWrapper?.SetWWWL(_viewHandle, ww, wl);
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
        _logger?.LogDebug($"PrintImageViewer action info:Start PrintImageViewer Zoom");
        _cliWrapper?.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_Zoom, ROIType.ROI_None);
        _logger?.LogDebug($"PrintImageViewer action info:End PrintImageViewer Zoom");
    }

    /// <summary>
    /// 2D角度旋转
    /// </summary>
    public void Rotate(int degree)
    {
        WindowsFormsHost?.Dispatcher.Invoke(() =>
        {
            _cliWrapper?.Rotate2D(_viewHandle, degree);
        });
    }

    /// <summary>
    /// 2D ROI重置
    /// </summary>
    public void Reset()
    {
        _cliWrapper?.ResetView(_viewHandle);

        ShowOverlay(true);
        SetMouseActions();
    }

    /// <summary>
    /// 布局 2d/3d 通用
    /// </summary>
    public void SetLayout(ViewLayout viewLayout)
    {
        _cliWrapper?.SetLayout(_viewHandle, viewLayout);
    }

    /// 布局 2d/3d 通用
    /// </summary>
    public void SetLayout(int columnNum, int rowNum)
    {
        _cliWrapper?.SetLayout(_viewHandle, columnNum, rowNum);
    }

    /// <summary>
    /// 设置用户自定义布局
    /// </summary>
    /// <param name="layoutItemList"></param>
    public void SetUserDefineLayout(List<ItemRect> layoutItemList)
    {
        _cliWrapper?.SetUserDefineLayout(_viewHandle, layoutItemList);
    }

    /// <summary>
    /// 设置布局模式
    /// </summary>
    public void SetImageBrowserMode(BrowseMode browseMode)
    {
        _cliWrapper?.SetImageBrowserMode(_viewHandle, browseMode);
    }

    public void SetMultipleViewDataFolder(int x, int y, List<string> folderList)
    {
        _cliWrapper?.SetDropFilesEvent(_viewHandle, x, y, folderList);
    }

    public void ReverseSequence()
    {
        _cliWrapper?.Inverse(_viewHandle);
    }

    public void Invert()
    {
        if (_lookupTable == LookupTable.LUT_BW)
        {
            _cliWrapper?.SetLookupTable(_viewHandle, LookupTable.LUT_BWInverse);
            _lookupTable = LookupTable.LUT_BWInverse;
        }
        else
        {
            _cliWrapper?.SetLookupTable(_viewHandle, LookupTable.LUT_BW);
            _lookupTable = LookupTable.LUT_BW;
        }
    }

    public void Kernel(string kernelType)
    {
        switch (kernelType)
        {
            case "None":
                _cliWrapper?.SetImageFilter(_viewHandle, ImageFilter.ImageFilter_None);
                break;
            case "Sharp":
                _cliWrapper?.SetImageFilter(_viewHandle, ImageFilter.ImageFilter_Sharpen);
                break;
            case "Sharp+":
                _cliWrapper?.SetImageFilter(_viewHandle, ImageFilter.ImageFilter_Very_Sharpen);
                break;
            case "Smooth":
                _cliWrapper?.SetImageFilter(_viewHandle, ImageFilter.ImageFilter_Smooth);
                break;
            case "Smooth+":
                _cliWrapper?.SetImageFilter(_viewHandle, ImageFilter.ImageFilter_Very_Smooth);
                break;
        }
    }

    public void FlipVertical()
    {
        //TODO:后期鼠标操作统一调整
        //_cliWrapper.SetViewMouseAction(SIEMouseButton.SIELeftMouseButton, SIEBrowserMouseAction.SIEBrowserMouseActionSelector);
        _cliWrapper?.FlipY(_viewHandle);
    }

    public void FlipHorizontal()
    {
        //_cliWrapper.SetViewMouseAction(SIEMouseButton.SIELeftMouseButton, SIEBrowserMouseAction.SIEBrowserMouseActionSelector);
        _cliWrapper?.FlipX(_viewHandle);
    }

    public void Copy()
    {
        _cliWrapper?.CopySelectedPirntViewData(_viewHandle);
    }

    public void Paste()
    {
        _cliWrapper?.PasteViewData(_viewHandle);
    }

    public void Cut()
    {
        Copy();
        Delete();
    }
    public void SelectAll()
    {
        _cliWrapper?.SelectedAllPrintViewData(_viewHandle,true);
    }
    public void UnSelectAll()
    {
        _cliWrapper?.SelectedAllPrintViewData(_viewHandle,false);
    }

    public void Delete()
    {
        _cliWrapper?.RemoveSelectRenderData(_viewHandle);
    }

    public void ShowCursorRelativeValue(bool flag)
    {
        _cliWrapper?.ShowCursorRelativeValue(_viewHandle, flag);
    }

    public void LoadImageWithFilePath(string imageFile)
    {
        if (!string.IsNullOrEmpty(imageFile) && File.Exists(imageFile))
        {
            WindowsFormsHost.Dispatcher.Invoke(() =>
            {
                _logger?.LogDebug($"PrintImageViewer action info:Start PrintImageViewer SetViewDataFile:{imageFile}");
                ShowOverlay(false);
                _cliWrapper?.SetViewDataFile(_viewHandle, imageFile);
                _logger?.LogDebug($"PrintImageViewer action info:End PrintImageViewer SetViewDataFile");
            });
        }
    }

    public void LoadImageWithDirectoryPath(string imagePath)
    {
        if (!string.IsNullOrEmpty(imagePath) && Directory.Exists(imagePath))
        {
            WindowsFormsHost?.Dispatcher.Invoke(() =>
            {
                _cliWrapper?.SetMultipleViewDataFloder(_viewHandle, new List<string> { imagePath }, 0);

                ShowOverlay(true);
            });

        }
    }
    public void SetFourCornersMessage(OverlayTextStyle overlayTextStyle, List<OverlayText> overlayTexts)
    {
        if (overlayTexts.Count > 0)
        {
            WindowsFormsHost?.Dispatcher.Invoke(() =>
            {
                _cliWrapper?.InitOverlayText(_viewHandle, overlayTextStyle, overlayTexts, 0);
            });
        }
    }
    
    public void SetPrintViewData(List<MedImageProperty> imageList)
    {
        _cliWrapper?.SetPrintViewData(_viewHandle, imageList);
    }

    public void AppendPrintViewData(List<MedImageProperty> imageList)
    {
        _cliWrapper?.AppendPrintViewData(_viewHandle, imageList);
    }

    public void SetShowRuler(bool isShowRuler = false)
    {
        _cliWrapper?.ShowRuler(_viewHandle, isShowRuler);
    }

    public void TurnToPrintPageByNumber(int pageNumber)
    {
        if (pageNumber < 0)
        {
            _logger?.LogError($"The pageNumber is not valid in TurnToPrintPage: {pageNumber}");
            return;
        }

        _cliWrapper?.SetPrintRenderViewPage(_viewHandle, pageNumber);
    }


    public bool? GetAllPagesOfPrintPreview(int scale)
    {
        return _cliWrapper?.GetPrintRenderViewTotalImage(_viewHandle, scale);
    }
    public bool? GetSinglePrintPageByIndex(int index, int scale)
    {
        return _cliWrapper?.GetPrintRenderViewPagetImage(_viewHandle, index, scale);
    }
    public int? GetTotalNumberOfPrintPages()
    {
        return _cliWrapper?.GetPrintTotalPage(_viewHandle);
    }

    public string GetSelectedRenderViewDataPath()
    {
        return _cliWrapper?.GetSelectedRenderViewDataPath(_viewHandle);
    }

    public void MergeCell()
    {
        _cliWrapper?.MergeCell(_viewHandle);
    }

    public void SplitCell(int cols, int rows)
    {
        _cliWrapper?.SplitCell(_viewHandle, cols, rows);
    }

    public void CreateROI(ROIType roiType)
    {
        _cliWrapper?.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_CreateROI, roiType);
    }

    public void RemoveAllROI()
    {
        _cliWrapper?.RemoveAllROI(_viewHandle);
        _cliWrapper?.SetVolumePolyDataCut(_viewHandle, false, true);
    }

    public void ResetMouseSelector()
    {
        _cliWrapper?.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.BrowserMouseAction_WWWL, ROIType.ROI_None);
    }
    #endregion

    #region 通用方法
    public void SetMultipleViewDataFolder(List<string> folderList)
    {
        _cliWrapper?.SetMultipleViewDataFloder(_viewHandle, folderList, 0);
    }
    /// <summary>
    /// 设置为选中状态
    /// </summary>
    public void SetSelector()
    {
        _cliWrapper?.SetViewMouseAction(_viewHandle, MouseButton.LeftMouseButton, ViewMouseAction.MouseAction_None, ROIType.ROI_None);
    }

    public void LoadDicomSeriesReverseOrder()
    {
        _cliWrapper?.Inverse(_viewHandle);
    }

    public void SetSliceIndex(int currentIndex)
    {
        if (currentIndex < 0)
        {
            return;
        }
        _cliWrapper?.SetSliceIndex(_viewHandle, currentIndex);
    }

    public void LoadImageWithDirectoryPath(string imagePath, int matrixHeight, int direction)
    {
        if (!string.IsNullOrEmpty(imagePath) && Directory.Exists(imagePath))
        {
            WindowsFormsHost?.Dispatcher.Invoke(() =>
            {
                _cliWrapper?.SetViewDataFloder(_viewHandle, imagePath, matrixHeight, direction);
                ShowOverlay(true);
            });
        }
    }
    #endregion
}