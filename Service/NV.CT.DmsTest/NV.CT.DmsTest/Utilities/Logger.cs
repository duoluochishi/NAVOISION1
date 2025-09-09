using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NV.CT.Service.Common;


namespace NV.CT.DmsTest.Utilities
{
    internal class Logger
    {
        public static void Debug(string log)
        {
            LogService.Instance.Debug(ServiceCategory.DMSTest, log);
        }
        public static void Info(string log)
        {
            LogService.Instance.Info(ServiceCategory.DMSTest, log);
        }
        public static void Warn(string log)
        {
            LogService.Instance.Warn(ServiceCategory.DMSTest, log);
        }
        public static void Error(string log)
        {
            LogService.Instance.Error(ServiceCategory.DMSTest, log);
        }
        public static void Fatal(string log)
        {
            LogService.Instance.Fatal(ServiceCategory.DMSTest, log);
        }
    }
}
