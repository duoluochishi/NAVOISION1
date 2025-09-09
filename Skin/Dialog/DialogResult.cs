//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;

namespace NV.MPS.UI.Dialog;

public class DialogResult : IDialogResult
{
    public DialogResult(ButtonResult result)
    {
        Result = result;
    }
    public DialogResult(ButtonResult result, IDialogParameters parameters)
    {
        Result = result;
        Parameters = parameters;
    }

    public DialogResult()
    {

    }

    public IDialogParameters Parameters { get; private set; } = new DialogParameters();

    public ButtonResult Result { get; private set; } = ButtonResult.None;
}