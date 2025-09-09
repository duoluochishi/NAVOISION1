using NV.CT.CTS.Enums;
using NV.CT.Examination.ApplicationService.Contract.Interfaces;

namespace NV.CT.Examination.ApplicationService.Contract.ScanControl.Rule;

public class HasUnFinishedRTDReconRule : IUIControlEnableRule
{
    private readonly IProtocolHostService _protocolHostService;
    public HasUnFinishedRTDReconRule(IProtocolHostService protocolHostService)
    {
        _protocolHostService = protocolHostService;

        _protocolHostService.PerformStatusChanged += _protocolHostService_PerformStatusChanged;
        _protocolHostService.StructureChanged += _protocolHostService_StructureChanged;
    }

    private void _protocolHostService_StructureChanged(object? sender, CTS.EventArgs<(Protocol.Models.BaseModel Parent, Protocol.Models.BaseModel Current, Protocol.Models.StructureChangeType ChangeType)> e)
    {
        UIStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    private void _protocolHostService_PerformStatusChanged(object? sender, CTS.EventArgs<(Protocol.Models.BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
    {
        UIStatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsEnabled()
    {
        //ReconAll可用条件：是ProtocolHost里面所有的Scan对应的 RTD已经完成，还有其他非RTD没完成的
        return _protocolHostService.Models.Select(n => n.Scan).Any(n => n.Status == PerformStatus.Performed && n.Children.Any(recon => !recon.IsRTD && recon.Status == PerformStatus.Unperform));
    }

    public string GetFailReason()
    {
        return "All scan and recon has finished.";
    }

    public event EventHandler? UIStatusChanged;
}