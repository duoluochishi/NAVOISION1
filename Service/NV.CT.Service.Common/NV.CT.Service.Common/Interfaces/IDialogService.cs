using System;

namespace NV.CT.Service.Common.Interfaces
{
    public interface IDialogService
    {
        [Obsolete]
        bool ShowMessage(string msg);

        [Obsolete]
        bool ShowMessage(string title, string msg);

        /// <summary>
        /// 弹窗显示普通信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void ShowInfo(string message, string title = "");

        /// <summary>
        /// 弹窗显示警示信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void ShowWarning(string message, string title = "");

        // <summary>
        /// 弹窗显示错误信息（非标准错误码）
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void ShowError(string message, string title = "");

        // <summary>
        /// 弹窗显示用户确认信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        bool ShowConfirm(string message, string title = "");

        // <summary>
        /// 弹窗显示标准的错误码信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        void ShowErrorCode(string errorCode, params string[] args);
    }
}