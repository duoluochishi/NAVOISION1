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
using System.Windows.Media.Imaging;

namespace NV.MPS.UI.Dialog.Model;

public class ErrorDialogViewModel : BindableBase, IDialogAware
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _solution = string.Empty;
    public string Solution
    {
        get => _solution;
        set => SetProperty(ref _solution, value);
    }

    private string _content = string.Empty;
    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    private BitmapImage _infoImage = new BitmapImage(new Uri("pack://application:,,,/NV.MPS.UI.Dialog;component/Icons/info.png", UriKind.RelativeOrAbsolute));
    public BitmapImage InfoImage
    {
        get => _infoImage;
        set => SetProperty(ref _infoImage, value);
    }

    public MessageLeveles MessageLevel
    {
        set
        {
            switch (value)
            {
                case MessageLeveles.Info:
                    InfoImage = new BitmapImage(new Uri("pack://application:,,,/NV.MPS.UI.Dialog;component/Icons/info.png", UriKind.RelativeOrAbsolute));
                    break;
                case MessageLeveles.Warning:
                    InfoImage = new BitmapImage(new Uri("pack://application:,,,/NV.MPS.UI.Dialog;component/Icons/dosewarning.png", UriKind.RelativeOrAbsolute));
                    break;
                case MessageLeveles.Error:
                    InfoImage = new BitmapImage(new Uri("pack://application:,,,/NV.MPS.UI.Dialog;component/Icons/doseerror.png", UriKind.RelativeOrAbsolute));
                    break;
                default:
                    InfoImage = new BitmapImage(new Uri("pack://application:,,,/NV.MPS.UI.Dialog;component/Icons/info.png", UriKind.RelativeOrAbsolute));
                    break;
            }
        }
    }

    public event Action<IDialogResult> RequestClose;

    public DelegateCommand OKCommand { get; set; }

    public DelegateCommand CloseCommand { get; set; }

    public ErrorDialogViewModel()
    {
        OKCommand = new DelegateCommand(OnDialogOK);
        CloseCommand = new DelegateCommand(OnDialogClosed);
    }

    public bool CanCloseDialog()
    {
        return true;
    }

    public void OnDialogOK()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
    }

    public void OnDialogClosed()
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Close));
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters is not null)
        {
            if (parameters.ContainsKey("ErrorMessage"))
            {
                Content = parameters.GetValue<string>("ErrorMessage");
            }
            if (parameters.ContainsKey("Solution"))
            {
                Solution = parameters.GetValue<string>("Solution");
            }
            if (parameters.ContainsKey("ErrorCode"))
            {
                Title = $"{parameters.GetValue<string>("ErrorCode")}";
            }
            if (parameters.ContainsKey("MessageLevel"))
            {
                MessageLevel = parameters.GetValue<MessageLeveles>("MessageLevel");
            }
        }
    }
}