//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;
using Autofac.Features.OwnedInstances;
using Microsoft.VisualBasic;
using NV.CT.ErrorCodes;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Extension;
using NV.MPS.UI.Dialog.Helper;
using NV.MPS.UI.Dialog.Model;
using NV.MPS.UI.Dialog.Service;
using System;
using System.ComponentModel;
using System.Security.Policy;
using System.Windows;
using System.Windows.Interop;

namespace NV.MPS.UI.Dialog;

public class DialogService : IDialogService
{
    public DialogService(ILifetimeScope lifetimeScope)
    {
        scope = lifetimeScope;
    }

    private readonly ILifetimeScope scope;

    public void Show(bool confirm, MessageLeveles messageLevel, string title, string message, Action<IDialogResult>? callback, IntPtr owner)
    {
        IDialogParameters parameters = new DialogParameters();
        parameters.Add("MessageLevel", messageLevel);
        parameters.Add("Title", title);
        parameters.Add("Message", message);
        if (confirm)
        {
            ShowDialogInternal(DialogManager.ConfirmDialog, parameters, callback, false, owner);
        }
        else
        {
            ShowDialogInternal(DialogManager.InfoDialog, parameters, callback, false, owner);
        }
    }

    public void Show(bool confirm, MessageLeveles messageLevel, string title, string message, Action<IDialogResult>? callback, string windowName, IntPtr owner)
    {
        IDialogParameters parameters = new DialogParameters();
        parameters.Add("MessageLevel", messageLevel);
        parameters.Add("Title", title);
        parameters.Add("Message", message);
        if (confirm)
        {
            ShowDialogInternal(DialogManager.ConfirmDialog, parameters, callback, false, owner, windowName);
        }
        else
        {
            ShowDialogInternal(DialogManager.InfoDialog, parameters, callback, false, owner, windowName);
        }
    }

    public void ShowDialog(bool confirm, MessageLeveles messageLevel, string title, string message, Action<IDialogResult>? callback, IntPtr owner)
    {
        IDialogParameters parameters = new DialogParameters();
        parameters.Add("MessageLevel", messageLevel);
        parameters.Add("Title", title);
        parameters.Add("Message", message);
        if (confirm)
        {
            ShowDialogInternal(DialogManager.ConfirmDialog, parameters, callback, true, owner);
        }
        else
        {
            ShowDialogInternal(DialogManager.InfoDialog, parameters, callback, true, owner);
        }
    }

    public void ShowDialog(bool confirm, MessageLeveles messageLevel, string title, string message, Action<IDialogResult>? callback, string windowName, IntPtr owner)
    {
        IDialogParameters parameters = new DialogParameters();
        parameters.Add("MessageLevel", messageLevel);
        parameters.Add("Title", title);
        parameters.Add("Message", message);
        if (confirm)
        {
            ShowDialogInternal(DialogManager.ConfirmDialog, parameters, callback, true, owner, windowName);
        }
        else
        {
            ShowDialogInternal(DialogManager.InfoDialog, parameters, callback, true, owner, windowName);
        }
    }

    private void Show(string name, IDialogParameters parameters, Action<IDialogResult>? callback, IntPtr owner)
    {
        ShowDialogInternal(name, parameters, callback, false, owner);
    }

    private void Show(string name, IDialogParameters parameters, Action<IDialogResult>? callback, string windowName, IntPtr owner)
    {
        ShowDialogInternal(name, parameters, callback, false, owner, windowName);
    }

    private void ShowDialog(string name, IDialogParameters parameters, Action<IDialogResult>? callback, IntPtr owner)
    {
        ShowDialogInternal(name, parameters, callback, true, owner);
    }

    private void ShowDialog(string name, IDialogParameters parameters, Action<IDialogResult>? callback, string windowName, IntPtr owner)
    {
        ShowDialogInternal(name, parameters, callback, true, owner, windowName);
    }

