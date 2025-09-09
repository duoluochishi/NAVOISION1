//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/17 9:57:32     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.SystemInterface.MCSRuntime.Contract;

/// <summary>
/// 大小信息
/// </summary>
public class SizeInfo
{
    /// <summary>
    /// Byte 长度
    /// </summary>
    public long ByteLength { get; private set; }

    /// <summary>
    /// 大小
    /// </summary>
    public decimal Size { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    public UnitType SizeType { get; set; }

    public SizeInfo(long byteLength)
    {
        ByteLength = byteLength;
    }
}
