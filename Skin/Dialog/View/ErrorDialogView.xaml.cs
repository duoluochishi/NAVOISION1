//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;
using NV.MPS.UI.Dialog.Model;
using System.Windows.Controls;

namespace NV.MPS.UI.Dialog.View;

public partial class ErrorDialogView : UserControl
{
    public ErrorDialogView(ILifetimeScope lifetimeScope)
    {
        InitializeComponent();
        this.DataContext = lifetimeScope.Resolve<ErrorDialogViewModel>();
    }
}