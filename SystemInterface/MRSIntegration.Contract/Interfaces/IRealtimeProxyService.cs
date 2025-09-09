//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/9/14 15:38:16           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IRealtimeProxyService
{
    event EventHandler<AcqReconStatusArgs> AcqReconStatusChanged;

    event EventHandler<SystemStatusArgs> SystemStatusChanged;

    event EventHandler<ImageSavedEventArgs> ReconImageSaved;

    event EventHandler<CycleStatusArgs> CycleStatusChanged;

    event EventHandler<RealtimeEventArgs> RealTimeStatusChanged;

    event EventHandler<ConnectionStatusArgs> DeviceConnectionChanged;

    event EventHandler<ConnectionStatusArgs> ReconConnectionChanged;

    event EventHandler<List<string>> DeviceErrorOccurred;

    event EventHandler<List<string>> ScanReconErrorOccurred;

    event EventHandler<RawImageSavedEventArgs> RawImageSaved;

    bool IsDeviceConnected { get; }

    bool IsReconConnected { get; }

    AuxBoard AuxBoard { get; }

    RealtimeStatus RealtimeStatus { get; }

    SystemStatus SystemStatus { get; }
}
