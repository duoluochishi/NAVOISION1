using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Components.XRaySource;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest.UserControls.Components.XRaySource
{
    public partial class XRaySourceChartPlotView : UserControl
    {
        public XRaySourceChartPlotView()
        {
            InitializeComponent();

            this.DataContext = Ioc.Default.GetRequiredService<XRaySourceChartPlotViewModel>();
        }
    }
}
