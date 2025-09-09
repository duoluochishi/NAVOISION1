//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/19 16:09:18     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.Protocol.Models;

namespace NV.CT.ProtocolService.Contract;

public interface IProtocolOperation
{
    List<ProtocolTemplateModel> GetAllProtocolTemplates();

    List<ProtocolPresentationModel> GetPresentations();

    ProtocolTemplateModel GetProtocolTemplate(string templateId);

    ProtocolTemplateModel GetEmergencyProtocolTemplate();

    void Save(ProtocolTemplateModel protocolTemplate, bool isTopping = false);

    void Delete(string templateId);

    void Export(List<string> templateIds);
    void ExportProtocol(string templateId, string outPath);

    (bool, string) Import(List<string> files);

    event EventHandler<string> ProtocolChanged;

    List<ProtocolSettingItem> GetProtocolSettingItems();

    void SaveSettingItemList(List<ProtocolSettingItem> protocolSettings);

    void SaveOrUpdate(ProtocolSettingItem protocolSetting);

    void RemoveProtocolSetting(ProtocolSettingItem protocolSetting);
}