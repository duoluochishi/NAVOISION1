using Microsoft.Extensions.DependencyInjection;
using NV.CT.ErrorCodes;
using NV.CT.Service.Common.Interfaces;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System;
using System.Windows;
using System.Windows.Interop;
using IDialogService = NV.CT.Service.Common.Interfaces.IDialogService;
using IDialogService_MPS = NV.MPS.UI.Dialog.Service.IDialogService;

namespace NV.CT.Service.Common
{
    public class DialogService : IDialogService
    {
        private static readonly Lazy<DialogService> _instance = new Lazy<DialogService>(() => new DialogService());
        public static DialogService Instance => _instance.Value;

        /// <summary>
        /// MCS框架的DialogService
        /// </summary>
        private readonly IDialogService_MPS _dialogServiceCore;
        private readonly ILogService _logService;

        #region Init

        private DialogService()
        {
            _dialogServiceCore = (IDialogService_MPS)ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService(typeof(IDialogService_MPS));
            _logService = LogService.Instance;
        }

        #endregion Init

        /// <summary>
        /// 已过时，建议使用 ShowConfirm
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Obsolete]
        public bool ShowMessage(string msg)
        {
            return ShowConfirm(msg);
        }

        /// <summary>
        /// 已过时，建议使用 ShowConfirm
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Obsolete]
        public bool ShowMessage(string title, string msg)
        {
            return ShowConfirm(msg, title);
        }

        #region Show Dialog, eg. Info / Warning / Error / Confirm

        /// <summary>
        /// 弹窗显示普通信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public void ShowInfo(string message, string title = Title_Info)
        {
            if (string.IsNullOrEmpty(title))
            {
                title = Title_Info;
            }

            ShowDialogCore(false, MessageLeveles.Info, title, message);
        }

        /// <summary>
        /// 弹窗显示警示信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public void ShowWarning(string message, string title = Title_Warning)
        {
            if (string.IsNullOrEmpty(title))
            {
                title = Title_Warning;
            }

            ShowDialogCore(false, MessageLeveles.Warning, title, message);
        }

        // <summary>
        /// 弹窗显示错误信息（非标准错误码）
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public void ShowError(string message, string title = Title_Error)
        {
            if (string.IsNullOrEmpty(title))
            {
                title = Title_Error;
            }

            ShowDialogCore(false, MessageLeveles.Error, title, message);
        }

        // <summary>
        /// 弹窗显示标准的错误码信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public void ShowErrorCode(string errorCode, params string[] args)
        {
            var owner = CT.ServiceFramework.Global.Instance.MainWindowHwnd;
            DispatcherWrapper.Invoke(() => _dialogServiceCore.ShowErrorDialog(errorCode, owner, null, args));
        }

        /// <summary>
        /// 弹出疑问对话框，并且返回是否确认（true）
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public bool ShowConfirm(string message, string title = Title_Confirm)
        {
            if (string.IsNullOrEmpty(title))
            {
                title = Title_Confirm;
            }

            ButtonResult buttonResult = ButtonResult.None;
            //为了获取用户选择了哪一个按钮，重新包装一下回调，保存用户的选择
            Action<IDialogResult> callbackWrapper = (arg) => buttonResult = arg.Result;
            ShowDialogCore(true, MessageLeveles.Info, title, message, callbackWrapper);

            //返回用户的选择
            return buttonResult == ButtonResult.OK || buttonResult == ButtonResult.Yes;
        }

        #endregion Show Dialog, eg. Info / Warning / Error / Confirm

        /// <summary>
        /// 设置窗口的父窗口（1.MCS的窗口句柄；或者2.触发元素所在的窗口）
        /// </summary>
        /// <param name="dialogContent"></param>
        /// <param name="title"></param>
        /// <param name="callback"></param>
        public void SetOwner(object sender, Window popupWindow)
        {
            Window parentWindow = null;
            if (sender is FrameworkElement element)
            {
                _logService.Info(ServiceCategory.Common, "Finding the parent Window by sender");
                parentWindow = Window.GetWindow(element);
            }

            if (parentWindow != null)
            {
                _logService.Info(ServiceCategory.Common, $"Set owner by the found parent Window({parentWindow.GetType()}#{parentWindow.GetHashCode()})");
                popupWindow.Owner = parentWindow;
            }
            else
            {
                _logService.Info(ServiceCategory.Common, "Not find the parent Window by sender");

                var hwnd = CT.ServiceFramework.Global.Instance.MainWindowHwnd;
                if (hwnd != IntPtr.Zero)
                {
                    _logService.Info(ServiceCategory.Common, $"Set owner by the MCS MainWindowHwnd");
                    var wih = new WindowInteropHelper(popupWindow);
                    wih.Owner = hwnd;
                }
                else
                {
                    popupWindow.Topmost = true;
                    _logService.Info(ServiceCategory.Common, $"Failed to find the parent Window by sender");
                }
            }
        }

        private void ShowDialogCore(bool isConfirm, MessageLeveles messageLevel, string title, string message, Action<IDialogResult>? callback = null)
        {
            DispatcherWrapper.Invoke(() => { _dialogServiceCore.ShowDialog(isConfirm, messageLevel, title, message, callback, CT.ServiceFramework.Global.Instance.MainWindowHwnd); });
        }

        private const string Title_Warning = "Warning";
        private const string Title_Info = "Info";
        private const string Title_Error = "Error";
        private const string Error_Unknown = "Unknown error";
        private const string Title_Confirm = "Confirm";

        /// <summary>
        /// 示意如何使用错误码弹窗
        /// </summary>
        public void DemoShowErrorCode()
        {
            //Not exist errorCode
            DialogService.Instance.ShowErrorCode("aREC000000001-95x");//test, [ToDo]remove

            //large error message
            //REC000000001	Error	离线重建计算机掉线	1,离线重建计算机控制进程崩溃或卡死.
            //            2,Host计算机与离线重建计算机网线掉落.
            //3,线缆损坏.
            //4,网卡损坏. 1,检查网线,确保网线已经链接;
            //            2,重启计算机;
            //            3,上述2步依然不能解决问题,考虑更换网线;
            //            4,上述3步依然不能解决问题,考虑更换网卡;
            //            5,请联系厂家服务工程师解决.                        
            DialogService.Instance.ShowErrorCode("REC000000001");//test, [ToDo]remove

            DialogService.Instance.ShowErrorCode("aREC000000001-96x");//test, [ToDo]remove

            //Warning level
            //MCS001004004 MCS_ImageDataLost   MCS001004001 Warning 图像数据缺失 MRS图像保存数据缺失
            DialogService.Instance.ShowErrorCode("MCS001004004");//test, [ToDo]remove

            //Error level
            //MCS001003001	MCS_ProxyCommunicationFailed	MCS001003001	Error	MRS扫描代理通信失败	1.代理端口是否开通;             2.通信断联
            DialogService.Instance.ShowErrorCode("MCS001003001");//test, [ToDo]remove

            //No solution for error
            //MCS001007001 MCS_ProtocolEmpty   MCS001007001 Error   参数：协议为空
            DialogService.Instance.ShowErrorCode("MCS001007001");//test, [ToDo]remove
        }
    }
}