namespace NV.CT.Service.HardwareTest.Share.Utils
{
    public static class DateTimeUtils
    {

        public static DateTime Combine(DateTime dateTime1, DateTime dateTime2) 
        {
            return new DateTime(
                dateTime1.Year, dateTime1.Month, dateTime1.Day,
                dateTime2.Hour, dateTime2.Minute, dateTime2.Second);
        }

        public static double GetLongTimeStamp(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static DateTime FromLongTimeStamp(string timeStamp)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(timeStamp)).DateTime.ToLocalTime();
        }

    }
}
