//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/10/19 9:53:04           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IShutdownProxyService
{
    event EventHandler<(bool IsConnected, ShutdownScope ShutdownScope)>? ConnectionStatusChanged;

    event EventHandler<ShutdownStatusArgs>? ShutdownStatusChanged;

    BaseCommandResult Shutdown(ShutdownScope scope);

    BaseCommandResult Restart(ShutdownScope scope);

    BaseCommandResult CanShutdown(ShutdownScope scope);

    BaseCommandResult CanRestart(ShutdownScope scope);
}
