//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:43:11    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Print.Extensions
{
    public class WindowDialogShow
    {
        public static void DialogShow(Window window)
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
                    window.Show();
                    window.Activate();
                }
            }
        }

    }
}
