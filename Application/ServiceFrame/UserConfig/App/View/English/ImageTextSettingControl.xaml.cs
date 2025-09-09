using Microsoft.Extensions.DependencyInjection;
using NV.CT.ServiceFramework.Contract;
using NV.CT.UserConfig.ViewModel;
using System;
using System.Windows.Controls;

namespace NV.CT.UserConfig.View.English;

/// <summary>
/// ImageTextSettingControl.xaml 的交互逻辑
/// </summary>
public partial class ImageTextSettingControl : UserControl, IServiceControl
{
    public ImageTextSettingControl()
    {        
        InitializeComponent();
        this.DataContext = ServiceFramework.Global.Instance.ServiceProvider?.GetRequiredService<ImageTextSettingViewModel>();
    }

    public string GetServiceAppName()
    {
        return string.Empty;
    }

    public string GetServiceAppID()
    {
        return string.Empty;
    }

    public string GetTipOnClosing()
    {
        return string.Empty;
    }
}