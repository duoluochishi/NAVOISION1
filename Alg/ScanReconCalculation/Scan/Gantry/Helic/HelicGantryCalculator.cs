//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:41:39    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Helic;


/// <summary>
/// 螺旋扫机架旋转计算
/// </summary>
public class HelicGantryCalculator : IScanGantryCalculator
{
    public bool CanAccept(GantryControlInput input)
    {
        return input.ScanOption is FacadeProxy.Common.Enums.ScanOption.Helical;
    }

    public GantryControlOutput GetGantryControlOutput(GantryControlInput input)
    {
        var output = new GantryControlOutput();

        var totalAngle = GetTotalGantryAngle(input);
        // todo :  input.CurrentGantryPos 应先根据最大最小GantryPosition进行调整后再进行计算

        if (totalAngle > GantryCommonConfig.MaxGantryPos - GantryCommonConfig.MinGantryPos)
        {
            throw new InvalidOperationException($"Requested Gantry {totalAngle} is greater than limit, current max/min:{GantryCommonConfig.MaxGantryPos}/{GantryCommonConfig.MinGantryPos} ");
        }

        //判断方向：能正则正，不能正则反，正反均不符合找最近可能方向，并偏转。
        if (GantryCommonConfig.MaxGantryPos - totalAngle > input.CurrentGantryPos)      //满足不调整正转
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
        else if (GantryCommonConfig.MaxGantryPos - input.CurrentGantryPos > input.CurrentGantryPos - GantryCommonConfig.MinGantryPos) //离最小角度近，正转
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
        output.GantrySpeed = GetHelicGantrySpeed(input);
        if (output.GantrySpeed > GantryCommonConfig.MaxRotationSpeed)
        {
            throw new InvalidOperationException($"Requested Gantry speed {output.GantrySpeed} is greater than limit, current max rotation speed:{GantryCommonConfig.MaxRotationSpeed} ");
        }

        output.GantryAccTime = GetGantryAccTime(input) + GantryCommonConfig.GantryMoveToleranceTime;
        return output;
    }

    public double GetTotalGantryAngle(GantryControlInput input)
    {
        return GetTotalScanAngle(input) + GetGantryAccAngle(input) * 2;
    }

    public double GetTotalScanAngle(GantryControlInput input)
    {
        var time = Math.Abs(input.DataBeginPos - input.DataEndPos) / input.TableSpeed;
        return time * GetHelicGantrySpeed(input);
    }
    private double GetGantryAccAngle(GantryControlInput input)
    {
        var speed = GetHelicGantrySpeed(input);
        return speed * speed / 2/ input.GantryAcc;
    }

    private double GetGantryAccTime(GantryControlInput input)
    {
        return GetHelicGantrySpeed(input) / input.GantryAcc * GantryCommonConfig.TimeScaleSecToUS;
    }


    private double GetHelicGantrySpeed(GantryControlInput input)
    {
        var effectGantryCycle = GantryCommonConfig.GantryCycleAngle / input.TotalSourceCount;
        return effectGantryCycle / GetCycleTime(input);
    }
    private double GetCycleTime(GantryControlInput input)
    {
        return input.FrameTime * input.FramesPerCycle / input.ExpSourceCount / GantryCommonConfig.TimeScaleSecToUS;
    }
}
