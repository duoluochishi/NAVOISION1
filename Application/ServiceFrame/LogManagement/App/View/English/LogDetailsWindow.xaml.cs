using Microsoft.Extensions.DependencyInjection;
using NV.CT.LogManagement.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace NV.CT.LogManagement.View.English
{
    /// <summary>
    /// LogDetailsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LogDetailsWindow : Window
    {
        public LogDetailsWindow()
        {
            InitializeComponent();

            MouseDown += (_, _) =>
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            DataContext = ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService<LogDetailsWindowViewModel>();
        }
    }
}
