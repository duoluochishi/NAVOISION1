//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/2/4 13:18:25    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------



using NV.CT.Alg.ScanReconCalculation.Scan.Gantry.Topo;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Gantry.DualTopo;


/// <summary>
/// 双定位像计算：
/// 前提：双定位像只支持90度夹角。即正、侧位各一张。
/// 计算过程：  首先根据球管热容，获取可用的双定位像球管组合。
///             然后计算从组合中筛选距离当前位置最近的。
/// </summary>
public class DualTopoGantryCalculator : TopoGantryCalculator
{
    public override bool CanAccept(GantryControlInput input)
    {
        return input.ScanOption is ScanOption.DualScout;
    }

    public override GantryControlOutput GetGantryControlOutput(GantryControlInput input)
    {
        //检查当前配置，若无法进行正侧位曝光，则报错。即总球管个数必须是4的倍数。
        if(GantryCommonConfig.TubeCount % 4 != 0)
        {
            throw new InvalidOperationException($"Not possible to do Dual Topo with current tube count:{GantryCommonConfig.TubeCount}");
        }
        //检查当前球管方向，只能进行正、侧位曝光，否则报错。
        if (input.TubePositions is null)
        {
            throw new InvalidOperationException($"Tube option is null for Dual Topo");
        }
        if (input.TubePositions.Length < 2)
        {
            throw new InvalidOperationException($"Tube option Length is less than 2 for Dual Topo");
        }
        if (Math.Abs(input.TubePositions[0] - input.TubePositions[1]) != GantryCommonConfig.QuarterCycleAngle 
            && Math.Abs(input.TubePositions[0] - input.TubePositions[1]) != GantryCommonConfig.FullCycleAngle - GantryCommonConfig.QuarterCycleAngle)
        {
            throw new InvalidOperationException($"Current Tube Options can not fit Dual Topo: {input.TubePositions[0]}/{input.TubePositions[1]}");
        }

        //所有球管热容不符，直接报错
        if (input.HeatCaps.All(x => x > GantryCommonConfig.HeatCapacityThreshold))   
        {
            throw new InvalidOperationException($"HeatCapacities for all tubes exceed the threshold:{GantryCommonConfig.HeatCapacityThreshold}");
        }

        //确保存在符合热容范围的球管组合。
        var preferPairList = GetDualTopoTubePair(input,GantryCommonConfig.HeatCapacityThreshold);
        if(preferPairList.Count == 0) {
            throw new InvalidOperationException($"No suitable tubes for dual topo scanning:{GantryCommonConfig.HeatCapacityThreshold}");
        }

        var output = new GantryControlOutput();
        //获取当前正向顺序排前面的TubePosition
        var (firstTubeNum, secondTubeNum, newGantryPos) = GetPreferDualTubeNumAndNewPosition(input,GantryCommonConfig.TubeCount / 2,GantryCommonConfig.HeatCapacityThreshold);

        output.GantryStartPos = newGantryPos;
        output.GantryEndPos = newGantryPos;
        output.SelectedTube[0] = firstTubeNum + 1;  //内部计算序号为0-23，输出为1-24
        output.SelectedTube[1] = secondTubeNum + 1; //内部计算序号为0-23，输出为1-24
        output.GantryDirection = GantryControlOutput.GantryDirectionInc;
        output.GantrySpeed = 0;
        output.GantryAccTime = GantryCommonConfig.GantryMoveToleranceTime;

        return output;
    }

