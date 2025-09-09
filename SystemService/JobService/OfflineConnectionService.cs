//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/10 14:12:18    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.JobService.Contract;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.JobService;

public class OfflineConnectionService : IOfflineConnection
{
    private readonly IOfflineConnectionService _connectionService;

    public OfflineConnectionService(IOfflineConnectionService connectionService)
    {
        _connectionService = connectionService;
        _connectionService.ConnectionStatusChanged += _connectionService_ConnectionStatusChanged;
    }

    private void _connectionService_ConnectionStatusChanged(object? sender, EventArgs<ServiceStatusInfo> e)
    {
        ConnectionStatusChanged?.Invoke(this, e);
    }

    public event EventHandler<EventArgs<ServiceStatusInfo>> ConnectionStatusChanged;

    public bool GetConnectionStatus() => _connectionService.IsConnected;
}
