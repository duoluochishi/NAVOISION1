using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.Protocol.Interfaces;
using NV.CT.Protocol.Models;
using NV.MPS.Exception;

namespace NV.CT.Protocol.Services
{
    public class ProtocolPerformStatusService : IProtocolPerformStatusService
    {
        public event EventHandler<EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)>>? PerformStatusChanged;

        public void SetPerformStatus(BaseModel instance, PerformStatus performStatus, FailureReasonType reasonType = FailureReasonType.None)
        {
            if (instance.Status == performStatus) return;

            var sourceStatus = instance.Status;
            instance.Status = performStatus;
            if (performStatus == PerformStatus.Performed && reasonType != FailureReasonType.None)
            {
                instance.FailureReason = reasonType;
            }
            PerformStatusChanged?.Invoke(this, new EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)>((instance, sourceStatus, performStatus)));
        }

        /// <summary>
        /// 此方法暂不支持ReconModel参数设置，待后续完善
        /// </summary>
        public void SetPerformStatus(ProtocolModel instance, string modelId, PerformStatus performStatus, FailureReasonType reasonType = FailureReasonType.None)
        {
            var items = ProtocolHelper.Expand(instance);
            BaseModel model = items.FirstOrDefault(item => item.Frame.Descriptor.Id == modelId).Frame;
            if (model is null)
            {
                model = items.FirstOrDefault(item => item.Measurement.Descriptor.Id == modelId).Measurement;
            }
            if (model is null)
            {
                model = items.FirstOrDefault(item => item.Scan.Descriptor.Id == modelId).Scan;
            }
            //TODO: ReconModel的处理暂缓实现
            if (model is null)
            {
                throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Instance_NotExist_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Instance_NotExist_Description, "Instance Protocol Model"), new ArgumentNullException("modelId"));
            }
            SetPerformStatus(model, performStatus, reasonType);
        }
    }
}
