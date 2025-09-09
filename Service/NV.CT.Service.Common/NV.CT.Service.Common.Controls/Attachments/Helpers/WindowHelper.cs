using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common.Controls.Attachments.EventArguments;
using NV.CT.Service.Common.Controls.Universal;
using NV.CT.Service.Common.Controls.ViewModels.Universal;
using NV.CT.Service.Common.Framework;
using System;
using System.Windows.Controls;

namespace NV.CT.Service.Common.Controls.Attachments.Helpers
{
    public static class WindowHelper
    {
        static WindowHelper()
        {
            DialogView = IocContainer.Instance.Services.GetRequiredService<UniversalPopUpView>();
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
            DialogView = IocContainer.Instance.Services.GetRequiredService<UniversalPopUpView>();
            /** 如果是MCS启动，设置窗口Owner为MCS句柄 **/
            var handle = ServiceFramework.Global.Instance.MainWindowHwnd;
            if (handle != IntPtr.Zero)
            {
                WindowOwnerHelper.SetWindowOwner(DialogView, handle);
            }
        }

        #endregion

        public static void Show<T>(uint windowWidth = DefaultWindowWidth, uint windowHeight = DefaultWindowHeight,
            object dataContext = null) where T : UserControl
        {
            UpdateDialogService();
            var content = IocContainer.Instance.Services.GetRequiredService<T>();
            if(dataContext != null)
            {
                content.DataContext = dataContext;
            }

            DialogViewModel.CurrentShowContent = content;
            DialogViewModel.InvokeAdjustWindowPositionEvent(
                new AdjustWindowEventArgs() { Width = windowWidth, Height = windowHeight });
            DialogView.ShowDialog();
        }

        public static void Show(object content)
        {
            UpdateDialogService();
            DialogViewModel.CurrentShowContent = content;
            DialogView.ShowDialog();
        }

        public static void Close()
        {
            DialogView.Close();
        }
    }
}
