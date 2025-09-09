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

namespace NV.CT.Examination.ApplicationService.Impl.ValidateRules;

public class CollimatorParameterValidationRule : IGoValidateRule
{
    private readonly IGoValidateDialogService _goValidateDialogService;
    private readonly IProtocolHostService _protocolHostService;

    public CollimatorParameterValidationRule(IProtocolHostService protocolHostService, IGoValidateDialogService goValidateDialogService)
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
        stringBuilder.Append(ValidateCollimatorZ(scanModel.CollimatorZ));

        return stringBuilder.ToString();
    }

    private string ValidateCollimatorZ(uint collimatorZ)
    {
        StringBuilder stringBuilder = new StringBuilder();
        CollimatorSettingInfo node = SystemConfig.CollimatorConfig.CollimatorSetting;

        if (node.Collimator.Min > collimatorZ)
        {
            stringBuilder.Append(string.Format($"Collimator Z cannot be smaller than {node.Collimator.Min};"));
        }
        if (node.Collimator.Max < collimatorZ)
        {
            stringBuilder.Append(string.Format($"Collimator Z cannot be greater  than {node.Collimator.Max};"));
        }
        return stringBuilder.ToString();
    }
}