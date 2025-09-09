using NV.CT.Examination.ApplicationService.Contract.ScanControl;

namespace NV.CT.Examination.ApplicationService.Impl.ScanControl;

/// <summary>
/// 控制状态服务的实现
/// </summary>
public class UIControlStatusService : IUIControlStatusService
{
    public bool IsGoButtonEnable => _goButtonControlEnableService.IsEnabled();
    public bool IsCancelButtonEnable => _cancelButtonControlEnableService.IsEnabled();
    public bool IsReconAllButtonEnable => _reconAllButtonControlEnableService.IsEnabled();
    public bool IsConfirmButtonEnable => _confirmButtonControlEnableService.IsEnabled();
    public string GoFailReason => _goButtonControlEnableService.GetFirstFailReason();
    public string CancelFailReason => _cancelButtonControlEnableService.GetFirstFailReason();
    public string ReconAllFailReason => _reconAllButtonControlEnableService.GetFirstFailReason();
    public string ConfirmFailReason => _confirmButtonControlEnableService.GetFirstFailReason();

    private readonly GoButtonControlEnableService _goButtonControlEnableService;
    private readonly CancelButtonControlEnableService _cancelButtonControlEnableService;
    private readonly ReconAllButtonControlEnableService _reconAllButtonControlEnableService;
    private readonly ConfirmButtonControlEnableService _confirmButtonControlEnableService;
    public UIControlStatusService(GoButtonControlEnableService goButtonControlEnableService,
        CancelButtonControlEnableService cancelButtonControlEnableService,
        ReconAllButtonControlEnableService reconAllButtonControlEnableService,
        ConfirmButtonControlEnableService confirmButtonControlEnableService)
    {
        _goButtonControlEnableService = goButtonControlEnableService;
        _goButtonControlEnableService.UIStatusChanged += _goButtonControlEnableService_UIStatusChanged;

        _cancelButtonControlEnableService = cancelButtonControlEnableService;
        _cancelButtonControlEnableService.UIStatusChanged += _cancelButtonControlEnableService_UIStatusChanged;

        _reconAllButtonControlEnableService = reconAllButtonControlEnableService;
        _reconAllButtonControlEnableService.UIStatusChanged += _reconAllButtonControlEnableService_UIStatusChanged;

        _confirmButtonControlEnableService = confirmButtonControlEnableService;
        _confirmButtonControlEnableService.UIStatusChanged += _confirmButtonControlEnableService_UIStatusChanged;
    }

    private void _goButtonControlEnableService_UIStatusChanged(object? sender, (bool, string) e)
    {
        GoButtonStatusChanged?.Invoke(this, EventArgs.Empty);
    }
    private void _cancelButtonControlEnableService_UIStatusChanged(object? sender, (bool, string) e)
    {
        CancelButtonStatusChanged?.Invoke(this, EventArgs.Empty);
    }
    private void _reconAllButtonControlEnableService_UIStatusChanged(object? sender, (bool, string) e)
    {
        ReconAllButtonStatusChanged?.Invoke(this, EventArgs.Empty);
    }
    private void _confirmButtonControlEnableService_UIStatusChanged(object? sender, (bool, string) e)
    {
        ConfirmButtonStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? GoButtonStatusChanged;
    public event EventHandler? CancelButtonStatusChanged;
    public event EventHandler? ReconAllButtonStatusChanged;
    public event EventHandler? ConfirmButtonStatusChanged;
}