using Microsoft.Extensions.DependencyInjection;
using NV.CT.LogManagement.ViewModel;
using System;

namespace NV.CT.LogManagement.View.English
{
    /// <summary>
    /// LogViewerOriginalLocator.xaml 的交互逻辑
    /// </summary>
    public partial class LogViewerOriginalLocator : System.Windows.Controls.UserControl
    {
        public LogViewerOriginalLocator()
        {
            InitializeComponent();

            DataContext = ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService<LogViewerOriginalLocatorViewModel>();
        }

    }
}
