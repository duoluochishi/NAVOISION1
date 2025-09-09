using NV.CT.Language;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace NV.CT.ProtocolManagement.ViewModels.Common
{
    public static class EnumConverter
    {
        public static Dictionary<string, EnumViewModel<T>> ToDic<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return values.Cast<T>().ToDictionary(e => e.GetDescription(), e => new EnumViewModel<T>
            {
                EnumValue = e,
                StrValue = e.ToString()
            });
        }
    }

    public class EnumViewModel<T> where T : Enum
    {
        public T EnumValue { get; set; }
        public string StrValue { get; set; }
    }

    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
            var resourceKey = $"Enum_{value.GetType().Name}_{value}";
            var languageName = LanguageResource.ResourceManager.GetString(resourceKey);
            return attribute?.Description ?? languageName ?? value.ToString();
        }
    }
}
