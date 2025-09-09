using Microsoft.Extensions.DependencyInjection;
using NV.CT.JobViewer.ViewModel;
using System;
using System.Windows.Controls;

namespace NV.CT.JobViewer.View.English
{
    /// <summary>
    /// PrintTaskControl.xaml 的交互逻辑
    /// </summary>
    public partial class PrintTaskControl : UserControl
    {
        public PrintTaskControl()
        {
            InitializeComponent();
            DataContext = Global.Instance?.ServiceProvider?.GetRequiredService<PrintTaskViewModel>();
        }
    }
}
