//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/17 13:04:41     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace NV.CT.SystemInterface.MCSRuntime.Contract.CPU;

/// <summary>
/// 
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct FILETIME
{
    /// <summary>
    /// 时间的低位部分
    /// </summary>
    public uint DateTimeLow;

    /// <summary>
    /// 时间的高位部分
    /// </summary>
    public uint DateTimeHigh;
}