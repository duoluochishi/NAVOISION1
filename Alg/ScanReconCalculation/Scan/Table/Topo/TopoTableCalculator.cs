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
using NV.MPS.Configuration;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Table.Topo;

/// <summary>
/// 定位像检查床控制参数实现类。
/// 定位像检查床运动参数的计算依赖于以下参数：
/// 协议参数：
///     ScanOption                              扫描选项必须为Surview
///     TableDirection                          进出床方向
///     TableFeed                               扫描进床间隔  
///     CollimatedSliceWidth                    准直后有效层宽（有效探测器宽度）
///     FrameTime                               帧时间
///     PreIgnoredFrames                        前忽略帧数
///     ReconVolumeBeginPos                     计划可重建区域开始位置
///     ReconVolumeEndPos                       计划可重建区域结束位置
///     TableAcc                                检查床加速度
/// 配置参数:
///     TopoTableMoveTollerance                 检查床移动余量定义，保证能够进入匀速段
/// 
/// </summary>
public class TopoTableCalculator : IScanTableCalculator
{

    public static double TopoTableMoveTolleranceWeight = 0.33;

    public static double MinTopoTableMoveTollerance = 0;


    public virtual bool CanAccept(TableControlInput input)
    {
        return input.ScanOption is ScanOption.Surview;
    }

    public virtual TableControlOutput CalculateTableControlInfo(TableControlInput input)
    {
        var result = new TableControlOutput();

        var preOffsetD2V = GetPreOffsetD2V(input);

        var preOffsetT2D = GetPreOffsetT2D(input);
        var postOffsetT2D = GetPostOffsetT2D(input);

        var dataBeginPos = GetDataBeginPos(preOffsetD2V, input);
        var dataEndPos = GetDataEndPos(dataBeginPos, input);
        var tableBeginPos = dataBeginPos + preOffsetT2D;
        var tableEndPos = dataEndPos + postOffsetT2D;

        result.ReconVolumeBeginPos = input.ReconVolumeBeginPos;
        result.ReconVolumeEndPos = input.ReconVolumeEndPos;
        result.DataBeginPos = dataBeginPos;
        result.DataEndPos = dataEndPos;
        result.TableBeginPos = tableBeginPos;
        result.TableEndPos = tableEndPos;

        result.TableSpeed = input.TableFeed / input.FrameTime * IScanTableCalculator.TimeScaleSecToUS;

        result.TableAccTime = result.TableSpeed / input.TableAcc * IScanTableCalculator.TimeScaleSecToUS;
        result.NumOfScan = GetNumOfScans(input, result);

        result.TotalFrames = result.NumOfScan;

        return result;
    }

    protected virtual int GetNumOfScans(TableControlInput input, TableControlOutput output)
    {
        return (int)Math.Round(Math.Abs(output.DataBeginPos - output.DataEndPos) / input.TableFeed) + 1;
    }

    protected virtual double GetDataBeginPos(double offset, TableControlInput input)
    {
        return input.ReconVolumeBeginPos + offset;
    }

    protected virtual double GetDataEndPos(double dataBeginPos, TableControlInput input)
    {
        var factor = CommonCalHelper.GetTableDirectionFactor(input.TableDirection);
        var postOffsetD2V = GetPostOffsetD2V(input);
        return Math.Ceiling(Math.Abs(input.ReconVolumeEndPos + postOffsetD2V - dataBeginPos) / input.TableFeed) * input.TableFeed * factor + dataBeginPos;
    }

    public double GetPreOffsetD2V(TableControlInput input)
    {
        double offset = 0;
        var directionFactor = CommonCalHelper.GetTableDirectionFactor(input.TableDirection);

        //ignored N
        offset += GetIgnoredND2V(input.PreIgnoredFrames, input.TableFeed) * (-1*directionFactor);

        //Offset
        if (input.TableDirection is TableDirection.In)
        {
            offset += GetLargeSideOffsetD2V(input);
        }
        else
        {
            offset += GetSmallSideOffsetD2V(input);
        }
        return offset;
    }

    public double GetPostOffsetD2V(TableControlInput input)
    {
        double offset = 0;
        //Offset
        if (input.TableDirection is TableDirection.In)
        {
            offset += GetSmallSideOffsetD2V(input);
        }
        else
        {
            offset += GetLargeSideOffsetD2V(input);
        }
        return offset;
    }
    public double GetPreOffsetT2D(TableControlInput input)
    {
        var factor = CommonCalHelper.GetTableDirectionFactor(input.TableDirection) * -1;
        return factor * GetTopoAccLength(input);
    }

    public double GetPostOffsetT2D(TableControlInput input)
    {
        var factor = CommonCalHelper.GetTableDirectionFactor(input.TableDirection);
        return factor * GetTopoAccLength(input);
    }

    private double GetTopoAccLength(TableControlInput input)
    {
        var accLength = CommonCalHelper.GetAccelerateLength(GetTopoTableSpeed(input.TableFeed, input.FrameTime), input.TableAcc);
        var tol = accLength * TopoTableMoveTolleranceWeight;
        //加速余量使用1/3理论加速距离。
        //同时定义最小加速余量，若1/3理论加速距离比最小加速余量小，则使用最小加速余量
        tol = tol > MinTopoTableMoveTollerance ? tol : MinTopoTableMoveTollerance;      
        return accLength + tol; 
    }

    public double GetSmallSideOffsetD2V(TableControlInput input)
    {
        return TableCommonConfig.FullSliceWidth / 2;
    }

    public double GetLargeSideOffsetD2V(TableControlInput input)
    {
        return  -input.CollimatedSliceWidth + TableCommonConfig.FullSliceWidth / 2 ;
    }

    public double GetIgnoredND2V(int ignoredN, double topoTableFeed)
    {
        return topoTableFeed * ignoredN;
    }

    public double GetTopoTableSpeed(double tableFeed, double frameTime)
    {
        //时间单位为us，所以需要换算为s

        return tableFeed / frameTime * IScanTableCalculator.TimeScaleSecToUS;
    }

    /// <summary>
    /// 定位扫描不修正
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public double TryCorrectReconVolumnLength(TableControlInput input)
    {
        return input.ReconVolumeEndPos - input.ReconVolumeBeginPos;
    }
    public double TryCorrectReconVolumnLength(TableControlInput input, double newScanLength)
    {
        if (newScanLength < SystemConfig.ScanningParamConfig.ScanningParam.TopoLength.Min)
        {
            return SystemConfig.ScanningParamConfig.ScanningParam.TopoLength.Min;
        }

        if (newScanLength > SystemConfig.ScanningParamConfig.ScanningParam.TopoLength.Max)
        {
            return SystemConfig.ScanningParamConfig.ScanningParam.TopoLength.Max;
        }

        return newScanLength;
    }
}
