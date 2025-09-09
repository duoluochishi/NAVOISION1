using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NV.CT.Service.UI.Util
{
    public static class WindowExtension
    {
        public static void Register(this Window win, string key)
        {
            WindowHelper.Register(key, win.GetType());
        }

        public static void Register(this Window win, string key, Type t)
        {
            WindowHelper.Register(key, t);
        }

        public static void Register<T>(this Window win, string key)
        {
            WindowHelper.Register<T>(key);
        }
    }
}
