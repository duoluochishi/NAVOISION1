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

namespace NV.CT.Alg.ScanReconCalculation.Scan.Table.Axial;

/// <summary>
/// 轴扫检查床控制参数实现类。
/// 轴扫检查床运动参数的计算依赖于以下参数：
/// 协议参数：    ///     
///     ScanOption                              扫描选项必须为Axial
///     TableDirection                          进出床方向
///     TableFeed                               扫描进床间隔  
///     CollimatedSliceWidth                    准直后有效层宽（有效探测器宽度）
///     ReconVolumeBeginPos                     计划可重建区域开始位置
///     ReconVolumeEndPos                       计划可重建区域结束位置
/// 配置参数：
/// </summary>
public class AxialTableCalculator : IScanTableCalculator
{

    /// <summary>
    /// 轴扫为多次固定位置曝光，根据TableFeed进行位置定位，定点移动，没有TableSpeed和AccTime定义。
    /// 另：验证ReconVol是否符合轴扫数据要求
    /// 协议参数：
    ///     ScanOption                              扫描选项必须为Axial
    ///     TableDirection                          进出床方向
    ///     TableFeed                               轴扫进床间隔  （当前使用30mm，即300）
    ///     CollimatedSliceWidth                    准直后有效层宽（有效探测器宽度，全开口位256排，42.24mm，即422.4）
    ///     FrameTime                               帧时间			(帧时间10ms，即10000)
    ///     PreIgnoredFrames                        前忽略帧数		（当前使用1）
    ///     ReconVolumeBeginPos                     计划可重建区域开始位置
    ///     ReconVolumeEndPos                       计划可重建区域结束位置
    ///     TableAcc                                检查床加速度	（当前使用200mm/s2,即2000）

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

        result.TableSpeed = TableCommonConfig.MaxTableSpeed;
        result.TableAccTime = TableCommonConfig.MaxTableAccelation;

        result.NumOfScan = (int)Math.Round(Math.Abs(result.DataBeginPos - result.DataEndPos) / input.TableFeed) + 1;

        result.TotalFrames = (input.FramesPerCycle + input.PreIgnoredFrames) / input.ExpSourceCount;

        result.PreDeleteLength = CommonCalHelper.GetAxialPreDeleteLength(input.CollimatorZ);
        result.PostDeleteLength = input.PostDeleteLegnth == 0 ? CommonCalHelper.GetAxialPostDeleteLength(input.CollimatorZ) : input.PostDeleteLegnth;

        result.SmallAngleDeleteLength = result.PreDeleteLength;
        result.LargeAngleDeleteLength = result.PostDeleteLength;        
        return result;
    }

    private bool ValidateBeginEnd(double begin,double end,double feed)
    {
        return CommonCalHelper.IsDoubleMod(begin-end,feed);
    }


    public bool CanAccept(TableControlInput input)
    {
        return input.ScanOption is ScanOption.Axial;
    }

    public double GetPreOffsetD2V(TableControlInput input)
    {
        double offset = 0;

        //Offset
        if (input.TableDirection is TableDirection.In)
        {
            offset += GetLargeSideOffsetD2V(input) + CommonCalHelper.GetAxialPostDeleteLength(input.CollimatorZ);
        }
        else
        {
            offset += GetSmallSideOffsetD2V(input) - CommonCalHelper.GetAxialPreDeleteLength(input.CollimatorZ);
        }
        return offset;
    }

    public double GetPostOffsetD2V(TableControlInput input)
    {
        double offset = 0;

        //Offset
        if (input.TableDirection is TableDirection.In)
        {
            offset += GetSmallSideOffsetD2V(input) - CommonCalHelper.GetAxialPreDeleteLength(input.CollimatorZ);
        }
        else
        {
            offset += GetLargeSideOffsetD2V(input) + CommonCalHelper.GetAxialPostDeleteLength(input.CollimatorZ);
        }

        return offset;

    }


    /// <summary>
    /// No offset from Data to table. return 0;
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public double GetPreOffsetT2D(TableControlInput input)
    {
        return 0;
    }


    /// <summary>
    /// No offset from Data to table. return 0;
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public double GetPostOffsetT2D(TableControlInput input)
    {
        return 0;
    }


    public double GetLargeSideOffsetD2V(TableControlInput input)
    {
        return -input.CollimatedSliceWidth + TableCommonConfig.FullSliceWidth / 2;
    }

    public double GetSmallSideOffsetD2V(TableControlInput input)
    {
        return TableCommonConfig.FullSliceWidth / 2;
    }

    public double TryCorrectReconVolumnLength(TableControlInput input)
    {
        var factor = (input.ReconVolumeEndPos - input.ReconVolumeBeginPos) >= 0 ? 1 : -1;
        var oriLength = Math.Abs(input.ReconVolumeEndPos - input.ReconVolumeBeginPos);
        var singleAxialReconLength = input.CollimatedSliceWidth - CommonCalHelper.GetAxialPreDeleteLength(input.CollimatorZ)
            - CommonCalHelper.GetAxialPostDeleteLength(input.CollimatorZ);
        if(oriLength < singleAxialReconLength)
        {
            return singleAxialReconLength * factor;
        }
        
        var feedNum = Math.Ceiling(Math.Round((oriLength - singleAxialReconLength) / input.TableFeed,1));
        return factor * (feedNum * input.TableFeed + singleAxialReconLength);
    }

    public double TryCorrectReconVolumnLength(TableControlInput input, double newScanLength)
    {
        var singleAxialReconLength = input.CollimatedSliceWidth - CommonCalHelper.GetAxialPreDeleteLength(input.CollimatorZ)
            - CommonCalHelper.GetAxialPostDeleteLength(input.CollimatorZ);
        if (newScanLength < singleAxialReconLength)
        {
            return singleAxialReconLength ;
        }

        var feedNum = Math.Ceiling(Math.Round((newScanLength - singleAxialReconLength) / input.TableFeed, 1));

        //当前限制步进10次，即11圈
        if (feedNum > 10)
        {
            feedNum = 10;
        }

        return feedNum * input.TableFeed + singleAxialReconLength;

    }
}
