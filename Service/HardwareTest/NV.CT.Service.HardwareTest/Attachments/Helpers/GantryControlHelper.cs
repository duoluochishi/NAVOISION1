using NV.CT.Service.HardwareTest.Share.Utils;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    public static class GantryControlHelper
    {
        /// <summary>
        /// 机架是否到位
        /// </summary>
        public static bool IsGantryInPlace(uint value) => ByteUtils.GetBit(value, 12) == 1;
    }
}