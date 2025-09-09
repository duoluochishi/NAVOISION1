//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/8 12:36:50     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.SystemConfig;

namespace NV.CT.ClientProxy.ConfigService;

public class ErrorCodeService : IErrorCodeService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public ErrorCodeService(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public event EventHandler ConfigRefreshed;

    public ErrorConfig GetConfigs()
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IErrorCodeService).Namespace,
            SourceType = nameof(IErrorCodeService),
            ActionName = nameof(IErrorCodeService.GetConfigs),
            Data = string.Empty
        });
        if (response is not null && response.Success)
        {
            return JsonConvert.DeserializeObject<ErrorConfig>(response.Data);
        }
        return default;
    }

    public void Save(ErrorConfig errorConfig)
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IErrorCodeService).Namespace,
            SourceType = nameof(IErrorCodeService),
            ActionName = nameof(IErrorCodeService.Save),
            Data = JsonConvert.SerializeObject(errorConfig)
        });
    }
}
