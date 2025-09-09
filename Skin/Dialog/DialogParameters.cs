//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Prism.Common;
using NV.MPS.UI.Dialog.Service;

namespace NV.MPS.UI.Dialog;

public class DialogParameters : ParametersBase, IDialogParameters
{
    public DialogParameters() : base()
    {

    }

    public DialogParameters(string query) : base(query)
    {

    }
}