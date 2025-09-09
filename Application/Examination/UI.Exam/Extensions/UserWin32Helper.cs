//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Runtime.InteropServices;

namespace NV.CT.UI.Exam.Extensions;
public class UserWin32Helper
{
    public static IntPtr intPtr = IntPtr.Zero;

    #region user32 API
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetParent(IntPtr hWndChild, IntPtr hWndParent);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

    [DllImport("user32.dll")]
    static extern IntPtr GetTopWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern IntPtr GetWindow(IntPtr hWnd, UInt32 uCmd);

    [DllImport("user32.dll")]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindow(string lpszClass, string lpszWindow);      //按照窗体类名或窗体标题查找窗体

    [DllImport("user32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
    public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);                  //设置窗体属性

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
    public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
    public static extern long GetWindowLong(IntPtr hWnd, int nIndex);

    #endregion

    public static bool FindWindow(string formName)
    {
        for (int i = 0; i < 100; i++)
        {
            //按照窗口标题查找Python窗口
            IntPtr vHandle = FindWindow(string.Empty, formName);
            if (vHandle == IntPtr.Zero)
            {
                Thread.Sleep(100);  //每100ms查找一次，直到找到，最多查找10s
                continue;
            }
            else      //找到返回True
            {
                intPtr = vHandle;
                return true;
            }
        }
        intPtr = IntPtr.Zero;
        return false;
    }

    public static bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy)
    {
        return MoveWindow(intPtr, 0, 0, cx, cy, true);
    }
}