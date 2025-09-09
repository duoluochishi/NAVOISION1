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


using NV.CT.Alg.ScanReconCalculation.Scan.Common;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Topo;

public class TopoGantryCalculator : IScanGantryCalculator
{
    public virtual bool CanAccept(GantryControlInput input)
    {
        return input.ScanOption is ScanOption.Surview;
    }

    public virtual GantryControlOutput GetGantryControlOutput(GantryControlInput input)
    {
        if (input.HeatCaps.All(x => x > GantryCommonConfig.HeatCapacityThreshold))    //所有球管热容不符，直接报错
        {
            throw new InvalidOperationException($"HeatCapacity for all tubes exceed the threshold:{GantryCommonConfig.HeatCapacityThreshold}");
        }

        var output = new GantryControlOutput();

        var (index, pos) = GetSelectedTubeNumAndNewGantryPosition(input);

        output.GantryStartPos = pos;
        output.GantryEndPos = pos;
        output.SelectedTube[0] = index+1;       //内部计算序号为0-23，输出为1-24
        output.GantryDirection = GantryControlOutput.GantryDirectionInc;
        output.GantrySpeed = 0;
        output.GantryAccTime = GantryCommonConfig.GantryMoveToleranceTime;

        return output;
    }


    /// <summary>
    /// 获取指定球管在指定球管位置曝光时的机架角度
    /// </summary>
    /// <param name="tubeNum"></param>
    /// <param name="tubePosition"></param>
    /// <returns></returns>
    public double GetGantryPosForTubeAtTubePosition(int tubeNum,TubePosition tubePosition)
    {
        var baseffsetTo3 = GantryCommonConfig.GetTubePositionOffsetTo3(tubePosition);
        var result = tubeNum * GantryCommonConfig.TubeInterval + baseffsetTo3 + GantryCommonConfig.FirstTubeCalibrationPos;

        result = result > GantryCommonConfig.GantryCycleAngle + GantryCommonConfig.FirstTubeCalibrationPos? result - GantryCommonConfig.GantryCycleAngle : result;

        return result;
    }



    /// <summary>
    /// 获取指定床位下距离指定球管位置最近的球管及使用该球管曝光时的球管位置。
    /// </summary>
    /// <param name="currentGantry"></param>
    /// <param name="tubePosition"></param>
    /// <returns></returns>
    public (int tubeNum, double newGantryPos) GetNearestTubeNumAndNewPosition(double gantryPos, TubePosition tubePosition)
    {
        var targetOffsetFrom3 = GantryCommonConfig.GetTubePositionOffsetTo3(tubePosition) ;
        var targetOffset = gantryPos - GantryCommonConfig.FirstTubeCalibrationPos - targetOffsetFrom3;  

        var result = (int)Math.Round(targetOffset / GantryCommonConfig.TubeInterval);
        var newGantry = result * GantryCommonConfig.TubeInterval + GantryCommonConfig.FirstTubeCalibrationPos + targetOffsetFrom3;
        if (newGantry < GantryCommonConfig.MinGantryPos)
        {
            result++;
            newGantry += GantryCommonConfig.TubeInterval;
        }
        else if (newGantry > GantryCommonConfig.MaxGantryPos)
        {
            result --;
            newGantry -= GantryCommonConfig.TubeInterval;
        }

        while(result >= GantryCommonConfig.TubeCount)
        {
            result -= GantryCommonConfig.TubeCount;
        }

        while (result < 0)
        {
            result += GantryCommonConfig.TubeCount;
        }

        return (result,newGantry);
    }



    /// <summary>
    /// 根据当前目标球管位置最近的球管，查看附近球管状况，选择实际曝光用球管。
    /// 获取最近的符合条件的球管?选择逻辑是什么?
    /// </summary>
    /// <param name="input"></param>
    /// <param name="nearestTube"></param>
    /// <returns></returns>
    public (int, double) GetSelectedTubeNumAndNewGantryPosition(GantryControlInput input)
    {
        //看是否有优先选取
        var (preferTube, preferTubePos) = GetPreferTubeNumAndNewPosition(input, GantryCommonConfig.PreferTubeCount, GantryCommonConfig.PreferHeatCapacity);
        if (preferTube != -1)
        {
            return (preferTube, preferTubePos);
        }

        //没有优先选取，则选热容最小球管
        return GetMinTubeNumAndNewGantryPosition(input);
    }

