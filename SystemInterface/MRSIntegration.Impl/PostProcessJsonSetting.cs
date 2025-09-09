using Newtonsoft.Json;
using NV.CT.FacadeProxy.Common.Converters.JsonConverters;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class PostProcessJsonSetting
{
    private static readonly Lazy<PostProcessJsonSetting> _instance = new Lazy<PostProcessJsonSetting>(() => new PostProcessJsonSetting());

    private JsonSerializerSettings _settings;

    private PostProcessJsonSetting()
    {
        _settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new PostProcessJsonConverter() }
        };
    }

    public static PostProcessJsonSetting Instance => _instance.Value;

    public JsonSerializerSettings Settings => _settings;
}
