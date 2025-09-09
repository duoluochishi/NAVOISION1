//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using Prism.Commands;
using Prism.Mvvm;
using System;

namespace NV.MPS.UI.Dialog.Model;

public class ContentDialogViewModel : BindableBase, IDialogAware
{
    private int _width = 550;
    public int Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    private int _height = 350;
    public int Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private object dialogContent = new object();
    public object DialogContent
    {
        get => dialogContent;
        set => SetProperty(ref dialogContent, value);
    }

    public event Action<IDialogResult> RequestClose;

    public DelegateCommand OKCommand { get; set; }

    public DelegateCommand CancelCommand { get; set; }

    public DelegateCommand CloseCommand { get; set; }

    public ContentDialogViewModel()
    {
        CloseCommand = new DelegateCommand(OnDialogClosed);
        OKCommand = new DelegateCommand(OnDialogOK);
        CancelCommand = new DelegateCommand(OnDialogCanceled);
    }

    public void OnDialogOK()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
    }

    public void OnDialogClosed()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Close));
    }

    public void OnDialogCanceled()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters is not null)
        {
            if (parameters.ContainsKey("Width"))
            {
                Width = parameters.GetValue<int>("Width");
            }
            if (parameters.ContainsKey("Height"))
            {
                Height = parameters.GetValue<int>("Height");
            }
            if (parameters.ContainsKey("Title"))
            {
                Title = parameters.GetValue<string>("Title");
            }
            if (parameters.ContainsKey("DialogContent"))
            {
                DialogContent = parameters.GetValue<object>("DialogContent");
            }
        }
    }

    public bool CanCloseDialog()
    {
        return true;
    }
}