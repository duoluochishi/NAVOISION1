using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Components.XRaySource;

namespace NV.CT.Service.HardwareTest.UserControls.Components.XRaySource
{
    /// <summary>
    /// XRaySourceAddKVMAPairView.xaml 的交互逻辑
    /// </summary>
    public partial class XRaySourceAddKVMAPairView
    {
        public XRaySourceAddKVMAPairView()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<XRaySourceAddKVMAPairViewModel>();
        }
    }
}