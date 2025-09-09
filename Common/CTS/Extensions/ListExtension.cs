//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.ObjectModel;

namespace NV.CT.CTS.Extensions
{
    public static class ListExtension
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IList<T> list)
        {
            return new ObservableCollection<T>(list);
        }


        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T element in source)
                action(element);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }

        /// <summary>
        /// 找到满足条件的集合里面的下一个元素
        /// </summary>
        public static T? FindNext<T>(this IEnumerable<T?> source, Predicate<T> predicate)
        {
            if (source is null)
                return default;

            var currentFinishedScan = source.FirstOrDefault(n => predicate(n));
            if (currentFinishedScan is null)
            {
                return default;
            }

            var findIndex = source.ToList().IndexOf(currentFinishedScan);
            if (findIndex + 1 <= source.Count() - 1)
            {
                return source.ToList()[findIndex + 1];
            }

            return default;
        }


        //public static List<T> ToList<T>(this IList list)
        //{
        //    var realList = new List<T>();

        //    if (list is null)
        //    {
        //        return realList;
        //    }

        //    foreach (var row in list)
        //    {
        //        realList.Add((T)row);
        //    }

        //    return realList;
        //}
    }
}
