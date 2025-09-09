//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Protocol.Models;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface IProtocolHostService
{
    event EventHandler<EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>> StructureChanged;

    event EventHandler<EventArgs<(BaseModel, List<string>)>>? ParameterChanged;
    event EventHandler<EventArgs<BaseModel>>? DescriptorChanged;
    event EventHandler<EventArgs<(BaseModel, List<string>)>>? VolumnChanged;
    event EventHandler<EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)>>? PerformStatusChanged;

    string CurrentMeasurementID { get; }

    ProtocolModel Instance { get; set; }

    IList<(FrameOfReferenceModel Frame, MeasurementModel Measurement, ScanModel Scan)> Models { get; }

    bool IsPatientPositionChanged { get; }

    void SetModelName(BaseModel model, string modelName);

    void SetParameters(BaseModel model, List<ParameterModel> parameters);

    void SetParameters(Dictionary<BaseModel, List<ParameterModel>> modelParameters);

    void SetParameter<TParameter>(BaseModel model, string parameterName, TParameter parameter, bool linkModification = true);

    void SetPerformStatus(BaseModel instance, PerformStatus performStatus, FailureReasonType reasonType = FailureReasonType.None);

    void AddProtocol(ProtocolModel sourceProtocol, List<string> removedMeasurementIds, bool isKeepLocalizer = false, bool isKeepFOR = false);

    void ReplaceProtocol(ProtocolModel sourceProtocol, List<string> removedMeasurementIds, bool isKeepLocalizer = false, bool isKeepFOR = false);

    void ResetProtocol(ProtocolModel protocolModel);

    void ModifyFOR(string measurementId, PatientPosition newPatientPosition);

    void DeleteScan(string scanId);

    void DeleteScan(List<string> scanIds);

    void PasteScan(string sourceScanId, string destinationScanId);

    void PasteScan(List<string> sourceScanIds, string destinationScanId);

    void RepeatScan(string scanId);

    void RepeatScan(List<string> scanIds);

    void DeleteRecon(string scanId, string reconId);

    void CopyRecon(string scanId, string reconId, bool isRTD = false);

    void SetCurrentMeasurementID(string measurementId);
}