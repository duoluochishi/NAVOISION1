//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/17 9:55:51     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.SystemInterface.MCSRuntime.Contract;

/// <summary>
/// 单位
/// </summary>
public enum UnitType : int
{
    /// <summary>
    /// Byte
    /// </summary>
    /// 
    B = 0,
    /// <summary>
    /// KB
    /// </summary>
    KB,

    /// <summary>
    /// MB
    /// </summary>
    MB,

    /// <summary>
    /// GB
    /// </summary>
    GB,

    /// <summary>
    /// TB
    /// </summary>
    TB,

    /// <summary>
    /// PB
    /// </summary>
    PB
}
