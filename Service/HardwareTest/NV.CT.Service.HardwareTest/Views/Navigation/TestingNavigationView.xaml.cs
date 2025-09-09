using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.ViewModels.Navigation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NV.CT.Service.HardwareTest.Views.Navigation
{
    public partial class NavigationView : Grid
    {
        public NavigationView()
        {
            InitializeComponent();
            /** Navigation Visibility Control **/
            this.LeftMenuVisibilityToggleButton.Click += LeftMenuVisibilityToggleButton_Click;
        }

        private void TestingNavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            /** DataContext **/
            this.DataContext = Ioc.Default.GetRequiredService<NavigationViewModel>();
        }

        private void LeftMenuVisibilityToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var toggle = (ToggleButton)sender;
            /** 非空判断 **/
            if (!toggle.IsChecked.HasValue) return;
            /** 显隐控制 **/
            if (toggle.IsChecked.Value) 
            {
                /** 隐藏导航栏 **/
                this.ComponentGroupBox.Visibility = Visibility.Collapsed;
                this.TestItemGroupBox.Visibility = Visibility.Collapsed;
            }
            else 
            {
                /** 显示导航栏 **/
                this.ComponentGroupBox.Visibility = Visibility.Visible;
                this.TestItemGroupBox.Visibility = Visibility.Visible;
            }            
        }

    }
}
