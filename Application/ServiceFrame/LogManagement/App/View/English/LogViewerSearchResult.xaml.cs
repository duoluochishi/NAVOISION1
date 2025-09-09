using Microsoft.Extensions.DependencyInjection;
using NV.CT.LogManagement.ViewModel;
using System;
using System.Windows.Controls;

namespace NV.CT.LogManagement.View.English
{
    /// <summary>
    /// LogViewerSearchResult.xaml 的交互逻辑
    /// </summary>
    public partial class LogViewerSearchResult : UserControl
    {
        public LogViewerSearchResult()
        {
            InitializeComponent();
            DataContext = ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService<LogViewerSearchResultViewModel>();

        }
    }
}
