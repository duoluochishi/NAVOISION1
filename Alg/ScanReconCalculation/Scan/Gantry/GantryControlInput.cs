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


using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Gantry;

public class GantryControlInput
{
    /// <summary>
    /// 扫描模式：定位扫描，轴扫，螺旋扫描，双定位扫
    /// </summary>
    public ScanOption ScanOption { get; set; }

    /// <summary>
    /// 扫描类型； 1：平扫； 2：灌注扫描（使用摇篮床）,3 小剂量扫描，4 电影，5 心脏 6 能谱
    /// </summary>
    public ScanMode ScanMode { get; set; }

    /// <summary>
    /// 球管位置（曝光使用）
    /// 0度，90度，180度，270度
    /// </summary>
    public TubePosition[] TubePositions { get; set; } = new TubePosition[2];


    private double _currentGantryPosition;
    /// <summary>
    /// 当前机架位置（角度），单位：0.01度
    /// </summary>
    public double CurrentGantryPos
    {
        get => _currentGantryPosition;
        set
        {
            if (value >= GantryCommonConfig.MinGantryPos && value <= GantryCommonConfig.MaxGantryPos)
            {
                _currentGantryPosition = value;
            }
            else if (value > GantryCommonConfig.MaxGantryPos)
            {
                _currentGantryPosition = GantryCommonConfig.MaxGantryPos;
            }
            else if (value < GantryCommonConfig.MinGantryPos)
            {
                _currentGantryPosition = GantryCommonConfig.MinGantryPos;
            }
        }
    }

    /// <summary>
    /// 球管油温
    /// </summary>
    public double[] OilTem { get; set; } = new double[24];

    /// <summary>
    /// 球管热熔
    /// </summary>
    public double[] HeatCaps { get; set; } = new double[24];

    /// <summary>
    /// 前忽略帧数
    /// </summary>
    public int PreIgnoredN { get; set; }

    /// <summary>
    /// 帧时间，单位：微秒
    /// </summary>
    public double FrameTime { get; set; }

    /// <summary>
    /// 每圈帧数（View）
    /// </summary>
    public double FramesPerCycle { get; set; }

    /// <summary>
    /// 总球管数量
    /// </summary>
    public double TotalSourceCount { get; set; }

    /// <summary>
    /// 曝光模式(球管数量)。
    /// 曝光模式：单源，双源(能谱)，三源，6源(用于心脏扫描)
    /// </summary>
    public double ExpSourceCount { get; set; }

    /// <summary>
    /// 待定
    /// </summary>
    public double NumOfScan { get; set; }

    /// <summary>
    /// 步进（轴扫），单位：微米
    /// </summary>
    public double TableFeed { get; set; }

    /// <summary>
    /// 床运动加速度，单位：微米/秒^2
    /// </summary>
    public double TableAcc { get; set; }

    /// <summary>
    /// 床运动速度，单位：微米/秒
    /// </summary>
    public double TableSpeed { get; set; }

    /// <summary>
    /// 数据曝光区域起始位置，单位：微米
    /// </summary>
    public double DataBeginPos { get; set; }

    /// <summary>
    /// 数据曝光区域终止位置，单位：微米
    /// </summary>
    public double DataEndPos { get; set; }

    /// <summary>
    /// 机架旋转加速度（角度）：单位：0.01度/秒^2
    /// </summary>
    public double GantryAcc { get; set; }

    public uint Loops { get; set; }

    public double LoopTime { get; set; }
}