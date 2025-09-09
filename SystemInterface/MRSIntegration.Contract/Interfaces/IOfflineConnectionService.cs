//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/10 13:03:49    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;
using NV.CT.CTS;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IOfflineConnectionService
{
    bool IsConnected { get; }

    event EventHandler<EventArgs<ServiceStatusInfo>> ConnectionStatusChanged;

    event EventHandler<(string Timestamp, string ErrorCode)> ErrorOccured;
}
