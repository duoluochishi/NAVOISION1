//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:19    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS.Models;

namespace NV.CT.SystemInterface.MCSRuntime.Contract;

/// <summary>
/// MCS shutdown interface
/// </summary>
public interface IShutdownService
{
    BaseCommandResult CanShutdown();

    BaseCommandResult Shutdown();

    BaseCommandResult CanRestart();

    BaseCommandResult Restart();
}
