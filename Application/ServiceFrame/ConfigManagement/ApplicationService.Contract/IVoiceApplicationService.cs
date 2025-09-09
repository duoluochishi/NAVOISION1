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

using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract.Models;
namespace NV.CT.ConfigManagement.ApplicationService.Contract;

public interface IVoiceApplicationService
{
    event EventHandler<EventArgs<(OperationType operation, VoiceModel voiceModel)>> VoiceInfoChanged;

    event EventHandler VoiceListReload;

    void SetVoiceInfo(OperationType operation, VoiceModel voiceModel);

    void ReloadVoiceList();

    List<VoiceModel> GetVoiceInfo(string front);

    List<VoiceModel> GetVoiceModels();

    bool SetDefault(VoiceModel voiceModel);

    bool Add(VoiceModel voiceModel);

    bool Update(VoiceModel voiceModel);

    bool Delete(VoiceModel voiceModel);

    VoiceModel GetVoiceInfoByID(string id);

    ushort GetMaxInternalId();

    bool AddOrUpdate(VoiceModel voiceModel);
}