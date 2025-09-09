using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.Common.Extensions;
using NV.CT.Service.HardwareTest.ViewModels.Components.XRaySource;

namespace NV.CT.Service.HardwareTest.Views.Components.XRaySource
{
    /// <summary>
    /// XRaySourceKVMACoefficentsView.xaml 的交互逻辑
    /// </summary>
    public partial class XRaySourceKVMACoefficientsView
    {
        public XRaySourceKVMACoefficientsView()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetService<XRaySourceKVMACoefficientsViewModel>();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
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