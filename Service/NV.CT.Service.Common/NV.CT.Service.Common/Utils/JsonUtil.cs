using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace NV.CT.Service.Common.Utils
{
    public static class JsonUtil
    {
        public static string Serialize<T>(T instance, JsonConverter? converter = null)
        {
            var option = GetDefaultJsonOptions();

            if (converter != null)
            {
                option.Converters.Add(converter);
            }

            return JsonSerializer.Serialize(instance, typeof(T), option);
        }

        public static T? Deserialize<T>(string data, JsonConverter? converter = null)
        {
            var option = GetDefaultJsonOptions();

            if (converter != null)
            {
                option.Converters.Add(converter);
            }

            return JsonSerializer.Deserialize<T>(data, option);
        }

        public static JsonSerializerOptions GetDefaultJsonOptions()
        {
            var option = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs),
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                WriteIndented = true,
            };
            option.Converters.Add(new JsonStringEnumConverter());
            return option;
        }
    }
}