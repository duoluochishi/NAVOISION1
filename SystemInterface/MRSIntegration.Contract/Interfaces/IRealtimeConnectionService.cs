//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/10 13:05:09    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


namespace NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

public interface IRealtimeConnectionService
{
    bool IsDeviceConnected { get; }

    bool IsReconConnected { get; }

    /// <summary>
    /// 设备连接状态变更事件
    /// </summary>
    event EventHandler<bool> DeviceConnectionStatusChanged;

    /// <summary>
    /// 采集重建(实时重建)服务连接状态变更事件
    /// </summary>
    event EventHandler<bool> ReconConnectionStatusChanged;
}
