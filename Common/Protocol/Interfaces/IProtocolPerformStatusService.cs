using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.Protocol.Models;

namespace NV.CT.Protocol.Interfaces
{
    public interface IProtocolPerformStatusService
    {
        void SetPerformStatus(BaseModel instance, PerformStatus performStatus, FailureReasonType reasonType = FailureReasonType.None);

        void SetPerformStatus(ProtocolModel instance, string modelId, PerformStatus performStatus, FailureReasonType reasonType = FailureReasonType.None);

        event EventHandler<EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)>> PerformStatusChanged;
    }
}
