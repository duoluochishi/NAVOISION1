//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) $year$, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/02/02 09:42:22    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.Alg.ScanReconCalculation.Scan.Gantry;

public class GantryControlOutput
{
    public const int GantryDirectionInc = 0;

    public const int GantryDirectionDec = 1;

    /// <summary>
    /// 机架旋转起始位置，角度，单位：0.01度
    /// </summary>
    public double GantryStartPos { get; set; }

    /// <summary>
    /// 机架旋转终止位置，角度，单位：0.01度
    /// </summary>
    public double GantryEndPos { get; set; }

    /// <summary>
    /// 机架旋转方向，顺时针方向 或 逆时针方向
    /// </summary>
    public int GantryDirection { get;set; }             //机架绝对角度递增，direction = 2；机架绝对角度递减，direction=1

    /// <summary>
    /// 机架旋转速度，单位：0.01度/秒
    /// </summary>
    public double GantrySpeed { get; set; }

    /// <summary>
    /// 机架旋转加速时间，单位：微秒
    /// </summary>
    public double GantryAccTime { get; set; }

    /// <summary>
    /// 选中的球管
    /// </summary>
    public int[] SelectedTube { get; set; } = new int[2];
}
