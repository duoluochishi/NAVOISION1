using System;

namespace NV.CT.Service.Common.Helper
{
    // TODO:临时复制IdGenerator供使用，后续等提到MPS后引用那边的

    /// <summary>
    /// ID生成器
    /// </summary>
    public static class IdGenerator
    {
        private static int _current;
        private const int MaxValue = 999;
        private static long _lastTimestamp = GetTimestamp(DateTime.Now);
        private static readonly object ObjLock = new();
        private static readonly DateTime Original = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 获取下一个ID
        /// </summary>
        /// <param name="category">类别，仅支持0-9</param>
        /// <returns></returns>
        public static string Next(int category = 0)
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

                return $"{DateTime.Now.ToString("yyMMddHHmmss")}{category}{_current.ToString("D3")}";
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

        /// <summary>
        /// 获取下一个自动ID
        /// </summary>
        /// <returns></returns>
        public static string NextRandomID()
        {
            lock (ObjLock)
            {
                var random = new Random(DateTime.Now.Second);
                return $"{DateTime.Now.ToString("yyMMddHHmmss")}{random.Next(0, 9)}";
            }
        }
    }
}