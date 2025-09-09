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
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Configuration;

namespace NV.CT.Examination.ApplicationService.Impl.ValidateRules;

public class HeatCapacityRule : IGoValidateRule
{
    private readonly IHeatCapacityService _heatCapacityService;
    private readonly IGoValidateDialogService _goValidateDialogService;
    public HeatCapacityRule(IHeatCapacityService heatCapacityService, IGoValidateDialogService goValidateDialogService)
    {
        _heatCapacityService = heatCapacityService;
        _goValidateDialogService = goValidateDialogService;
    }

    public void ValidateGo()
    {
        var message = string.Empty;
        if (!IsCondition(out message))
        {
            _goValidateDialogService.PopValidateMessageChanged(message, RuleDialogType.CommonNotificationDialog);
        }
    }

    private bool IsCondition(out string message)
    {
        bool condition = true;
        double min = SystemConfig.SourceComponentConfig.SourceComponent.PreheatCapThreshold.Value;
        double max = SystemConfig.SourceComponentConfig.SourceComponent.AlertHeatCapThreshold.Value;
        message = string.Empty;
        if (_heatCapacityService.MaxHeatCapacity > max)
        {
            condition = false;
            message = "The heat capacity is too high!";
        }
        if (_heatCapacityService.MinHeatCapacity < min)
        {
            condition = false;
            message += "The heat capacity is too lower!";
        }
        return condition;
    }
}