using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.SystemEnvironment;

namespace NV.CT.Service.HardwareTest.Views.Integrations.SystemEnvironment
{
    /// <summary>
    /// SystemEnvironmentView.xaml 的交互逻辑
    /// </summary>
    public partial class SystemEnvironmentView
    {
        public SystemEnvironmentView()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<SystemEnvironmentViewModel>();
        }
    }
}