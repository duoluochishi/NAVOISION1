//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/8/11 13:05:21           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
namespace NV.CT.UI.Exam.Extensions;

public class WindowDialogShow
{
    public static void Show(Window window)
    {
        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        var wih = new WindowInteropHelper(window);
        if (ConsoleSystemHelper.WindowHwnd != IntPtr.Zero)
        {
            if (wih.Owner == IntPtr.Zero)
            {
                wih.Owner = ConsoleSystemHelper.WindowHwnd;
            }
            if (!window.IsVisible)
            {
                //隐藏底部状态栏
                window.Topmost = true;
                window.Show();
            }
        }
        else
        {
            if (Application.Current.MainWindow is not null && wih.Owner == IntPtr.Zero)
            {
                wih.Owner = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            }
            if (!window.IsVisible)
            {
                window.Show();
                window.Activate();
            }
        }
    }
	public static void DialogShow(Window window, bool isCenterScreen = true)
    {
        if (isCenterScreen)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        var wih = new WindowInteropHelper(window);
        if (ConsoleSystemHelper.WindowHwnd != IntPtr.Zero)
        {
            if (wih.Owner == IntPtr.Zero)
            {
                wih.Owner = ConsoleSystemHelper.WindowHwnd;
            }
            if (!window.IsVisible)
            {
                //隐藏底部状态栏
                window.ShowInTaskbar = false;
                window.Topmost = true;
                window.ShowDialog();
            }
        }
        else
        {
            if (Application.Current.MainWindow is not null && wih.Owner == IntPtr.Zero)
            {
                wih.Owner = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            }
            if (!window.IsVisible)
            {
                window.Topmost = true;
                window.ShowInTaskbar = false;
                window.Show();
                window.Activate();
            }
        }
    }
}