//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/8/2 12:36:50     V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;

namespace NV.CT.ClientProxy.ConfigService;

public class FilterConfigService : IFilterConfigService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public FilterConfigService(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public event EventHandler ConfigRefreshed;

    public FilterConfig GetConfigs()
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IFilterConfigService).Namespace,
            SourceType = nameof(IFilterConfigService),
            ActionName = nameof(IFilterConfigService.GetConfigs),
            Data = string.Empty
        });
        if (response is not null && response.Success)
        {
            return JsonConvert.DeserializeObject<FilterConfig>(response.Data);
        }
        return default;
    }

    public void Save(FilterConfig filterConfig)
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IFilterConfigService).Namespace,
            SourceType = nameof(IFilterConfigService),
            ActionName = nameof(IFilterConfigService.Save),
            Data = JsonConvert.SerializeObject(filterConfig)
        });
    }
}
