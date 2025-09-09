//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/31 09:16:59     V1.0.0       张震
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Impl;
using NV.CT.ConfigService.Models.SystemConfig;
using NV.MPS.Environment;

namespace NV.CT.ConfigService.Server.Services;

//public class HardwareService : IHardwareService
//{
//    private readonly ConfigRepository<HardwareConfig> _repository;

//    public HardwareService()
//    {
//        _repository = new ConfigRepository<HardwareConfig>(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "ConfigService", "HardwareConfig.json"));
//        _repository.ConfigRefreshed += HardwareConfig_ConfigRefreshed;
//    }

//    private void HardwareConfig_ConfigRefreshed(object? sender, EventArgs e)
//    {
//        ConfigRefreshed?.Invoke(sender, e);
//    }

//    public event EventHandler ConfigRefreshed;

//    public HardwareConfig GetConfigs()
//    {
//        return _repository.GetConfigs();
//    }

//    public void Save(HardwareConfig hardwareConfig)
//    {
//        _repository.Save(hardwareConfig);
//    }
//}
