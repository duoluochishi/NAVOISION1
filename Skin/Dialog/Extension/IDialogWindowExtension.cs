//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.MPS.UI.Dialog.Service;

namespace NV.MPS.UI.Dialog.Extension;

internal static class IDialogWindowExtension
{
    internal static IDialogAware GetDialogViewModel(this IDialogWindow dialogWindow)
    {
        return (IDialogAware)dialogWindow.DataContext;
    }
}