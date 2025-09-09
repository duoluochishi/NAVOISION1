//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Windows;

namespace NV.MPS.UI.Dialog.Service;

public interface IDialogWindow
{  
    object Content { get; set; }

    void Close();   

    void Show();

    bool? ShowDialog();

    object DataContext { get; set; }

    event RoutedEventHandler Loaded;

    event EventHandler Closed;

    event CancelEventHandler Closing;

    IDialogResult Result { get; set; }

    Style Style { get; set; }
}