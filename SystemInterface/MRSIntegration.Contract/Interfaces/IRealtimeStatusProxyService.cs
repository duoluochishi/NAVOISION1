//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/10 13:11:49     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Models;
using NV.CT.CTS;
using NV.CT.SystemInterface.MRSIntegration.Contract.Models;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IRealtimeStatusProxyService
{
    /// <summary>
    /// 周期性状态变更事件
    /// </summary>
    event EventHandler<EventArgs<DeviceSystem>> CycleStatusChanged;
    /// <summary>
    /// 实时状态变更事件
    /// </summary>
    event EventHandler<EventArgs<RealtimeInfo>> RealtimeStatusChanged;

    event EventHandler<EventArgs<RealtimeInfo>> EmergencyStopped;

    event EventHandler<EventArgs<RealtimeInfo>> ErrorStopped;

    event EventHandler<EventArgs<List<string>>> DeviceErrorOccurred;

    event EventHandler<EventArgs<List<string>>> ScanReconErrorOccurred;

    RealtimeStatus PreviewStatus { get; }

    RealtimeStatus CurrentStatus { get; }

    bool IsDoorClosed { get; }

    bool IsFrontRearCoverClosed { get; }

    bool IsDetectorTemperatureNormalStatus { get; }
}
