//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/28 10:51:02           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Enums;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface IGoValidateDialogService
{
    event EventHandler<EventArgs<(string message, RuleDialogType ruleDialogType, object? extendModel)>> PopValidateMessage;

    event EventHandler<EventArgs<RuleDialogType>> PopValidateMessageWindow;

    void PopValidateMessageChanged(string message, RuleDialogType ruleDialogType, object? extendModel = null);
}