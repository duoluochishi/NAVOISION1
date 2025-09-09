//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/10 15:27:28       V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class DoorStatusService : IDoorStatusService
{
    private readonly ILogger<DoorStatusService> _logger;
    private readonly IRealtimeStatusProxyService _realtimeStatusService;

    public DoorStatusService(ILogger<DoorStatusService> logger, IRealtimeStatusProxyService realtimeStatusService)
    {
        _logger = logger;
        _realtimeStatusService = realtimeStatusService;
        _logger.LogDebug($"DoorStatus initialized: {(_realtimeStatusService.IsDoorClosed ? "Close":"Open")}");
        IsClosed = _realtimeStatusService.IsDoorClosed;
        _realtimeStatusService.CycleStatusChanged += RealtimeStatusService_CycleStatusChanged;
    }

    private void RealtimeStatusService_CycleStatusChanged(object? sender, CTS.EventArgs<Contract.Models.DeviceSystem> e)
    {
        if (IsClosed != e.Data.DoorClosed)
        {
            _logger.LogDebug($"DoorStatus changed: {(IsClosed? "Close" : "Open")} => {(e.Data.DoorClosed? "Close":"Open")}");
            IsClosed = e.Data.DoorClosed;
            StatusChanged?.Invoke(this, IsClosed);
        }
    }

    public event EventHandler<bool>? StatusChanged;

    public bool IsClosed { get; private set; }
}
