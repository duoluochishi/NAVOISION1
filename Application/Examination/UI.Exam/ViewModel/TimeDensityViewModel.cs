//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//----------------------------------------------------------------------

using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing; 
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using NVCTImageViewerInterop;
using SkiaSharp;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Environment;
using NV.CT.UI.Exam.Extensions;

namespace NV.CT.UI.Exam.ViewModel;

public class TimeDensityViewModel : BaseViewModel
{
    private readonly IProtocolHostService _protocolHostService;
    private readonly ISelectionManager _selectionManager;
    private readonly IImageOperationService _imageOperationService;
    private static string Color1String = "1E90FF";
    private static string Color2String = "5DE2E7";
    private static string Color3String = "EEEE00";
    private static string NameFirst = "First";
    private static string NameSecond = "Second";
    private static string NameThird = "Third";

    private static int FirstID = 1;
    private static int SecondID = 2;
    private static int ThirdID = 3;

    private readonly ILogger<TimeDensityViewModel> _logger;
    private SolidColorPaint _LabelsPaint = new SolidColorPaint { Color = new(25, 118, 210), StrokeThickness = 1f };
    public ObservableCollection<ISeries> _series = new ObservableCollection<ISeries>();
    public ObservableCollection<ISeries> Series { get { return _series; } set { SetProperty(ref _series, value); } }

    public IPaint<SkiaSharpDrawingContext> _legendTextPaint = new SolidColorPaint(SKColor.Parse(Color1String), 2);
    public IPaint<SkiaSharpDrawingContext> LegendTextPaintB { get { return _legendTextPaint; } set { SetProperty(ref _legendTextPaint, value); } }

    private Axis[] _xAxis = default;
    public Axis[] XAxis { get { return _xAxis; } set { SetProperty(ref _xAxis, value); } }

    private Axis[] _yAxis = default;
    public Axis[] YAxis { get { return _yAxis; } set { SetProperty(ref _yAxis, value); } }

    private ReconModel? CurrentReconModel { get; set; }

    private ScanModel? CurrentScanModel { get; set; }

    Dictionary<string, CycleROIModel> RoiDict = new Dictionary<string, CycleROIModel>();

    Dictionary<int, ISeries> SerieDict = new Dictionary<int, ISeries>();

    private List<ObservablePoint> mHu1 = new List<ObservablePoint>();
    private List<ObservablePoint> mHu2 = new List<ObservablePoint>();
    private List<ObservablePoint> mHu3 = new List<ObservablePoint>();

    public TimeDensityViewModel(IImageOperationService imageOperationService,
        IProtocolHostService protocolHostService,
        ISelectionManager selectionManager,
        ILogger<TimeDensityViewModel> logger)
    {
        _protocolHostService = protocolHostService;
        _selectionManager = selectionManager;
        _imageOperationService = imageOperationService;
        _logger = logger;

        Commands.Add(CommandParameters.COMMAND_ADD, new DelegateCommand(Add, () => true));
        Commands.Add(CommandParameters.COMMAND_REMOVE, new DelegateCommand(Remove, () => true));
        Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand<object>(Closed, _ => true));

        InitAxis();
        InitSeriesPoints();
        _imageOperationService.TimeDensityInfoChangedNotify -= ImageOperationService_TimeDensityInfoChangedNotify;
        _imageOperationService.TimeDensityInfoChangedNotify += ImageOperationService_TimeDensityInfoChangedNotify;

        _selectionManager.SelectionReconChanged -= SelectionManager_SelectionReconChanged;
        _selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;

        _imageOperationService.TimeDensityRoiRemoved -= ImageOperationService_TimeDensityRoiRemoved;
        _imageOperationService.TimeDensityRoiRemoved += ImageOperationService_TimeDensityRoiRemoved;

        _selectionManager.SelectionScanChanged -= SelectionManager_SelectionScanChanged;
        _selectionManager.SelectionScanChanged += SelectionManager_SelectionScanChanged;

