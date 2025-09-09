//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.Model;
using NV.CT.ImageViewer.View;
using NV.CT.UI.Controls.Controls;
using NV.MPS.Configuration;
using System.Threading;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;

namespace NV.CT.ImageViewer.ViewModel;

public class Image3DViewModel : BaseViewModel
{
    private readonly ILogger<Image3DViewModel> _logger;
    private readonly IImageAnnotationService _imageAnnotationService;
    private System.Timers.Timer _timer;
    private int _timerinterval = 500;
    public GeneralImageViewer CurrentImageViewer { get; set; }
    private ObservableCollection<WindowingInfo>? _wwwlItems = new();
    public ObservableCollection<WindowingInfo>? WWWLItems
    {
        get => _wwwlItems;
        set => SetProperty(ref _wwwlItems, value);
    }
    private Switch3DButtonType _switchButtonItem = Switch3DButtonType.txtWWWL;
    public Switch3DButtonType SwitchButtonItem
    {
        get => _switchButtonItem;
        set => SetProperty(ref _switchButtonItem, value);
    }
    private bool _isVRorSSDMode = false;

    public bool IsVRorSSDMode
    {
        get => _isVRorSSDMode;
        set => SetProperty(ref _isVRorSSDMode, value);
    }
    private double _progress;

    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }
    private Visibility _progressbarvisibility=Visibility.Collapsed;

    public Visibility ProgressBarVisibility
    {
        get => _progressbarvisibility;
        set => SetProperty(ref _progressbarvisibility, value);
    }
    
    public Image3DViewModel(ILogger<Image3DViewModel> logger, IImageAnnotationService imageAnnotationService)
    {
        _logger = logger;
        _imageAnnotationService = imageAnnotationService;
        CurrentImageViewer = new GeneralImageViewer(MainControlViewerModel.Width, MainControlViewerModel.Height, true);
        EventAggregator.Instance.GetEvent<StudyChangedEvent>().Subscribe(ClearViewData);
        EventAggregator.Instance.GetEvent<SelectedSeriesChangedEvent>().Subscribe(ShowSeriesImage);
        EventAggregator.Instance.GetEvent<SeletedDataGridSeriesChangedEvent>().Subscribe(ShowDataGridSeries);
        CurrentImageViewer.OnROICreateSucceedEvent += CurrentImageViewer_OnROICreateSucceed;
        CurrentImageViewer.OnTableRemoveFinishedEvent += CurrentImageViewer_OnTableRemoveFinishedEvent;
        _timer = new System.Timers.Timer();
        _timer.Interval = _timerinterval;
        _timer.Elapsed += _timer_Elapsed;
        Init();
    }

    private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (CurrentImageViewer.GetAutoRemoveTableProcess() < 1)
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(
                () =>
                {
                    Progress = CurrentImageViewer.GetAutoRemoveTableProcess();
                });
        }
    }
    private void ClearViewData(bool obj)
    {
        CurrentImageViewer?.ClearView();
    }
    private void CurrentImageViewer_OnTableRemoveFinishedEvent(object? sender, (int, string) e)
    {
        System.Windows.Application.Current?.Dispatcher?.Invoke(() => { Progress = 1; _timer.Stop(); ProgressBarVisibility = Visibility.Collapsed; });
    }

    private void CurrentImageViewer_OnROICreateSucceed(object? sender, int e)
    {
        //if (e != CurrentImageViewer.ViewHandle)
        //{
        //    return;
        //}
        //EventAggregator.Instance.GetEvent<Update3DPreviousStateButtonEvent>().Publish(TextBlockListType.ViewAndMark);
        //SetDefaultSwitchButton(SwitchButtonItem);
    }
    private void SetDefaultSwitchButton(Switch3DButtonType switchButtonType)
    {
        switch (switchButtonType)
        {
            case Switch3DButtonType.txtMove:
                CurrentImageViewer.Move3D();
                break;
            case Switch3DButtonType.tbZoom:
                CurrentImageViewer.Zoom3D();    
                break;
            case Switch3DButtonType.txtWWWL:
                CurrentImageViewer.SetWWWL3D();    
                break;
            case Switch3DButtonType.txtRotate:
                CurrentImageViewer.SetRotate3D();
                break;
            default:
                CurrentImageViewer.SetWWWL3D();
                break;
        }
        EventAggregator.Instance.GetEvent<Update3DSwitchButtonEvent>().Publish(switchButtonType.GetDisplayName());
    }
    private void Init()
    {
        var windowTypes = UserConfig.WindowingConfig.Windowings;
        if (windowTypes is null)
            return;
        //添加自定义ww/wl
        //windowTypes.Add(new WindowingInfo()
        //{
        //    Width = new ItemField<int> { Value = 350,Default = 350 },
        //    Level = new ItemField<int> { Value = 20 , Default = 20 },
        //    BodyPart = "Custom",
        //    Shortcut = "F12",
        //    Description = "Custom"
        //});
        //foreach (var windowType in windowTypes)
        //{
        //    windowType.Description = windowType.BodyPart + "(" + windowType.Shortcut + ")";
        //}
        WWWLItems = windowTypes.ToObservableCollection();

        CurrentImageViewer?.ShowCursorRelativeValue(true);
        InitFourCornerInformation();

        //_Screenshot.ReturnScreenShotEvent = GetScreen;

        //全局层厚
        CurrentImageViewer?.SetGlobalSliceThickness(10);

        ViewCommands();
        MarkCommands();
        ViewModeCommands();
        AdvancedCommands();
        BatchReconCommands();
    }

    private void GetScreen(BitmapSource bitmap)
    {
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            var randomName = $"{DateTime.Now:yyyy-MM-dd_HHmmss}.png";
            bitmap?.SaveToFile($"D:\\{randomName}");
        });
    }

    public void InitFourCornerInformation()
    {
        var (topoTextStyle, topoTexts) = ImageSettingToOverlayText.Get(_imageAnnotationService.GetConfigs().MPRSettings);
        if (topoTexts.Count > 0)
        {
            CurrentImageViewer?.SetFourCornersMessage(topoTextStyle, topoTexts);
        }
    }

    public void ShowSeriesImage(ImageModel imageModel)
    {
        if (imageModel is null)
            return;
        if (CTS.Global.ServiceProvider.GetRequiredService<MainControlViewerModel>().CurrentView != ViewScene.View3D)
            return;
        CurrentImageViewer?.ClearView();
        if (imageModel.SeriesType == SeriesType.ScreenshotTypeImage || imageModel.SeriesType == SeriesType.DoseReport || imageModel.ImageType == Constants.IMAGE_TYPE_TOPO)
            return;
        if (imageModel.IsFile)
        {
            CurrentImageViewer?.LoadImageWithFilePath(imageModel.SeriesPath);
        }
        else
        {
            CurrentImageViewer?.LoadImageWithDirectoryPath3D(imageModel.SeriesPath);
        }
    }
    public void ShowDataGridSeries(SeriesModel seriesModel)
    {
        if (seriesModel is null)
            return;
        if (CTS.Global.ServiceProvider.GetRequiredService<MainControlViewerModel>().CurrentView != ViewScene.View3D)
            return;
        CurrentImageViewer?.ClearView();
        if (seriesModel.SeriesType == SeriesType.ScreenshotTypeImage|| seriesModel.SeriesType == SeriesType.DoseReport|| seriesModel.ImageType==Constants.IMAGE_TYPE_TOPO)
            return;
            CurrentImageViewer?.LoadImageWithDirectoryPath3D(seriesModel.SeriesPath);
    }
    private void UpdateCPRState()
    {
        //_log.Debug("cpr arg1 " + arg1.ToString());
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            isInCPRmode = false;
            //ToggleBackgroundImage(txtsurface, false);
        });
    }

    public bool isInCPRmode { get; set; }
    public bool Wrapper3DLoadImageState { get; private set; }

    private void UpdatePropertyState()
    {
        Wrapper3DLoadImageState = true;
    }

    private void ViewCommands()
    {
        if (CurrentImageViewer is null)
            return;

        Commands.Add(CommandName.Move, new DelegateCommand(()=> {
            CurrentImageViewer.Move3D();
            SwitchButtonItem = Switch3DButtonType.txtMove;
        }));
        Commands.Add(CommandName.Zoom, new DelegateCommand(() =>{
            CurrentImageViewer.Zoom3D();
            SwitchButtonItem = Switch3DButtonType.tbZoom;
        }));
        Commands.Add(CommandName.Wwwl, new DelegateCommand<string?>(_ =>
        {
            CurrentImageViewer.SetWWWL3D();
            SwitchButtonItem = Switch3DButtonType.txtWWWL;
        }));

        Commands.Add(CommandName.Rotate2D, new DelegateCommand(() => { 
            CurrentImageViewer.SetRotate2D();
            SwitchButtonItem = Switch3DButtonType.txtRotate;
            EventAggregator.Instance.GetEvent<Update3DPreviousStateButtonEvent>().Publish(TextBlockListType.ViewAndMark);
            EventAggregator.Instance.GetEvent<Update3DSwitchButtonEvent>().Publish(ConstName3D.txtRotate);
        }));
        Commands.Add(CommandName.Rotate3D, new DelegateCommand(() => { 
            CurrentImageViewer.SetRotate3D();
            SwitchButtonItem = Switch3DButtonType.txtRotate;
            EventAggregator.Instance.GetEvent<Update3DPreviousStateButtonEvent>().Publish(TextBlockListType.ViewAndMark);
            EventAggregator.Instance.GetEvent<Update3DSwitchButtonEvent>().Publish(ConstName3D.txtRotate);
        } ));
        Commands.Add(CommandName.Invert, new DelegateCommand(() => { UpdateStateButton(StateButtonType3D.txtInvert); }));
        Commands.Add(CommandName.HideAxis, new DelegateCommand(() => { UpdateStateButton(StateButtonType3D.tbAxis); }));
        Commands.Add(CommandName.HiddenText, new DelegateCommand(() => { UpdateStateButton(StateButtonType3D.tbHideTexts); }));
        Commands.Add(CommandName.InitialState, new DelegateCommand(() =>
        {
            CurrentImageViewer.Reset3D();
            Init3DButtonState();
            EventAggregator.Instance.GetEvent<Update3DPreviousStateButtonEvent>().Publish(TextBlockListType.ViewAndMark);
        }));
        Commands.Add(CommandName.Layout, new DelegateCommand<string?>(layoutStr =>
        {
            _logger?.LogInformation($"3D Layout to {layoutStr}");
            if (layoutStr is not null)
            {
                switch (layoutStr)
                {
                    case "L2R1":
                        CurrentImageViewer.Layout(ViewLayout.OrthoMPRL2R1);
                        break;
                    case "L1R2":
                        CurrentImageViewer.Layout(ViewLayout.OrthoMPRL1R2);
                        break;
                    case "C3":
                        CurrentImageViewer.Layout(ViewLayout.OrthoMPR1x3);
                        break;
                    case "U2D1":
                        CurrentImageViewer.Layout(ViewLayout.OrthoMPRU2D1);
                        break;
                    case "U1D2":
                        CurrentImageViewer.Layout(ViewLayout.OrthoMPRU1D2);
                        break;
                    case "R3":
                        CurrentImageViewer.Layout(ViewLayout.OrthoMPR3x1);
                        break;
                    case "R2C2":
                        CurrentImageViewer.Layout(ViewLayout.OrthoMPR2x2);
                        break;
                    default:
                        CurrentImageViewer.Layout(ViewLayout.OrthoMPRL2R1);
                        break;
                }
            }
        }, _ => true));
    }
    private void Init3DButtonState()
    {
        ConstName3D.StateButtonDictionary[ConstName3D.tbHideTexts] = true;
        ConstName3D.StateButtonDictionary[ConstName3D.tbAxis] = true;
        CurrentImageViewer.InitInvert3D();
    }
    private void UpdateStateButton(StateButtonType3D stateButtonType)
    {
        StateButton stateButton = new StateButton();
        switch (stateButtonType)
        {
            case StateButtonType3D.tbHideTexts:
                ConstName3D.StateButtonDictionary[ConstName3D.tbHideTexts] = !ConstName3D.StateButtonDictionary[ConstName3D.tbHideTexts];
                stateButton.ButtonName = ConstName3D.tbHideTexts;
                stateButton.ButtonState = ConstName3D.StateButtonDictionary[ConstName3D.tbHideTexts];
                CurrentImageViewer.ToggleOverlay();
                break;
            case StateButtonType3D.tbAxis:
                ConstName3D.StateButtonDictionary[ConstName3D.tbAxis] = !ConstName3D.StateButtonDictionary[ConstName3D.tbAxis];
                stateButton.ButtonName = ConstName3D.tbAxis;
                stateButton.ButtonState = ConstName3D.StateButtonDictionary[ConstName3D.tbAxis];
                CurrentImageViewer.ToggleAxis3D();
                break;
            case StateButtonType3D.txtCut:
                ConstName3D.StateButtonDictionary[ConstName3D.txtCut] = !ConstName3D.StateButtonDictionary[ConstName3D.txtCut];
                stateButton.ButtonName = ConstName3D.txtCut;
                stateButton.ButtonState = ConstName3D.StateButtonDictionary[ConstName3D.txtCut];
                break;
            case StateButtonType3D.tbClipBox:
                ConstName3D.StateButtonDictionary[ConstName3D.tbClipBox] = !ConstName3D.StateButtonDictionary[ConstName3D.tbClipBox];
                stateButton.ButtonName = ConstName3D.tbClipBox;
                stateButton.ButtonState = ConstName3D.StateButtonDictionary[ConstName3D.tbClipBox];
                break;
            case StateButtonType3D.tbRemoveTable:
                ConstName3D.StateButtonDictionary[ConstName3D.tbRemoveTable] = !ConstName3D.StateButtonDictionary[ConstName3D.tbRemoveTable];
                stateButton.ButtonName = ConstName3D.tbRemoveTable;
                stateButton.ButtonState = ConstName3D.StateButtonDictionary[ConstName3D.tbRemoveTable];
                break;
            case StateButtonType3D.txtInvert:
                ConstName3D.StateButtonDictionary[ConstName3D.txtInvert] = !ConstName3D.StateButtonDictionary[ConstName3D.txtInvert];
                stateButton.ButtonName = ConstName3D.txtInvert;
                stateButton.ButtonState = ConstName3D.StateButtonDictionary[ConstName3D.txtInvert];
                CurrentImageViewer.Invert3D();
                break;
            default:
                break;
        }
        EventAggregator.Instance.GetEvent<Update3DStateButtonEvent>().Publish(stateButton);
    }
    private void MarkCommands()
    {
        if (CurrentImageViewer is null)
            return;

        Commands.Add(CommandName.Create_Length_ROI, new DelegateCommand(() => CurrentImageViewer.CreateROI(ROIType.ROI_Length)));
        Commands.Add(CommandName.Create_Angle_ROI, new DelegateCommand(() => CurrentImageViewer.CreateROI(ROIType.ROI_Angle)));
        Commands.Add(CommandName.Create_Circle_ROI, new DelegateCommand(() => CurrentImageViewer.CreateROI(ROIType.ROI_Circle)));
        Commands.Add(CommandName.Create_Arrow_ROI, new DelegateCommand(() => CurrentImageViewer.CreateROI(ROIType.ROI_Arrow)));
        Commands.Add(CommandName.Remove_All_ROI3D, new DelegateCommand(CurrentImageViewer.RemoveAllROI3D));
    }

    private void ViewModeCommands()
    {
        if (CurrentImageViewer is null)
            return;

        Commands.Add(CommandName.MPR, new DelegateCommand<ImageResliceMode?>(_ => {SwitchImageResliceMode(ImageResliceMode.MPR);}, _ => true));
        Commands.Add(CommandName.MIP, new DelegateCommand<ImageResliceMode?>(_ => {SwitchImageResliceMode(ImageResliceMode.MIP);}, _ => true));
        Commands.Add(CommandName.MinIP, new DelegateCommand<ImageResliceMode?>(_ =>{SwitchImageResliceMode(ImageResliceMode.MinIP);}, _ => true));
        Commands.Add(CommandName.AVG, new DelegateCommand<ImageResliceMode?>(_ =>{SwitchImageResliceMode(ImageResliceMode.Avg);}, _ => true));

        Commands.Add(CommandName.VR, new DelegateCommand(()=> {
            IsVRorSSDMode = true;
            CurrentImageViewer.SetVR();            
        }));
        Commands.Add(CommandName.SSD, new DelegateCommand(()=> {
            IsVRorSSDMode = true;

            CurrentImageViewer.CloseCPRMode();
            CurrentImageViewer.SetSSD();
        }));
        Commands.Add(CommandName.CPR, new DelegateCommand(()=> {
            SwitchViewMode();
            CurrentImageViewer.SetCPR();
        }));
        Commands.Add(CommandName.VRTReset, new DelegateCommand(() => {
            ConstName3D.StateButtonDictionary[ConstName3D.txtCut] = true;
            ConstName3D.StateButtonDictionary[ConstName3D.tbClipBox] = true;
            EventAggregator.Instance.GetEvent<Update3DPreviousStateButtonEvent>().Publish(TextBlockListType.Advance);
            CurrentImageViewer.SetVRTReset();
        }));
    }
    private  void SwitchImageResliceMode(ImageResliceMode imageResliceMode)
    {
        CurrentImageViewer.SetMPR(imageResliceMode);
        SwitchViewMode();
        EventAggregator.Instance.GetEvent<Update3DPreviousStateButtonEvent>().Publish(TextBlockListType.ViewMode);
        EventAggregator.Instance.GetEvent<Update3DSwitchButtonEvent>().Publish(ConstName3D.txtMMPR);
    }
    private void SwitchViewMode()
    {
        IsVRorSSDMode = false;
        CurrentImageViewer.InitVRorSSDViewMode();
        EventAggregator.Instance.GetEvent<Update3DPreviousStateButtonEvent>().Publish(TextBlockListType.Advance);
        ConstName3D.StateButtonDictionary[ConstName3D.txtCut] = true;
        ConstName3D.StateButtonDictionary[ConstName3D.tbClipBox] = true;
    }
    private void AdvancedCommands()
    {
        if (CurrentImageViewer is null)
            return;

        Commands.Add(CommandName.Screenshot, new DelegateCommand(() =>
        {
            var dicomDetailItems= CTS.Global.ServiceProvider.GetService<Image2DViewModel>()?.DicomDetailItems?.ToList();
            CTS.Global.ServiceProvider.GetService<ScreenshotViewModel>()?.ScreeShot(dicomDetailItems, CurrentImageViewer);
        }));

        Commands.Add(CommandName.ClipBox, new DelegateCommand(()=> 
        {
            CurrentImageViewer.InitVRorSSDViewMode();
            EventAggregator.Instance.GetEvent<Update3DPreviousStateButtonEvent>().Publish(TextBlockListType.Advance);
            UpdateStateButton(StateButtonType3D.tbClipBox);
            ConstName3D.StateButtonDictionary[ConstName3D.txtCut] = true;
            CurrentImageViewer.SetClipBox(!ConstName3D.StateButtonDictionary[ConstName3D.tbClipBox]);
        }));
        Commands.Add(CommandName.CutMode, new DelegateCommand(() =>
        {
            InitVolumePolyDataCut();
            UpdateStateButton(StateButtonType3D.txtCut);
            CurrentImageViewer.SetCutState(!ConstName3D.StateButtonDictionary[ConstName3D.txtCut]);
        }));
        Commands.Add(CommandName.SelectedCut, new DelegateCommand(()=> 
        {
            InitVolumePolyDataCut();
            CurrentImageViewer.SetSelectedCut();
        }));
        Commands.Add(CommandName.UnselectedCut, new DelegateCommand(()=> 
        {
            InitVolumePolyDataCut();
            CurrentImageViewer.SetUnselectedCut();
        }));
        Commands.Add(CommandName.UndoVolumeCut, new DelegateCommand(()=> 
        { 
            CurrentImageViewer.UndoVolumeCut(); 
        }));
        Commands.Add(CommandName.RedoVolumeCut, new DelegateCommand(()=> 
        { 
            CurrentImageViewer.RedoVolumeCut(); 
        }));
        Commands.Add(CommandName.RemoveTable, new DelegateCommand(() =>
        {
            UpdateStateButton(StateButtonType3D.tbRemoveTable);
            CurrentImageViewer.AutoRemoveTable(!ConstName3D.StateButtonDictionary[ConstName3D.tbRemoveTable]);
            if (CurrentImageViewer.GetAutoRemoveTableProcess() < 1)
            {
                System.Windows.Application.Current?.Dispatcher?.Invoke(
                    () =>
                    {
                        _timer.Start();
                        Progress = CurrentImageViewer.GetAutoRemoveTableProcess();
                        ProgressBarVisibility = Visibility.Visible;
                    });
            }    
        }));
        //preset
        Commands.Add(CommandName.AdvancePreset, new DelegateCommand<string?>(presetName =>
        {
            if (string.IsNullOrEmpty(presetName))
                return;
            CurrentImageViewer.SetAdvancedPreset(presetName);
        }));
    }

    private void BatchReconCommands()
    {
        if (CurrentImageViewer is null)
            return;
        Commands.Add(CommandName.MPRRecon, new DelegateCommand(() =>
        {
            //CommonMethod.ShowCustomWindow(typeof(BatchSettingWindow));
            //CurrentImageViewer.MPRRecon(4,350,ViewOrientation.Sagittal,ReconMode.RectRecon);
        }));
        Commands.Add(CommandName.MultiAngleRecon, new DelegateCommand(() =>
        {
            //CurrentImageViewer.MPRRecon(4, 350, ViewOrientation.Axial, ReconMode.RectRotateRecon);
        }));
        Commands.Add(CommandName.FreeSliceRecon, new DelegateCommand(() =>
        {
            //CurrentImageViewer.MPRRecon(4, 350, ViewOrientation.Sagittal, ReconMode.StraightLineSliceRecon);
        }));
        Commands.Add(CommandName.RGBDataRecon, new DelegateCommand(() =>
        {
        }));
    }
    private void InitVolumePolyDataCut()
    {
        CurrentImageViewer.InitVRorSSDViewMode();
        ConstName3D.StateButtonDictionary[ConstName3D.tbClipBox] = true;
        EventAggregator.Instance.GetEvent<Update3DPreviousStateButtonEvent>().Publish(TextBlockListType.Advance);
        EventAggregator.Instance.GetEvent<Update3DSwitchButtonEvent>().Publish(ConstName3D.txtCut);
    }

}
