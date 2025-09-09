//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.UI.Exam.ViewModel;

using Microsoft.Extensions.Logging;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.UI.Exam.Extensions;

public class AutoPositioningViewModel : BaseViewModel
{
    private readonly ISmartPositioningService _smartPositioningService;
    private readonly ISelectionManager _selectionManager;
    private readonly IProtocolHostService _protocolHostService;
    private readonly ITablePositionService _tablePositionService;
    private readonly ISmartPositionContract _smartPositionContract;
    private const double DoubleDivider = 1000.0;
    AutoPositioningWindow? _autoPositioningWindow;
    private ILogger<AutoPositioningViewModel> _logger;
    private ScanModel currentScanModel = new();

    private string _message = string.Empty;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private bool _isMaxEnable;
    public bool IsMaxEnable
    {
        get => _isMaxEnable;
        set => SetProperty(ref _isMaxEnable, value);
    }

    private bool _isMinEnable = true;
    public bool IsMinEnable
    {
        get => _isMinEnable;
        set => SetProperty(ref _isMinEnable, value);
    }

    private bool IsDetecting = false;

    private bool _rangeMessageShow;
    public bool RangeMessageShow
    {
        get => _rangeMessageShow;
        set => SetProperty(ref _rangeMessageShow, value);
    }

    private double _rangeStart;
    public double RangeStart
    {
        get => _rangeStart;
        set => SetProperty(ref _rangeStart, value);
    }

    public double LeftLineChange = 0;

    private double _rangeEnd = 5.0;
    public double RangeEnd
    {
        get => _rangeEnd;
        set => SetProperty(ref _rangeEnd, value);
    }

    public double RightLineChange = 0;

    private double _bedHeight = 140.0;
    public double BedHeight
    {
        get => _bedHeight;
        set => SetProperty(ref _bedHeight, value);
    }

    private double _rangeLength;
    public double RangeLength
    {
        get => _rangeLength;
        set => SetProperty(ref _rangeLength, value);
    }

    private bool _rangeLineShow;
    public bool RangeLineShow
    {
        get => _rangeLineShow;
        set => SetProperty(ref _rangeLineShow, value);
    }

    /// <summary>
    /// 实际床码值
    /// </summary>
    private double _beginRange;
    public double BeginRange
    {
        get => _beginRange;
        set => SetProperty(ref _beginRange, value);
    }
    public double LeftRangeLineChange = 0;

    /// <summary>  
    /// 实际床码值  
    /// </summary>
    private double _endRange = 5.0;
    public double EndRange
    {
        get => _endRange;
        set => SetProperty(ref _endRange, value);
    }
    public double RightRangeLineChange = 0;

    private bool _isButtonShow = true;
    public bool IsButtonShow
    {
        get => _isButtonShow;
        set => SetProperty(ref _isButtonShow, value);
    }

    private double _ppd = 1.85;             //   PPD
    public double PixelBedCode
    {
        get => _ppd;
        set => SetProperty(ref _ppd, value);
    }

    private double _t0 = 396;               //  To 396
    public double PLENGTH
    {
        get => _t0;
        set => SetProperty(ref _t0, value);
    }
    private double _tc = 600;               //  Tc
    public double Tc
    {
        get => _tc;
        set => SetProperty(ref _tc, value);
    }

    public double LastHorizontalPosition = 0;
    public double LastPosition = 0;

    public double CurrentHPosition = 0;
    public double IsDetectingHPosition = 0;
    public double IsDetectingRangeStart = 0;
    public double IsDetectingRangeEnd = 0;

    public double LastPPosition = 0;
    public double CurrentPPosition = 0;
    public double LastHorizontalZ = double.MinValue;

    private string _tableMoveH = string.Empty;
    public string TableMoveH
    {
        get => _tableMoveH;
        set => SetProperty(ref _tableMoveH, value);
    }

    private string _headMoveH = string.Empty;
    public string HeadMoveH
    {
        get => _headMoveH;
        set => SetProperty(ref _headMoveH, value);
    }

