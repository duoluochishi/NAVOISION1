//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/10 13:09:23    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class RealtimeConnectionService : IRealtimeConnectionService
{
    private readonly ILogger<RealtimeConnectionService> _logger;
    private readonly IRealtimeProxyService _proxyService;

    public bool IsDeviceConnected { get; private set; }

    public bool IsReconConnected { get; private set; }

    public event EventHandler<bool> DeviceConnectionStatusChanged;
    public event EventHandler<bool> ReconConnectionStatusChanged;

    public RealtimeConnectionService(ILogger<RealtimeConnectionService> logger, IRealtimeProxyService proxyService)
    {
        _logger = logger;
        _proxyService = proxyService;
        IsDeviceConnected = _proxyService.IsDeviceConnected;
        IsReconConnected = _proxyService.IsReconConnected;
        _proxyService.DeviceConnectionChanged += RealtimeReconProxy_DeviceConnectionChanged;
        _proxyService.ReconConnectionChanged += RealtimeReconProxy_ReconConnectionChanged;
    }

    private void RealtimeReconProxy_ReconConnectionChanged(object arg1, ConnectionStatusArgs connectinStatusArg)
    {
        _logger.LogDebug($"FacadeProxy.ReconConnectionStatusChanged parameters: {connectinStatusArg.Connected}");
        IsReconConnected = connectinStatusArg.Connected;
        ReconConnectionStatusChanged?.Invoke(this, IsReconConnected);
    }

    private void RealtimeReconProxy_DeviceConnectionChanged(object arg1, ConnectionStatusArgs connectinStatusArg)
    {
        _logger.LogDebug($"FacadeProxy.DeviceConnectionChanged parameters: {connectinStatusArg.Connected}");
        IsDeviceConnected = connectinStatusArg.Connected;
        DeviceConnectionStatusChanged?.Invoke(this, IsDeviceConnected);
    }
}
