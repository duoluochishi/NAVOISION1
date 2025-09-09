//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/17 10:49:52     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.SystemInterface.MCSRuntime.Contract.CPU;

/// <summary>
/// CPU空闲和工作时间
/// </summary>
public struct CPUTime
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idleTime"></param>
    /// <param name="systemTime"></param>
    public CPUTime(ulong idleTime, ulong systemTime)
    {
        IdleTime = idleTime;
        SystemTime = systemTime;
    }

    /// <summary>
    /// CPU 空闲时间
    /// </summary>
    public ulong IdleTime { get; private set; }

    /// <summary>
    /// CPU 工作时间
    /// </summary>
    public ulong SystemTime { get; private set; }
}