    private bool _isEnableIntelligentIdentify;
    public bool IsEnableIntelligentIdentify
    {
        get => _isEnableIntelligentIdentify;
        set => SetProperty(ref _isEnableIntelligentIdentify, value);
    }

    private WriteableBitmap _positionImage = new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Images/SmartPositionTest.png", UriKind.RelativeOrAbsolute)));
    public WriteableBitmap PositionImage
    {
        get => _positionImage;
        set => SetProperty(ref _positionImage, value);
    }

    public AutoPositioningViewModel(ISmartPositioningService smartPositioningService,
           ISelectionManager selectionManager,
           IProtocolHostService protocolHostService,
           ISmartPositionContract smartPositionContract,
           ITablePositionService tablePositionService,
           ILogger<AutoPositioningViewModel> logger)
    {
        _selectionManager = selectionManager;
        _smartPositioningService = smartPositioningService;
        _protocolHostService = protocolHostService;
        _tablePositionService = tablePositionService;
        _logger = logger;

        _smartPositionContract = smartPositionContract;
        _smartPositionContract.StartSmartPositionService();

        Commands.Add(CommandParameters.COMMAND_MAX, new DelegateCommand<object>(Max, _ => true));
        Commands.Add(CommandParameters.COMMAND_MIN, new DelegateCommand<object>(Min, _ => true));
        Commands.Add(CommandParameters.COMMAND_CLOSE, new DelegateCommand<object>(Close, _ => true));
        Commands.Add(CommandParameters.COMMAND_IS_SMART_POSITION, new DelegateCommand<object>(IsSmartPositioning, _ => true));

        Commands.Add("EnableIntelligentIdentify", new DelegateCommand(EnableIntelligentIdentify, () => true));

        Commands.Add("Detect", new DelegateCommand<object>(Detect, _ => true));
        Commands.Add("Recording", new DelegateCommand(Recording, () => true));
        Commands.Add("Calculation", new DelegateCommand(Calculation, () => true));

        _smartPositioningService.RangePotitionChanged -= SmartPositioningService_RangePotitionChanged;
        _smartPositioningService.RangePotitionChanged += SmartPositioningService_RangePotitionChanged;

        _smartPositioningService.RangePotitionDistanceChanged -= SmartPositioningService_RangePotitionDistanceChanged;
        _smartPositioningService.RangePotitionDistanceChanged += SmartPositioningService_RangePotitionDistanceChanged;

        _smartPositionContract.UploadPositioningChanged -= SmartPositionContract_UploadPositioningChangedImage;
        _smartPositionContract.UploadPositioningChanged += SmartPositionContract_UploadPositioningChangedImage;

        _tablePositionService.TablePositionChanged -= TablePositionService_TablePositionChanged;
        _tablePositionService.TablePositionChanged += TablePositionService_TablePositionChanged;

        _protocolHostService.ParameterChanged -= ProtocolModificationService_ParameterChanged;
        _protocolHostService.ParameterChanged += ProtocolModificationService_ParameterChanged;

        _smartPositionContract.UploadPositioningChanged -= SmartPositionContract_UploadPositioningChangedData;
        _smartPositionContract.UploadPositioningChanged += SmartPositionContract_UploadPositioningChangedData;
    }

    /// <summary>
    /// 参数变更页面上两条线需要跟着变化(暂不响应变化)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    [UIRoute]
    private void ProtocolModificationService_ParameterChanged(object? sender, EventArgs<(BaseModel baseModel, List<string> strings)> e)
    {
        if (e is null|| e.Data.baseModel is null || e.Data.strings is null)
        {
            return;
        }
        //if (_autoPositioningWindow is not null && e.Data.baseModel is ScanModel scanModel && currentScanModel is not null && scanModel.Descriptor.Id.Equals(currentScanModel.Descriptor.Id))
        //{
        //    if (e.Data.strings.FirstOrDefault(t => t.Equals(ParameterNames.SCAN_START_POSITION)) is not null)
        //    {
        //        RangeStart = Math.Abs(GetPa(GetBedRange(scanModel.StartPosition / DoubleDivider)));
        //    }
        //    if (e.Data.strings.FirstOrDefault(t => t.Equals(ParameterNames.SCAN_END_POSITION)) is not null)
        //    {
        //        RangeEnd = Math.Abs(GetPa(GetBedRange(scanModel.EndPosition / DoubleDivider)));
        //    }                     
        //    Canvas.SetLeft(_autoPositioningWindow.RangeStartControl, RangeStart);
        //    Canvas.SetLeft(_autoPositioningWindow.RangeEndControl, RangeEnd);             
        //}
    }

