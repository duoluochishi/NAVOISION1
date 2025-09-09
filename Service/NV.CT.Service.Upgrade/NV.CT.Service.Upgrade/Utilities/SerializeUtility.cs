using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace NV.CT.Service.Upgrade.Utilities
{
    public class SerializeUtility
    {
        public static string JsonSerialize<T>(T instance)
        {
            return JsonSerializer.Serialize(instance, typeof(T), new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs),
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });
        }

        public static T? JsonDeserialize<T>(string data)
        {
            var option = new JsonSerializerOptions()
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };
            option.Converters.Add(new JsonStringEnumConverter());
            return JsonSerializer.Deserialize<T>(data, option);
        }
    }
}