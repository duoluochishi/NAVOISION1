using NV.CT.Service.HardwareTest.Share.Utils;
using System;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    public static class MotionControlHelper
    {
        /// <summary>
        /// 扫描床是否水平到位
        /// </summary>
        public static Func<uint, bool> IsTableHorizontallyInPlace = (uint value) => ByteUtils.GetBit(value, 12) == 1;
        /// <summary>
        /// 扫描床是否垂直到位
        /// </summary>
        public static Func<uint, bool> IsTableVerticallyInPlace = (uint value) => ByteUtils.GetBit(value, 13) == 1;
        /// <summary>
        /// 扫描床是否X轴到位
        /// </summary>
        public static Func<uint, bool> IsTableAxisXInPlace = (uint value) => ByteUtils.GetBit(value, 14) == 1;
    }
}
