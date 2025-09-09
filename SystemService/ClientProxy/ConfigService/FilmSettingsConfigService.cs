//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/5 12:36:50     V1.0.0        胡安
// </summary>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;

namespace NV.CT.ClientProxy.ConfigService;

public class FilmSettingsConfigService : IFilmSettingsConfigService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public FilmSettingsConfigService(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public event EventHandler ConfigRefreshed;

    public FilmSettingsConfig GetConfigs()
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IFilmSettingsConfigService).Namespace,
            SourceType = nameof(IFilmSettingsConfigService),
            ActionName = nameof(IFilmSettingsConfigService.GetConfigs),
            Data = string.Empty
        });
        if (response is not null && response.Success)
        {
            return JsonConvert.DeserializeObject<FilmSettingsConfig>(response.Data);
        }
        return default;
    }

    public void Save(FilmSettingsConfig filmSettingsConfig)
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IFilmSettingsConfigService).Namespace,
            SourceType = nameof(IFilmSettingsConfigService),
            ActionName = nameof(IFilmSettingsConfigService.Save),
            Data = JsonConvert.SerializeObject(filmSettingsConfig)
        });
    }
}
