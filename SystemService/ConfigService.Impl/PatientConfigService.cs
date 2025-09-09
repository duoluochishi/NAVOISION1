//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/6 16:16:34     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Impl;
using NV.CT.ConfigService.Models.UserConfig;
using NV.MPS.Environment;

namespace NV.CT.ConfigService.Server.Services;

public class PatientConfigService : IPatientConfigService
{
    private readonly ConfigRepository<PatientConfig> _repository;

    public event EventHandler ConfigRefreshed;

    public PatientConfigService()
    {
        _repository = new ConfigRepository<PatientConfig>(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "ConfigService", "PatientConfig.xml"), false);
        _repository.ConfigRefreshed += PatientConfig_ConfigRefreshed;
    }

    private void PatientConfig_ConfigRefreshed(object? sender, EventArgs e)
    {
        ConfigRefreshed?.Invoke(sender, e);
    }

    public void Save(PatientConfig patientConfig)
    {
        _repository.Save(patientConfig);
    }
    
    public PatientConfig GetConfigs()
    {
        return _repository.GetConfigs();
    }
}