    private void SelectionManager_SelectionTopoScanChanged(object? sender, EventArgs<ScanModel> e)
    {
        if (e.Data is null || !(e.Data.ScanOption == ScanOption.Surview || e.Data.ScanOption == ScanOption.DualScout))
        {
            return;
        }
        currentScanModel = e.Data;
        LeftLineChange = 0;
        RightLineChange = 0;
        _smartPositioningService.IsManual = false;
    }

    [UIRoute]
    private void TablePositionService_TablePositionChanged(object? sender, EventArgs<TablePositionInfo> e)
    {
        if (e is null || e.Data is null)
        {
            return;
        }
        BedHeight = Math.Round(e.Data.VerticalPosition / DoubleDivider, 1);
        CurrentHPosition = Math.Round(e.Data.HorizontalPosition / DoubleDivider, 1);
        if (_autoPositioningWindow is not null)
        {
            RangeStart = GetPa(IsDetectingRangeStart, CurrentHPosition);
            RangeEnd = GetPa(IsDetectingRangeEnd, CurrentHPosition);

            Canvas.SetLeft(_autoPositioningWindow.RangeStartControl, RangeStart);
            Canvas.SetLeft(_autoPositioningWindow.RangeEndControl, RangeEnd);
        }
    }

    [UIRoute]
    private void SmartPositioningService_RangePotitionDistanceChanged(object? sender, EventArgs<(PotisionStartEndLine potisionStartEndLine, double position)> e)
    {
        if (e is null)
        {
            return;
        }
        if (e.Data.potisionStartEndLine == PotisionStartEndLine.StartLine)
        {
            LeftLineChange = e.Data.position;
        }
        if (e.Data.potisionStartEndLine == PotisionStartEndLine.EndLine)
        {
            RightLineChange = e.Data.position;
        }
    }

