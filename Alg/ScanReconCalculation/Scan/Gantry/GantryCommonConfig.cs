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
using NV.MPS.Configuration;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Gantry;

/// <summary>
/// 约定：除了必要的int值，所有计算过程均使用double保证精度，在最终下参过程中再进行强制转换。
/// </summary>
public static class GantryCommonConfig
{
    public const double TimeScaleSecToUS = 1000000;

    public const double GantryCycleAngle = 36000;

    /// <summary>
    /// 机架最大角度（旋转）
    /// 当前单位：0.01度
    /// </summary>
    public static double MaxGantryPos = SystemConfig.GantryConfig.Gantry.Angle.Max;

    /// <summary>
    /// 机架旋转前冗余保留时间
    /// </summary>
    public static double GantryMoveToleranceTime = 500000;

    /// <summary>
    /// 机架最小角度（旋转）
    /// 当前单位：0.01度
    /// </summary>
    public static double MinGantryPos = SystemConfig.GantryConfig.Gantry.Angle.Min;

    public static double GantryMoveToleranceWeight = 0.33;         
    public static double MinGantryMoveTolerance = 100;

    /// <summary>
    /// 球管（射线源）个数
    /// </summary>
    public static int TubeCount = (int)SystemConfig.SourceComponentConfig.SourceComponent.SourceCount; //此前默认值：24;

    /// <summary>
    /// 第一个球管的角度位置
    /// 当前单位：0.01度
    /// </summary>
    public static double FirstTubeCalibrationPos = SystemConfig.GantryConfig.Gantry.FirstTubeOffset.Value; //此前默认值：6000;

    /// <summary>
    /// 球管曝光热容限制
    /// </summary>
    public static double HeatCapacityThreshold = SystemConfig.SourceComponentConfig.SourceComponent.MaxHeatCapacity.Value; //此前默认值：260;

    /// <summary>
    /// 最大旋转速度（角度）
    /// 单位：0.01度/秒
    /// </summary>
    public static double MaxRotationSpeed = SystemConfig.GantryConfig.Gantry.MaxRotationSpeed.Value; //默认值为：5000;

    /// <summary>
    /// 最大旋转速度（角度）
    /// 单位：0.01度/秒^2
    /// </summary>
    public static double MaxRotationAcceleration = SystemConfig.GantryConfig.Gantry.MaxRotationAcceleration.Value; //默认值为：3000;

    public static double OilTempThreshould = 90;             //曝光油温限制

    public static int PreferTubeCount = 3;
    public static double PreferHeatCapacity = 260;        //曝光热容优先
    public static double PreferOilTemp = 90;             //曝光油温优先

    public static int QuarterCycleAngle = 90;
    public static int FullCycleAngle = 360;

    public static double[] CalibratedTubePositions
    {
        get
        {
            if(TubeCount < 1)
            {
                throw new InvalidOperationException($"The total cube count {TubeCount} is less than 1");
            }
            var result = new double[TubeCount];
            var interval = TubeInterval;
            result[0] = FirstTubeCalibrationPos;
            for (int i = 1; i < TubeCount; i++) {
                result[i] += interval;
            }

            return result;
        }
    }

    public static double TubeInterval { get
        {
            return GantryCycleAngle / TubeCount;
        } 
    }

    /// <summary>
    /// 获取球管曝光位置与基准位置的偏移,即1号球管在指定曝光位置进行曝光的机架角度。
    /// 当前基准位置的定义为一号球管在3点钟方向时的机架位置。单位mm
    /// 方向为顺时针方向
    /// </summary>
    /// <param name="tubePosition"></param>
    /// <returns></returns>
    public static double GetTubePositionOffsetTo3(TubePosition tubePosition)
    {
        switch (tubePosition)
        {
            case TubePosition.Angle90:
                return GantryCycleAngle / 4 * 0;
            case TubePosition.Angle0:
                return GantryCycleAngle / 4 * 3;
            case TubePosition.Angle270:
                return GantryCycleAngle / 4 * 2;
            case TubePosition.Angle180:
                return GantryCycleAngle / 4 * 1;                
        }

        return 0;
    }
}
