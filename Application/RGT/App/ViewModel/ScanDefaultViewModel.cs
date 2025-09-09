using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.RGT.ViewModel;

public class ScanDefaultViewModel : BaseViewModel
{
    private const int TIME_DIVIDER = 1000000;
    private readonly IDataSync _dataSync;
    public ScanDefaultViewModel(IDataSync dataSync)
    {
        _dataSync = dataSync;
        Commands.Add("Work", new DelegateCommand<object>(Work, _ => true));

        _dataSync.SelectionScanChanged += DataSync_SelectionScanChanged;
        _dataSync.RealtimeStatusChanged += DataSync_RealtimeStatusChanged;

        SceneDefault();
    }

    private void DataSync_RealtimeStatusChanged(object? sender, EventArgs<CTS.Models.RealtimeInfo> e)
    {
        switch (e.Data.Status)
        {
            case RealtimeStatus.Init:
            case RealtimeStatus.Standby:
            //SceneDefault();
            //break;
            case RealtimeStatus.Validated:
            case RealtimeStatus.ParamConfig:
            case RealtimeStatus.MovingPartEnable:
            case RealtimeStatus.MovingPartEnabling:
            case RealtimeStatus.MovingPartEnabled:
            case RealtimeStatus.ExposureEnable:
                break;
            case RealtimeStatus.ExposureStarted:
                SceneDelay();
                break;
            case RealtimeStatus.ExposureSpoting:
                SceneScanning();
                break;
            case RealtimeStatus.ExposureFinished:
            case RealtimeStatus.ScanStopping:
            case RealtimeStatus.NormalScanStopped:
            case RealtimeStatus.EmergencyScanStopped:
            case RealtimeStatus.Error:
                SceneNotScan();
                break;
            default:
                SceneNotScan();
                break;
        }
    }

    private void DataSync_SelectionScanChanged(object? sender, EventArgs<RgtScanModel> e)
    {
        UpdateDisplayParameters(e.Data);
    }

    private void StudyHostService_ScanChanged(object? sender, EventArgs<RgtScanModel> e)
    {
        UpdateDisplayParameters(e.Data);
    }

    [UIRoute]
    private void UpdateDisplayParameters(RgtScanModel model)
    {
        Protocol = model.Protocol;
        ScanType = model.ScanType;
        Kv = model.Kv;
        Ma = model.Ma;
        PatientPosition = model.PatientPosition;
        ScanLength = model.ScanLength;
        CTDlvol = model.CTDlvol;
        DLP = model.DLP;
        ScanTime = model.ScanTime;
        DelayTime = model.DelayTime / TIME_DIVIDER;
        ExposureTime = model.ExposureTime;
        FrameTime = model.FrameTime;
    }

    private void Work(object obj)
    {

    }

    #region 属性

    private string _protocol = string.Empty;
    public string Protocol
    {
        get => _protocol;
        set => SetProperty(ref _protocol, value);
    }

    private string _scanType = string.Empty;
    public string ScanType
    {
        get => _scanType;
        set => SetProperty(ref _scanType, value);
    }

    private uint _kv;
    public uint Kv
    {
        get => _kv;
        set => SetProperty(ref _kv, value);
    }

    private decimal _ma;
    public decimal Ma
    {
        get => _ma;
        set => SetProperty(ref _ma, value);
    }

    private string _patientPosition = string.Empty;
    public string PatientPosition
    {
        get => _patientPosition;
        set => SetProperty(ref _patientPosition, value);
    }

    private uint _scanLength;
    public uint ScanLength
    {
        get => _scanLength;
        set => SetProperty(ref _scanLength, value);
    }


    private float _ctdlvol;
    public float CTDlvol
    {
        get => _ctdlvol;
        set => SetProperty(ref _ctdlvol, value);
    }

    private float _dlp;
    public float DLP
    {
        get => _dlp;
        set => SetProperty(ref _dlp, value);
    }

    private decimal _scanTime;
    public decimal ScanTime
    {
        get => _scanTime;
        set => SetProperty(ref _scanTime, value);
    }

    private decimal _delayTime;
    public decimal DelayTime
    {
        get => _delayTime;
        set => SetProperty(ref _delayTime, value);
    }

    private float _exposureTime;
    public float ExposureTime
    {
        get => _exposureTime;
        set => SetProperty(ref _exposureTime, value);
    }

    private float _frameTime;
    public float FrameTime
    {
        get => _frameTime;
        set => SetProperty(ref _frameTime, value);
    }

    private Visibility _delayVisibility;

    public Visibility DelayVisibility
    {
        get => _delayVisibility;
        set => SetProperty(ref _delayVisibility, value);
    }

    private Visibility _scanningVisibility;

    public Visibility ScanningVisibility
    {
        get => _scanningVisibility;
        set => SetProperty(ref _scanningVisibility, value);
    }
    private Visibility _notScanVisibility;

    public Visibility NotScanVisibility
    {
        get => _notScanVisibility;
        set => SetProperty(ref _notScanVisibility, value);
    }
    #endregion

    private void SceneNotScan()
    {
        NotScanVisibility = Visibility.Visible;
        ScanningVisibility = Visibility.Hidden;
        DelayVisibility = Visibility.Hidden;
    }

    private void SceneScanning()
    {
        ScanningVisibility = Visibility.Visible;
        NotScanVisibility = Visibility.Hidden;
        DelayVisibility = Visibility.Hidden;
    }

    private void SceneDelay()
    {
        DelayVisibility = Visibility.Visible;
        ScanningVisibility = Visibility.Hidden;
        NotScanVisibility = Visibility.Hidden;
    }

    private void SceneDefault()
    {
        DelayVisibility = Visibility.Hidden;
        ScanningVisibility = Visibility.Hidden;
        NotScanVisibility = Visibility.Hidden;
    }
}