    void ShowDialogInternal(string name, IDialogParameters parameters, Action<IDialogResult>? callback, bool isModal, IntPtr owner, string windowName = "")
    {
        parameters ??= new DialogParameters();

        IDialogWindow dialogWindow = CreateDialogWindow(windowName);
        var wih = new WindowInteropHelper(dialogWindow as Window);
        wih.Owner = owner;

        ConfigureDialogWindowEvents(dialogWindow, callback);
        ConfigureDialogWindowContent(name, dialogWindow, parameters);

        ShowDialogWindow(dialogWindow, isModal);
    }

    protected virtual void ShowDialogWindow(IDialogWindow dialogWindow, bool isModal)
    {
        if (isModal)
        {          
            dialogWindow.ShowDialog();
        }
        else
        {          
            dialogWindow.Show();
        }
    }

    /// <summary>
    /// 配置对话窗口的内容
    /// </summary>
    /// <param name="dialogName">Dialog传入的名称</param>
    /// <param name="window">传入的window</param>
    /// <param name="parameters">传入的参数</param>
    /// <exception cref="NullReferenceException"></exception>
    protected virtual void ConfigureDialogWindowContent(string dialogName, IDialogWindow window, IDialogParameters parameters)
    {
        //通过Autofac解析指定名称的服务
        var content = scope.ResolveNamed<object>(dialogName);

        bool isFrameworkElement = content is FrameworkElement;
        if (content is not FrameworkElement dialogContent)
            throw new NullReferenceException("A dialog's content must be a FrameworkElement");

        if (dialogContent.DataContext is not IDialogAware viewModel)
            throw new NullReferenceException("A dialog's ViewModel must implement the IDialogAware interface");

        ConfigureDialogWindowProperties(window, dialogContent, viewModel);

        MvvmHelpers.ViewAndViewModelAction<IDialogAware>(viewModel, d => d.OnDialogOpened(parameters));
    }

