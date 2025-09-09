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
using NV.CT.FacadeProxy.Common.Models.PostProcess;
using NV.MPS.Configuration;

namespace NV.CT.Alg.ScanReconCalculation.Scan.Table.Helic;

public class HelicTableCalculator : IScanTableCalculator
{
    public static double HelicPreOffsetWeightD2V = 1;

    public static double HelicPostOffsetWeightD2V = 1;

    public static double HelicTableMoveTolleranceWeight = 0.33;
    public static double MinHelicTableMoveTollerance = 0;

    public static double PostOffsetAcqTime = 0.2;

    /// <summary>
    /// 螺旋扫描检查床控制参数实现类。
    /// 螺旋扫描检查床运动参数的计算依赖于以下参数：
    /// 协议参数：
    ///     ScanOption                          扫描选项必须为Helic
    ///     TableDirection                      进出床方向
    ///     TableAcc                            检查床加速度
    ///     CollimatedSliceWidth                准直后有效层宽（有效探测器宽度）
    ///     Pitch                               Pitch
    ///     FrameTime                           帧时间
    ///     FramesPerCycle                      单圈View数
    ///     ExpSouceCount                       曝光射线源数
    ///     PreIgnoredFrames                    前忽略帧数
    ///     ReconVolumeBeginPos
    ///     ReconVolumeEndPos
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public TableControlOutput CalculateTableControlInfo(TableControlInput input)
    {
        var result = new TableControlOutput();

        var preOffsetD2V = GetPreOffsetD2V(input);
        var postOffsetD2V = GetPostOffsetD2V(input);

        var preOffsetT2D = GetPreOffsetT2D(input);
        var postOffsetT2D = GetPostOffsetT2D(input);

        var dataBeginPos = input.ReconVolumeBeginPos + preOffsetD2V;
        var dataEndPos = input.ReconVolumeEndPos + postOffsetD2V;
        var tableBeginPos = dataBeginPos + preOffsetT2D;
        var tableEndPos = dataEndPos + postOffsetT2D;

        result.ReconVolumeBeginPos = input.ReconVolumeBeginPos;
        result.ReconVolumeEndPos = input.ReconVolumeEndPos;
        result.DataBeginPos = dataBeginPos;
        result.DataEndPos = dataEndPos;
        result.TableBeginPos = tableBeginPos;
        result.TableEndPos = tableEndPos;

        result.TableSpeed = GetHelicTableSpeed(input);
        result.TableEndPos = result.TableEndPos + result.TableSpeed * PostOffsetAcqTime * CommonCalHelper.GetTableDirectionFactor(input.TableDirection);

        result.NumOfScan = 1;

        result.TableAccTime = result.TableSpeed / input.TableAcc * IScanTableCalculator.TimeScaleSecToUS;
        result.TotalFrames = ((int)(Math.Abs(dataBeginPos - dataEndPos) / (input.CollimatedSliceWidth * input.Pitch) * input.FramesPerCycle) + input.PreIgnoredFrames) / input.ExpSourceCount;

        result.PreDeleteLength = CommonCalHelper.GetHelicPreDeleteLength(input.CollimatorZ, input.ObjectFov, input.PreDeleteRatio);
        result.PostDeleteLength = input.PostDeleteLegnth == 0 ?
                CommonCalHelper.GetHelicPostDeleteLength(input.BodyPart, input.CollimatorZ, input.ObjectFov) : input.PostDeleteLegnth;

        result.SmallAngleDeleteLength = input.CollimatedSliceWidth + result.PreDeleteLength;
        result.LargeAngleDeleteLength = input.CollimatedSliceWidth + result.PostDeleteLength;
        
        return result;
    }

    public bool CanAccept(TableControlInput input)
    {
        return input.ScanOption is ScanOption.Helical;
    }

    public double GetPreOffsetD2V(TableControlInput input)
    {
        double offset = 0;

        var directionFactor = -1 * CommonCalHelper.GetTableDirectionFactor(input.TableDirection);

        //Offset
        if (input.TableDirection is TableDirection.In) //进床，开始为大锥角方向
        {
            offset += GetLargeSideOffsetD2V(input);
            var largeDeleteLength = input.PostDeleteLegnth == 0 ?
                CommonCalHelper.GetHelicPostDeleteLength(input.BodyPart, input.CollimatorZ, input.ObjectFov) : input.PostDeleteLegnth;
            offset += largeDeleteLength;
        }
        else                                          //出床，开始为小锥角方向
        {
            offset += GetSmallSideOffsetD2V(input);
            var smallDeleteLength = CommonCalHelper.GetHelicPreDeleteLength(input.CollimatorZ, input.ObjectFov, input.PreDeleteRatio);
            offset -= smallDeleteLength;
        }

        offset += GetIgnoredND2V(input) * directionFactor;
        offset += HelicPreOffsetWeightD2V * input.CollimatedSliceWidth * directionFactor;

        return offset;
    }


