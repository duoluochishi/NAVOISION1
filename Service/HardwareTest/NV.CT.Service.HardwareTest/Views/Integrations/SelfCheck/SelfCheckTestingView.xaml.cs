using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.SelfCheck;

namespace NV.CT.Service.HardwareTest.Views.Integrations.SelfCheck
{
    /// <summary>
    /// SelfCheckTestingView.xaml 的交互逻辑
    /// </summary>
    public partial class SelfCheckTestingView
    {
        public SelfCheckTestingView()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<SelfCheckTestingViewModel>();
        }
    }
}
