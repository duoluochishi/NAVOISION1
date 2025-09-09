using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.HardwareTest.Services.Universal.Navigation.Abstractions;
using System;
using System.Windows;

namespace NV.CT.Service.HardwareTest.Services.Universal.Navigation
{
    public class NavigationService : INavigationService
    {

        #region Properties

        private FrameworkElement? currentView;
        public FrameworkElement? CurrentView
        {
            get => currentView;
            set
            {
                currentView = value;
                /** 通知CurrentView已发生变化 **/
                CurrentViewChanged?.Invoke();
            }
        }
        #endregion

        #region Events

        public event Action? CurrentViewChanged;

        #endregion

        #region Navigate Related

        public void NavigateTo<T>() where T : FrameworkElement
        {
            /** 获取要导航的页面 **/
            var navigationView = Ioc.Default.GetService<T>() as FrameworkElement;
            /** 导航 **/
            NavigateCommon(navigationView);
        }

        public void NavigateTo(Type type)
        {
            /** 根据 **/
            var navigationView = Ioc.Default.GetService(type) as FrameworkElement;
            /** 导航 **/
            NavigateCommon(navigationView);
        }

        private void NavigateCommon(FrameworkElement? navigationView)
        {
            /** 若导航窗口为空或与上次一致，跳出 **/
            if (navigationView is null || navigationView == currentView)
            {
                return;
            }
            /** 获取当前页INavigateAware接口 **/
            var currentViewAware = currentView?.DataContext as INavigationAware;
            /** 获取导航页INavigateAware接口 **/
            var navigationViewAware = navigationView.DataContext as INavigationAware;
            /** 执行通用方法 **/
            if (currentViewAware is not null)
            {
                /** 执行当前页 BeforeNavigateToOtherPage **/
                currentViewAware.BeforeNavigateToOtherPage();
            }
            if (navigationViewAware is not null)
            {
                /** 执行导航页 BeforeNavigateToCurrentPage **/
                navigationViewAware.BeforeNavigateToCurrentPage();
            }
            /** 更新当前页 **/
            CurrentView = navigationView;
        }

        #endregion

        #region Module Navigate Related

        /// <summary>
        /// 非初始化，当重新载入Module时，调用当前页面的BeforeNavigateToCurrentPage绑定事件
        /// </summary>
        public void AfterLoadHardwareTestModule() 
        {
            if (this.CurrentView is not null)
            {
                var navigationAware = this.CurrentView.DataContext as INavigationAware;
                navigationAware?.BeforeNavigateToCurrentPage();
            }
        }

        /// <summary>
        /// 若要离开模块时，调用当前页面的BeforeNavigateToOtherPage解绑事件
        /// </summary>
        public void BeforeLeaveHardwareTestModule() 
        {
            if (this.CurrentView is not null) 
            {
                var navigationAware = this.CurrentView.DataContext as INavigationAware;
                navigationAware?.BeforeNavigateToOtherPage();
            }
        }

        #endregion

    }
}
