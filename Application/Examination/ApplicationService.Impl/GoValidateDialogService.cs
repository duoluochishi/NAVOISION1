//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/28 9:48:00           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS;

namespace NV.CT.Examination.ApplicationService.Impl;

public class GoValidateDialogService : IGoValidateDialogService
{
    public event EventHandler<EventArgs<(string message, RuleDialogType ruleDialogType, object? extendModel)>>? PopValidateMessage;

    public event EventHandler<EventArgs<RuleDialogType>>? PopValidateMessageWindow;

    public void PopValidateMessageChanged(string message, RuleDialogType ruleDialogType, object? extendModel = null)
    {
        if (!string.IsNullOrEmpty(message))
        {
            PopValidateMessage?.Invoke(this, new EventArgs<(string message, RuleDialogType ruleDialogType, object? extendModel)>((message, ruleDialogType, extendModel)));
            PopValidateMessageWindow?.Invoke(this, new EventArgs<RuleDialogType>(ruleDialogType));
        }
    }
}