    public (int,double) GetMinTubeNumAndNewGantryPosition(GantryControlInput input)
    {
        var selectedIndex = CommonCalHelper.GetSortedIndex(input.HeatCaps)[0];
        var (tubeNum, gantryPos) = GetNearestTubeNumAndNewPosition(input.CurrentGantryPos, input.TubePositions[0]);

        var offset = selectedIndex - tubeNum;   //看是否能够就近转
        var newGantry = gantryPos + GantryCommonConfig.TubeInterval * offset;
        if (newGantry > GantryCommonConfig.MinGantryPos && newGantry < GantryCommonConfig.MaxGantryPos)
        {
            return (selectedIndex, newGantry);
        }

        //如果不能就近转，则反向转到最低热容球管。
        offset = tubeNum - selectedIndex;
        newGantry = gantryPos + GantryCommonConfig.TubeInterval * offset;
        return (selectedIndex, newGantry);
    }

    /// <summary>
    /// 
    /// 获取选取球管逻辑，依据：
    /// 获取当前球管位置最近的球管为暂定的目标球管，
    /// 目标球管位置附近指定的N个球管范围内，若球管热容小于指定的球管热容，即可用来进行定位像曝光。
    /// 若范围内没有，则返回-1，double.NaN
    /// </summary>
    /// <param name="input"></param>
    /// <param name="preferTubeNum"></param>
    /// <returns></returns>
    public (int,double) GetPreferTubeNumAndNewPosition(GantryControlInput input,int preferTubeCount, double preferHeatCapacity)
    {
        //获取当前球管曝光位置的最近球管与曝光机架位置
        var (tubeNum, gantryPos) = GetNearestTubeNumAndNewPosition(input.CurrentGantryPos, input.TubePositions[0]);


        //以当前正对范围的球管为中心，向两边获取preferTubeCount内距离最近的在Prefer热容范围内的球管。
        var offset = GetTubeOffset(tubeNum, gantryPos, 1, input.HeatCaps, preferTubeCount, preferHeatCapacity);
        var direction = offset == int.MaxValue ? 0 : 1;
        var antiOffset = GetTubeOffset(tubeNum, gantryPos, -1, input.HeatCaps, preferTubeCount, preferHeatCapacity);
        if (antiOffset < offset)
        {
            offset = antiOffset;
            direction = -1;
        }

        if (direction == 0)
        {
            return (-1, double.NaN);
        }

        var newTubeNum = CorrectTubeNum(tubeNum + offset * direction);
        var newGantryPos = gantryPos + offset * direction * GantryCommonConfig.TubeInterval;

        return (newTubeNum, newGantryPos);
    }

    /// <summary>
    /// 按照当前距离最近球管位置与机架角度，获取符合热容条件的最近偏移球管。
    /// </summary>
    /// <param name="tubeNum"></param>
    /// <param name="gantryPos"></param>
    /// <param name="offsetDirection">顺时针方向1，逆时针方向-1</param>
    /// <param name="heatCpas"></param>
    /// <param name="preferTubeCount"></param>
    /// <param name="preferHeatCapacity"></param>
    /// <returns></returns>
    private int GetTubeOffset(int tubeNum, double gantryPos, int offsetDirection, double[] heatCpas, int preferTubeCount,double preferHeatCapacity)
    {
        for (int i = 0; i < preferTubeCount; i++)
        {
            if ((gantryPos + i * GantryCommonConfig.TubeInterval * offsetDirection) > GantryCommonConfig.MaxGantryPos
                || (gantryPos + i * GantryCommonConfig.TubeInterval * offsetDirection) < GantryCommonConfig.MinGantryPos)
            {
                break;
            }
            var tmpNum = CorrectTubeNum(tubeNum + i * offsetDirection);
            if (heatCpas[tmpNum] < preferHeatCapacity)
            {
                return i;
            }
        }
        return int.MaxValue;
    }

    protected int CorrectTubeNum(int tubeNum)
    {
        tubeNum = tubeNum >= GantryCommonConfig.TubeCount ? tubeNum - GantryCommonConfig.TubeCount : tubeNum;
        tubeNum = tubeNum < 0 ? GantryCommonConfig.TubeCount + tubeNum : tubeNum;
        return tubeNum;
    }
}
