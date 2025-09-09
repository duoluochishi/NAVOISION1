//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/07/05 11:36:50     V1.0.0        an.hu
// </summary>
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.MPS.Environment;

namespace NV.CT.ConfigService.Impl
{
    public class FilmSettingsConfigService : IFilmSettingsConfigService
    {
        private readonly ConfigRepository<FilmSettingsConfig>? _repository;
        public event EventHandler? ConfigRefreshed;

        public FilmSettingsConfigService()
        {
            _repository = new ConfigRepository<FilmSettingsConfig>(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "ConfigService", "FilmSettings.xml"), false);
            _repository.ConfigRefreshed += PrintProtocolConfig_Refreshed; ;
        }

        private void PrintProtocolConfig_Refreshed(object? sender, EventArgs e)
        {
            ConfigRefreshed?.Invoke(sender, e);
        }

        public void Save(FilmSettingsConfig printProtocolConfig)
        {
            _repository?.Save(printProtocolConfig);
        }

        public FilmSettingsConfig GetConfigs()
        {
            return _repository.GetConfigs();
        }

    }
}
