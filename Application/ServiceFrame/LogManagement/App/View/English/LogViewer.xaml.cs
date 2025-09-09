using Microsoft.Extensions.DependencyInjection;
using NV.CT.LogManagement.ViewModel;
using NV.CT.ServiceFramework.Contract;
using System;
using System.Windows.Controls;

namespace NV.CT.LogManagement.View.English
{
    /// <summary>
    /// LogViewer.xaml 的交互逻辑
    /// </summary>
    public partial class LogViewer : UserControl, IServiceControl
    {
        public LogViewer()
        {
            InitializeComponent();

            Global.Instance.CreateDefaultBuilder();
            DataContext = ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService<LogViewerViewModel>();
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
}
