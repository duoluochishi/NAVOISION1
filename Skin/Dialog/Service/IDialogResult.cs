//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.MPS.UI.Dialog.Enum;

namespace NV.MPS.UI.Dialog.Service;

public interface IDialogResult
{
    IDialogParameters Parameters { get; }

    ButtonResult Result { get; }
}