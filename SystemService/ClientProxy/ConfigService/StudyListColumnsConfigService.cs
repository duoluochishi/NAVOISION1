//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/30 12:36:50     V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;

namespace NV.CT.ClientProxy.ConfigService;

public class StudyListColumnsConfigService : IStudyListColumnsConfigService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public StudyListColumnsConfigService(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public event EventHandler ConfigRefreshed;

    public StudyListColumnsConfig GetConfigs()
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IStudyListColumnsConfigService).Namespace,
            SourceType = nameof(IStudyListColumnsConfigService),
            ActionName = nameof(IStudyListColumnsConfigService.GetConfigs),
            Data = string.Empty
        });
        if (response is not null && response.Success)
        {
            return JsonConvert.DeserializeObject<StudyListColumnsConfig>(response.Data);
        }
        return default;
    }

    public void Save(StudyListColumnsConfig studyListColumnsConfig)
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IStudyListColumnsConfigService).Namespace,
            SourceType = nameof(IStudyListColumnsConfigService),
            ActionName = nameof(IStudyListColumnsConfigService.Save),
            Data = JsonConvert.SerializeObject(studyListColumnsConfig)
        });
    }
}
