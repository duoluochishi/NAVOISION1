using CommunityToolkit.Mvvm.Messaging;
using NV.CT.Service.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.DmsTest.Utilities
{
    internal class ConsoleLogPrinter
    {
        public static void Debug(string log)
        {
            StrongReferenceMessenger.Default.Send($"{DateTime.Now} : Debug {log}");
            LogService.Instance.Debug(ServiceCategory.DMSTest, log);
        }
        public static void Info(string log)
        {
            StrongReferenceMessenger.Default.Send($"{DateTime.Now} : Info {log}");
            LogService.Instance.Info(ServiceCategory.DMSTest, log);
        }
        public static void Warn(string log)
        {
            StrongReferenceMessenger.Default.Send($"{DateTime.Now} : Warn {log}");
            LogService.Instance.Warn(ServiceCategory.DMSTest, log);
        }
        public static void Error(string log)
        {
            StrongReferenceMessenger.Default.Send($"{DateTime.Now} : Error {log}");
            LogService.Instance.Error(ServiceCategory.DMSTest, log);
        }
        public static void Fatal(string log)
        {
            StrongReferenceMessenger.Default.Send($"{DateTime.Now} : Fatal {log}");
            LogService.Instance.Fatal(ServiceCategory.DMSTest, log);
        }
    }
}
