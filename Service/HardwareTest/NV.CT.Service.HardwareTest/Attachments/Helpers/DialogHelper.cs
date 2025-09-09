using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.HardwareTest.Attachments.EventArguments;
using NV.CT.Service.HardwareTest.Attachments.Interfaces;
using NV.CT.Service.HardwareTest.UserControls.Universal;
using NV.CT.Service.HardwareTest.ViewModels.Universal;
using System;
using System.Windows.Controls;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    /// <summary>
    /// 窗口帮助类
    /// </summary>
    public static class DialogHelper
    {
        static DialogHelper()
        {
              DialogView = Ioc.Default.GetRequiredService<UniversalPopUpView>();
        }

        #region Fields

        private const uint DefaultWindowWidth = 1440;
        private const uint DefaultWindowHeight = 810;

        #endregion

        #region Properties

        private static UniversalPopUpView DialogView { get; set; }
        private static UniversalPopUpViewModel DialogViewModel
            => (UniversalPopUpViewModel)DialogView.DataContext;

        private static void UpdateDialogService()
        {
            /** 获取DialogView **/
            DialogView = Ioc.Default.GetRequiredService<UniversalPopUpView>();
            /** 如果是MCS启动，设置窗口Owner为MCS句柄 **/
            var handle = ServiceFramework.Global.Instance.MainWindowHwnd;
            if (handle != IntPtr.Zero)
            {
                WindowOwnerHelper.SetWindowOwner(DialogView, handle);
            }
        }

        #endregion

        /// <summary>
        /// 显示Dialog，指定宽高
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="windowWidth"></param>
        /// <param name="windowHeight"></param>
        public static void ShowDialog<T>(uint windowWidth = DefaultWindowWidth, uint windowHeight = DefaultWindowHeight) where T : UserControl
        {
            //重新设置窗体Owner
            UpdateDialogService();
            //获取要打开的内容
            var content = Ioc.Default.GetRequiredService<T>();
            //切换窗体内容为要打开的Dialog
            DialogViewModel.CurrentShowContent = content;
            //更新窗体Size
            DialogViewModel.InvokeAdjustWindowPositionEvent(
                new AdjustWindowEventArgs() { Width = windowWidth, Height = windowHeight});
            //显示
            DialogView.ShowDialog();
        }

        /// <summary>
        /// 显示Dialog，传参，并指定宽高
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <param name="windowWidth"></param>
        /// <param name="windowHeight"></param>
        public static void ShowDialog<T>(object[] parameters, uint windowWidth = DefaultWindowWidth, uint windowHeight = DefaultWindowHeight) where T : UserControl
        {
            //重新设置窗体Owner
            UpdateDialogService();
            //获取要打开的内容
            var content = Ioc.Default.GetRequiredService<T>();
            //若需要传参，且被打开的窗体实现了IDialogParameters接口
            if (parameters != null && content.DataContext is IDialogAware)
            {
                //传入参数
                ((IDialogAware)content.DataContext).EnterDialog(parameters);
            }
            //切换窗体内容为要打开的Dialog
            DialogViewModel.CurrentShowContent = content;
            //更新窗体Size
            DialogViewModel.InvokeAdjustWindowPositionEvent(
                new AdjustWindowEventArgs() { Width = windowWidth, Height = windowHeight });
            //显示
            DialogView.ShowDialog();
        }

        /// <summary>
        /// 直接根据内容显示Dialog
        /// </summary>
        /// <param name="content"></param>
        public static void ShowDialog(object content) 
        {
            //重新设置窗体Owner
            UpdateDialogService();
            //切换窗体内容为要打开的Dialog
            DialogViewModel.CurrentShowContent = content;
            //显示
            DialogView.ShowDialog();
        }

        /// <summary>
        /// 关闭打开的Dialog
        /// </summary>
        public static void Close() 
        {
            DialogView.Close();
        }
    }
}
