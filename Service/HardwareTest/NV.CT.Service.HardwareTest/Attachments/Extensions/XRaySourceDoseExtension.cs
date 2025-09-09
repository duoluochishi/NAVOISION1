using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using System;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    public static class XRaySourceDoseExtension
    {
        public static XRaySourceHistoryData ToXRaySourceHistoryData(this XRaySourceDose dose) 
        {
            return new XRaySourceHistoryData()
            {
                Index = dose.Index,
                TimeStamp = DateTime.Now,
                Value = dose.Value,
                DataType = dose.Type switch 
                {
                    XRaySourceDoseType.kV => XRaySourceHistoryDataType.kV,
                    XRaySourceDoseType.mA => XRaySourceHistoryDataType.mA,
                    XRaySourceDoseType.ms => XRaySourceHistoryDataType.ms,
                    _ => throw new NotImplementedException()
                },
            };
        }
    }
}
