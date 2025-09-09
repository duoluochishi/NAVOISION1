//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Examination.ApplicationService.Impl;

public class ProtocolHostService : IProtocolHostService
{
    private readonly IProtocolStructureService _protocolStructureService;
    private readonly IProtocolModificationService _protocolModificationService;
    private readonly IProtocolPerformStatusService _protocolPerformStatusService;
    public string CurrentMeasurementID { get; private set; } = string.Empty;

    public bool IsPatientPositionChanged { get; private set; } = false;

    public ProtocolHostService(IProtocolStructureService protocolStructureService,
         IProtocolModificationService protocolModificationService,
         IProtocolPerformStatusService protocolPerformStatusService)
    {
        Instance = new ProtocolModel
        {
            Children = new List<FrameOfReferenceModel>()
        };
        Models = ProtocolHelper.Expand(Instance);
        _protocolStructureService = protocolStructureService;
        _protocolStructureService.ProtocolStructureChanged += ProtocolStructureService_StructureChanged;
        _protocolModificationService = protocolModificationService;
        _protocolModificationService.ParameterChanged += ProtocolModificationService_ParameterChanged;
        _protocolModificationService.VolumnChanged += ProtocolModificationService_VolumnChanged;
        _protocolModificationService.DescriptorChanged += ProtocolModificationService_DescriptorChanged;
        _protocolPerformStatusService = protocolPerformStatusService;
        _protocolPerformStatusService.PerformStatusChanged += ProtocolPerformStatusService_PerformStatusChanged;
    }

    private void ProtocolStructureService_StructureChanged(object? sender, EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)> e)
    {
        Models = ProtocolHelper.Expand(Instance);
        //Instance.IsEnhanced = Models.Any(m => m.Scan.IsEnhanced);
        StructureChanged?.Invoke(this, e);
    }

    private void ProtocolModificationService_ParameterChanged(object? sender, EventArgs<(BaseModel, List<string>)> e)
    {
        ParameterChanged?.Invoke(this, e);
    }

    private void ProtocolModificationService_VolumnChanged(object? sender, EventArgs<(BaseModel, List<string>)> e)
    {
        VolumnChanged?.Invoke(this, e);
    }

    private void ProtocolModificationService_DescriptorChanged(object? sender, EventArgs<BaseModel> e)
    {
        DescriptorChanged?.Invoke(this, e);
    }

    private void ProtocolPerformStatusService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
    {
        PerformStatusChanged?.Invoke(this, e);
    }

    public void SetModelName(BaseModel model, string modelName)
    {
        _protocolModificationService.SetModelName(model, modelName);
    }

    public void SetParameters(BaseModel model, List<ParameterModel> parameters)
    {
        _protocolModificationService.SetParameters(model, parameters);
    }

    public void SetParameters(Dictionary<BaseModel, List<ParameterModel>> modelParameters)
    {
        _protocolModificationService.SetParameters(modelParameters);
    }

    public void SetParameter<TParameter>(BaseModel model, string parameterName, TParameter parameter, bool linkModification = true)
    {
        _protocolModificationService.SetParameter(model, parameterName, parameter, linkModification);
    }

    public void SetPerformStatus(BaseModel instance, PerformStatus performStatus, FailureReasonType reasonType = FailureReasonType.None)
    {
        _protocolPerformStatusService.SetPerformStatus(instance, performStatus, reasonType);
    }

    public void ModifyFOR(string measurementId, PatientPosition newPatientPosition)
    {
        _protocolStructureService.ModifyFOR(Instance, measurementId, newPatientPosition);
    }

    public ProtocolModel Instance { get; set; }

    public IList<(FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan)> Models { get; private set; }

    public void AddProtocol(ProtocolModel sourceProtocol, List<string> removedMeasurementIds, bool isKeepLocalizer = false, bool isKeepFOR = false)
    {
        _protocolStructureService.AddProtocol(Instance, sourceProtocol, removedMeasurementIds, isKeepLocalizer, isKeepFOR);
    }

    public void ReplaceProtocol(ProtocolModel sourceProtocol, List<string> removedMeasurementIds, bool isKeepLocalizer = false, bool isKeepFOR = false)
    {
        var isModifyProtocolName = Instance is null || Instance.Children.IsEmpty();
        if (!isModifyProtocolName && Instance is not null && Instance.Children.Any())
        {
            var models = ProtocolHelper.Expand(Instance);
            isModifyProtocolName |= models.All(item => item.Scan.Status == PerformStatus.Unperform);
        }

        _protocolStructureService.ReplaceProtocol(Instance, sourceProtocol, removedMeasurementIds, isKeepLocalizer, isKeepFOR);

        if (isModifyProtocolName)
        {
            _protocolModificationService.SetModelName(Instance, sourceProtocol.Descriptor.Name);
        }
    }

    public void ResetProtocol(ProtocolModel protocolModel)
    {
        Instance = protocolModel;
        Models = ProtocolHelper.Expand(Instance);
        StructureChanged?.Invoke(this, new EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>((Instance, Instance, StructureChangeType.Replace)));
    }

    public void DeleteScan(string scanId)
    {
        _protocolStructureService.DeleteScan(Instance, scanId);
    }

    public void DeleteScan(List<string> scanIds)
    {
        _protocolStructureService.DeleteScan(Instance, scanIds);
    }

    public void PasteScan(string sourceScanId, string destinationScanId)
    {
        _protocolStructureService.PasteScan(Instance, sourceScanId, destinationScanId);
    }

    public void PasteScan(List<string> sourceScanIds, string destinationScanId)
    {
        _protocolStructureService.PasteScan(Instance, sourceScanIds, destinationScanId);
    }

    public void RepeatScan(string scanId)
    {
        _protocolStructureService.RepeatScan(Instance, scanId);
    }

    public void RepeatScan(List<string> scanIds)
    {
        _protocolStructureService.RepeatScan(Instance, scanIds);
    }

    public void DeleteRecon(string scanId, string reconId)
    {
        _protocolStructureService.DeleteRecon(Instance, scanId, reconId);
    }

    public void CopyRecon(string scanId, string reconId, bool isRTD = false)
    {
        _protocolStructureService.CopyRecon(Instance, scanId, reconId, isRTD);
    }

    public event EventHandler<EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>>? StructureChanged;

    public event EventHandler<EventArgs<(BaseModel, List<string>)>>? ParameterChanged;
    public event EventHandler<EventArgs<BaseModel>>? DescriptorChanged;
    public event EventHandler<EventArgs<(BaseModel, List<string>)>>? VolumnChanged;

    public event EventHandler<EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)>>? PerformStatusChanged;

    public event EventHandler<EventArgs<double>>? CenterPositionChanged;

    public void SetCurrentMeasurementID(string measurementId)
    {
        this.CurrentMeasurementID = measurementId;
    }
}