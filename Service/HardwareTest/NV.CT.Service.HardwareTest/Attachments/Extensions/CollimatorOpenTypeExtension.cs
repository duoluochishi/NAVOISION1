using NV.CT.FacadeProxy.Common.Enums.Collimator;
using NV.CT.Service.HardwareTest.Share.Enums.Integrations;
using System;

namespace NV.CT.Service.HardwareTest.Attachments.Extensions
{
    internal static class CollimatorOpenTypeExtension
    {
        public static CollimatorOpenMode ToOpenMode(this CollimatorValidOpenType openType) 
        {
            return openType.ToString().StartsWith("NearSmallAngle") ? CollimatorOpenMode.NearSmallAngle : CollimatorOpenMode.NearCenter;
        }

        public static uint ToOpenWidth(this CollimatorValidOpenType openType)
        {
            return Convert.ToUInt32(openType.ToString().Split("_")[1]);
        }

    }
}
