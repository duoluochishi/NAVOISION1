//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/10/23 15:07:54           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigService.Contract;
using NV.CT.DicomImageViewer;
using NV.CT.InterventionalScan.Models; 
using NV.CT.InterventionScan.ApplicationService.Contract;
using NVCTImageViewerInterop;
using System.Windows.Forms.Integration;

namespace NV.CT.InterventionalScan.ViewModel;
public class DicomImageViewModel : BaseViewModel
{
    private readonly IInterventionService _interventionService;
    private readonly IImageAnnotationService _imageAnnotationService;
    private WindowsFormsHost _windowsFormsHost = new WindowsFormsHost();

    public WindowsFormsHost WindowsFormsHost
    {
        get { return _windowsFormsHost; }
        set { SetProperty(ref _windowsFormsHost, value); }
    }
    public TomoImageViewer TomoImageViewer { get; set; }

    /// <summary>
    /// 序列窗位
    /// </summary>
    private double _wl = 0.0;
    public double Wl
    {
        get => _wl;
        set => SetProperty(ref _wl, value);
    }

    /// <summary>
    /// 序列窗宽
    /// </summary>
    private double _ww = 0.0;
    public double Ww
    {
        get => _ww;
        set => SetProperty(ref _ww, value);
    }

    private ObservableCollection<(string name, double[] pointStart, double[] pointEnd, ROI_Common_ViewStyle style)> _needles = new();
    public ObservableCollection<(string name, double[] pointStart, double[] pointEnd, ROI_Common_ViewStyle style)> Needles
    {
        get => _needles;
        set => SetProperty(ref _needles, value);
    }

    bool isIntervention = false;
    int intervertionIndex = -1;
    bool isSerialLoad = false;
    int imageTotal = 0;
    public DicomImageViewModel(ILogger<TomoImageViewer> logger,
        IInterventionService interventionService,
        IImageAnnotationService imageAnnotationService)
    {
        _interventionService = interventionService;
        _imageAnnotationService = imageAnnotationService;
        TomoImageViewer = new TomoImageViewer(1823, 995, logger, true);
        WindowsFormsHost = TomoImageViewer.WindowsFormsHost;
        TomoImageViewer.SetLayout(ViewLayout.Browser1a2);
        InitFourCornersMessage();
        TomoImageViewer.ShowCursorRelativeValue(true);

        TomoImageViewer.InterventionNeedleEvent -= TomoImageViewer_InterventionNeedleEvent;
        TomoImageViewer.InterventionNeedleEvent += TomoImageViewer_InterventionNeedleEvent;
        TomoImageViewer.SliceIndexChanged -= TomoImageViewer_SliceIndexChanged;
        TomoImageViewer.SliceIndexChanged += TomoImageViewer_SliceIndexChanged;
        TomoImageViewer.SerialLoaded -= TomoImageViewer_SerialLoaded;
        TomoImageViewer.SerialLoaded += TomoImageViewer_SerialLoaded;
        TomoImageViewer.WindowWidthLevelChanged -= TomoImageViewer_WindowWidthLevelChanged;
        TomoImageViewer.WindowWidthLevelChanged += TomoImageViewer_WindowWidthLevelChanged;
        TomoImageViewer.InterventionAddNeedleEvent -= TomoImageViewer_InterventionAddNeedleEvent;
        TomoImageViewer.InterventionAddNeedleEvent += TomoImageViewer_InterventionAddNeedleEvent;
        TomoImageViewer.InterventionSelectNeedleChanged -= TomoImageViewer_InterventionSelectNeedleChanged;
        TomoImageViewer.InterventionSelectNeedleChanged += TomoImageViewer_InterventionSelectNeedleChanged;
        TomoImageViewer.InterventionDelectNeedleEvent -= TomoImageViewer_InterventionDelectNeedleEvent;
        TomoImageViewer.InterventionDelectNeedleEvent += TomoImageViewer_InterventionDelectNeedleEvent;

        _interventionService.SetImagePathChanged -= InterventionService_SetImagePathChanged;
        _interventionService.SetImagePathChanged += InterventionService_SetImagePathChanged;
        _interventionService.OperationCommandChanged -= ImageOperationService_ImageOperationCommandChanged;
        _interventionService.OperationCommandChanged += ImageOperationService_ImageOperationCommandChanged;
    }

    [UIRoute]
    private void TomoImageViewer_InterventionAddNeedleEvent(object? sender, string needleName)
    {
        _interventionService.AddNeedleNotify(needleName);
    }

