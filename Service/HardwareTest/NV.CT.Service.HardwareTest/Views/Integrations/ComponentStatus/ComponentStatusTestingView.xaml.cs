using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.ComponentStatus;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest.Views.Integrations.ComponentStatus
{
    public partial class ComponentStatusTestingView : Grid
    {
        public ComponentStatusTestingView()
        {
            InitializeComponent();
            /** DataContext **/
            this.DataContext = Ioc.Default.GetService<ComponentStatusTestingViewModel>();
        }
    }
}
