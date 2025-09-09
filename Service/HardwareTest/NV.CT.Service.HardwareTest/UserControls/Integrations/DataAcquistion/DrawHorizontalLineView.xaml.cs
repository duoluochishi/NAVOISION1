using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.DataAcquisition;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest.UserControls.Integrations.DataAcquisition
{
    public partial class DrawHorizontalLineView : UserControl
    {
        public DrawHorizontalLineView()
        {
            InitializeComponent();
            /** DataContext **/
            this.DataContext = Ioc.Default.GetRequiredService<DrawHorizontalLineViewModel>();
        }
    }
}
