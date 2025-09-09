using NV.CT.Service.HardwareTest.Share.Enums.Components;

namespace NV.CT.Service.HardwareTest.Models.Components.XRaySource
{
    public class XRaySourceDose
    {
        public uint Index { get; set; }
        public XRaySourceDoseType Type { get; set; }
        public uint Address { get; set; }
        public float Value { get; set; }
    }
}
