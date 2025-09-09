using System;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public static class DateTimeExtension 
    {

        public static bool Between(this DateTime dateTime, DateTime start, DateTime end) 
        {
            return dateTime >= start && dateTime <= end;
        }

    }
}
