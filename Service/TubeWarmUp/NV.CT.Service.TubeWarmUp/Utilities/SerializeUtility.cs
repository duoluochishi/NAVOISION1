using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace NV.CT.Service.TubeWarmUp.Utilities
{
    public class SerializeUtility
    {
        public static string Serialize<T>(T instance)
        {
            return JsonSerializer.Serialize(instance, typeof(T));
        }
        public static T? Deserialize<T>(string data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }
    }
}
