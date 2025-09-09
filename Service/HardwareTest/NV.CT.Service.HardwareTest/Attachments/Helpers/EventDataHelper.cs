using NV.CT.Service.HardwareTest.Share.Models;
using System;
using System.Linq;

namespace NV.CT.Service.HardwareTest.Attachments.Helpers
{
    public static class EventDataHelper
    {
        /// <summary>
        /// 将EventData的数据解析成地址-内容对
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static AddressContentPair ParseEventData(byte[] result)
        {
            byte[] tempAddress = new byte[4];
            byte[] tempData = new byte[4];

            Buffer.BlockCopy(result, 0, tempAddress, 0, 4);
            Buffer.BlockCopy(result, 4, tempData, 0, 4);

            uint address = BitConverter.ToUInt32(tempAddress.Reverse().ToArray(), 0);
            uint data = BitConverter.ToUInt32(tempData.Reverse().ToArray(), 0);

            return new AddressContentPair(address, data);
        }
    }
}
