//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/9/5 14:00:48           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.DatabaseService.Impl.Repository;

namespace NV.CT.DatabaseService.Impl;

public class VoiceService : IVoiceService
{
    private readonly VoiceRepository _repository;

    public VoiceService(VoiceRepository repository)
    {
        _repository = repository;
    }

    public List<VoiceModel> GetAll()
    {
        return _repository.GetAll();
    }

    public List<VoiceModel> GetValidVoices()
    {
        return _repository.Get(true);
    }

    public List<VoiceModel> GetDefaultList()
    {
        return _repository.GetDefaultList();
    }

    public bool SetDefaultList(List<VoiceModel> list)
    {
        return _repository.SetDefaultList(list);
    }

    public bool SetDefault(VoiceModel model)
    {
        return _repository.SetDefault(model);
    }

    public bool Insert(VoiceModel voiceModel)
    {
        return _repository.Insert(voiceModel);
    }

    public bool Update(VoiceModel voiceModel)
    {
        return _repository.Update(voiceModel);
    }

    public bool Delete(VoiceModel voiceModel)
    {
        return _repository.Delete(voiceModel);
    }

    public List<VoiceModel> GetAllByFrontType(string front)
    {
        return _repository.GetAllByFrontType(front);
    }

    public VoiceModel GetVoiceInfo(string id)
    {
        return _repository.GetVoiceInfo(id);
    }
}