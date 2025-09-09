//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/20 15:58:37     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using NV.CT.CTS.Extensions;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class HeatCapacityService : IHeatCapacityService
{
    private readonly ILogger<HeatCapacityService> _logger;
    private List<Contract.Models.Tube> _current = new();
    private readonly IRealtimeStatusProxyService _realtimeStatusProxyService;
    private readonly object _lock = new object();

    public HeatCapacityService(ILogger<HeatCapacityService> logger, IRealtimeStatusProxyService realtimeStatusProxyService)
    {
        _logger = logger;
        _realtimeStatusProxyService = realtimeStatusProxyService;
        _realtimeStatusProxyService.CycleStatusChanged += ProxyService_CycleStatusChanged;
    }

    private void ProxyService_CycleStatusChanged(object? sender, CTS.EventArgs<Contract.Models.DeviceSystem> e)
    {
        lock (_lock)
        {
            _current = e.Data.Tubes;
        }

        HeatCapacityChanged?.Invoke(this, _current);
    }

    public List<Contract.Models.Tube> Current
    {
        get
        {
            lock (_lock)
            {
                return _current.Clone();
            }
        }
    }

    public float MaxHeatCapacity => _current.Count != 0 ? _current.Max(c => c.RaySource.HeatCapacity) : 0;

    public float MinHeatCapacity => _current.Count != 0 ? _current.Min(c => c.RaySource.HeatCapacity) : 0;

    public event EventHandler<List<Contract.Models.Tube>>? HeatCapacityChanged;
}