        _imageOperationService.TimeDensityDeleteAllRoi -= ImageOperationService_TimeDensityDeleteAllRoi;
        _imageOperationService.TimeDensityDeleteAllRoi += ImageOperationService_TimeDensityDeleteAllRoi;
    }

    [UIRoute]
    private void ImageOperationService_TimeDensityDeleteAllRoi(object? sender, EventArgs e)
    {
        Series = new ObservableCollection<ISeries>();
        SerieDict.Clear();
        InitSeriesPoints();
    }

    [UIRoute]
    private void SelectionManager_SelectionScanChanged(object? sender, EventArgs<ScanModel> e)
    {
        if (e is null || e.Data is null)
        {
            return;
        }
        CurrentScanModel = e.Data;
    }

    [UIRoute]
    private void ImageOperationService_TimeDensityRoiRemoved(object? sender, EventArgs<string> e)
    {
        if (RoiDict.ContainsKey(e.Data))
        {
            RoiDict.Remove(e.Data);
        }
        int index = -1;
        if (int.TryParse(e.Data, out index) && SerieDict.ContainsKey(index))
        {
            SerieDict.Remove(index);
        }
        AddRoiToView();
        AddRoiToRecon();
    }

    [UIRoute]
    private void SelectionManager_SelectionReconChanged(object? sender, EventArgs<ReconModel> e)
    {
        if (e is null || e.Data is null)
        {
            return;
        }
        CurrentReconModel = e.Data;
        InitSeriesPoints();
        //清掉
        RoiDict = new Dictionary<string, CycleROIModel>();
        if (e.Data is ReconModel reconModel
            && (reconModel.Parent.ScanOption == ScanOption.NVTestBolus || reconModel.Parent.ScanOption == ScanOption.TestBolus))
        {
            DicomImageExtension.SetTestBolusCycleROIsByBase(_protocolHostService, reconModel);
        }
    }

    private void InitSeriesPoints()
    {
        mHu1 = new List<ObservablePoint>() { };

        mHu2 = new List<ObservablePoint>() { };

        mHu3 = new List<ObservablePoint>() { };
    }

    [UIRoute]
    private void ImageOperationService_TimeDensityInfoChangedNotify(object? sender, EventArgs<string> e)
    {
        if (e.Data is null || string.IsNullOrEmpty(e.Data))
        {
            return;
        }
        _logger.LogInformation($"ImageOperationService_TimeDensityInfoChangedNotify:{e.Data}");
        try
        {
            if (JsonConvert.DeserializeObject<TimeDensityInfo>(e.Data) is TimeDensityInfo info
                && CurrentReconModel is ReconModel reconModel)
            {
                if (reconModel.Status == PerformStatus.Performed)
                {
                    SetPerformedRTDReconRoiParams(info);
                }
                if (reconModel.Status == PerformStatus.Performing)
                {
                    SetPerformingRTDReconRoiParams(info);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ImageOperationService_TimeDensityInfoChangedNotify error:{ex.Message}");
        }
    }

    public object Sync { get; } = new object();

    [UIRoute]
    private void SetPerformingRTDReconRoiParams(TimeDensityInfo info)
    {
        lock (Sync)
        {
            List<ObservablePoint> mHu = new List<ObservablePoint>();
            foreach (AreaPointROIInfo item in info.AreaPointROIInfoList)
            {
                ObservablePoint timePoint = new ObservablePoint();
                timePoint.Y = item.MaxHu;
                timePoint.X = GetDataTime(item.ImageAcqTime).TotalSeconds;
                mHu.Add(timePoint);
            }
            switch (info.RoiParam.Id)
            {
                case "1":
                    mHu1.AddRange(mHu);
                    mHu1.OrderBy(t => t.X);
                    ISeries series1 = GetFirstLineSeries(mHu1);
                    if (SerieDict.ContainsKey(FirstID))
                    {
                        SerieDict[FirstID] = series1;
                    }
                    else
                    {
                        SerieDict.Add(FirstID, series1);
                    }
                    break;
                case "2":
                    mHu2.AddRange(mHu);
                    mHu2.OrderBy(t => t.X);
                    ISeries series2 = GetSecondLineSeries(mHu2);
                    if (SerieDict.ContainsKey(SecondID))
                    {
                        SerieDict[SecondID] = series2;
                    }
                    else
                    {
                        SerieDict.Add(SecondID, series2);
                    }
                    break;
                case "3":
                    mHu3.AddRange(mHu);
                    mHu3.OrderBy(t => t.X);
                    ISeries series3 = GetThirdLineSeries(mHu3);
                    if (SerieDict.ContainsKey(ThirdID))
                    {
                        SerieDict[ThirdID] = series3;
                    }
                    else
                    {
                        SerieDict.Add(ThirdID, series3);
                    }
                    break;
            }
            AddRoiToView();
        }
    }

    double minY = 0;

    [UIRoute]
    private void SetPerformedRTDReconRoiParams(TimeDensityInfo info)
    {
        List<ObservablePoint> mHu = new List<ObservablePoint>() { };
        foreach (AreaPointROIInfo item in info.AreaPointROIInfoList)
        {
            ObservablePoint timePoint = new ObservablePoint();
            timePoint.Y = item.MaxHu;
            timePoint.X = GetDataTime(item.ImageAcqTime).TotalSeconds;
            mHu.Add(timePoint);
        }

        switch (info.RoiParam.Id)
        {
            case "1":
                if (SerieDict.ContainsKey(FirstID))
                {
                    SerieDict[FirstID] = GetFirstLineSeries(mHu);
                }
                else
                {
                    SerieDict.Add(FirstID, GetFirstLineSeries(mHu));
                }
                break;
            case "2":
                if (SerieDict.ContainsKey(SecondID))
                {
                    SerieDict[SecondID] = GetSecondLineSeries(mHu);
                }
                else
                {
                    SerieDict.Add(SecondID, GetSecondLineSeries(mHu));
                }
                break;
            case "3":
                if (SerieDict.ContainsKey(ThirdID))
                {
                    SerieDict[ThirdID] = GetThirdLineSeries(mHu);
                }
                else
                {
                    SerieDict.Add(ThirdID, GetThirdLineSeries(mHu));
                }
                break;
        }

        AddRoiToView();

        SetRTDReconRoiParams(info);

        if (YAxis.Count() > 0)
        {
            double min = (double)mHu.Min(t => t.Y);
            if (min < minY)
            {
                minY = min - 5;
            }
        }
        SetYAxisMinLimit();
    }

    private void SetYAxisMinLimit()
    {
        if (YAxis.Count() > 0)
        {
            YAxis[0].MinLimit = minY;
        }
    }

    private void SetRTDReconRoiParams(TimeDensityInfo info)
    {
        if (info.RoiParam.Points.Count > 0)
        {
            CycleROIModel cycleROIModel = new CycleROIModel();
            cycleROIModel.CenterX = UnitConvert.Millimeter2Micron(info.RoiParam.Points[0].x);
            cycleROIModel.CenterY = UnitConvert.Millimeter2Micron(info.RoiParam.Points[0].y);
            cycleROIModel.CenterZ = UnitConvert.Millimeter2Micron(info.RoiParam.Points[0].z);
            cycleROIModel.Radius = UnitConvert.Millimeter2Micron(ProtocolParameterNames.RECON_DEFAULT_ROI_RADIUS);
            if (RoiDict.Keys.Contains(info.RoiParam.Id))
            {
                RoiDict.Remove(info.RoiParam.Id);
            }
            RoiDict.Add(info.RoiParam.Id, cycleROIModel);

            AddRoiToRecon();
        }
    }

    [UIRoute]
    private void AddRoiToView()
    {
        Series = new ObservableCollection<ISeries>();
        foreach (var item in SerieDict.Values)
        {
            Series.Add(item);
        }
    }

    private void AddRoiToRecon()
    {
        if (CurrentReconModel is ReconModel reconBase && reconBase.Parent.ScanOption == ScanOption.NVTestBolusBase)
        {
            AddCycleROIModelToRecon(reconBase);
            var m = _protocolHostService.Models.FirstOrDefault(t => t.Scan.Descriptor.Id.Equals(reconBase.Parent.Descriptor.Id));
            int index = _protocolHostService.Models.IndexOf(m);
            if (index >= 0 && _protocolHostService.Models.Count >= index + 1)
            {
                var pairs = _protocolHostService.Models[index + 1];
                if (pairs.Scan is ScanModel scan
                    && scan.Status == PerformStatus.Unperform
                    && (scan.ScanOption == ScanOption.NVTestBolus || scan.ScanOption == ScanOption.TestBolus)
                    && scan.Children.FirstOrDefault(t => t.IsRTD && t.Status == PerformStatus.Unperform) is ReconModel reconTestBolus)
                {
                    AddCycleROIModelToRecon(reconTestBolus);
                }
            }
        }
    }

    private void AddCycleROIModelToRecon(ReconModel reconModel)
    {
        reconModel.CycleROIs = new List<CycleROIModel>();
        foreach (var item in RoiDict.Values)
        {
            reconModel.CycleROIs.Add(item);
        }
    }

    private TimeSpan GetDataTime(string timestr)
    {
        TimeSpan timeSpan = new TimeSpan();
        var str = timestr.Split('.');
        if (!timestr.Contains(".") && str[0].Length != 6)
        {
            return new TimeSpan();
        }
        int hh = 0;
        int mm = 0;
        int ss = 0;
        int tz = 0;
        int.TryParse(timestr.Substring(0, 2), out hh);
        int.TryParse(timestr.Substring(2, 2), out mm);
        int.TryParse(timestr.Substring(4, 2), out ss);
        if (str.Length > 1)
            int.TryParse(str[1], out tz);

        TimeSpan span = new TimeSpan(0, hh, mm, ss, 0, tz);
        if (CurrentReconModel is ReconModel reconModel && reconModel.Parent is ScanModel scan)
        {
            DateTime dateTime = scan.ExposureStartTime;
            timeSpan = span - dateTime.TimeOfDay;
        }
        return timeSpan;
    }

    private void Add()
    {
        if (CurrentScanModel is ScanModel scanModel
            && scanModel.ScanOption is ScanOption.NVTestBolus or ScanOption.TestBolus
            && scanModel.Status != PerformStatus.Unperform)
        {
            return;
        }
        if (Series.Count == 3)
        {
            return;
        }
        if (!SerieDict.ContainsKey(FirstID))
        {
            _imageOperationService.SetCommondToTimeDensity(CommandParameters.COMMAND_ADD, FirstID + "," + Color1String);
            return;
        }
        if (!SerieDict.ContainsKey(SecondID))
        {
            _imageOperationService.SetCommondToTimeDensity(CommandParameters.COMMAND_ADD, SecondID + "," + Color2String);
            return;
        }
        if (!SerieDict.ContainsKey(ThirdID))
        {
            _imageOperationService.SetCommondToTimeDensity(CommandParameters.COMMAND_ADD, ThirdID + "," + Color3String);
            return;
        }
    }

    private void Remove()
    {
        if (CurrentScanModel is ScanModel scanModel
            && scanModel.ScanOption is ScanOption.NVTestBolus or ScanOption.TestBolus
            && scanModel.Status != PerformStatus.Unperform)
        {
            return;
        }
        int index = SerieDict.Keys.Max();
        if (index > 0)
        {
            _imageOperationService.SetCommondToTimeDensity(CommandParameters.COMMAND_REMOVE, (index).ToString());
        }
    }

    private void InitAxis()
    {
        XAxis = new Axis[] { GetXAxis() };
        YAxis = new Axis[] { GetYAxis() };
    }

    private Axis GetXAxis()
    {
        Axis axis = new Axis();
        axis.ShowSeparatorLines = false;
        axis.TicksPaint = new SolidColorPaint { Color = new(25, 118, 210), StrokeThickness = 1.5f };
        axis.SubticksPaint = new SolidColorPaint { Color = new(25, 118, 210), StrokeThickness = 1f };
        axis.LabelsPaint = _LabelsPaint;
        axis.CrosshairLabelsPaint = new SolidColorPaint(SKColors.DarkRed, 1);
        axis.CrosshairPaint = new SolidColorPaint(SKColors.DarkOrange, 1);
        axis.Labeler = value => value.ToString("N2");
        axis.MinLimit = 0;
        axis.ZeroPaint = new SolidColorPaint { Color = new(25, 118, 210), StrokeThickness = 1.5f };
        return axis;
    }

    private Axis GetYAxis()
    {
        Axis axis = new Axis();
        axis.ShowSeparatorLines = false;
        axis.CrosshairLabelsPaint = new SolidColorPaint(SKColors.DarkRed, 1);
        axis.CrosshairPaint = new SolidColorPaint(SKColors.DarkOrange, 1);
        axis.Labeler = value => string.Empty;

        return axis;
    }

    private ISeries GetFirstLineSeries(List<ObservablePoint> ts)
    {
        return GetLineSeries(NameFirst, ts, new SolidColorPaint(SKColor.Parse(Color1String), 2), new SolidColorPaint(SKColor.Parse(Color1String)));
    }

    private ISeries GetSecondLineSeries(List<ObservablePoint> ts)
    {
        return GetLineSeries(NameSecond, ts, new SolidColorPaint(SKColor.Parse(Color2String), 2), new SolidColorPaint(SKColor.Parse(Color2String)));
    }

    private ISeries GetThirdLineSeries(List<ObservablePoint> ts)
    {
        return GetLineSeries(NameThird, ts, new SolidColorPaint(SKColor.Parse(Color3String), 2), new SolidColorPaint(SKColor.Parse(Color3String)));
    }

    private LineSeries<ObservablePoint> GetLineSeries(string name, List<ObservablePoint> ts, SolidColorPaint colorWidth, SolidColorPaint color)
    {
        LineSeries<ObservablePoint> lineSeries = new LineSeries<ObservablePoint>();
        lineSeries.Values = ts;
        lineSeries.Fill = null;
        lineSeries.Name = name;
        lineSeries.GeometryStroke = colorWidth;
        lineSeries.Stroke = colorWidth;
        lineSeries.DataLabelsPaint = color;
        lineSeries.GeometrySize = 5;
        lineSeries.DataLabelsFormatter = (point) => string.Empty;

        return lineSeries;
    }

    public void Closed(object parameter)
    {
        if (parameter is Window window)
        {
            window.Hide();
        }
    }
}