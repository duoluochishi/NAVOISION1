using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.Common;
using NV.CT.Service.HardwareTest.Initializer;
using NV.CT.Service.HardwareTest.Services.Universal.Navigation.Abstractions;
using System.Windows;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            /** 初始化组件 **/
            InitializeComponent();
        }

        private static bool firstInitialize = true;

        private void MainView_Loaded(object sender, RoutedEventArgs e)
        {
            if (firstInitialize) 
            {
                /** 更新标志位 **/
                firstInitialize = false;
                /** 初始化IOC **/
                ContainerInitializer.ConfigureServices();
                /** 记录 **/
                LogService.Instance.Info(ServiceCategory.HardwareTest, $"[HardwareTest] Module initialization has completed.");
            }
            else
            {
                var navigationService = Ioc.Default.GetRequiredService<INavigationService>();
                /** 重新载入模块时，调用当前页面的绑定事件 **/
                if (navigationService is not null)
                {
                    navigationService.AfterLoadHardwareTestModule();
                }
                /** 记录 **/
                LogService.Instance.Info(ServiceCategory.HardwareTest, $"[HardwareTest] Module is loaded.");
            }
        }

        private void MainView_Unloaded(object sender, RoutedEventArgs e)
        {
            var navigationService = Ioc.Default.GetRequiredService<INavigationService>();
            /** 离开模块时，调用当前页面的解绑事件 **/
            if (navigationService is not null) 
            {
                navigationService.BeforeLeaveHardwareTestModule();
            }
            /** 记录 **/
            LogService.Instance.Info(
                ServiceCategory.HardwareTest, $"[HardwareTest] Module has un-registered events.");
        }
    
    }
}
