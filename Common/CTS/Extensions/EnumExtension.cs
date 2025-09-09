//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NV.CT.CTS.Extensions
{
    public static class EnumExtension
    {
        public static TEnum To<TEnum>(this string enumName)
        {
            var enumValue = (TEnum)Enum.Parse(typeof(TEnum), enumName);
            return enumValue;
        }

        public static Dictionary<TKey, string> ToDictionary<TKey>(this Type enumType) where TKey : notnull
        {
            var dictionary = new Dictionary<TKey, string>();
            foreach (var enumItem in Enum.GetValues(enumType))
            {
                dictionary.Add((TKey)enumItem, enumItem.ToString() ?? string.Empty);
            }
            return dictionary;
        }

        private static readonly ConcurrentDictionary<Type, Dictionary<string, string>> DisplayNameDictionary =
            new ConcurrentDictionary<Type, Dictionary<string, string>>();

        /// <summary>
        /// 得到枚举 DisplayAttribute 特性内容
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string GetDisplayName(this Enum input)
        {
            var enumType = input.GetType();
            var name = input.ToString();
            if (DisplayNameDictionary.TryGetValue(enumType, out var dic))
                if (dic.ContainsKey(name))
                    return dic[name];

            var displayName = enumType.GetField(name)?.GetCustomAttribute<DisplayAttribute>()?.Name;
            if (string.IsNullOrEmpty(displayName)) return name;

            DisplayNameDictionary.TryGetValue(enumType, out dic);
            dic ??= new Dictionary<string, string>();
            dic.Add(name, displayName);
            DisplayNameDictionary.TryAdd(enumType, dic);
            return displayName;
        }

        private static readonly ConcurrentDictionary<Type, Dictionary<string, string>> DescDictionary =
            new ConcurrentDictionary<Type, Dictionary<string, string>>();

        /// <summary>
        /// 得到枚举 DescriptionAttribute 特性内容
        /// </summary>
        /// <param name="input"></param>
        /// <returns>没有 DescriptionAttribute 特性返回枚举名</returns>
        public static string GetDescription(this Enum input)
        {
            var enumType = input.GetType();
            var name = input.ToString();
            if (DescDictionary.TryGetValue(enumType, out var dic))
                if (dic.ContainsKey(name))
                    return dic[name];

            var description = enumType.GetField(name)?.GetCustomAttribute<DescriptionAttribute>()?.Description;
            if (string.IsNullOrEmpty(description)) return name;

            DescDictionary.TryGetValue(enumType, out dic);
            dic ??= new Dictionary<string, string>();
            dic.Add(name, description);
            DescDictionary.TryAdd(enumType, dic);
            return description;
        }

        //public static List<T> GetList<T>(this Enum enumName)
        //{
        //    return Enum.GetValues(enumName.GetType()).Cast<T>().ToList();
        //}

        ///// <summary>
        ///// 将枚举类型转成结合多语言机制的ItemModel的list数据源
        ///// </summary>
        ///// <param name="enumType">枚举类型</param>
        ///// <returns></returns>
        //public static ObservableCollection<KeyValuePair<int, string>> EnumToItems(Type enumType)
        //{
        //    ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
        //    foreach (var enumItem in Enum.GetValues(enumType))
        //    {
        //        var value = LanguageResource.ResourceManager.GetString("Enum_" + enumType.Name + "_" + (enumItem is null ? "" : enumItem.ToString()));
        //        if (!string.IsNullOrEmpty(value))
        //        {
        //            list.Add(new KeyValuePair<int, string>(enumItem is null ? 0 : (int)enumItem, value));
        //        }
        //    }
        //    return list;
        //}
        public static ObservableCollection<KeyValuePair<int, string>> EnumToList(Type enumType)
        {
            ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
            foreach (var enumItem in Enum.GetValues(enumType))
            {
                list.Add(new KeyValuePair<int, string>(enumItem is null ? 0 : (int)enumItem, enumItem?.ToString() ?? string.Empty));
            }
            return list;
        }
    }
}