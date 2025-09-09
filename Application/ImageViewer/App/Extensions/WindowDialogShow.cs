//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.ImageViewer.Extensions;

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
}
