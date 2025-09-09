//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/17 14:29:08     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.SystemInterface.MCSRuntime.Contract;

public static class SizeExtension
{
    /// <summary>
    /// 将字节单位转换为合适的单位
    /// </summary>
    /// <param name="totalLength">字节长度</param>
    /// <returns></returns>
    public static SizeInfo ToSize(this long byteLength)
    {
        UnitType unit = 0;
        decimal number = byteLength;
        if (byteLength < 1000)
        {
            return new SizeInfo(byteLength)
            {
                Size = byteLength,
                SizeType = UnitType.B
            };
        }
        // 避免出现 1023B 这种情况；这样 1023B 会显示 0.99KB
        while (Math.Round(number / 1000) >= 1)
        {
            number = number / 1024;
            unit++;
        }

        return new SizeInfo(byteLength)
        {
            Size = Math.Round(number, 2),
            SizeType = unit
        };
    }
}
