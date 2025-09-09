using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using NV.CT.Service.HardwareTest.Attachments.Messages;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.DataAcquisition;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest.Views.Integrations.DataAcquisition
{
    public partial class DataAcquisitionTestingView : Grid
    {
        public DataAcquisitionTestingView()
        {
            InitializeComponent();
            /** DataContext **/
            DataContext = Ioc.Default.GetService<DataAcquisitionTestingViewModel>();
        }

        private void FeedbackConsole_TextChanged(object sender, TextChangedEventArgs args)
        {
            feedbackConsole.ScrollToEnd();
        }

        private void ImageControlScrollBar_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new ScrollBarThumbChangedMessage());
        }
    }
}
