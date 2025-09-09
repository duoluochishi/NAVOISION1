using NV.CT.CTS;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Protocol.Models;

namespace NV.CT.Protocol.Interfaces;
public interface IProtocolStructureService
{
    bool AddProtocol(ProtocolModel instanceProtocol, ProtocolModel sourceProtocol, List<string> removedMeasurementIds, bool isKeepLocalizer, bool isKeepFOR);

    bool ReplaceProtocol(ProtocolModel instanceProtocol, ProtocolModel sourceProtocol, List<string> removedMeasurementIds, bool isKeepLocalizer, bool isKeepFOR);

    bool DeleteScan(ProtocolModel instanceProtocol, string scanId);

    bool DeleteScan(ProtocolModel instanceProtocol, List<string> scanIds);

    bool PasteScan(ProtocolModel instanceProtocol, string sourceScanId, string destinationScanId);

    bool PasteScan(ProtocolModel instanceProtocol, List<string> sourceScanIds, string destinationScanId);

    bool RepeatScan(ProtocolModel instanceProtocol, string scanId);

    bool RepeatScan(ProtocolModel instanceProtocol, List<string> scanIds);

    bool ModifyFOR(ProtocolModel instanceProtocol, string measurementId, PatientPosition patientPosition);

    bool CopyRecon(ProtocolModel instanceProtocol, string scanId, string sourceReconId, bool isRTD = false);

    bool DeleteRecon(ProtocolModel instanceProtocol, string scanId, string reconId);

    event EventHandler<EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)>> ProtocolStructureChanged;
}