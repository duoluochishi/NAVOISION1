using Microsoft.Extensions.DependencyInjection;
using NV.CT.JobViewer.ViewModel;
using System.Windows.Controls;

namespace NV.CT.JobViewer.View.English
{
    /// <summary>
    /// ImportTaskControl.xaml 的交互逻辑
    /// </summary>
    public partial class ImportTaskControl : UserControl
    {
        public ImportTaskControl()
        {
            InitializeComponent();
            DataContext = Global.Instance?.ServiceProvider?.GetRequiredService<ImportTaskViewModel>();
        }
    }
}
