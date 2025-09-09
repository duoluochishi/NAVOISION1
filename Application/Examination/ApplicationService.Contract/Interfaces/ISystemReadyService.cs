//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/6/14 15:36:41     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Models;
using NV.CT.SystemInterface.MRSIntegration.Contract.Models;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface ISystemReadyService
{
	/// <summary>
	/// 系统是否可用Ready，包括门控，连接状态,CTBox状态等
	/// </summary>
	bool Status { get; }

	string LatestFailReason { get; }

    event EventHandler<EventArgs<(bool status, bool isSyatemStatus)>> StatusChanged;

    public event EventHandler<CTS.EventArgs<RealtimeInfo>>? RealtimeStatusChanged;
    public event EventHandler<CTS.EventArgs<DeviceSystem>>? CycleStatusChanged;
    public event EventHandler<CTS.EventArgs<RealtimeInfo>>? EmergencyStopped;
    public event EventHandler<CTS.EventArgs<RealtimeInfo>>? ErrorStopped;
    public event EventHandler<bool>? DoorStatusChanged;


    bool CurrentConnectionStatus { get; }

	bool CurrentRealtimeStatus { get; }

	bool CurrentDoorStatus { get; }

	bool CurrentCTBoxStatus { get; }

	bool CurrentTableStatus { get; }
}