using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace NV.CT.Service.Common.Framework
{
    public  class WindowOwnerHelper
    {
        public static void SetWindowOwner(Window window,IntPtr ptr)
        {
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
            windowInteropHelper.Owner = ptr;
        }
    }
}
