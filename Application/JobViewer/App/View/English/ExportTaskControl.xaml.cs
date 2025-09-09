using Microsoft.Extensions.DependencyInjection;
using NV.CT.JobViewer.ViewModel;
using System.Windows.Controls;

namespace NV.CT.JobViewer.View.English
{
    /// <summary>
    /// ExportTaskControl.xaml 的交互逻辑
    /// </summary>
    public partial class ExportTaskControl : UserControl
    {
        public ExportTaskControl()
        {
            InitializeComponent();
            DataContext = Global.Instance?.ServiceProvider?.GetRequiredService<ExportTaskViewModel>();
        }
    }
}
