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
using NV.MPS.Environment;
using System.Text;

namespace NV.CT.Examination.ApplicationService.Impl.ValidateRules;

public class GantryParameterValidationRule : IGoValidateRule
{
    private readonly IGoValidateDialogService _goValidateDialogService;
    private readonly IProtocolHostService _protocolHostService;

    public GantryParameterValidationRule(IProtocolHostService protocolHostService,
        IGoValidateDialogService goValidateDialogService)
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
        stringBuilder.Append(ValidateGantryStartPosition(scanModel.GantryStartPosition));
        stringBuilder.Append(ValidateGantryEndPosition(scanModel.GantryEndPosition));
        stringBuilder.Append(ValidateGantrySpeed(scanModel.GantrySpeed));
        return stringBuilder.ToString();
    }

    private string ValidateGantryStartPosition(uint gantryStartPosition)
    {
        StringBuilder stringBuilder = new StringBuilder();
        GantryInfo node = SystemConfig.GantryConfig.Gantry;
        if (node.Angle.Min > gantryStartPosition)
        {
            stringBuilder.Append(string.Format($"Gantry start position cannot be smaller than {(int)UnitConvert.ReduceHundred(node.Angle.Min)};"));
        }
        if (node.Angle.Max < gantryStartPosition)
        {
            stringBuilder.Append(string.Format($"Gantry start position cannot be greater  than {(int)UnitConvert.ReduceHundred(node.Angle.Max)};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateGantryEndPosition(uint gantryEndPosition)
    {
        StringBuilder stringBuilder = new StringBuilder();
        GantryInfo node = SystemConfig.GantryConfig.Gantry;
        if (node.Angle.Min > gantryEndPosition)
        {
            stringBuilder.Append(string.Format($"Gantry end position cannot be smaller than {(int)UnitConvert.ReduceHundred(node.Angle.Min)};"));
        }
        if (node.Angle.Max < gantryEndPosition)
        {
            stringBuilder.Append(string.Format($"Gantry end position cannot be greater  than {(int)UnitConvert.ReduceHundred(node.Angle.Max)};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateGantrySpeed(uint gantrySpeed)
    {
        StringBuilder stringBuilder = new StringBuilder();
        GantryInfo node = SystemConfig.GantryConfig.Gantry;
        if (node.MaxRotationSpeed.Value < gantrySpeed)
        {
            stringBuilder.Append(string.Format($"Gantry Rotation speed cannot be greater than {node.MaxRotationSpeed.Value};"));
        }
        return stringBuilder.ToString();
    }
}