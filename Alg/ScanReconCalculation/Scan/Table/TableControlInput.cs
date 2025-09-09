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

namespace NV.CT.Alg.ScanReconCalculation.Scan.Table;

public class TableControlInput
{

    /// <summary>
    /// 检查床控制输入。
    /// </summary>
    /// <param name="scanOption"></param>
    /// <param name="scanMode"></param>
    /// <param name="tableDirection"></param>
    /// <param name="reconVolumeBeginPos"></param>
    /// <param name="reconVolumeEndPos"></param>
    /// <param name="frameTime"></param>
    /// <param name="framesPerCycle"></param>
    /// <param name="preIgnoredFrames"></param>
    /// <param name="collimatedSliceWidth"></param>
    /// <param name="tableFeed"></param>
    /// <param name="tableAcc"></param>
    /// <param name="exposureTime"></param>
    public TableControlInput(
        ScanOption scanOption, ScanMode scanMode,
        TableDirection tableDirection,
        double reconVolumeBeginPos, double reconVolumeEndPos,
        double frameTime, int framesPerCycle,
        int preIgnoredFrames,
        double collimatedSliceWidth,
        double tableFeed, double tableAcc,
        int collimatorZ, uint objectFov,
        double exposureTime,
        double pitch)
    {
        ScanOption = scanOption;
        ScanMode = scanMode;
        TableDirection = tableDirection;
        ReconVolumeBeginPos = reconVolumeBeginPos;
        ReconVolumeEndPos = reconVolumeEndPos;
        FrameTime = frameTime;
        FramesPerCycle = framesPerCycle;
        PreIgnoredFrames = preIgnoredFrames;
        CollimatedSliceWidth = collimatedSliceWidth;
        TableFeed = tableFeed;
        TableAcc = tableAcc;
        CollimatorZ = collimatorZ;
        ObjectFov = objectFov;
        ExposureTime = exposureTime;
        Pitch = pitch;
    }

    public TableControlInput() { }

    /// <summary>
    /// 扫描模式：定位扫描，轴扫，螺旋扫描，双定位扫
    /// </summary>
    public ScanOption ScanOption { get; set; }

    /// <summary>
    /// 扫描类型； 1：平扫； 2：灌注扫描（使用摇篮床）,3 小剂量扫描，4 电影，5 心脏 6 能谱
    /// </summary>
    public ScanMode ScanMode { get; set; }

    /// <summary>
    /// 床运动方向：进床，出床
    /// </summary>
    public TableDirection TableDirection { get; set; }

    /// <summary>
    /// 重建区域起始位置，单位：微米
    /// </summary>
    public double ReconVolumeBeginPos { get; set; }

    /// <summary>
    /// 重建区域终止位置，单位：微米
    /// </summary>
    public double ReconVolumeEndPos { get; set; }

    /// <summary>
    /// 帧时间，单位：微秒
    /// </summary>
    public double FrameTime { get; set; }

    /// <summary>
    /// 每圈帧数（View）
    /// </summary>
    public int FramesPerCycle { get; set; }

    /// <summary>
    /// 前忽略帧数
    /// </summary>
    public int PreIgnoredFrames { get; set; }

    /// <summary>
    /// 准直后的开口大小，单位：微米
    /// 排数 * 165，（0.165mm）
    /// </summary>
    public double CollimatedSliceWidth { get; set; }

    /// <summary>
    /// 步进（轴扫），单位：微米
    /// </summary>
    public double TableFeed { get; set; }

    /// <summary>
    /// 床加速度，单位：微米/秒^2
    /// </summary>
    public double TableAcc { get; set; }

    /// <summary>
    /// 曝光时间，单位：微秒
    /// </summary>
    public double ExposureTime { get; set; }

    /// <summary>
    /// 螺距（螺旋扫描），系数：实际值的百倍
    /// 33 - 150，即：0.33 - 1.5
    /// </summary>
    public double Pitch { get; set; } = 1;

    /// <summary>
    /// 曝光模式(球管数量)。
    /// 曝光模式：单源，双源(能谱)，三源，6源(用于心脏扫描)
    /// </summary>
    public int ExpSourceCount { get; set; } = 1;

    /// <summary>
    /// 总球管数量
    /// </summary>
    public int TotalSourceCount { get; set; } = 1;

    public uint Loops { get; set; }

    /// <summary>
    /// 扫描对象FOV
    /// </summary>
    public uint ObjectFov { get; set; }

    /// <summary>
    /// 有效探测器通道数
    /// </summary>
    public int CollimatorZ { get; set; }

    /// <summary>
    /// 后删图长度
    /// </summary>
    public double PostDeleteLegnth { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public double PreDeleteRatio { get; set; }

    /// <summary>
    /// 部位
    /// </summary>
    public BodyPart BodyPart { get; set; }
}