using NV.CT.Service.HardwareTest.Share.Enums.Components;
using System;

namespace NV.CT.Service.HardwareTest.Models.Components.XRaySource
{
    public class XRaySourceHistoryData
    {
        public uint Index { get; set; }
        public DateTime TimeStamp { get; set; }
        public XRaySourceHistoryDataType DataType { get; set; }
        public float Value { get; set; }
    }
}