    [UIRoute]
    private void TomoImageViewer_InterventionDelectNeedleEvent(object? sender, string needleName)
    {
        _interventionService.DelectNeedleNotify(needleName);
    }

    [UIRoute]
    private void TomoImageViewer_InterventionSelectNeedleChanged(object? sender, string needleName)
    {
        _interventionService.SelectNeedleNotify(needleName);
    }

    [UIRoute]
    private void TomoImageViewer_WindowWidthLevelChanged(object? sender, (double ww, double wl) e)
    {
        this.Wl = e.wl;
        this.Ww = e.ww;
    }

    [UIRoute]
    private void TomoImageViewer_SerialLoaded(object? sender, (int handle, int readerID, int imageTotal) e)
    {
        if (e.imageTotal < 0)
        {
            return;
        }
        imageTotal = e.imageTotal;
        if (isIntervention && intervertionIndex < 0)
        {
            if (e.imageTotal % 2 == 0 && e.imageTotal > 0)
            {
                intervertionIndex = e.imageTotal / 2 - 1;
            }
            else
            {
                intervertionIndex = e.imageTotal / 2;
            }
            //索引是从零开始的
            if (intervertionIndex >= 1)
            {
                intervertionIndex -= 1;
            }
        }
        if (isIntervention)
        {
            SetSliceIndex(intervertionIndex);
            InitNeedle(true);
            TomoImageViewer.SetSelector();
        }
        if (Ww != 0.0 && Wl != 0.0)
        {
            TomoImageViewer.SetWWWL(Ww, Wl);
        }
    }

    [UIRoute]
    private void TomoImageViewer_SliceIndexChanged(object? sender, (int index, double pos, int total) e)
    {
        if (e.total <= 0)
        {
            return;
        }
        if (Math.Abs(e.total - imageTotal) <= 2 && !isSerialLoad && isIntervention && intervertionIndex >= 0)
        {
            intervertionIndex = e.index;
        }
    }

    [UIRoute]
    private void TomoImageViewer_InterventionNeedleEvent(object? sender, (string name, double[] pointStart, double[] pointEnd, ROI_Common_ViewStyle viewStyle) e)
    {
        if (string.IsNullOrEmpty(e.name))
        {
            return;
        }
        _interventionService.ImageSelectNeedleNameNotify(e.name);
        var needle = Needles.FirstOrDefault(t => t.name.Equals(e.name));
        if (string.IsNullOrEmpty(needle.name))
        {
            needle.name = e.name;
            needle.pointStart = e.pointStart;
            needle.pointEnd = e.pointEnd;
            needle.style = e.viewStyle;
            Needles.Add(needle);
        }
        else
        {
            Array.Copy(e.pointStart, needle.pointStart, 3);
            Array.Copy(e.pointEnd, needle.pointEnd, 3);
        }
    }

    private void InitFourCornersMessage()
    {
        var (tomoTextStyle, tomoTexts) = ImageSettingToOverlayText.Get(_imageAnnotationService.GetConfigs().ScanTomoSettings);
        if (tomoTexts.Count > 0)
        {
            TomoImageViewer.SetFourCornersMessage(tomoTextStyle, tomoTexts);
        }
    }

    [UIRoute]
    private void InterventionService_SetImagePathChanged(object? sender, EventArgs<(string path, bool isIntervention)> e)
    {
        isSerialLoad = true;
        if (e is null || !Directory.Exists(e.Data.path))
        {
            return;
        }
        if (Directory.Exists(e.Data.path))
        {
            TomoImageViewer.LoadImageWithDirectoryPath(e.Data.path);
        }
        else
        {
            TomoImageViewer.LoadImageWithFilePath(e.Data.path);
        }
        isIntervention = e.Data.isIntervention;
        isSerialLoad = false;
    }

