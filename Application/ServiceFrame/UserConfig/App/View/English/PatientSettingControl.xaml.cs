using Microsoft.Extensions.DependencyInjection;
using NV.CT.ServiceFramework.Contract;
using NV.CT.UserConfig.ViewModel;
using System;
using System.Windows.Controls;

namespace NV.CT.UserConfig.View.English;
/// <summary>
/// PatientSettingControl.xaml 的交互逻辑
/// </summary>
public partial class PatientSettingControl : UserControl, IServiceControl
{
    public PatientSettingControl()
    {
        InitializeComponent();
        this.DataContext = ServiceFramework.Global.Instance.ServiceProvider?.GetRequiredService<PatientSettingViewModel>();
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