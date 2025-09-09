using Newtonsoft.Json;

namespace NV.CT.CTS.Extensions;

public static class StringExtension
{
    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsNotNullOrEmpty(this string str)
    {
        return !string.IsNullOrEmpty(str);
    }

    public static T DeserializeTo<T>(this string str)
    {
        var ret = JsonConvert.DeserializeObject<T>(str);
        if (ret is not null)
        {
            return ret;
        }
        return Activator.CreateInstance<T>();
    }
}