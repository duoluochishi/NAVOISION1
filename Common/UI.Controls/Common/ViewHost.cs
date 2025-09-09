using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace NV.CT.UI.Controls.Common
{
    public class ViewHost : HwndHost
    {
        private readonly IntPtr _hwnd;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public extern static IntPtr SetParent(HandleRef hwnd, HandleRef hwndParent);

        public ViewHost(IntPtr handle) => _hwnd = handle;

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            try
            {
                SetParent(new HandleRef(null, _hwnd), hwndParent);
                return new HandleRef(this, _hwnd);
            }
            catch (Exception)
            {
                return new HandleRef(this, IntPtr.Zero);
                //throw;
            }
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
        }
    }
}
