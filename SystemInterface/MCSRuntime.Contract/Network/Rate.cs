//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/17 14:09:47     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.SystemInterface.MCSRuntime.Contract.Network;

/// <summary>
/// 
/// </summary>
public struct Rate
{
    public Rate(DateTime startTime, long receivedLength, long sendLength)
    {
        StartTime = startTime;
        ReceivedLength = receivedLength;
        SendLength = sendLength;
    }

    /// <summary>
    /// 记录时间
    /// </summary>
    public DateTime StartTime { get; private set; }

    /// <summary>
    /// 此网卡总接收网络流量字节数
    /// </summary>
    public long ReceivedLength { get; private set; }

    /// <summary>
    /// 此网卡总发送网络流量字节数
    /// </summary>
    public long SendLength { get; private set; }
}