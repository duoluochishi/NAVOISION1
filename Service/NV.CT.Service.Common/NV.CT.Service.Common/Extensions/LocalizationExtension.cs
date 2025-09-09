using NV.CT.Service.Common.Resources;
using System.Text.RegularExpressions;

namespace NV.CT.Service.Common.Extensions
{
    public static class LocalizationExtension
    {
        public static string GetLocalizationStr(this string? str)
        {
            return string.IsNullOrWhiteSpace(str) ? string.Empty : Regex.Replace(str, @"{(?<name>\S+?)}", m =>
            {
                var value = Config_Lang.ResourceManager.GetString(m.Groups["name"].Value);
                return value ?? m.Value;
            });
        }
    }
}