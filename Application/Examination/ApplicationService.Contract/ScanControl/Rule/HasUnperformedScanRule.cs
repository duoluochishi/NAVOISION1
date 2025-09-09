using NV.CT.CTS.Enums;
using NV.CT.Examination.ApplicationService.Contract.Interfaces;
using NV.CT.Protocol;

namespace NV.CT.Examination.ApplicationService.Contract.ScanControl.Rule;

public class HasUnperformedScanRule : IUIControlEnableRule
{
    private readonly IProtocolHostService _protocolHostService;
    public HasUnperformedScanRule(IProtocolHostService protocolHostService)
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
        return ProtocolHelper.Expand(_protocolHostService.Instance)
            .Any(n => n.Scan.Status == PerformStatus.Unperform);
    }

    public string GetFailReason()
    {
        return "Protocol did not have any unperformed scan.";
    }

    public event EventHandler? UIStatusChanged;
}
