//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/20 8:25:30     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.CT.ProtocolService.Contract;
using NV.CT.Protocol.Models;

namespace NV.CT.ClientProxy;

public class ProtocolOperationService : IProtocolOperation
{
    private readonly MCSServiceClientProxy _clientProxy;

    public ProtocolOperationService(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }
#pragma warning disable 67
    public event EventHandler<string>? ProtocolChanged;
#pragma warning restore 67
    public void Delete(string templateId)
    {
        _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.Delete),
            Data = templateId
        });
    }
    public void Export(List<string> templateIds)
    {
        _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.Export),
            Data = JsonConvert.SerializeObject(templateIds)
        });
    }
    public void ExportProtocol(string templateId, string outPath)
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.ExportProtocol),
            Data = JsonConvert.SerializeObject((templateId, outPath))
        });

    }

    public List<ProtocolTemplateModel> GetAllProtocolTemplates()
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.GetAllProtocolTemplates),
            Data = string.Empty
        });
        if (response.Success)
        {
            return JsonConvert.DeserializeObject<List<ProtocolTemplateModel>>(response.Data);
        }
        return default;
    }

    public List<ProtocolPresentationModel> GetPresentations()
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.GetPresentations),
            Data = string.Empty
        });
        if (response.Success)
        {
            return JsonConvert.DeserializeObject<List<ProtocolPresentationModel>>(response.Data);
        }
        return default;
    }

    public ProtocolTemplateModel GetEmergencyProtocolTemplate()
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.GetEmergencyProtocolTemplate),
            Data = string.Empty
        });
        if (response.Success)
        {
            return JsonConvert.DeserializeObject<ProtocolTemplateModel>(response.Data);
        }
        return default;
    }

    public ProtocolTemplateModel GetProtocolTemplate(string templateId)
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.GetProtocolTemplate),
            Data = templateId
        });
        if (response.Success)
        {
            return JsonConvert.DeserializeObject<ProtocolTemplateModel>(response.Data);
        }
        return default;
    }

    public (bool, string) Import(List<string> files)
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.Import),
            Data = JsonConvert.SerializeObject(files)
        });
        if (response.Success)
        {
            return JsonConvert.DeserializeObject<(bool, string)>(response.Data);
        }
        return (false, "fail");
    }

    public void Save(ProtocolTemplateModel protocolTemplate, bool isTopping = false)
    {
        _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.Save),
            Data = JsonConvert.SerializeObject((protocolTemplate, isTopping))
        });
    }

    public List<ProtocolSettingItem> GetProtocolSettingItems()
    {
        var response = _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.GetProtocolSettingItems),
            Data = string.Empty
        });
        if (response.Success)
        {
            return JsonConvert.DeserializeObject<List<ProtocolSettingItem>>(response.Data);
        }
        return default;
    }

    public void SaveSettingItemList(List<ProtocolSettingItem> protocolSettings)
    {
        _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.SaveSettingItemList),
            Data = JsonConvert.SerializeObject(protocolSettings)
        });
    }

    public void SaveOrUpdate(ProtocolSettingItem protocolSetting)
    {
        _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.SaveOrUpdate),
            Data = JsonConvert.SerializeObject(protocolSetting)
        });
    }

    public void RemoveProtocolSetting(ProtocolSettingItem protocolSetting)
    {
        _clientProxy.ExecuteCommand(new MPS.Communication.CommandRequest
        {
            Namespace = typeof(IProtocolOperation).Namespace,
            SourceType = nameof(IProtocolOperation),
            ActionName = nameof(IProtocolOperation.RemoveProtocolSetting),
            Data = JsonConvert.SerializeObject(protocolSetting)
        });
    }
}