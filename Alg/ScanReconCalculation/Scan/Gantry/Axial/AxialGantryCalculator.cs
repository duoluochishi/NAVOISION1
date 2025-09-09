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

namespace NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Axial;

/// <summary>
/// 轴扫机架旋转计算，按照不停方案。
/// 即轴扫开始后，在扫描与病床运动时间内匀速旋转。
/// 总旋转角度=扫描旋转角度+病床运动时间旋转角度+加减速角度+加速余量
/// </summary>
public class AxialGantryCalculator : IScanGantryCalculator
{
    
    

    public bool CanAccept(GantryControlInput input)
    {
        return input.ScanOption is ScanOption.Axial;
    }

    public GantryControlOutput GetGantryControlOutput(GantryControlInput input)
    {
        GantryControlOutput output = new GantryControlOutput();
        var totalAngle = GetTotalGantryAngle(input);

        if (totalAngle > GantryCommonConfig.MaxGantryPos - GantryCommonConfig.MinGantryPos) {
            throw new InvalidOperationException($"Requested Gantry {totalAngle} is greater than limit, current max/min:{GantryCommonConfig.MaxGantryPos}/{GantryCommonConfig.MinGantryPos} ");
        }
        
        //判断方向：能正则正，不能正则反，正反均不符合找最近可能方向，并偏转。
        if (GantryCommonConfig.MaxGantryPos - totalAngle > input.CurrentGantryPos )      //满足不调整正转
        {
            output.GantryDirection = GantryControlOutput.GantryDirectionInc;
            output.GantryStartPos = input.CurrentGantryPos;
            output.GantryEndPos = input.CurrentGantryPos + totalAngle;
        }
        else if (input.CurrentGantryPos - totalAngle > GantryCommonConfig.MinGantryPos)   //满足不调整反转
        {
            output.GantryDirection = GantryControlOutput.GantryDirectionDec;
            output.GantryStartPos = input.CurrentGantryPos;
            output.GantryEndPos = input.CurrentGantryPos + totalAngle * -1;
        }
        else if(GantryCommonConfig.MaxGantryPos - input.CurrentGantryPos > input.CurrentGantryPos - GantryCommonConfig.MinGantryPos) //离最小角度近，正转
        {
            output.GantryDirection = GantryControlOutput.GantryDirectionInc;
            output.GantryEndPos = GantryCommonConfig.MaxGantryPos;
            output.GantryStartPos = GantryCommonConfig.MaxGantryPos - totalAngle;
        }
        else //离最大角度近，反转。
        {
            output.GantryDirection = GantryControlOutput.GantryDirectionDec;
            output.GantryEndPos = GantryCommonConfig.MinGantryPos;
            output.GantryStartPos = GantryCommonConfig.MinGantryPos + totalAngle;
        }

        output.GantrySpeed = GetAxialGantrySpeed(input);
        if (output.GantrySpeed > GantryCommonConfig.MaxRotationSpeed)
        {
            throw new InvalidOperationException($"Requested Gantry speed {output.GantrySpeed} is greater than limit, current max rotation speed:{GantryCommonConfig.MaxRotationSpeed} ");
        }

        output.GantryAccTime = GetGantryAccTime(input) + GantryCommonConfig.GantryMoveToleranceTime;

        return output;
    }


    /// <summary>
    /// 总旋转角度=扫描旋转角度+病床运动时间旋转角度+加减速角度+加速余量
    /// </summary>
    /// <returns></returns>
    public double GetTotalGantryAngle(GantryControlInput input)
    {
        var totalGantryAngle = 0d;
        var gantrySpeed = GetAxialGantrySpeed(input);
        var totalScanTime = GetTotalScanTime(input); ;
        var totalMoveTime = GetTotalMoveTime(input);
        var gantryAccAngle = GetGantryAccAngle(input);
        totalGantryAngle += gantrySpeed * totalScanTime + gantrySpeed * totalMoveTime + gantryAccAngle * 2;
        totalGantryAngle += GetAccMoveTolerance(gantryAccAngle);

        return totalGantryAngle;
    }

    public double GetAccMoveTolerance(double accAngle)
    {
        var result = accAngle * GantryCommonConfig.GantryMoveToleranceWeight;
        result = result > GantryCommonConfig.MinGantryMoveTolerance ? result : GantryCommonConfig.MinGantryMoveTolerance;
        return result;
    }

    /// <summary>
    /// 获取移动段总时间，单位秒.
    /// 移动次数为扫描次数减1
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private double GetTotalMoveTime(GantryControlInput input)
    {
        //按照现在200mm/s2的加速度，当前支持的TableFeed都不可能加速到匀速段。
        return  2 * Math.Sqrt(input.TableFeed  / input.TableAcc) * (input.NumOfScan - 1);
    }

    public double GetGantryAccAngle(GantryControlInput input)
    {
        var speed = GetAxialGantrySpeed(input);
        return speed * speed / 2 / input.GantryAcc;
    }

    public double GetGantryAccTime(GantryControlInput input)
    {
        return GetAxialGantrySpeed(input) / input.GantryAcc * GantryCommonConfig.TimeScaleSecToUS;
    }

    /// <summary>
    /// 获取扫描段总时间，单位秒
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public double GetTotalScanTime(GantryControlInput input)
    {
        return GetEffectCycleTime(input) * input.NumOfScan;
    }

    private double GetEffectCycleTime(GantryControlInput input)
    {
        return input.FrameTime * (input.FramesPerCycle + input.PreIgnoredN) / input.ExpSourceCount / GantryCommonConfig.TimeScaleSecToUS;
    }
    public double GetAxialGantrySpeed(GantryControlInput input)
    {
        var effectGantryCycle = GantryCommonConfig.GantryCycleAngle / input.TotalSourceCount;
        return effectGantryCycle / GetCycleTime(input);
    }
    private double GetCycleTime(GantryControlInput input)
    {
        return input.FrameTime * input.FramesPerCycle / input.ExpSourceCount / GantryCommonConfig.TimeScaleSecToUS;
    }

}
