using Newtonsoft.Json;

namespace NV.CT.CTS.Extensions
{
    public static class ObjectExtension
    {
        public static T Clone<T>(this T source) where T : notnull
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            //var temp = JsonConvert.SerializeObject(source);
            var temp = JsonConvert.SerializeObject(source, settings);
            return JsonConvert.DeserializeObject<T>(temp) ?? Activator.CreateInstance<T>();
        }

        public static string ToJson(this object item)
        {
            return JsonConvert.SerializeObject(item);
        }
    }
}