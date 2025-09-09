using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NV.CT.UI.Controls.Common
{
    public static class ViewHelper
    {
        public static IntPtr ToHwnd(FrameworkElement element)
        {
            var hwndParameters = new HwndSourceParameters
            {
                ParentWindow = new IntPtr(-3),
                WindowStyle = 0x40000000 | 0x02000000
            };
            var hwndSource = new HwndSource(hwndParameters)
            {
                RootVisual = element,
                SizeToContent = SizeToContent.Manual
            };
            hwndSource.CompositionTarget.BackgroundColor = Colors.White;
            return hwndSource.Handle;
        }
    }
}
