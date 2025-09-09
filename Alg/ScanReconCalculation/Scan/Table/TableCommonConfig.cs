//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/3/22 10:54:58    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------


using NV.MPS.Configuration;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Table;

public static class TableCommonConfig
{
    /// <summary>
    /// 移床最大加速度
    /// 当前单位：微米/秒^2
    /// </summary>

    public static double MaxTableAccelation = SystemConfig.TableConfig.Table.MaxZAcc.Value; //此前默认值为：2700

    /// <summary>
    /// 移床最大速度
    /// 当前单位：微米/秒^2
    /// </summary>
    public static double MaxTableSpeed = SystemConfig.TableConfig.Table.MaxZSpeed.Value; //此前默认值为：2500; 

    /// <summary>
    /// 探测器全开口大小（即288排，0.165 * 288 = 47.52毫米）
    /// 当前单位：微米
    /// </summary>
    public static double FullSliceWidth = SystemConfig.DetectorConfig.Detector.Width.Value;

    /// <summary>
    /// Z Offset，单位微米
    /// </summary>
    public static double ZOffset = SystemConfig.DetectorConfig.Detector.ZOffset.Value;//todo: 从配置中读取

    /// <summary>
    /// Z向探测器通道总数
    /// </summary>
    public static int ZChannelCount = SystemConfig.DetectorConfig.Detector.ZChannelCount.Value;

    /// <summary>
    /// Z向物理分辨率
    /// </summary>
    public static double ResolutionZ = SystemConfig.DetectorConfig.Detector.Resolution.Value;

    /// <summary>
    /// 单位：微米
    /// </summary>
    public static double SID = SystemConfig.DetectorConfig.Detector.SID.Value;

    /// <summary>
    /// 单位：微米
    /// </summary>
    public static double SOD = SystemConfig.DetectorConfig.Detector.SOD.Value;

    /// <summary>
    /// Z基础像素，当前使用0.3333mm
    /// </summary>
    public static double DVoxelZ = 1000.0 / 3;

}
