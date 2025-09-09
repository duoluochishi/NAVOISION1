//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/19 13:19:52     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
using NV.CT.CTS;
using NV.CT.Protocol.Models;
using NV.CT.AppService.Contract;

namespace NV.CT.ProtocolManagement.ApplicationService.Contract;

public interface IProtocolApplicationService
{
    ProtocolTemplateModel GetProtocolTemplate(string templateId);

    List<ProtocolTemplateModel> GetAllProtocolTemplates();

    void Save(ProtocolTemplateModel templateFileModel);

    (bool, string) Import(List<string> paths);

    void Delete(string templateId);

    bool DeleteScanTask(string templateId, string scanID);

    void DeleteMeasurementTask(string templateId, string measurementID);

    bool DeleteReconTask(string templateId, string reconID);

    void SetDeleteNode();

    void SetRepeatNode();
    void SetMoveNode(string moveType);
    void SetExpandNode();
    void SetCollapseNode();

    void Search(string searchTXT);

    string Repeat(string protocolTemplateID, string protocolName);

    void SendNewNodeName(string newNodeName);

    MeasurementModel RepeatMeasurementTask(string templateId, string measurementId);

    string RepeatScanTask(string templateId, string scanId, string newScanName);

    string RepeatReconTask(string templateId, string reconId, string newReconName);

    void ExcuteProtocolOperation(string operation);

    void SwitchEmergencyProtocol();
    void ChangeBodyPartForProtocol(string name);

    void FilterProtocolByCondition((string, bool, bool, bool, bool, bool, bool) filterCondition);

    void ChangeTreeNodeSelect((string NodeType, string NodeId, string TemplateId) nodyPara);

    bool Export(ProtocolTemplateModel templateFileModel, string outPath);

    ICollection<string> GetValuesNameToType(SystemSettingKeys kv);

    event EventHandler<EventArgs<string>> SearchButtonClicked;

    event EventHandler<string> ProtocolChanged;

    event EventHandler NodeDeleting;
    
    event EventHandler<EventArgs<string>> NodeMoving;

    event EventHandler NodeRepeating;

    event EventHandler NodeExpanding;

    event EventHandler NodeCollapsing;

    event EventHandler<EventArgs<string>> SelectBodyPartForProtocolChanged;

    event EventHandler<EventArgs<(string NodeType, string NodeId, string TemplateId)>> ProtocolTreeSelectNodeChanged;

    event EventHandler<EventArgs<string>> ProtocolOperationClicked;

    event EventHandler<EventArgs<ProtocolTemplateModel>> InputClick;

    event EventHandler<ApplicationResponse> ApplicationClosing;

    event EventHandler<ProtocolTemplateModel> EmergencyProtocolSwitching;

    event EventHandler<EventArgs<string>> NewNodeNameSended;

    event EventHandler<EventArgs<List<ProtocolTemplateModel>>> ProtocolConditionFilterChanging;
}