    [UIRoute]
    private void ImageOperationService_ImageOperationCommandChanged(object? sender, EventArgs<(string commandStr, string parms)> e)
    {
        switch (e.Data.commandStr)
        {
            case CommandParameters.IMAGE_OPERATE_LAYOUT:
                SetLayout(e.Data.parms);
                break;
            case CommandParameters.IMAGE_OPERATE_ZOOM:
                TomoImageViewer.Zoom();
                break;
            case CommandParameters.IMAGE_OPERATE_MOVE:
                TomoImageViewer.Move();
                break;
            case CommandParameters.IMAGE_OPERATE_WL:
                SetWwWl(e.Data.parms);
                break;
            case CommandParameters.IMAGE_OPERATE_ROI:
                CreateCircleRoi(e.Data.parms);
                break;
            case CommandParameters.IMAGE_OPERATE_RESET:
                TomoImageViewer.Reset();
                Needles.Clear();
                break;
            case CommandParameters.IMAGE_OPERATE_SERIESMOVE:
                SeriesMove(e.Data.parms);
                break;
            case CommandParameters.IMAGE_OPERATE_SELECTNEEDLE:
                SelectNeedle(e.Data.parms);
                break;
            case CommandParameters.IMAGE_OPERATE_AddNEEDLE:
                AddNeedle(e.Data.parms);
                break;
            case CommandParameters.IMAGE_OPERATE_DELNEEDLE:
                DelNeedle(e.Data.parms);
                break;
            case CommandParameters.IMAGE_OPERATE_INITNEEDLE:
                bool init = false;
                if (bool.TryParse(e.Data.parms, out init))
                {
                    InitNeedle(init);
                }
                break;
            case CommandParameters.IMAGE_OPERATE_SETSLICEINDEX:
                //SetSliceIndex();
                break;
            default:
                TomoImageViewer.SetSelector();
                break;
        }
    }

    private void SetLayout(string param)
    {
        switch (param)
        {
            case CommandParameters.IMAGE_LAYOUT1_2:
                TomoImageViewer.SetLayout(ViewLayout.Browser1a2);
                break;
            case CommandParameters.IMAGE_LAYOUT1_3:
                TomoImageViewer.SetLayout(ViewLayout.Browser1x3);
                break;
            default:
                TomoImageViewer.SetLayout(ViewLayout.Browser1x1);
                break;
        }
    }

    private void CreateCircleRoi(string param)
    {
        switch (param)
        {
            case CommandParameters.IMAGE_OPERATE_DISTANCE:
                TomoImageViewer.CreateLengthRoi();
                break;
            case CommandParameters.IMAGE_OPERATE_ANGLE:
                TomoImageViewer.CreateAngleRoi();
                break;
            case CommandParameters.IMAGE_OPERATE_RECTANGLE:
                TomoImageViewer.CreateRectangleRoi();
                break;
            case CommandParameters.IMAGE_OPERATE_ARROW:
                TomoImageViewer.CreateArrowRoi();
                break;
            case CommandParameters.IMAGE_OPERATE_CIRCLE:
            default:
                TomoImageViewer.CreateCircleRoi();
                break;
        }
    }

    private void SetWwWl(string param)
    {
        if (string.IsNullOrEmpty(param))
        {
            TomoImageViewer.SetWWWL();
        }
        else
        {
            var wwwl = param.Split('*');
            if (wwwl.Length > 1)
            {
                double ww = 0;
                double wl = 0;
                double.TryParse(wwwl[0], out ww);
                double.TryParse(wwwl[1], out wl);
                TomoImageViewer.SetWindowWidthLevel(ww, wl);
            }
        }
    }

    private void SeriesMove(string param)
    {
        switch (param)
        {
            case CommandParameters.IMAGE_OPERATE_FORWARD:
                TomoImageViewer.MoveToNextSlice();
                break;
            case CommandParameters.IMAGE_OPERATE_BACK:
            default:
                TomoImageViewer.MoveToPriorSlice();
                break;
        }
    }

    private void AddNeedle(string needleNameColor)
    {
        if (!string.IsNullOrEmpty(needleNameColor) && needleNameColor.IndexOf(',') > 0)
        {
            var li = needleNameColor.Split(',');
            TomoImageViewer.CreateInterventionNeedle(li[0], li[1]);
        }
    }

    private void InitNeedle(bool isInit)
    {
        if (!(isInit && Needles.Count > 0))
        {
            return;
        }
        foreach (var needle in Needles)
        {
            TomoImageViewer.CreateInterventionNeedle(needle.name, needle.style);
        }
        TomoImageViewer.SetInterventionNeedle(Needles);
    }

    private void SelectNeedle(string needleName)
    {
        if (!string.IsNullOrEmpty(needleName))
        {
            TomoImageViewer.SelectNeedle(needleName);
        }
    }

    private void DelNeedle(string needleName)
    {
        if (string.IsNullOrEmpty(needleName))
        {
            return;
        }
        TomoImageViewer.DeleteNeedle(needleName);
        var needle = Needles.FirstOrDefault(t => t.name.Equals(needleName));
        if (!string.IsNullOrEmpty(needle.name))
        {
            Needles.Remove(needle);
        }
    }

    private void SetSliceIndex(int index)
    {
        int centerIndex = index;
        if (centerIndex > 0)
        {
            TomoImageViewer.SetInterventionViewSliceIndex(centerIndex);
        }
    }
}