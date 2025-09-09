using Microsoft.Extensions.DependencyInjection;
using NV.CT.LogManagement.ViewModel;
using System;
using System.Windows.Controls;

namespace NV.CT.LogManagement.View.English
{
    /// <summary>
    /// LogViewerSearchCriteria.xaml 的交互逻辑
    /// </summary>
    public partial class LogViewerSearchCriteria : UserControl
    {
        public LogViewerSearchCriteria()
        {
            InitializeComponent();
            DataContext = ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService<LogViewerSearchCriteriaViewModel>();

        }
    }
}
