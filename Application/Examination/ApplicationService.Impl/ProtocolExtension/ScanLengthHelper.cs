//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/10/16 13:28:29           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.Alg.ScanReconCalculation.Scan.Table;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.UI.Exam.Extensions;

public static class ScanLengthHelper
{
   public static string errorMessage = string.Empty;

    public static void GetCorrectedScanLength(IProtocolHostService protocolHostService, MeasurementModel measurement)
    {
        foreach (var item in measurement.Children)
        {
            GetCorrectedScanLength(protocolHostService, item);
        }
    }

    public static void GetCorrectedScanLength(IProtocolHostService protocolHostService, ScanModel scanModel)
    {
        if (scanModel is not null && scanModel.Status == PerformStatus.Unperform)
        {
            GetCorrectedScanLength(protocolHostService, scanModel, UnitConvert.Micron2Millimeter(scanModel.ScanLength));
        }
    }

    public static void GetCorrectedScanLength(IProtocolHostService protocolHostService, ScanModel scanModel, double ScanLength)
    {
        TableControlInput input = new TableControlInput(scanModel.ScanOption, scanModel.ScanMode, scanModel.TableDirection,
            scanModel.ReconVolumeStartPosition, scanModel.ReconVolumeEndPosition, scanModel.FrameTime, (int)scanModel.FramesPerCycle,
            (int)scanModel.AutoDeleteNum, scanModel.CollimatorSliceWidth, scanModel.TableFeed, scanModel.TableAcceleration, (int)scanModel.CollimatorZ, scanModel.ObjectFOV, scanModel.ExposureTime, UnitConvert.ReduceHundred((float)scanModel.Pitch));


        var length = ScanTableCalculator.Instance.TryCorrectReconVolumnLength(input, UnitConvert.Millimeter2Micron(ScanLength));
        protocolHostService.SetParameter(scanModel, ProtocolParameterNames.SCAN_LENGTH, (int)length);
        //if (scanModel is not null && scanModel.Status == PerformStatus.Unperform)
        //{
        //    double length = UnitConvert.Millimeter2Micron(ScanLength);
        //    var collimatedSW = (int)scanModel.CollimatorZ * 165;
        //    switch (scanModel.ScanOption)
        //    {
        //        case ScanOption.Surview:
        //            protocolHostService.SetParameter(scanModel, ProtocolParameterNames.SCAN_LENGTH, ScanLengthCorrectionHelper.GetCorrectedSurviewScanLength((int)length, SurviewActuralSize: collimatedSW));
        //            break;
        //        case ScanOption.Helical:
        //            protocolHostService.SetParameter(scanModel, ProtocolParameterNames.SCAN_LENGTH, ScanLengthCorrectionHelper.GetCorrectedHelicalScanLength((int)length, scanModel.Children[0].SliceThickness));
        //            break;
        //        case ScanOption.Axial:
        //            protocolHostService.SetParameter(scanModel, ProtocolParameterNames.SCAN_LENGTH, ScanLengthCorrectionHelper.GetCorrectedAxialScanLength((int)length, (int)scanModel.TableFeed, collimatedSW: collimatedSW));
        //            break;
        //        case ScanOption.NVTestBolusBase:
        //            protocolHostService.SetParameter(scanModel, ProtocolParameterNames.SCAN_LENGTH, ScanLengthCorrectionHelper.GetCorrectedNVTestBolusBaseScanLength((int)length));
        //            break;
        //        case ScanOption.NVTestBolus:
        //            protocolHostService.SetParameter(scanModel, ProtocolParameterNames.SCAN_LENGTH, ScanLengthCorrectionHelper.GetCorrectedNVTestBolusScanLength((int)length));
        //            break;
        //        default:
        //            protocolHostService.SetParameter(scanModel, ProtocolParameterNames.SCAN_LENGTH, ScanLengthCorrectionHelper.GetCorrectedSurviewScanLength((int)length, SurviewActuralSize: collimatedSW));
        //            break;
        //    }
        //}
    }

    public static int GetCorrectedScanLength(ScanModel scanModel, int sLength)
    {
        TableControlInput input = new TableControlInput(scanModel.ScanOption, scanModel.ScanMode, scanModel.TableDirection,
            scanModel.ReconVolumeStartPosition, scanModel.ReconVolumeEndPosition, scanModel.FrameTime, (int)scanModel.FramesPerCycle,
            (int)scanModel.AutoDeleteNum, scanModel.CollimatorSliceWidth, scanModel.TableFeed, scanModel.TableAcceleration, (int)scanModel.CollimatorZ, scanModel.ObjectFOV, scanModel.ExposureTime, UnitConvert.ReduceHundred((float)scanModel.Pitch));


        return (int)ScanTableCalculator.Instance.TryCorrectReconVolumnLength(input, UnitConvert.Millimeter2Micron(sLength));
    }

    public static bool GetCorrectedScanLengthMaxMin(IHeatCapacityService heatCapacityService, IMapper mapper, MeasurementModel measurement)
    {
        bool flag = true;
        foreach (ScanModel scanModel in measurement.Children)
        {
            switch (scanModel.ScanOption)
            {
                case ScanOption.Surview:
                case ScanOption.DualScout:
                    flag &= GetSurviewScanLengthMaxMin(scanModel);
                    break;
                case ScanOption.Axial:
                    flag &= GetAxialScanLengthMaxMin(heatCapacityService, mapper, scanModel);
                    break;
                case ScanOption.Helical:
                    flag &= GetHelicalScanLengthMaxMin(scanModel);
                    break;
                default:
                    flag &= true;
                    break;
            }
            if (!flag)
            {
                break;
            }
        }
        return flag;
    }

