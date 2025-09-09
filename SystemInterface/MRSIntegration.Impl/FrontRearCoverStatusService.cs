using Microsoft.Extensions.Logging;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class FrontRearCoverStatusService : IFrontRearCoverStatusService
{
    private readonly ILogger<FrontRearCoverStatusService> _logger;
    private readonly IRealtimeStatusProxyService _realtimeStatusService;

    public FrontRearCoverStatusService(ILogger<FrontRearCoverStatusService> logger, IRealtimeStatusProxyService realtimeStatusService)
    {
        _logger = logger;
        _realtimeStatusService = realtimeStatusService;
        _logger.LogDebug($"FrontRearCover initialized: {(_realtimeStatusService.IsFrontRearCoverClosed ? "Close" : "Open")}");
        IsClosed = _realtimeStatusService.IsFrontRearCoverClosed;
        _realtimeStatusService.CycleStatusChanged += RealtimeStatusService_CycleStatusChanged;
    }

    private void RealtimeStatusService_CycleStatusChanged(object? sender, CTS.EventArgs<Contract.Models.DeviceSystem> e)
    {
        if (IsClosed != e.Data.Gantry.FrontRearCoverClosed)
        {
            _logger.LogDebug($"FrontRearCover changed: {(IsClosed ? "Close" : "Open")} => {(e.Data.Gantry.FrontRearCoverClosed ? "Close" : "Open")}");
            IsClosed = e.Data.Gantry.FrontRearCoverClosed;
            StatusChanged?.Invoke(this, IsClosed);
        }
    }

    public bool IsClosed { get; private set; }

    public event EventHandler<bool> StatusChanged;
}
