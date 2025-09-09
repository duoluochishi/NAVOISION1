using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.ProtocolManagement.ViewModels;

namespace NV.CT.ProtocolManagement.Views.English
{
    /// <summary>
    /// ProtocolOperation.xaml 的交互逻辑
    /// </summary>
    public partial class ProtocolOperationControl : UserControl
    {
        public ProtocolOperationControl()
        {
            InitializeComponent();
            using (var scope = Global.Instance.ServiceProvider.CreateScope())
            {
                DataContext = scope.ServiceProvider.GetRequiredService<ProtocolOperationControlViewModel>();
            }
        }
    }
}
