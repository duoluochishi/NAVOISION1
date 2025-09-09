//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/10 14:09:13    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;
using NV.CT.CTS;

namespace NV.CT.JobService.Contract;

public interface IOfflineConnection
{
    bool GetConnectionStatus();

    event EventHandler<EventArgs<ServiceStatusInfo>> ConnectionStatusChanged;
}
