//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/31 14:45:22           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.MPS.Configuration;
using System.Text;
using NV.CT.FacadeProxy.Common.Enums;
using NV.MPS.Environment;

namespace NV.CT.Examination.ApplicationService.Impl.ValidateRules;

public class ScanningParameterValidationRule : IGoValidateRule
{
    private readonly IGoValidateDialogService _goValidateDialogService;
    private readonly IProtocolHostService _protocolHostService;

    public ScanningParameterValidationRule(IProtocolHostService protocolHostService, IGoValidateDialogService goValidateDialogService)
    {
        _protocolHostService = protocolHostService;
        _goValidateDialogService = goValidateDialogService;
    }

    public void ValidateGo()
    {
        var activeItem = _protocolHostService.Models.FirstOrDefault(item => item.Measurement.Descriptor.Id == _protocolHostService.CurrentMeasurementID);
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var scan in activeItem.Measurement.Children)
        {
            if (scan.Status == PerformStatus.Unperform)
            {
                stringBuilder.Append(IsCondition(scan));
            }
        }
        if (stringBuilder.Capacity > 0)
        {
            _goValidateDialogService.PopValidateMessageChanged(stringBuilder.ToString(), RuleDialogType.CommonNotificationDialog);
        }
    }

    private string IsCondition(ScanModel scanModel)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(ValidateFov(scanModel.Kilovolt, scanModel.ScanOption));
        stringBuilder.Append(ValidateTubeCurrent(scanModel.Milliampere, scanModel.ScanOption));
        if (scanModel.ScanOption == ScanOption.Surview || scanModel.ScanOption == ScanOption.DualScout)
        {
            stringBuilder.Append(ValidateTopoScanLength(scanModel.ScanLength));
        }
        if (scanModel.ScanOption == ScanOption.Axial
            || scanModel.ScanOption == ScanOption.TestBolus
            || scanModel.ScanOption == ScanOption.NVTestBolus
            || scanModel.ScanOption == ScanOption.NVTestBolusBase
            || scanModel.ScanOption == ScanOption.BolusTracking)
        {
            stringBuilder.Append(ValidateAXIALScanLength(scanModel.ScanLength));
        }
        if (scanModel.ScanOption == ScanOption.Helical)
        {
            stringBuilder.Append(ValidateHELICALScanLength(scanModel.ScanLength));
        }
        stringBuilder.Append(ValidatePitch((int)scanModel.Pitch));
        stringBuilder.Append(ValidateScanFOV((int)scanModel.ScanFOV));
        return stringBuilder.ToString();
    }

    private string ValidateFov(uint[] fov, ScanOption scanOption)
    {
        StringBuilder stringBuilder = new StringBuilder();
        ScanningParamInfo node = SystemConfig.ScanningParamConfig.ScanningParam;

        if (node.AvailableVoltages.Min > fov[0])
        {
            stringBuilder.Append(string.Format($"Kilovolt cannot be smaller than {UnitConvert.ReduceThousand((double)node.AvailableVoltages.Min)};"));
        }
        if (node.AvailableVoltages.Max < fov[0])
        {
            stringBuilder.Append(string.Format($"Kilovolt cannot be greater  than {UnitConvert.ReduceThousand((double)node.AvailableVoltages.Max)};"));
        }

        if (scanOption == ScanOption.DualScout)
        {
            if (node.AvailableVoltages.Min > fov[1])
            {
                stringBuilder.Append(string.Format($"Kilovolt cannot be smaller than {UnitConvert.ReduceThousand((double)node.AvailableVoltages.Min)};"));
            }
            if (node.AvailableVoltages.Max < fov[1])
            {
                stringBuilder.Append(string.Format($"Kilovolt cannot be greater  than {UnitConvert.ReduceThousand((double)node.AvailableVoltages.Max)};"));
            }
        }
        return stringBuilder.ToString();
    }

    private string ValidateTubeCurrent(uint[] current, ScanOption scanOption)
    {
        StringBuilder stringBuilder = new StringBuilder();
        ScanningParamInfo node = SystemConfig.ScanningParamConfig.ScanningParam;

        if (UnitConvert.ReduceThousand((double)node.TubeCurrent.Min) > current[0])
        {
            stringBuilder.Append(string.Format($"Milliampere cannot be smaller than {UnitConvert.ReduceThousand((double)node.TubeCurrent.Min)};"));
        }
        if (UnitConvert.ReduceThousand((double)node.TubeCurrent.Max) < current[0])
        {
            stringBuilder.Append(string.Format($"Milliampere cannot be greater  than {UnitConvert.ReduceThousand((double)node.TubeCurrent.Max)};"));
        }
        if (scanOption == ScanOption.DualScout)
        {
            if (UnitConvert.ReduceThousand((double)node.TubeCurrent.Min) > current[1])
            {
                stringBuilder.Append(string.Format($"Milliampere cannot be smaller than {UnitConvert.ReduceThousand((double)node.TubeCurrent.Min)};"));
            }
            if (UnitConvert.ReduceThousand((double)node.TubeCurrent.Max) < current[1])
            {
                stringBuilder.Append(string.Format($"Milliampere cannot be greater  than {UnitConvert.ReduceThousand((double)node.TubeCurrent.Max)};"));
            }
        }
        return stringBuilder.ToString();
    }

    private string ValidateTopoScanLength(uint scanLength)
    {
        StringBuilder stringBuilder = new StringBuilder();
        ScanningParamInfo node = SystemConfig.ScanningParamConfig.ScanningParam;

        if (node.TopoLength.Min > scanLength)
        {
            stringBuilder.Append(string.Format($"Topo scan length cannot be smaller than {UnitConvert.Micron2Millimeter((double)node.TopoLength.Min)};"));
        }
        if (node.TopoLength.Max < scanLength)
        {
            stringBuilder.Append(string.Format($"Topo scan length cannot be greater  than {UnitConvert.Micron2Millimeter((double)node.TopoLength.Max)};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateAXIALScanLength(uint scanLength)
    {
        StringBuilder stringBuilder = new StringBuilder();
        ScanningParamInfo node = SystemConfig.ScanningParamConfig.ScanningParam;

        if (node.AxialLength.Min > scanLength)
        {
            stringBuilder.Append(string.Format($"Axial scan length cannot be smaller than {UnitConvert.Micron2Millimeter((double)node.AxialLength.Min)};"));
        }
        if (node.AxialLength.Max < scanLength)
        {
            stringBuilder.Append(string.Format($"Axial scan length cannot be greater  than {UnitConvert.Micron2Millimeter((double)node.AxialLength.Max)};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateHELICALScanLength(uint scanLength)
    {
        StringBuilder stringBuilder = new StringBuilder();
        ScanningParamInfo node = SystemConfig.ScanningParamConfig.ScanningParam;

        if (node.SpiralLength.Min > scanLength)
        {
            stringBuilder.Append(string.Format($"Spiral scan length cannot be smaller than {UnitConvert.Micron2Millimeter((double)node.SpiralLength.Min)};"));
        }
        if (node.SpiralLength.Max < scanLength)
        {
            stringBuilder.Append(string.Format($"Spiral scan length cannot be greater  than {UnitConvert.Micron2Millimeter((double)node.SpiralLength.Max)};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidatePitch(int pitch)
    {
        StringBuilder stringBuilder = new StringBuilder();
        ScanningParamInfo node = SystemConfig.ScanningParamConfig.ScanningParam;
        if (node.Pitch.Max < pitch)
        {
            stringBuilder.Append(string.Format($"Pitch greater than maximum: {pitch};"));
        }
        if (node.Pitch.Min > pitch)
        {
            stringBuilder.Append(string.Format($"Pitch less than the minimum: {pitch};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateScanFOV(int canFOV)
    {
        StringBuilder stringBuilder = new StringBuilder();
        ScanningParamInfo node = SystemConfig.ScanningParamConfig.ScanningParam;
        if (!node.ScanFOV.Ranges.Contains(canFOV))
        {
            stringBuilder.Append(string.Format($"Scan FOV unavailable: {canFOV};"));
        }
        return stringBuilder.ToString();
    }
}