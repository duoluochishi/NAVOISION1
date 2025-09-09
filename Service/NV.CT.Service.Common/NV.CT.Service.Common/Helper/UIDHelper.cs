using System;

namespace NV.CT.Service.Common.Helper
{
    // TODO:临时复制UIDHelper供使用，后续等提到MPS后引用那边的
    public static class UIDHelper
    {
        private static int _current;
        private const int MaxValue = 99;
        private static long _lastTimestamp = GetTimestamp(DateTime.Now);
        private static readonly object ObjLock = new();
        private static readonly DateTime Original = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static string CreateStudyInstanceUID() => $"1.2.840.1.59.0.8559.{Next(10)}.141";
        public static string CreateSeriesInstanceUID() => $"1.2.840.1.59.0.8569.{Next(40)}.141";
        public static string CreateReconInstanceUID() => $"1.2.840.1.59.0.8579.{Next(30)}.141";
        public static string CreateSOPClassUID() => "1.2.840.10008.5.1.4.1.1.2";
        public static string CreateSOPInstanceUID() => $"1.2.840.1.59.0.8589.{Next(20)}.141";
        public static string CreateStudyID() => NextWithMaxLength16(); //按照DICOM标准，StudyID SH 最大长度为16chars

        /// <summary>
        /// 获取下一个ID
        /// </summary>
        /// <param name="category">类别，仅支持0-9</param>
        /// <returns></returns>
        private static string Next(int category = 0)
        {
            lock (ObjLock)
            {
                var currentTimestamp = GetTimestamp(DateTime.Now);
                if (currentTimestamp == _lastTimestamp)
                {
                    _current++;
                    if (_current >= MaxValue)
                    {
                        NextTimestamp();
                        _current = 0;
                    }
                }
                else
                {
                    _lastTimestamp = currentTimestamp;
                    _current = 0;
                }

                return $"{DateTime.Now.ToString("yyMMddHHmmss")}{category.ToString("D2")}{_current.ToString("D2")}";
            }
        }

        /// <summary>
        /// 获取下一个最大长度为16的ID
        /// </summary>
        /// <returns></returns>
        private static string NextWithMaxLength16()
        {
            lock (ObjLock)
            {
                var currentTimestamp = GetTimestamp(DateTime.Now);
                if (currentTimestamp == _lastTimestamp)
                {
                    _current++;
                    if (_current >= MaxValue)
                    {
                        NextTimestamp();
                        _current = 0;
                    }
                }
                else
                {
                    _lastTimestamp = currentTimestamp;
                    _current = 0;
                }

                return $"NV{DateTime.Now.ToString("yyMMddHHmmss")}{_current.ToString("D2")}";
            }
        }

        private static void NextTimestamp()
        {
            var currentTimestamp = GetTimestamp(DateTime.Now);
            while (_lastTimestamp == currentTimestamp)
            {
                currentTimestamp = GetTimestamp(DateTime.Now);
            }

            _lastTimestamp = currentTimestamp;
        }

        private static long GetTimestamp(DateTime dt)
        {
            var ts = dt.Subtract(Original);
            return (long)ts.TotalSeconds;
        }
    }
}