    /// <summary>
    /// 获取符合热容条件的1/4圈球管组合
    /// 返回的组合第二个序号为第一个序号顺时针数1/4圈后的球管
    /// </summary>
    /// <param name="input"></param>
    /// <param name="heatCapThreshold"></param>
    /// <returns></returns>
    public List<(int,int)> GetDualTopoTubePair(GantryControlInput input, double heatCapThreshold) 
    {
        var resultList = new List<(int,int)> ();
        for (int aTubeNum = 0; aTubeNum < GantryCommonConfig.TubeCount; aTubeNum++)
        {
            var bTubeNum = aTubeNum + GantryCommonConfig.TubeCount / 4;
            bTubeNum = bTubeNum >= GantryCommonConfig.TubeCount? bTubeNum - GantryCommonConfig.TubeCount :bTubeNum;

            if (input.HeatCaps[aTubeNum] <= heatCapThreshold && input.HeatCaps[bTubeNum] <= heatCapThreshold)
            {
                resultList.Add((aTubeNum,bTubeNum));
            }
        }
        return resultList;
    }

    /// <summary>
    /// 获取在当前机架位置下, 距离两个曝光点最近的两个球管，并返回第一个曝光位置的球管及该球管达到曝光位置时的机架角度。
    /// </summary>
    /// <param name="input"></param>
    /// <param name="inputPair"></param>
    /// <returns></returns>
    public (int,int,double) GetPreferDualTubeNumAndNewPosition(GantryControlInput input, int preferTubeCount, double preferHeatCapacity)
    {
        //获取当前球管曝光位置的最近球管与曝光机架位置
        var (tubeNum, gantryPos) = GetNearestTubeNumAndNewPosition(input.CurrentGantryPos, input.TubePositions[0]);

        //以当前正对范围的球管为中心，向两边获取preferTubeCount内距离最近的在Prefer热容范围内的球管。
        var dualTubeDirection = GetDualTubeDirection(input.TubePositions);
        var offset = GetDualTubeOffset(tubeNum, gantryPos, 1, dualTubeDirection, input.HeatCaps, preferTubeCount, preferHeatCapacity);
        var direction = offset == int.MaxValue ? 0 : 1;
        var antiOffset = GetDualTubeOffset(tubeNum, gantryPos, -1, dualTubeDirection, input.HeatCaps, preferTubeCount, preferHeatCapacity);
        if (antiOffset < offset)
        {
            offset = antiOffset;
            direction = -1;
        }

        if (direction == 0)
        {
            return (-1,-1, double.NaN);
        }

        var firstTube = CorrectTubeNum(tubeNum + offset * direction);
        var secondTube = CorrectTubeNum(firstTube + GantryCommonConfig.TubeCount / 4 * dualTubeDirection);
        var newGantryPos = gantryPos + offset * direction * GantryCommonConfig.TubeInterval;

        return (firstTube, secondTube, newGantryPos);
    }

    private int GetDualTubeOffset(int tubeNum, double gantryPos,int offsetDirection, int dualTubeDirection, double[] heatCpas, int preferTubeCount, double preferHeatCapacity)
    {
        for (int i = 0; i < preferTubeCount; i++)
        {
            if ((gantryPos + i * GantryCommonConfig.TubeInterval * offsetDirection) > GantryCommonConfig.MaxGantryPos
                || (gantryPos + i * GantryCommonConfig.TubeInterval * offsetDirection) < GantryCommonConfig.MinGantryPos)
            {
                break;
            }
            var firstTubeNum = CorrectTubeNum(tubeNum + i * offsetDirection);
            var secondTubeNum = CorrectTubeNum(firstTubeNum + GantryCommonConfig.TubeCount / 4 * dualTubeDirection);

            if (heatCpas[firstTubeNum] < preferHeatCapacity && heatCpas[secondTubeNum] < preferHeatCapacity)
            {
                return i;
            }
        }
        return int.MaxValue;
    }

    
    private int GetDualTubeDirection(TubePosition[] tubePositions)
    {
        //判断DualTopo的球管顺序方向。
        //若第二个球管为第一个球管顺时针旋转1/4圈，则为-1.
        //若第二个球管为第一个球管逆时针旋转1/4圈，则为1。

        if (tubePositions[1] - tubePositions[0] == GantryCommonConfig.GantryCycleAngle / 400 
            || tubePositions[1] - tubePositions[0] == - GantryCommonConfig.GantryCycleAngle / 400 * 3) 
        {
                return -1;
        }
        return 1;
    }
}
