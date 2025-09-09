using NV.CT.CTS.Models;

namespace NV.CT.Examination.ApplicationService.Contract.Interfaces;

public interface IScanControlService
{
    RealtimeCommandResult StartMeasurement(string measurementId);

    RealtimeCommandResult CancelMeasurement();
}