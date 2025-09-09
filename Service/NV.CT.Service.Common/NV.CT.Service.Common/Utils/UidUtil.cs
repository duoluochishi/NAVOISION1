using NV.CT.FacadeProxy.Common.Enums;
using System;

namespace NV.CT.Service.Common.Utils
{
    public class UidUtil
    {
        /// <summary>
        /// 要求16位，并且不易重复
        /// </summary>
        /// <returns></returns>
        public static string GenerateUid_16()
        {
            return DateTime.Now.ToString("yyyy_MMdd_HHmmss");
        }

        /// <summary>
        /// Generate the uid by 16 chars for scan.
        /// e.g. 145406_DarkL_000
        /// e.g. 145406_AfterGlow
        /// e.g. 145406_er20_Down
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static string GenerateScanUid_16(RawDataType dataType)
        {
            var timestamp = DateTime.Now.ToString("HHmmss");//6位 UidUtil.GenerateUID_16();
            string uid = timestamp + separator;
            var rawDataType = $"{dataType}";
            if (rawDataType.Length >= maxCountOfRawDataType)
            {
                uid += rawDataType.Substring(rawDataType.Length - maxCountOfRawDataType);//16位：6+1+9
            }
            else
            {
                uid += rawDataType + separator;
                uid = uid.PadRight(16, padChar);
            }

            return uid;
        }


        public static readonly int maxCountOfRawDataType = 9;
        public static readonly char padChar = '0';
        public static readonly char separator = '_';
    }
}
