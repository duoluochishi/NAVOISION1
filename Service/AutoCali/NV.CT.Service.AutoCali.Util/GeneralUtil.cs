namespace NV.CT.Service.AutoCali.Util
{
    /// <summary>
    /// 通用的工具类，提供各种通用的工具方法，比如，枚举类型转列表
    /// </summary>
    public static class GeneralUtil
    {
        /// <summary>
        /// 通过枚举类型获取枚举列表;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IList<T> GetEnumList<T>() where T : Enum
        {
            IList<T> list = Enum.GetValues(typeof(T)).OfType<T>().ToList();
            return list;
        }
    }
}
