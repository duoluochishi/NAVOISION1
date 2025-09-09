using System;
using System.Windows.Threading;

namespace NV.CT.Service.Common
{
    public static class DispatcherWrapper
    {
        public static void Invoke(Delegate action, params object[] args)
        {
            CurrentDispatcher?.Invoke(action, args);
        }

        public static void BeginInvoke(Delegate action, params object[] args)
        {
            CurrentDispatcher?.BeginInvoke(action, args);
        }

        public static Dispatcher CurrentDispatcher =>
            System.Windows.Application.Current?.Dispatcher ??
            Dispatcher.CurrentDispatcher;
    }
}
