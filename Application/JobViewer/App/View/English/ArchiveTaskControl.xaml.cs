using Microsoft.Extensions.DependencyInjection;
using NV.CT.JobViewer.ViewModel;
using System.Windows.Controls;

namespace NV.CT.JobViewer.View.English
{
    /// <summary>
    /// ArchiveTaskControl.xaml 的交互逻辑
    /// </summary>
    public partial class ArchiveTaskControl : UserControl
    {
        public ArchiveTaskControl()
        {
            InitializeComponent();
            DataContext = Global.Instance?.ServiceProvider?.GetRequiredService<ArchiveTaskViewModel>();
        }
    }
}
