using Microsoft.Extensions.DependencyInjection;
using NV.CT.ProtocolManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace NV.CT.ProtocolManagement.Views.English;

/// <summary>
/// MainUserControl.xaml 的交互逻辑
/// </summary>
public partial class MainUserControl : UserControl
{
    public MainUserControl()
    {//
        InitializeComponent();
        using (var scope = Global.Instance.ServiceProvider.CreateScope())
        {
            this.DataContext = scope.ServiceProvider.GetRequiredService<MainControlViewModel>();
        }
    }
}
