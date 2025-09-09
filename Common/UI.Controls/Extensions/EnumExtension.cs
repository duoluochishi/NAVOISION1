using NV.CT.CTS.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using NV.CT.Language;

namespace NV.CT.UI.Controls.Extensions
{
    public static class EnumExtension
    {
        public static ObservableCollection<KeyValuePair<int, string>> EnumToList(Type enumType)
        {
            ObservableCollection<KeyValuePair<int, string>> list = new ObservableCollection<KeyValuePair<int, string>>();
            foreach (var enumItem in Enum.GetValues(enumType))
            {
                //if (enumItem?.ToString()?.IndexOf("Plus") > 0)
                //{
                //    list.Add(new KeyValuePair<int, string>(enumItem is null ? 0 : (int)enumItem, enumItem?.ToString()?.Replace("Plus", "+") ?? string.Empty));
                //}
                //else
                //{
                    list.Add(new KeyValuePair<int, string>(enumItem is null ? 0 : (int)enumItem, enumItem?.ToString() ?? string.Empty));
                //}
            }
            return list;
        }

        public static ObservableCollection<KeyValuePair<int, string>> EnumToItems(ICollection<string> list, int magnify = 1)
        {
            var resultList = new ObservableCollection<KeyValuePair<int, string>>();
            list?.Distinct().ForEach(item =>
            {
                var isSuccess = decimal.TryParse(item, out decimal decimalVal);
                var intVal = (int)(decimalVal * magnify);
                resultList.Add(isSuccess
                    ? new KeyValuePair<int, string>(intVal, item)
                    : new KeyValuePair<int, string>(0, item));
            });
            return resultList;
        }

        /// <summary>
        /// Todo: 备用
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static List<string> GetValues(Type enumType)
        {
            List<string> list = new List<string>();
            foreach (var enumItem in Enum.GetValues(enumType))
            {
                var value = LanguageResource.ResourceManager.GetString("Enum_" + enumType.Name + "_" + (enumItem is null ? "" : enumItem.ToString()));
                if (!string.IsNullOrEmpty(value))
                {
                    list.Add(value);
                }
                else
                {
                    list.Add(enumItem.ToString());
                }
            }
            return list;
        }
    }
}