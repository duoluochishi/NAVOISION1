using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.ComponentEnablement;

namespace NV.CT.Service.HardwareTest.Views.Integrations.ComponentEnablement
{
    /// <summary>
    /// ComponentEnablementView.xaml 的交互逻辑
    /// </summary>
    public partial class ComponentEnablementView
    {
        public ComponentEnablementView()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<ComponentEnablementViewModel>();
        }
    }
}