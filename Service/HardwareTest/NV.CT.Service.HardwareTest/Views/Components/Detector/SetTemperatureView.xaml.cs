using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.HardwareTest.Models.Components.Detector;
using NV.CT.Service.HardwareTest.ViewModels.Components.Detector;

namespace NV.CT.Service.HardwareTest.Views.Components.Detector
{
    /// <summary>
    /// SetTemperatureView.xaml 的交互逻辑
    /// </summary>
    public partial class SetTemperatureView
    {
        public SetTemperatureView()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<SetTemperatureViewModel>();
        }

        private void ListBoxItemGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is DependencyObject dependency)
            {
                var listBoxItem = dependency.FindParent<ListBoxItem>();

                if (listBoxItem != null)
                {
                    listBoxItem.IsSelected = true;
                }
            }
        }
    }
}