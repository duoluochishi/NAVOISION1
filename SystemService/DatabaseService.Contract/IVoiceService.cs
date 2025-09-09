//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/9/5 13:59:46           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.DatabaseService.Contract;

public interface IVoiceService
{
    List<VoiceModel> GetAll();
    List<VoiceModel> GetValidVoices();

    List<VoiceModel> GetDefaultList();

    bool SetDefaultList(List<VoiceModel> list);
    bool SetDefault(VoiceModel model);
    bool Insert(VoiceModel voiceModel);
    bool Update(VoiceModel voiceModel);
    bool Delete(VoiceModel voiceModel);

    VoiceModel GetVoiceInfo(string id);
    List<VoiceModel> GetAllByFrontType(string front);
}