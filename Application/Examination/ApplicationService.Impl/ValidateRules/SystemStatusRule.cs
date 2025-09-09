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
using System.Text;

namespace NV.CT.Examination.ApplicationService.Impl.ValidateRules;

public class SystemStatusRule : IGoValidateRule
{
    private readonly ISystemReadyService _systemReadyService;
    private readonly IGoValidateDialogService _goValidateDialogService;

    public SystemStatusRule(ISystemReadyService systemReadyService, IGoValidateDialogService goValidateDialogService)
    {
        _systemReadyService = systemReadyService;
        _goValidateDialogService = goValidateDialogService;
    }

    public void ValidateGo()
    {
        if (!IsCondition())
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!_systemReadyService.CurrentConnectionStatus)
            {
                stringBuilder.Append("AcqRecon connection failed!");
            }
            if (!_systemReadyService.CurrentCTBoxStatus)
            {
                stringBuilder.Append("CTBox connection failed!");
            }
            if (!_systemReadyService.CurrentRealtimeStatus)
            {
                stringBuilder.Append("The system is not ready!");
            }
            if (!_systemReadyService.CurrentDoorStatus)
            {
                stringBuilder.Append("The door is not closed!");
            }
            if (!_systemReadyService.CurrentTableStatus)
            {
                stringBuilder.Append("The bed was not ready!");
            }

            if (stringBuilder.Capacity > 0)
            {
                _goValidateDialogService.PopValidateMessageChanged(stringBuilder.ToString(), RuleDialogType.CommonNotificationDialog);
            }
        }
    }

    private bool IsCondition()
    {
        return _systemReadyService.Status;
    }
}