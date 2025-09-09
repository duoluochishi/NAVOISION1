using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using NV.CT.Service.HardwareTest.Attachments.Messages;
using NV.CT.Service.HardwareTest.ViewModels.Integrations.ImageChain;

namespace NV.CT.Service.HardwareTest.Views.Integrations.ImageChain
{
    public partial class ImageChainTestingView
    {
        public ImageChainTestingView()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<ImageChainTestingViewModel>();
        }

        private void ImageControlScrollBar_DragDelta(object sender, DragDeltaEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new ScrollBarThumbChangedMessage());
        }

        private void ScanReconTabControl_Selected(object sender, RoutedEventArgs e)
        {
            FocusManager.SetFocusedElement(this, sender as IInputElement);
        }
    }
}