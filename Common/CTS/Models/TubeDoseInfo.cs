//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/17 15:41:14     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.CTS.Models;

public class TubeDoseInfo
{
    /// <summary>
    ///源编号
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    ///kv峰值,单位 kV
    /// </summary>
    public float KVP { get; set; }

    /// <summary>
    ///mA峰值,单位 mA
    /// </summary>
    public float MaxMA { get; set; }

    /// <summary>
    ///平均电流,单位 mA
    /// </summary>
    public float MeanMA { get; set; }

    /// <summary>
    /// 射线源没圈曝光时间，单位ms
    /// </summary>
    public float ExposureTimePerRotate { get; set; }
}
