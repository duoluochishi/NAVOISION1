//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/19 13:20:52     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS;
using NV.CT.Protocol.Models;
using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.Protocol;
using NV.CT.ProtocolService.Contract;
using Newtonsoft.Json;
using NV.CT.ConfigService.Contract;
using NV.CT.AppService.Contract;

namespace NV.CT.ProtocolManagement.ApplicationService.Impl;

public class ProtocolApplicationService : IProtocolApplicationService
{
    private readonly IProtocolOperation _protocolOperation;    
    IApplicationCommunicationService _applicationCommunicationService;
    public event EventHandler<EventArgs<string>> NodeMoving;
    public event EventHandler NodeDeleting;
    public event EventHandler NodeRepeating;
    public event EventHandler NodeExpanding;
    public event EventHandler NodeCollapsing;
    public event EventHandler<ProtocolTemplateModel> EmergencyProtocolSwitching;
    public event EventHandler<ApplicationResponse> ApplicationClosing;
    public event EventHandler<string> ProtocolChanged;
    public event EventHandler<EventArgs<string>> SelectBodyPartForProtocolChanged;
    public event EventHandler<EventArgs<(string NodeType, string NodeId, string TemplateId)>> ProtocolTreeSelectNodeChanged;
    public event EventHandler<EventArgs<string>> ProtocolOperationClicked;
    public event EventHandler<EventArgs<string>> SearchButtonClicked;
    public event EventHandler<EventArgs<ProtocolTemplateModel>> InputClick;
    public event EventHandler<EventArgs<string>> NewNodeNameSended;
    public event EventHandler<EventArgs<List<ProtocolTemplateModel>>> ProtocolConditionFilterChanging;

