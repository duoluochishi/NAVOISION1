using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.ProtocolManagement.ViewModels;

namespace NV.CT.ProtocolManagement.Views.English
{
    /// <summary>
    /// ProtocolFilter.xaml 的交互逻辑
    /// </summary>
    public partial class ProtocolFilter : UserControl
    {
        public ProtocolFilter()
        {
            InitializeComponent();
            using (var scope = Global.Instance.ServiceProvider.CreateScope())
            {
                DataContext = scope.ServiceProvider.GetRequiredService<ProtocolFilterControlViewModel>();
            }
        }
    }
}
