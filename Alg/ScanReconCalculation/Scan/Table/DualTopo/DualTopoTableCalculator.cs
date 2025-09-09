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
using NV.CT.Alg.ScanReconCalculation.Scan.Table.Topo;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Table.DualTopo;

/// <summary>
/// 定位像检查床控制参数实现类。
/// 大部分计算过程与定位像类似，额外需要的参数：
/// 协议参数：
///     ExposureTime                            曝光时间
/// </summary>
public class DualTopoTableCalculator:TopoTableCalculator
{

    public static double DualTopoExposureInterval = 10000;   //us

    public override bool CanAccept(TableControlInput input)
    {
        return input.ScanOption is ScanOption.DualScout;
    }

    public override TableControlOutput CalculateTableControlInfo(TableControlInput input)
    {
        var output = base.CalculateTableControlInfo(input);
        output.TotalFrames = output.NumOfScan * 2;   //两个源
        return output;
    }


    protected override int GetNumOfScans(TableControlInput input, TableControlOutput output)
    {
        return (int)Math.Round(Math.Abs(output.DataBeginPos - output.DataEndPos) / input.TableFeed) + 1;
    }

    protected override double GetDataBeginPos(double offset, TableControlInput input)
    {
        var mFactor = -1 * CommonCalHelper.GetTableDirectionFactor(input.TableDirection);
        var dualTopoOffset = GetDualTopoOffset(input) * mFactor;

        return base.GetDataBeginPos(offset,input) + dualTopoOffset;
    }
    protected override double GetDataEndPos(double dataBeginPos, TableControlInput input)
    {
        var factor = 1 * CommonCalHelper.GetTableDirectionFactor(input.TableDirection);
        var dualTopoOffset = GetDualTopoOffset(input) * factor;

        return base.GetDataEndPos(dataBeginPos, input) + dualTopoOffset;
    }

    private double GetDualTopoOffset(TableControlInput input)
    {
        var tableSpeed = GetTopoTableSpeed(input.TableFeed, input.FrameTime);
        //先用input中的exposureTime作为两次曝光间隔。DualTopoExposureInterval保留。
        //因为nvysnc的线性，1# 结束到2# 开始 至少间隔一倍曝光时间，再算上1# 和 2# 的曝光时间，FrameTime要>3倍曝光时间。
        //此时1#开始与2#开始间隔为2倍曝光时间
        return input.ExposureTime * 2 / IScanTableCalculator.TimeScaleSecToUS * tableSpeed;
    }
}
