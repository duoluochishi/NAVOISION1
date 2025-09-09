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

namespace NV.CT.Alg.ScanReconCalculation.Scan.Table;

/// <summary>
/// 检查床控制输出。包括可重建区域、数据曝光区域、病床运动起始终止位置、病床速度、加速时间等信息。
/// </summary>
public class TableControlOutput
{
    /// <summary>
    /// 重建区域起始位置，单位：微米
    /// </summary>
    public double ReconVolumeBeginPos { get; set; }

    /// <summary>
    /// 重建区域终止位置，单位：微米
    /// </summary>
    public double ReconVolumeEndPos { get; set; }

    /// <summary>
    /// 数据曝光区域起始位置，单位：微米
    /// </summary>
    public double DataBeginPos { get; set; }

    /// <summary>
    /// 数据曝光区域终止位置，单位：微米
    /// </summary>
    public double DataEndPos { get; set; }

    /// <summary>
    /// 床运动起始位置，单位：微米
    /// </summary>
    public double TableBeginPos { get; set; }

    /// <summary>
    /// 床运动终止位置，单位：微米
    /// </summary>
    public double TableEndPos { get; set; } 

    /// <summary>
    /// 床运动速度，单位：微米/秒
    /// </summary>
    public double TableSpeed { get; set; }

    /// <summary>
    /// 床加速时间，即床运动加速至匀速时间
    /// </summary>
    public double TableAccTime { get; set; }

    /// <summary>
    /// 待定
    /// </summary>
    public int NumOfScan { get; set; }

    /// <summary>
    /// 总帧数
    /// </summary>
    public int TotalFrames { get; set; }

    public double PreDeleteLength { get; set; }

    public double PostDeleteLength { get; set; }


    public double SmallAngleDeleteLength {  get; set; }

    public double LargeAngleDeleteLength { get; set; }
}
