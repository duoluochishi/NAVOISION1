using NV.CT.Examination.ApplicationService.Contract.Interfaces;

namespace NV.CT.Examination.ApplicationService.Contract.ScanControl.Rule;

public class SystemReadyRule : IUIControlEnableRule
{
    private readonly ISystemReadyService _systemReadyService;
    public SystemReadyRule(ISystemReadyService systemReadyService)
    {
        _systemReadyService = systemReadyService;
        _systemReadyService.StatusChanged += SystemReadyService_StatusChanged;
    }

    private void SystemReadyService_StatusChanged(object? sender, CTS.EventArgs<(bool status, bool isSyatemStatus)> e)
    {
        UIStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsEnabled()
    {
        return _systemReadyService.Status;
    }

    public string GetFailReason()
    {
        return "SystemReadyService is not ok.";
    }

    public event EventHandler? UIStatusChanged;
}