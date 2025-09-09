//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.DatabaseService.Contract;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.CTS.Models;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Environment;

namespace NV.CT.ConfigManagement.ApplicationService.Impl;

public class VoiceApplicationService : IVoiceApplicationService
{
    private readonly IVoiceService _voiceService;
    private readonly IRealtimeVoiceService _realtimeVoiceService;

    public event EventHandler<EventArgs<(OperationType operation, VoiceModel voiceModel)>>? VoiceInfoChanged;
    public event EventHandler? VoiceListReload;

    public VoiceApplicationService(IVoiceService voiceService,
        IRealtimeVoiceService realtimeVoiceService)
    {
        _voiceService = voiceService;
        _realtimeVoiceService = realtimeVoiceService;
    }

    public void SetVoiceInfo(OperationType operation, VoiceModel voiceModel)
    {
        VoiceInfoChanged?.Invoke(this, new EventArgs<(OperationType operation, VoiceModel voiceModel)>((operation, voiceModel)));
    }

    public void ReloadVoiceList()
    {
        VoiceListReload?.Invoke(this, new EventArgs());
    }

    public List<VoiceModel> GetVoiceInfo(string front)
    {
        return _voiceService.GetAllByFrontType(front);
    }

    public bool SetDefault(VoiceModel voiceModel)
    {
        return _voiceService.SetDefault(voiceModel);
    }

    public List<VoiceModel> GetVoiceModels()
    {
        return _voiceService.GetValidVoices();
    }

    public bool Add(VoiceModel voiceModel)
    {
        return _voiceService.Insert(voiceModel);
    }
    public bool Update(VoiceModel voiceModel)
    {
        return _voiceService.Update(voiceModel);
    }

    public bool Delete(VoiceModel voiceModel)
    {
        bool flag = false;
        try
        {
            RealtimeCommandResult result = _realtimeVoiceService.Delete(voiceModel.InternalId);
            if (result.Status != CommandExecutionStatus.Success)
            {
                return false;
            }
            //删除语音文件
            var fullPath = Path.Combine(RuntimeConfig.Console.MCSVoices.Path, voiceModel.FilePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            voiceModel.IsValid = false;
            flag = _voiceService.Update(voiceModel);
        }
        catch (Exception ex)
        {
            flag = false;
            throw new Exception(ex.Message, ex.InnerException);
        }

        return flag;
    }

    public VoiceModel GetVoiceInfoByID(string id)
    {
        return _voiceService.GetVoiceInfo(id);
    }

    public ushort GetMaxInternalId()
    {
        var list = _voiceService.GetAll();
        var max = (ushort)0;
        if (list.Count > 0)
        {
            max = list.Max(x => x.InternalId);
        }
        return max;
    }

    public bool AddOrUpdate(VoiceModel voiceModel)
    {
        bool flag = false;
        var filePath = Path.Combine(RuntimeConfig.Console.MCSVoices.Path, voiceModel.FilePath);
        if (File.Exists(filePath))
        {
            flag = CommandExecutionStatus.Success == _realtimeVoiceService.AddOrUpdate(voiceModel.InternalId, filePath).Status;
        }
        return flag;
    }
}