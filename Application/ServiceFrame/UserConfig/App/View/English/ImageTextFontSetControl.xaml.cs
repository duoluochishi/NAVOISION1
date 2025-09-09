using Microsoft.Extensions.DependencyInjection;
using NV.CT.UserConfig.ViewModel;
using System;
using System.Windows.Controls;

namespace NV.CT.UserConfig.View.English;

/// <summary>
/// ImageTextFontSetControl.xaml 的交互逻辑
/// </summary>
public partial class ImageTextFontSetControl : UserControl
{
    public ImageTextFontSetControl()
    {
        InitializeComponent();
        this.DataContext = ServiceFramework.Global.Instance.ServiceProvider?.GetRequiredService<ImageTextFontSetViewModel>();
    }
}