    protected virtual IDialogWindow CreateDialogWindow(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            var window = scope.Resolve<IDialogWindow>();
            return window;
        }
        else
        {
            return scope.ResolveNamed<IDialogWindow>(name);
        }
    }

    protected virtual void ConfigureDialogWindowEvents(IDialogWindow dialogWindow, Action<IDialogResult>? callback)
    {
        void requestCloseHandler(IDialogResult result)
        {
            dialogWindow.Result = result;
            dialogWindow.Close();
        }

        void loadedHandler(object s, RoutedEventArgs e)
        {
            dialogWindow.Loaded -= loadedHandler;
            dialogWindow.GetDialogViewModel().RequestClose += requestCloseHandler;
        }

        dialogWindow.Loaded += loadedHandler;

        void closingHandler(object? s, CancelEventArgs e)
        {
            if (!dialogWindow.GetDialogViewModel().CanCloseDialog())
                e.Cancel = true;
        }

        dialogWindow.Closing += closingHandler;

        void closedHandler(object? s, EventArgs e)
        {
            dialogWindow.Closed -= closedHandler;
            dialogWindow.Closing -= closingHandler;
            dialogWindow.GetDialogViewModel().RequestClose -= requestCloseHandler;

            dialogWindow.GetDialogViewModel().OnDialogClosed();

            dialogWindow.Result ??= new DialogResult();

            callback?.Invoke(dialogWindow.Result);

            dialogWindow.DataContext = null;
            dialogWindow.Content = null;
        }

        dialogWindow.Closed += closedHandler;
    }

    protected virtual void ConfigureDialogWindowProperties(IDialogWindow window, FrameworkElement dialogContent, IDialogAware viewModel)
    {
        var windowStyle = Dialog.GetWindowStyle(dialogContent);
        if (windowStyle != null)
            window.Style = windowStyle;

        window.Content = dialogContent;
        window.DataContext = viewModel;
    }

    public void ShowContentDialog(string title, object dialogContent, IntPtr owner, Action<IDialogResult>? callBack = null, int width = 550, int height = 350)
    {
        IDialogParameters parameters = new DialogParameters();
        parameters.Add("DialogContent", dialogContent);
        parameters.Add("Title", title);
        parameters.Add("Width", width);
        parameters.Add("Height", height);
        ShowDialogInternal(parameters, false, owner, callBack);
    }

    public void ShowDialogContentDialog(string title, object dialogContent, IntPtr owner, Action<IDialogResult>? callBack = null, int width = 550, int height = 350)
    {
        IDialogParameters parameters = new DialogParameters();
        parameters.Add("DialogContent", dialogContent);
        parameters.Add("Title", title);
        parameters.Add("Width", width);
        parameters.Add("Height", height);
        ShowDialogInternal(parameters, true, owner, callBack);
    }

    void ShowDialogInternal(IDialogParameters parameters, bool isModal, IntPtr owner, Action<IDialogResult>? callBack)
    {
        parameters ??= new DialogParameters();

        IDialogWindow dialogWindow = CreateDialogWindow("");
        var wih = new WindowInteropHelper(dialogWindow as Window);
        wih.Owner = owner;

        ConfigureDialogWindowEvents(dialogWindow, callBack);
        ConfigureDialogWindowContent(DialogManager.ContentDialog, dialogWindow, parameters);

        ShowDialogWindow(dialogWindow, isModal);
    }

    public void ShowErrorDialog(string errorCode, string errorMessage, MessageLeveles messageLevel, IntPtr owner, string solution = "", Action<IDialogResult>? callback = null)
    {
        IDialogParameters parameters = new DialogParameters();
        parameters.Add("ErrorCode", errorCode);
        parameters.Add("ErrorMessage", errorMessage);
        parameters.Add("MessageLevel", messageLevel);
        parameters.Add("Solution", solution);

        ShowErrorDialogInternal(parameters, owner, callback);
    }

    void ShowErrorDialogInternal(IDialogParameters parameters, IntPtr owner, Action<IDialogResult>? callBack)
    {
        parameters ??= new DialogParameters();

        IDialogWindow dialogWindow = CreateDialogWindow("");
        var wih = new WindowInteropHelper(dialogWindow as Window);
        wih.Owner = owner;

        ConfigureDialogWindowEvents(dialogWindow, callBack);
        ConfigureDialogWindowContent(DialogManager.ErrorDialog, dialogWindow, parameters);
        ShowDialogWindow(dialogWindow, true);
    }

    public void ShowErrorDialog(string code, IntPtr owner, Action<IDialogResult>? callback = null, params string[] args)
    {
        IDialogParameters parameters = new DialogParameters();
        ErrorCode errorCode = ErrorCodeHelper.GetErrorCode(code);
        if (errorCode is not null)
        {
            parameters.Add("ErrorCode", errorCode.Code);
            if (errorCode.Description.IndexOf('{') >= 0 && args.Length > 0)
            {
                parameters.Add("ErrorMessage", string.Format(errorCode.Description, args));
            }
            else
            {
                parameters.Add("ErrorMessage", errorCode.Description);
            }
            switch (errorCode.Level)
            {
                case ErrorLevel.Warning:
                    parameters.Add("MessageLevel", MessageLeveles.Warning);
                    break;
                case ErrorLevel.Error:
                case ErrorLevel.Fatal:
                    parameters.Add("MessageLevel", MessageLeveles.Error);
                    break;
                case ErrorLevel.None:
                case ErrorLevel.Information:
                case ErrorLevel.NotDefined:
                default:
                    parameters.Add("MessageLevel", MessageLeveles.Info);
                    break;
            }
            parameters.Add("Solution", errorCode.Solution);
        }
        else
        {
            parameters.Add("MessageLevel", MessageLeveles.Info);
            parameters.Add("ErrorCode", $"Not found({code})");
            parameters.Add("ErrorMessage", $"The error code({code}) is not configured!");
            parameters.Add("Solution", string.Empty);
        }
        ShowErrorDialogInternal(parameters, owner, callback);
    }
}