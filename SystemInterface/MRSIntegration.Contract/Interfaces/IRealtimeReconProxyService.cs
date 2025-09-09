//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/15 10:11:43           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Models;

namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces
{
    public interface IRealtimeReconProxyService
    {
        SystemStatus SystemStatus { get; }

        event EventHandler<EventArgs<SystemStatusInfo>> SystemStatusChanged;

        event EventHandler<EventArgs<RealtimeReconInfo>> ImageReceived;

        event EventHandler<EventArgs<RealtimeReconInfo>> RealtimeReconStatusChanged;

        event EventHandler<EventArgs<RealtimeReconInfo>> RawDataSaved;

        //RealtimeCommandResult PrepareScan(IList<ScanReconParam> infos);

        RealtimeCommandResult StartScan(IList<ScanReconParam> infos);

        RealtimeCommandResult AbortScan(AbortCause abortCause, bool isDeleteRawData);

        RealtimeCommandResult SetCycleMessageInterval(uint interval);

        BaseCommandResult Resume();
    }
}
