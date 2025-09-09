using CommunityToolkit.Mvvm.Messaging.Messages;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource.Coefficients;

namespace NV.CT.Service.HardwareTest.Attachments.Messages
{
    public class AddKVMAPairMessage : RequestMessage<(bool, string)>
    {
        public CoefficientModel Item { get; }

        public AddKVMAPairMessage(CoefficientModel item)
        {
            Item = item;
        }
    }
}