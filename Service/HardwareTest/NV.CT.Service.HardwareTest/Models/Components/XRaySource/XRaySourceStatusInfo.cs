using NV.CT.Service.HardwareTest.Share.Enums.Components;

namespace NV.CT.Service.HardwareTest.Models.Components.XRaySource
{
    public class XRaySourceStatusInfo
    {
        public uint Index { get; set; }
        public uint Address { get; set; }
        public XRaySourceStatus Status { get; set; }
    }
}