    private static bool GetSurviewScanLengthMaxMin(ScanModel scanModel)
    {
        bool flag = true;
        if (scanModel.Status == PerformStatus.Unperform
            && (scanModel.ScanOption == ScanOption.Surview || scanModel.ScanOption == ScanOption.DualScout))
        {
            var node = SystemConfig.ScanningParamConfig.ScanningParam.TopoLength;
            int min = node.Min;
            int max = node.Max;
            int scanLength = (int)scanModel.ScanLength;
            if (min > scanLength || max < scanLength)
            {
                flag = false;
            }
            if (min > scanLength)
            {
                errorMessage = $"Scan({scanModel.Descriptor.Name}) length too short!";
            }
            if (max < scanLength)
            {
                errorMessage = $"Scan({scanModel.Descriptor.Name}) length too long!";
            }
        }
        return flag;
    }

    private static bool GetAxialScanLengthMaxMin(IHeatCapacityService heatCapacityService, IMapper mapper, ScanModel scanModel)
    {
        bool flag = true;
        if (scanModel.Status == PerformStatus.Unperform
            && scanModel.ScanOption == ScanOption.Axial)
        {
            TableControlOutput tableControlOutput = GetTableControlOutput(heatCapacityService, mapper, scanModel);
            int min = (int)(scanModel.CollimatorSliceWidth - tableControlOutput.PreDeleteLength - tableControlOutput.PostDeleteLength);
            int max = (int)(scanModel.CollimatorSliceWidth - tableControlOutput.PreDeleteLength - tableControlOutput.PostDeleteLength + 10 * scanModel.TableFeed);
            int scanLength = (int)scanModel.ScanLength;
            if (min > scanLength || max < scanLength)
            {
                flag = false;
            }
            if (min > scanLength)
            {
                errorMessage = $"Scan({scanModel.Descriptor.Name}) length too short!";
            }
            if (max < scanLength)
            {
                errorMessage = $"Scan({scanModel.Descriptor.Name}) length too long!";
            }
        }
        return flag;
    }

    private static bool GetHelicalScanLengthMaxMin(ScanModel scanModel)
    {
        bool flag = true;
        if (scanModel.Status == PerformStatus.Unperform
            && scanModel.ScanOption == ScanOption.Helical)
        {
            int min = (int)(scanModel.CollimatorSliceWidth);
            int max = (int)(scanModel.CollimatorSliceWidth + 25 * scanModel.Pitch / 100 * scanModel.CollimatorSliceWidth);
            int scanLength = (int)scanModel.ScanLength;
            if (min > scanLength || max < scanLength)
            {
                flag = false;
            }
            if (min > scanLength)
            {
                errorMessage = $"Scan({scanModel.Descriptor.Name}) length too short!";
            }
            if (max < scanLength)
            {
                errorMessage = $"Scan({scanModel.Descriptor.Name}) length too long!";
            }
        }
        return flag;
    }

    private static TableControlOutput GetTableControlOutput(IHeatCapacityService heatCapacityService, IMapper mapper, ScanModel scanModel)
    {
        TableControlInput tableControlInput = new TableControlInput();
        tableControlInput.ScanMode = scanModel.ScanMode;
        tableControlInput.ScanOption = scanModel.ScanOption;
        tableControlInput.Pitch = UnitConvert.ReduceHundred((float)scanModel.Pitch);
        tableControlInput.FramesPerCycle = (int)scanModel.FramesPerCycle;
        tableControlInput.CollimatedSliceWidth = scanModel.CollimatorSliceWidth;
        tableControlInput.ExposureTime = scanModel.ExposureTime;
        tableControlInput.ExpSourceCount = (int)scanModel.ExposureMode;
        tableControlInput.FrameTime = scanModel.FrameTime;
        tableControlInput.PreIgnoredFrames = (int)scanModel.AutoDeleteNum;
        tableControlInput.ReconVolumeBeginPos = scanModel.ReconVolumeStartPosition;
        tableControlInput.ReconVolumeEndPos = scanModel.ReconVolumeEndPosition;
        tableControlInput.TableAcc = scanModel.TableAcceleration;
        tableControlInput.TableFeed = scanModel.TableFeed;
        tableControlInput.TableDirection = scanModel.TableDirection;
        tableControlInput.TotalSourceCount = heatCapacityService.Current.Count;
        tableControlInput.Loops = scanModel.Loops;
        tableControlInput.PreDeleteRatio = 1;
        tableControlInput.PostDeleteLegnth = scanModel.PostDeleteLength;
        tableControlInput.ObjectFov = scanModel.ObjectFOV;
        tableControlInput.BodyPart = mapper.Map<FacadeProxy.Common.Enums.BodyPart>(scanModel.BodyPart);
        tableControlInput.CollimatorZ = (int)scanModel.CollimatorZ;
        TableControlOutput tableControlOutput = ScanTableCalculator.Instance.CalculateTableControlInfo(tableControlInput);
        return tableControlOutput;
    }
}