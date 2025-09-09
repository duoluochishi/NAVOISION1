using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Components.Collimator;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest.Views.Components.Collimator
{
    public partial class CollimatorCalibrationView : Grid
    {
        public CollimatorCalibrationView()
        {
            InitializeComponent();
            /** DataContext **/
            this.DataContext = Ioc.Default.GetRequiredService<CollimatorCalibrationViewModel>();
        }

        private void FeedbackConsole_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.feedbackConsole.ScrollToEnd();
        }
    }
}
