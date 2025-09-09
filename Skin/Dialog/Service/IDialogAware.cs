//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace NV.MPS.UI.Dialog.Service;

public interface IDialogAware
{
    string Title { get; }

    event Action<IDialogResult> RequestClose;

    bool CanCloseDialog();

    void OnDialogClosed();

    void OnDialogOpened(IDialogParameters parameters);
}