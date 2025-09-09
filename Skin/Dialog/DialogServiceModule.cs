//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;
using NV.MPS.UI.Dialog.Model;
using NV.MPS.UI.Dialog.Service;
using NV.MPS.UI.Dialog.View;

namespace NV.MPS.UI.Dialog;

public class DialogServiceModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<DialogService>().As<IDialogService>().OwnedByLifetimeScope();
        builder.RegisterType<DialogWindow>().As<IDialogWindow>().AsSelf();
        builder.RegisterType<DialogParameters>().As<IDialogParameters>().SingleInstance();
        builder.RegisterType<DialogResult>().As<IDialogResult>().SingleInstance();
        builder.RegisterType<ConfirmDialogViewModel>().As<IDialogAware>().SingleInstance();
        builder.RegisterType<ConfirmDialogViewModel>().Named<IDialogAware>(DialogManager.ConfirmDialog).SingleInstance();
        builder.RegisterType<ConfirmDialogViewModel>().SingleInstance();
        builder.RegisterType<ConfirmDialogView>().Named<object>(DialogManager.ConfirmDialog).SingleInstance();

        builder.RegisterType<InfoDialogViewModel>().As<IDialogAware>().SingleInstance();
        builder.RegisterType<InfoDialogViewModel>().Named<IDialogAware>(DialogManager.InfoDialog).SingleInstance();
        builder.RegisterType<InfoDialogViewModel>().SingleInstance();
        builder.RegisterType<InfoDialogView>().Named<object>(DialogManager.InfoDialog).SingleInstance();

        builder.RegisterType<ContentDialogViewModel>().As<IDialogAware>().SingleInstance();
        builder.RegisterType<ContentDialogViewModel>().Named<IDialogAware>(DialogManager.ContentDialog).SingleInstance();
        builder.RegisterType<ContentDialogViewModel>().SingleInstance();
        builder.RegisterType<ContentDialogView>().Named<object>(DialogManager.ContentDialog).SingleInstance();

        builder.RegisterType<ErrorDialogViewModel>().As<IDialogAware>().SingleInstance();
        builder.RegisterType<ErrorDialogViewModel>().Named<IDialogAware>(DialogManager.ErrorDialog).SingleInstance();
        builder.RegisterType<ErrorDialogViewModel>().SingleInstance();
        builder.RegisterType<ErrorDialogView>().Named<object>(DialogManager.ErrorDialog).SingleInstance();
    }
}