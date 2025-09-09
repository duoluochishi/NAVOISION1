using NV.CT.Alg.ScanReconCalculation.Scan.Table;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Environment;

namespace NV.CT.Examination.ApplicationService.Impl.ProtocolExtension;
/// <summary>
/// 扫描时间计算类
/// </summary>
public static class ScanTimeHelper
{
    //TODO: 当前计算参数定义为常量，以后从config中获取
    // private const double UnitChangeUmToS = 1000000;         //um到秒
    // private const double DetecterSize = 42200;               //探测器大小,单位0.1mm
    //private const double PitchUintConverter = 100;
    //返回值单位秒。

    private static double TimeTolleranceForAxialFeed = 0.1;
    public static double GetScanTime(ScanModel scanModel)
    {
        //TODO：灌注扫描根据周期时间与循环次数决定。扫描参数更新后添加计算		
        //非灌注扫描。
        switch (scanModel.ScanOption)
        {
            case ScanOption.Surview:
            case ScanOption.DualScout:
                return GetTopoScanTime(scanModel);
            case ScanOption.Axial:
                return GetAxialScanTime(scanModel);
            case ScanOption.Helical:
                return GetHelicScanTime(scanModel);
            case ScanOption.NVTestBolusBase:
                return GetTestBolusBase(scanModel);
            case ScanOption.NVTestBolus:
                return GetTestBolus(scanModel);
            case ScanOption.BolusTracking:
                return GetBolusTracking(scanModel);
            default:
                throw new InvalidOperationException($"不支持的ScanOption{scanModel.ScanOption}");
        }
    }

    private static double GetTopoScanTime(ScanModel scanModel)
    {
        var expLength = Math.Abs(GetExposureLength(scanModel));
        return expLength / scanModel.TableSpeed;
    }

    private static double GetAxialScanTime(ScanModel scanModel)
    {
        //TODO：当前没考虑床动的时间，等待北京确认。
        var frameTime = scanModel.FrameTime;                //单位微妙um
        var frameNumPerCycle = scanModel.FramesPerCycle;
        var cycleTime = UnitConvert.Microsecond2Second(frameTime * frameNumPerCycle);       //单位在此换算为s

        var expLength = Math.Abs(GetExposureLength(scanModel));

        var cycles = Math.Round(expLength / scanModel.TableFeed,2) + 1;

        var feedTime = GetFeedTimeForAxial(scanModel);

        return cycleTime * cycles + feedTime * (cycles - 1);
    }

    private static double GetHelicScanTime(ScanModel scanModel)
    {
        var expLength = Math.Abs(GetExposureLength(scanModel));
        var cycleLength = UnitConvert.ReduceHundred((float)(scanModel.Pitch * scanModel.CollimatorSliceWidth));
        var cycleTime = UnitConvert.Microsecond2Second(scanModel.FrameTime * scanModel.FramesPerCycle);       //单位在此换算为s
        return expLength / cycleLength * cycleTime;
    }

    public static double GetTestBolusBase(ScanModel scanModel)
    {
        return UnitConvert.Microsecond2Second(scanModel.FrameTime * scanModel.FramesPerCycle);
    }

    public static double GetTestBolus(ScanModel scanModel)
    {
        return GetBolusTestScanTime(scanModel);
    }

    public static double GetBolusTracking(ScanModel scanModel)
    {
        return GetHelicScanTime(scanModel);
    }

    private static double GetBolusTestScanTime(ScanModel scanModel)
    {
        var cycleTime = UnitConvert.Microsecond2Second(scanModel.Loops * scanModel.LoopTime);
        return cycleTime;
    }

    /// <summary>
    /// 计算在轴扫过程中相邻两次轴扫步进的移床时间。
    /// </summary>
    /// <param name="scanModel"></param>
    private static double GetFeedTimeForAxial(ScanModel scanModel)
    {
        var feed = (double)scanModel.TableFeed;
        var speed = (double)scanModel.TableSpeed;
        var acc = (double)scanModel.TableAcceleration;

        var accTime = speed / acc;

        var threshold = accTime * speed;

        if (feed >= threshold)      //移床距离足够进行加减速，有匀速移动过程
        {
            return (accTime * 2 + (feed - threshold) / speed) + TimeTolleranceForAxialFeed;
        }
        else
        {
            return (Math.Sqrt(feed / acc) * 2) + TimeTolleranceForAxialFeed;
        }
    }

    private static double GetExposureLength(ScanModel scanModel)
    {
        TableControlInput input = new TableControlInput(scanModel.ScanOption, scanModel.ScanMode, scanModel.TableDirection,
                scanModel.ReconVolumeStartPosition, scanModel.ReconVolumeEndPosition, scanModel.FrameTime, (int)scanModel.FramesPerCycle,
                (int)scanModel.AutoDeleteNum, scanModel.CollimatorSliceWidth, scanModel.TableFeed, scanModel.TableAcceleration, (int)scanModel.CollimatorZ,
                scanModel.ObjectFOV, scanModel.ExposureTime, UnitConvert.ReduceHundred((float)scanModel.Pitch));

        var output = ScanTableCalculator.Instance.CalculateTableControlInfo(input);

        return output.DataBeginPos - output.DataEndPos;
    }
}