    public ProtocolApplicationService(IProtocolOperation protocolOperation, IApplicationCommunicationService applicationCommunicationService)
    {
        _protocolOperation = protocolOperation;        
        _protocolOperation.ProtocolChanged += ProtocolOperation_ProtocolChanged;
        _applicationCommunicationService = applicationCommunicationService;
        _applicationCommunicationService.NotifyApplicationClosing += _applicationCommunicationService_NotifyApplicationClosing;
        //_protocolService.ImportClick += OnInputClick;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ProtocolOperation_ProtocolChanged(object? sender, string e)
    {
        ProtocolChanged?.Invoke(sender, e);
    }

    private void _applicationCommunicationService_NotifyApplicationClosing(object? sender, ApplicationResponse e)
    {
        ApplicationClosing?.Invoke(sender, e);
    }

    private void OnInputClick(object? sender, EventArgs<ProtocolTemplateModel> e)
    {
        InputClick.Invoke(sender, e);
    }

    public ICollection<string> GetValuesNameToType(SystemSettingKeys key)
    {
        //todo:待修改
        //return _itemService.GetValuesNameToType(key);
        return default;
    }

    public List<ProtocolTemplateModel> GetAllProtocolTemplates()
    {
        var protocolTemplates = _protocolOperation.GetAllProtocolTemplates();
        return protocolTemplates;
    }

    public ProtocolTemplateModel GetProtocolTemplate(string templateId)
    {
        return _protocolOperation.GetProtocolTemplate(templateId);
    }

    /// <summary>
    /// 查询急诊协议并显示
    /// </summary>
    public void SwitchEmergencyProtocol()
    {
        var allProtocolTemplates = _protocolOperation.GetAllProtocolTemplates();
        if (allProtocolTemplates.Any(p => p.Protocol.IsEmergency))
        {
            EmergencyProtocolSwitching.Invoke(this,allProtocolTemplates.FirstOrDefault(p => p.Protocol.IsEmergency));
        }
    }

    public void ChangeBodyPartForProtocol(string name)
    {
        OnSelectBodyPartForProtocolChanged(name);
    }

    private void OnSelectBodyPartForProtocolChanged(string name)
    {
        SelectBodyPartForProtocolChanged.Invoke(this, new EventArgs<string>(name));
    }
    public void FilterProtocolByCondition((string,bool,bool,bool,bool,bool,bool) filterCondition)//string bodyPart
    {
        string upperOfBodyPart = filterCondition.Item1.ToUpper();
        var protocolTemp = GetAllProtocolTemplates().Where(pt => pt.Protocol.BodyPart.ToString().ToUpper() == upperOfBodyPart);
        protocolTemp = protocolTemp.Where(pt => (pt.IsAdult == true && filterCondition.Item2 == true) || (pt.IsAdult == false && filterCondition.Item3 == true));
        protocolTemp = protocolTemp.Where(pt => (pt.IsFactory == true && filterCondition.Item6 == true) || (pt.IsFactory == false && filterCondition.Item7 == true));
        
        ProtocolConditionFilterChanging.Invoke(this, new EventArgs<List<ProtocolTemplateModel>>(protocolTemp.ToList()));
    }

    public void ChangeTreeNodeSelect((string NodeType, string NodeId, string TemplateId) nodeTypeNodeIdAndTemplateID)
    {
        OnProtocolTreeSelectNodeChanged(nodeTypeNodeIdAndTemplateID);
    }

    private void OnProtocolTreeSelectNodeChanged((string NodeType, string NodeId, string TemplateId) nodeTypeNodeIdAndTemplateID)
    {
        ProtocolTreeSelectNodeChanged.Invoke(this, new EventArgs<(string NodeType, string NodeId, string TemplateId)>(nodeTypeNodeIdAndTemplateID));
    }

    public bool Export(ProtocolTemplateModel templateModel, string outPath)
    {
        _protocolOperation.ExportProtocol(templateModel.Descriptor.Id,  outPath);
        return true;
    }

    public (bool,string) Import(List<string> paths)
    {
        return _protocolOperation.Import(paths);
    }

    public void Search(string searchTXT)
    {
        SearchButtonClicked.Invoke(this, new EventArgs<string>(searchTXT));
    }

    public void ExcuteProtocolOperation(string operation)
    {
        ProtocolOperationClicked.Invoke(this, new EventArgs<string>(operation));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="templateFileModel"></param>
    public void Save(ProtocolTemplateModel templateModel)
    {
        _protocolOperation.Save(templateModel);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="templateId"></param>
    public void Delete(string templateId)
    {
        _protocolOperation.Delete(templateId);
    }
    public void SetMoveNode(string moveType)
    {
        NodeMoving.Invoke(this, new EventArgs<string>(moveType));
    }
    public void SetDeleteNode()
    {
        NodeDeleting.Invoke(this, EventArgs.Empty);
    }

    public void SetRepeatNode()
    {
        NodeRepeating.Invoke(this, EventArgs.Empty);
    }
    public void SetExpandNode()
    {
        NodeExpanding.Invoke(this, EventArgs.Empty);
    }
    public void SetCollapseNode()
    {
        NodeCollapsing.Invoke(this, EventArgs.Empty);
    }

    public string Repeat(string templateId, string protocolName)
    {
        var protocolTemplate = GetProtocolTemplate(templateId).Clone();
        ProtocolManagerHelper.ResetId(protocolTemplate);

        //TODO:目前没有对应窗口
        protocolTemplate.Descriptor.Name = protocolName;
        protocolTemplate.Protocol.Descriptor.Name = protocolName;
        protocolTemplate.Protocol.Parameters.FirstOrDefault(p=>p.Name==ProtocolParameterNames.PROTOCOL_IS_FACTORY).Value = 0.ToString();

        #region //TODO:需要弹出窗口确认名称
        //protocolTemplate.Descriptor.Name += IdGenerator.Next(0);//TODO:需要弹出窗口确认名称
        protocolTemplate.FullPath += IdGenerator.Next(0);
        protocolTemplate.FileName += IdGenerator.Next(0);
        #endregion

        Save(protocolTemplate);
        return protocolTemplate.Descriptor.Id;
    }

    public void SendNewNodeName(string newNodeName)
    {
        NewNodeNameSended?.Invoke(this, new EventArgs<string>(newNodeName));
    }

    public MeasurementModel RepeatMeasurementTask(string templateId, string measurementId)
    {
        var protocolTemplate = GetProtocolTemplate(templateId);

        MeasurementModel? measurementModel = new();
        bool isComplateRepeat = false;
        protocolTemplate.Protocol.Children.Find(FOR =>
        {
            return FOR.Children.Find(measurement =>
            {
                if (measurement.Descriptor.Id == measurementId)
                {
                    measurementModel = measurement.Clone();
                    ProtocolManagerHelper.ResetId(measurementModel);
                    FOR.Children.Add(measurementModel);
                }
                return isComplateRepeat;
            }) is not null;
        });
        Save(protocolTemplate);

        return measurementModel;
    }

    public string RepeatScanTask(string templateId, string scanId, string newScanName)
    {
        var protocolTemplate = GetProtocolTemplate(templateId);

        var models = ProtocolHelper.Expand(protocolTemplate.Protocol);

        var current = models.FirstOrDefault(m => m.Scan.Descriptor.Id == scanId);

        var cloneScan = current.Scan.Clone();
        cloneScan.Descriptor.Name = newScanName;
        ProtocolManagerHelper.ResetId(cloneScan);
        current.Measurement.Children.Add(cloneScan);

        Save(protocolTemplate);
        return cloneScan.Descriptor.Id;
    }

    public string RepeatReconTask(string templateId, string reconId, string newReconName)
    {
        var protocolTemplate = GetProtocolTemplate(templateId);

        string reconID = string.Empty;
        var scanWithRecon = from frameOfReference in protocolTemplate.Protocol.Children
                            from measurement in frameOfReference.Children
                            from scan in measurement.Children
                            from recon in scan.Children
                            where recon.Descriptor.Id == reconId
                            select new { Scan = scan, Recon = recon };
        if (scanWithRecon.Any())
        {
            var reconTask = scanWithRecon.First().Recon.Clone();
            reconTask.Descriptor.Name = newReconName;
            ProtocolManagerHelper.ResetId(reconTask);
            reconID = reconTask.Descriptor.Id;
            reconTask.IsRTD = false;
            scanWithRecon.First().Scan.Children.Add(reconTask);
            Save(protocolTemplate);
        }

        return reconID;
    }

    public void DeleteMeasurementTask(string templateId, string measurementID)
    {
        var protocolTemplate = GetProtocolTemplate(templateId);
        _ = protocolTemplate.Protocol.Children.FirstOrDefault(frameOfReference =>
        {
            var measurement = frameOfReference.Children.FirstOrDefault(m => m.Descriptor.Id == measurementID);
            if (measurement is null) return false;
            frameOfReference.Children.Remove(measurement);
            if (frameOfReference.Children.Count == 0)
            {
                protocolTemplate.Protocol.Children.Remove(frameOfReference);
            }
            return true;
        });
        Save(protocolTemplate);
    }

    public bool DeleteScanTask(string templateId, string scanID)
    {
        var protocolTemplate = GetProtocolTemplate(templateId);
        _ = protocolTemplate.Protocol.Children
            .Any(frameOfReference => frameOfReference.Children
            .Any(measurement => measurement.Children
            .Any(scan =>
            {
                if (scan.Descriptor.Id != scanID) return false;
                measurement.Children.Remove(scan);
                if (measurement.Children.Count != 0) return true;
                frameOfReference.Children.Remove(measurement);
                if (frameOfReference.Children.Count == 0)
                {
                    protocolTemplate.Protocol.Children.Remove(frameOfReference);
                }
                return true;
            })));
        Save(protocolTemplate);
        return true;
    }

    public bool DeleteReconTask(string templateId, string reconID)
    {
        var protocolTemplate = GetProtocolTemplate(templateId);
        _= protocolTemplate.Protocol.Children
            .Any(frameOfReference => frameOfReference.Children
            .Any(measurement => measurement.Children
            .Any(scan => scan.Children
            .Any(recon =>
            {
                if (recon.Descriptor.Id != reconID) return false;
                scan.Children.Remove(recon);
                if (scan.Children.Count != 0) return true;
                measurement.Children.Remove(scan);
                if (measurement.Children.Count != 0) return true;
                frameOfReference.Children.Remove(measurement);
                if (frameOfReference.Children.Count == 0)
                {
                    protocolTemplate.Protocol.Children.Remove(frameOfReference);
                }
                return true;
            }))));
        Save(protocolTemplate);
        return true;
    }
}