    public double GetPostOffsetD2V(TableControlInput input)
    {
        double offset = 0;

        var directionFactor = CommonCalHelper.GetTableDirectionFactor(input.TableDirection);


        if (input.TableDirection is TableDirection.In)      //进床，结束为小锥角方向
        {
            offset += GetSmallSideOffsetD2V(input);
            var smallDeleteLength = CommonCalHelper.GetHelicPreDeleteLength(input.CollimatorZ, input.ObjectFov, input.PreDeleteRatio); 
            
            offset -= smallDeleteLength;
        }
        else                         //出床，结束为大锥角方向
        {
            offset += GetLargeSideOffsetD2V(input);
            var largeDeleteLength = input.PostDeleteLegnth == 0 ?
                CommonCalHelper.GetHelicPostDeleteLength(input.BodyPart, input.CollimatorZ, input.ObjectFov) : input.PostDeleteLegnth;
            offset += largeDeleteLength;
        }

        offset += HelicPostOffsetWeightD2V * input.CollimatedSliceWidth * directionFactor;

        return offset;
    }
    public double GetPreOffsetT2D(TableControlInput input)
    {
        var factor = CommonCalHelper.GetTableDirectionFactor(input.TableDirection) * -1;
        var accLength = GetAccLength(input);
        return accLength * factor;
    }


    public double GetPostOffsetT2D(TableControlInput input)
    {
        var factor = CommonCalHelper.GetTableDirectionFactor(input.TableDirection);
        var accLength = GetAccLength(input);
        return accLength * factor;
    }

    private double GetAccLength(TableControlInput input)
    {
        var accLength = CommonCalHelper.GetAccelerateLength(GetHelicTableSpeed(input), input.TableAcc);
        var tol = accLength * HelicTableMoveTolleranceWeight;
        tol = tol > MinHelicTableMoveTollerance ? tol : MinHelicTableMoveTollerance;
        return accLength + tol;
    }


    public double GetHelicTableSpeed(TableControlInput input)
    {
        var cycleTime = input.FrameTime * input.FramesPerCycle / input.ExpSourceCount / IScanTableCalculator.TimeScaleSecToUS;
        var effectCycleLength = input.CollimatedSliceWidth * input.Pitch;

        return effectCycleLength / cycleTime;
    }

    public double GetLargeSideOffsetD2V(TableControlInput input)
    {
        return -input.CollimatedSliceWidth + TableCommonConfig.FullSliceWidth / 2;
    }

    public double GetSmallSideOffsetD2V(TableControlInput input)
    {
        return TableCommonConfig.FullSliceWidth / 2;
    }

    public double GetIgnoredND2V(TableControlInput input)
    {
        return input.Pitch * input.CollimatedSliceWidth * input.PreIgnoredFrames / input.FramesPerCycle;
    }

    /// <summary>
    /// 螺旋扫描不修正
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public double TryCorrectReconVolumnLength(TableControlInput input)
    {
        return input.ReconVolumeEndPos - input.ReconVolumeBeginPos;
    }
    public double TryCorrectReconVolumnLength(TableControlInput input, double newScanLength)
    {
        var correctedScanLength = newScanLength;
        //首先按照配置文件中的最大最小螺旋长度进行限制
        if (correctedScanLength < SystemConfig.ScanningParamConfig.ScanningParam.SpiralLength.Min)
        {
            correctedScanLength = SystemConfig.ScanningParamConfig.ScanningParam.SpiralLength.Min;
        }
        if (correctedScanLength > SystemConfig.ScanningParamConfig.ScanningParam.SpiralLength.Max)
        {
            correctedScanLength = SystemConfig.ScanningParamConfig.ScanningParam.SpiralLength.Max;
        }

        //最小单次扫描长度为一圈有效数据长度，即CollimatedSliceWidth
        if(correctedScanLength < input.CollimatedSliceWidth)
        {
            correctedScanLength = input.CollimatedSliceWidth;
        }
        //最大单次扫描长度为25圈实际扫描数据长度，考虑pitch
        var maxHelicLength = input.CollimatedSliceWidth * input.Pitch * 25;
        if (correctedScanLength > maxHelicLength)
        {
            correctedScanLength = maxHelicLength;
        }

        return correctedScanLength;
    }
}