    [UIRoute]
    private void SmartPositionContract_UploadPositioningChangedImage(object? sender, SmartPositioningResponse e)
    {
        if (e is not null && !string.IsNullOrEmpty(e.Image))
        {
            string base64 = e.Image.Substring(e.Image.IndexOf(",") + 1);
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(base64)))
            {
                Bitmap bitmap = new Bitmap(ms);
                PositionImage = DicomImageHelper.Instance.BitmapToWriteableBitmap(bitmap);
            }
        }
    }

    [UIRoute]
    private void SmartPositionContract_UploadPositioningChangedData(object? sender, SmartPositioningResponse e)
    {
        if (_smartPositioningService.IsManual || e is null || _autoPositioningWindow is null || !IsDetecting)
        {
            return;
        }
        if (!(e.IsValidated && e.Result))
        {
            Message = "Not recognized !";
            RangeLineShow = false;
            return;
        }
        //PPD:计算PPD跟PLength的值
        //double pLength = Math.Abs(e.PLength);
        //if (pLength != 0)
        //{
        //    PixelBedCode = pLength / e.PPixel;
        //    PLENGTH = pLength;
        //}
        var scan = this._selectionManager.LastSelectionTopoScan;
        if (scan is null)
        {
            return;
        }
        var patientPosition = scan.Parent.Parent.PatientPosition;
        if (!e.PatientPosition.ToString().ToUpper().Equals(patientPosition.ToString().ToUpper()))
        {
            Message = "The patient is not in the right position !";
            return;
        }
        var scanBodyPart = e.BodyPositions.FirstOrDefault(t => t.BodyPart.ToString().Equals(scan.BodyPart.ToString()));
        if (scanBodyPart is null)
        {
            //RangeLineShow = false;
            return;
        }
        //if (LastHorizontalZ == double.MinValue)
        //{
        //    LastHorizontalZ = scanBodyPart.LeftTop.Z;
        //}
        //_logger.LogInformation($"[autoposition] --------------------------");
        //_logger.LogInformation($"[autoposition] HorizontalPosition = {LastHorizontalPosition}");
        //_logger.LogInformation($"[autoposition] LastHorizontalPosition = {(LastHorizontalZ - scanBodyPart.LeftTop.Z) * PixelBedCode}");
        ////模拟床位变化
        //LastHorizontalPosition = (LastHorizontalZ - scanBodyPart.LeftTop.Z) * PixelBedCode;
        //LastHorizontalPosition = 0;

        Message = string.Empty;
        IsDetectingHPosition = CurrentHPosition;
        RangeLineShow = true;
        if (scan.TableDirection == TableDirection.In)
        {
            BeginRange = GetBedRange(scanBodyPart.LeftTop.Z + LeftLineChange, CurrentHPosition);
            EndRange = GetBedRange(scanBodyPart.RightBottom.Z + RightLineChange, CurrentHPosition);

            RangeStart = GetPa(BeginRange, CurrentHPosition);
            RangeEnd = GetPa(EndRange, CurrentHPosition);
        }
        if (scan.TableDirection == TableDirection.Out)
        {
            BeginRange = GetBedRange(scanBodyPart.RightBottom.Z + LeftLineChange, CurrentHPosition);
            EndRange = GetBedRange(scanBodyPart.LeftTop.Z + RightLineChange, CurrentHPosition);

            RangeStart = GetPa(BeginRange, CurrentHPosition);
            RangeEnd = GetPa(EndRange, CurrentHPosition);
        }

        if (_autoPositioningWindow is not null)
        {
            Canvas.SetLeft(_autoPositioningWindow.RangeStartControl, RangeStart);
            Canvas.SetLeft(_autoPositioningWindow.RangeEndControl, RangeEnd);
        }
        IsDetectingRangeStart = BeginRange;
        IsDetectingRangeEnd = EndRange;

        BeginRange = -Math.Abs(double.Parse(BeginRange.ToString("0")));
        EndRange = -Math.Abs(double.Parse(EndRange.ToString("0")));
        RangeLength = Math.Abs(double.Parse((BeginRange - EndRange).ToString("0")));

        IsDetecting = false;
        //_logger.LogInformation($"[autoposition] --------------------------");
        //_logger.LogInformation($"[autoposition] HorizontalPosition = {LastHorizontalPosition}");
        //_logger.LogInformation($"[autoposition] LastHorizontalPosition = {(LastHorizontalZ - scanBodyPart.LeftTop.Z) * PixelBedCode}");
        //_logger.LogInformation($"[autoposition] PA = {scanBodyPart.LeftTop.Z}");
        //_logger.LogInformation($"[autoposition] originalTopZ = {LastHorizontalZ}");
        //_logger.LogInformation($"[autoposition] TX = {(scanBodyPart.LeftTop.Z - LastHorizontalZ) * PixelBedCode}");
        //_logger.LogInformation($"[autoposition] Ta = {BeginRange}");
        //_logger.LogInformation($"[autoposition] T0 = {pLength}");
        //_logger.LogInformation($"[autoposition] TC = {Tc}");

        SeveDataToProtocol();
    }

    private double GetBedRange(double Pa, double Tx)
    {
        //return Pa * PixelBedCode + Tx - PLENGTH + Tc;
        return -Pa * PixelBedCode + Tx + PLENGTH - Tc;
    }

    private double GetPa(double Ta, double Tx)
    {
        //return (PLENGTH - Tx + Ta - Tc) / PixelBedCode;
        return (PLENGTH + Tx - Ta - Tc) / PixelBedCode;
    }

    [UIRoute]
    private void SmartPositioningService_RangePotitionChanged(object? sender, EventArgs<(PotisionStartEndLine potisionStartEndLine, double position)> e)
    {
        if (e is not null)
        {
            if (e.Data.potisionStartEndLine == PotisionStartEndLine.StartLine)
            {
                GetStartLine(e.Data.position);
            }
            if (e.Data.potisionStartEndLine == PotisionStartEndLine.EndLine)
            {
                GetEndLine(e.Data.position);
            }
        }
    }

    private void GetStartLine(double position)
    {
        RangeStart = position;
        if (RangeStart > RangeEnd && _autoPositioningWindow is not null)
        {
            RangeStart = RangeEnd;
            Canvas.SetLeft(_autoPositioningWindow.RangeStartControl, RangeStart);
        }
        BeginRange = GetBedRange(RangeStart, CurrentHPosition);
        BeginRange = -Math.Abs(double.Parse(BeginRange.ToString("0")));
        RangeLength = Math.Abs(double.Parse((BeginRange - EndRange).ToString("0")));
        IsDetectingRangeStart = BeginRange;
        SeveDataToProtocol();
    }

    private void GetEndLine(double position)
    {
        RangeEnd = position;
        if (RangeEnd < RangeStart && _autoPositioningWindow is not null)
        {
            RangeEnd = RangeStart;
            Canvas.SetLeft(_autoPositioningWindow.RangeEndControl, RangeEnd);
        }
        EndRange = GetBedRange(RangeEnd, CurrentHPosition);
        EndRange = -Math.Abs(double.Parse(EndRange.ToString("0")));
        RangeLength = Math.Abs(double.Parse((BeginRange - EndRange).ToString("0")));
        IsDetectingRangeEnd = EndRange;
        SeveDataToProtocol();
    }

    private void SeveDataToProtocol()
    {
        if (this._selectionManager is null
         || this._selectionManager.CurrentSelection.Scan is null
         || !(this._selectionManager.CurrentSelection.Scan.ScanOption == ScanOption.Surview || this._selectionManager.CurrentSelection.Scan.ScanOption == ScanOption.DualScout)
         || this._selectionManager.CurrentSelection.Scan.Status != PerformStatus.Unperform)
        {
            return;
        }

        List<ParameterModel> parameters = new List<ParameterModel>();

        ParameterModel parameterModel = new ParameterModel();
        parameterModel.Name = ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION;
        parameterModel.Value = ((int)(BeginRange * DoubleDivider)).ToString();
        parameterModel.Type = typeof(int).Name;
        parameters.Add(parameterModel);

        parameterModel = new ParameterModel();
        parameterModel.Name = ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION;
        parameterModel.Value = ((int)(EndRange * DoubleDivider)).ToString();
        parameterModel.Type = typeof(int).Name;
        parameters.Add(parameterModel);

        parameterModel = new ParameterModel();
        parameterModel.Name = ProtocolParameterNames.SCAN_LENGTH;
        parameterModel.Value = ScanLengthHelper.GetCorrectedScanLength(this._selectionManager.CurrentSelection.Scan, (int)RangeLength).ToString();
        parameterModel.Type = typeof(int).Name;
        parameters.Add(parameterModel);

        this._protocolHostService.SetParameters(this._selectionManager.CurrentSelection.Scan, parameters);
    }

    [UIRoute]
    public void Max(object parameter)
    {
        var window = parameter as AutoPositioningWindow;
        if (window is null)
        {
            return;
        }
        IsMaxEnable = false;
        IsMinEnable = true;
        IsButtonShow = true;
        this.RangeMessageShow = true;
        //窗体起始位
        window.Left = 350;
        window.Top = 40;

        double rangeStartLeft = Canvas.GetLeft(window.RangeStartControl);
        double rangeEndLeft = Canvas.GetLeft(window.RangeEndControl);

        //Range起始线跟结束线的长度坐标点值      
        window.RangeStartLine.Y2 = 678;
        window.RangeEndLine.Y2 = 678;

        window.Width = 1208;
        window.Height = 740;

        rangeStartLeft = rangeStartLeft * 1208 / 336;
        rangeEndLeft = rangeEndLeft * 1208 / 336;

        Canvas.SetLeft(window.RangeStartControl, rangeStartLeft);
        Canvas.SetLeft(window.RangeEndControl, rangeEndLeft);
    }

    [UIRoute]
    public void Min(object parameter)
    {
        this.RangeMessageShow = false;
        var window = parameter as AutoPositioningWindow;
        if (window is null)
        {
            return;
        }
        IsMaxEnable = true;
        IsMinEnable = false;
        IsButtonShow = false;
        Message = string.Empty;

        //窗体起始位
        window.Left = 2;
        window.Top = 272;

        double rangeStartLeft = Canvas.GetLeft(window.RangeStartControl);
        double rangeEndLeft = Canvas.GetLeft(window.RangeEndControl);

        //Range起始线跟结束线的长度坐标点值        
        window.RangeStartLine.Y2 = 189;
        window.RangeEndLine.Y2 = 189;

        window.Width = 336;
        window.Height = 250;

        rangeStartLeft = rangeStartLeft * 336 / 1208;
        rangeEndLeft = rangeEndLeft * 336 / 1208;

        Canvas.SetLeft(window.RangeStartControl, rangeStartLeft);
        Canvas.SetLeft(window.RangeEndControl, rangeEndLeft);
    }

    public void Close(object parameter)
    {
        if (parameter is Window window)
        {
            window.Hide();
        }
    }

    public void IsSmartPositioning(object parameter)
    {
        //if (parameter is AutoPositioningWindow window && window.IsSmartPositioning.IsChecked == true)
        //{
        //    this._smartPositionContract.UploadPositioningChanged -= SmartPositionContract_UploadPositioningChangedData;
        //    this._smartPositionContract.UploadPositioningChanged += SmartPositionContract_UploadPositioningChangedData;

        //    _autoPositioningWindow = window;
        //    RangeLineShow = true;
        //    if (!IsMaxEnable)
        //    {
        //        RangeMessageShow = true;
        //    }
        //    if (this._selectionManager is null
        //       || this._selectionManager.CurrentSelection.Scan is null
        //       || this._selectionManager.CurrentSelection.Scan.ScanOption != ScanOption.SURVIEW)
        //    {
        //        return;
        //    }
        //}
        //else
        //{
        //    RangeMessageShow = false;
        //    this._smartPositionContract.UploadPositioningChanged -= SmartPositionContract_UploadPositioningChangedData;
        //}
    }

    private void EnableIntelligentIdentify()
    {
        //if (IsEnableIntelligentIdentify) {
        //    LastHorizontalPosition=CurrentHorizontalPosition; 
        //}

        //LastHorizontalPosition
        //try
        //{
        //    //EnableIntelligentIdentifyModel enableIntelligentIdentifyModel = new EnableIntelligentIdentifyModel();
        //    //enableIntelligentIdentifyModel.EnableIntelligentIdentify = IsEnableIntelligentIdentify;
        //    //string par = JsonConvert.SerializeObject(enableIntelligentIdentifyModel);
        //    //_smartPositionContract.SendSmartPositioningCommand(new RequestCommand
        //    //{
        //    //    Command = Command.EnableIntelligentIdentify,
        //    //    RequestID = 1,
        //    //    Parameter = par,
        //    //});

        //    _is = IsEnableIntelligentIdentify;
        //    if (_is)
        //    {
        //        _logger.LogDebug($"UnitTest_TableHMoveInfo:{LastHorizontalPosition - CurrentHorizontalPosition}");
        //        _logger.LogDebug($"UnitTest_ssSZMoveInfo:{LastPPosition - CurrentPPosition}");
        //    }
        //}
        //catch (NanoException ex)
        //{
        //    _logger.LogDebug(ex.Message);
        //}
    }

    private void Detect(object parameter)
    {
        IsDetecting = true;
        if (parameter is AutoPositioningWindow window && _autoPositioningWindow is null)
        {
            _autoPositioningWindow = window;
        }
        LeftLineChange = 0;
        RightLineChange = 0;
        RangeLineShow = true;
        if (!IsMaxEnable)
        {
            RangeMessageShow = true;
        }
    }

    private void Recording()
    {
        LastPosition = CurrentHPosition;
        LastPPosition = CurrentPPosition;
    }

    private void Calculation()
    {
        TableMoveH = "Table:" + Math.Round((CurrentHPosition - LastPosition) / DoubleDivider, 2).ToString();
        HeadMoveH = "Head:" + Math.Round(CurrentPPosition - LastPPosition, 2).ToString();
    }
}