//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/9/5 14:06:53           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.MPS.Communication;
using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.ClientProxy.DataService;

public class VoiceService : IVoiceService
{
    private readonly MCSServiceClientProxy _clientProxy;

    public VoiceService(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public List<VoiceModel> GetAll()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IVoiceService).Namespace,
            SourceType = nameof(IVoiceService),
            ActionName = nameof(IVoiceService.GetAll),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<VoiceModel>>(commandResponse.Data);
            return res;
        }
        return null;
    }

    public List<VoiceModel> GetValidVoices()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IVoiceService).Namespace,
            SourceType = nameof(IVoiceService),
            ActionName = nameof(IVoiceService.GetValidVoices),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<VoiceModel>>(commandResponse.Data);
            return res;
        }
        return null;
    }

    public List<VoiceModel> GetDefaultList()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IVoiceService).Namespace,
            SourceType = nameof(IVoiceService),
            ActionName = nameof(IVoiceService.GetDefaultList),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<VoiceModel>>(commandResponse.Data);
            return res;
        }
        return null;
    }

    public bool SetDefaultList(List<VoiceModel> list)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IVoiceService).Namespace,
            SourceType = nameof(IVoiceService),
            ActionName = nameof(IVoiceService.SetDefaultList),
            Data = list.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }
    public bool SetDefault(VoiceModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IVoiceService).Namespace,
            SourceType = nameof(IVoiceService),
            ActionName = nameof(IVoiceService.SetDefault),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }
    public bool Insert(VoiceModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IVoiceService).Namespace,
            SourceType = nameof(IVoiceService),
            ActionName = nameof(IVoiceService.Insert),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool Update(VoiceModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IVoiceService).Namespace,
            SourceType = nameof(IVoiceService),
            ActionName = nameof(IVoiceService.Update),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool Delete(VoiceModel model)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IVoiceService).Namespace,
            SourceType = nameof(IVoiceService),
            ActionName = nameof(IVoiceService.Delete),
            Data = model.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public VoiceModel GetVoiceInfo(string id)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IVoiceService).Namespace,
            SourceType = nameof(IVoiceService),
            ActionName = nameof(IVoiceService.GetVoiceInfo),
            Data = id
        });

        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<VoiceModel>(commandResponse.Data);
            return res;
        }
        return null;
    }

    public List<VoiceModel> GetAllByFrontType(string front)
    {
        CommandRequest commandRequest = new CommandRequest()
        {
            Namespace = typeof(IVoiceService).Namespace,
            SourceType = nameof(IVoiceService),
            ActionName = nameof(IVoiceService.GetAllByFrontType),
            Data = front
        };

        var commandResponse = _clientProxy.ExecuteCommand(commandRequest);

        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<VoiceModel>>(commandResponse.Data);
            return res;
        }
        return null;
    }
}