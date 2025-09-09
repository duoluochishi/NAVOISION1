using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Components.XRaySource;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest.Views.Components.XRaySource
{
    public partial class XRaySourceComprehensiveTestingView : Grid
    {
        public XRaySourceComprehensiveTestingView()
        {
            InitializeComponent();
            /** DataContext **/
            this.DataContext = Ioc.Default.GetService<XRaySourceComprehensiveTestingViewModel>();
            /** Console TextChanged Event **/
            this.feedbackConsole.TextChanged += FeedbackConsole_TextChanged;
        }

        private void FeedbackConsole_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.feedbackConsole.ScrollToEnd();
        }
    }
}
