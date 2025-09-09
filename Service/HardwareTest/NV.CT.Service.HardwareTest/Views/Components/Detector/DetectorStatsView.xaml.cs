using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Components.Detector;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest.Views.Components.Detector
{
    public partial class DetectorStatsView : Grid
    {
        public DetectorStatsView()
        {
            InitializeComponent();
            //DataContext
            DataContext = Ioc.Default.GetService<DetectorStatsViewModel>();
        }

        private void MarkableTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
        }
    }
}
