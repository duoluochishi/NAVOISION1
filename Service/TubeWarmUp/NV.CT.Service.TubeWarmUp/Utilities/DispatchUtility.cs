using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NV.CT.Service.TubeWarmUp.Utilities
{
    public static class DispatchUtility
    {
        public static void Invoke(Action action)
        {
            CurrentDispatcher?.Invoke(action);
        }
        public static void BeginInvoke(Action action)
        {
            CurrentDispatcher?.BeginInvoke(action);
        }
        public static Dispatcher CurrentDispatcher => System.Windows.Application.Current?.Dispatcher ??
            Dispatcher.CurrentDispatcher;
    }
}
