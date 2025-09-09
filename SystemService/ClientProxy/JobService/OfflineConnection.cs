//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/10 14:14:45    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.MPS.Communication;
using NV.CT.CTS;
using NV.CT.CTS.Models;

//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/10 14:14:45    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.JobService.Contract;

namespace NV.CT.ClientProxy.JobService;

public class OfflineConnection : IOfflineConnection
{
    private readonly JobClientProxy _clientProxy;

    public OfflineConnection(JobClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public event EventHandler<EventArgs<ServiceStatusInfo>> ConnectionStatusChanged;

    public bool GetConnectionStatus()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IOfflineConnection).Namespace,
            SourceType = nameof(IOfflineConnection),
            ActionName = nameof(IOfflineConnection.GetConnectionStatus),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }
}
