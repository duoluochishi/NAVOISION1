using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace NV.CT.Service.TubeCali.Utils
{
    internal static class JsonUtil
    {
        public static string Serialize<T>(T instance, JsonConverter? converter = null)
        {
            var option = GetJsonOptions();

            if (converter != null)
            {
                option.Converters.Add(converter);
            }

            return JsonSerializer.Serialize(instance, typeof(T), option);
        }

        public static T? Deserialize<T>(string data, JsonConverter? converter = null)
        {
            var option = GetJsonOptions();

            if (converter != null)
            {
                option.Converters.Add(converter);
            }

            return JsonSerializer.Deserialize<T>(data, option);
        }

        public static JsonSerializerOptions GetJsonOptions()
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