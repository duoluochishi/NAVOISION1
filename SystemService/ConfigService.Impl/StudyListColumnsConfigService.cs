//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/30 16:16:34     V1.0.0        胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Impl;
using NV.CT.ConfigService.Models.UserConfig;
using NV.MPS.Environment;

namespace NV.CT.ConfigService.Server.Services;

public class StudyListColumnsConfigService : IStudyListColumnsConfigService
{
    private readonly ConfigRepository<StudyListColumnsConfig> _repository;

    public event EventHandler ConfigRefreshed;

    public StudyListColumnsConfigService()
    {
        _repository = new ConfigRepository<StudyListColumnsConfig>(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "ConfigService", "StudyListColumnsConfig.xml"), false);
        _repository.ConfigRefreshed += Config_ConfigRefreshed;
    }

    private void Config_ConfigRefreshed(object? sender, EventArgs e)
    {
        ConfigRefreshed?.Invoke(sender, e);
    }

    public void Save(StudyListColumnsConfig patientConfig)
    {
        _repository.Save(patientConfig);
    }
    
    public StudyListColumnsConfig GetConfigs()
    {
        return _repository.GetConfigs();
    }
}
