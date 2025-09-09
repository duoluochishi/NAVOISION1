//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.MPS.UI.Dialog.Enum;
using System;

namespace NV.MPS.UI.Dialog.Service;

public interface IDialogService
{
    void Show(bool isConfirm, MessageLeveles messageLevel, string title, string message, Action<IDialogResult>? callback, IntPtr owner);
    void Show(bool isConfirm, MessageLeveles messageLevel, string title, string message, Action<IDialogResult>? callback, string windowName, IntPtr owner);

    /// <summary>
    /// 消息弹出框
    /// </summary>
    /// <param name="isConfirm">是否是确认框</param>
    /// <param name="messageLevel">消息登记</param>
    /// <param name="title">弹出框的标题</param>
    /// <param name="message">弹出框信息</param>
    /// <param name="callback">回调函数</param>
    void ShowDialog(bool isConfirm, MessageLeveles messageLevel, string title, string message, Action<IDialogResult>? callback, IntPtr owner);

    /// <summary>
    /// 消息弹出框
    /// </summary>
    /// <param name="isConfirm">是否是确认框</param>
    /// <param name="messageLevel">消息登记</param>
    /// <param name="title">弹出框的标题</param>
    /// <param name="message">弹出框信息</param>
    /// <param name="callback">回调函数</param>
    /// <param name="windowName">窗体名称</param>
    void ShowDialog(bool isConfirm, MessageLeveles messageLevel, string title, string message, Action<IDialogResult>? callback, string windowName, IntPtr owner);

    void ShowContentDialog(string title, object dialogContent, IntPtr owner, Action<IDialogResult>? callBack = null, int width = 550, int height = 350);

    void ShowDialogContentDialog(string title, object dialogContent, IntPtr owner, Action<IDialogResult>? callBack = null, int width = 550, int height = 350);

    void ShowErrorDialog(string errorCode, string errorMessage, MessageLeveles messageLevel, IntPtr owner, string solution = "", Action<IDialogResult>? callback = null);

    void ShowErrorDialog(string code, IntPtr owner, Action<IDialogResult>? callback = null, params string[] args);
}