using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Components.Detector;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest.UserControls.Components.Detector
{
    public partial class UpdateDetectorBoardSeriesNumberView : UserControl
    {
        public UpdateDetectorBoardSeriesNumberView()
        {
            InitializeComponent();
            //DataContext
            DataContext = Ioc.Default.GetService<UpdateDetectorBoardSeriesNumberViewModel>();
        }
    }
}
