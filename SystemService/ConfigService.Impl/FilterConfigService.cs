//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/2 16:16:34     V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Impl;
using NV.CT.ConfigService.Models.UserConfig;
using NV.MPS.Environment;

namespace NV.CT.ConfigService.Server.Services;

public class FilterConfigService : IFilterConfigService
{
    private readonly ConfigRepository<FilterConfig> _repository;

    public event EventHandler ConfigRefreshed;

    public FilterConfigService()
    {
        _repository = new ConfigRepository<FilterConfig>(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "ConfigService", "FilterConfig.xml"), false);
        _repository.ConfigRefreshed += OnConfigRefreshed;
    }

    private void OnConfigRefreshed(object? sender, EventArgs e)
    {
        ConfigRefreshed?.Invoke(sender, e);
    }

    public void Save(FilterConfig filterConfig)
    {
        _repository.Save(filterConfig);
    }
    
    public FilterConfig GetConfigs()
    {
        return _repository.GetConfigs();
    }
}
