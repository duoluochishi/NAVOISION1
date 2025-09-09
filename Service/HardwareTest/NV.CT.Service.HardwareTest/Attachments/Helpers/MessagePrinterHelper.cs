using NV.CT.Service.HardwareTest.Share.Enums;
using System;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    public static class MessagePrinterHelper
    {
        public static Func<string, PrintLevel, string> MessageWrapper = (message, level) 
            => $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [{Enum.GetName(level)}] {message}";
    }
}
