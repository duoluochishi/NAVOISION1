//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace NV.CT.CTS.Extensions
{
    public static class DateTimeExtension
    {
        private static readonly DateTime orginal = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public const string DateFormatter = "yyyy-MM-dd";
        public const string TimeFormatter = "HH:mm:ss";
        public const string DateTimeFormatter = "yyyy-MM-dd HH:mm:ss";
        public const string FullFormatter = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// 转换为时间戳(毫秒)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToTimestamp(this DateTime dateTime)
        {
            var ts = dateTime.ToUniversalTime().Subtract(orginal);
            return (long)ts.TotalMilliseconds;
        }

        /// <summary>
        /// 转换为时间戳(秒)
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            var ts = dateTime.ToUniversalTime().Subtract(orginal);
            return (long)ts.TotalSeconds;
        }

        /// <summary>
        /// 时间戳转换为DateTime
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long timestamp)
        {
            var dt = orginal.AddMilliseconds(timestamp).ToLocalTime();
            return dt;
        }

        /// <summary>
        /// 转换为yyyy-MM-dd格式字符串
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToDateString(this DateTime dateTime)
        {
            return dateTime.ToString(DateFormatter);
        }

        /// <summary>
        /// 转换为HH:mm:ss格式字符串
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToTimeString(this DateTime dateTime)
        {
            return dateTime.ToString(TimeFormatter);
        }

        /// <summary>
        /// 转换为yyyy-MM-dd HH:mm:ss格式字符串
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToDateTimeString(this DateTime dateTime)
        {
            return dateTime.ToString(DateTimeFormatter);
        }

        /// <summary>
        /// 转换为自定义格式字符串
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="formatter"></param>
        /// <returns></returns>
        public static string ToDateTimeString(this DateTime dateTime, string formatter)
        {
            return dateTime.ToString(formatter);
        }

        /// <summary>
        /// yyyy-MM-dd HH:mm:ss.fff
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string ToFullString(this DateTime dateTime)
        {
            return dateTime.ToString(FullFormatter);
        }

        /// <summary>
        /// yyyyMMddHHmmssfff
        /// </summary>
        /// <returns></returns>
        public static string GetSequence()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        /// <summary>
        /// yyyyMMddHHmmssfff
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string GetSequence(this DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddHHmmssfff");
        }
    }
}
