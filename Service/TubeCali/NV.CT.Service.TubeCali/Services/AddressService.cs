using System.Linq;
using NV.CT.Service.TubeCali.Models;
using NV.CT.Service.TubeCali.Services.Interface;

namespace NV.CT.Service.TubeCali.Services
{
    public class AddressService
    {
        private const uint TubeInterfaceBaseAddress = 0x0006A000;
        private const uint TubeInterfaceCaliBaseAddress = 0x04C8;
        public TubeAddress[] TubeAddressCollection { get; }

        public AddressService(IConfigService configService)
        {
            var (tubeCount, _, tubeCountPerTubeInterface) = configService.GetTubeAndTubeInterfaceCount();
            TubeAddressCollection = Enumerable.Range(1, tubeCount)
                                              .Select(i =>
                                               {
                                                   var tubeInterfaceNumber = (i - 1) / tubeCountPerTubeInterface;
                                                   var numberInTubeInterface = (i - 1) % tubeCountPerTubeInterface;
                                                   return new TubeAddress()
                                                   {
                                                       Number = i,
                                                       StatusAddress = CalculateAddress(tubeInterfaceNumber, TubeBaseAddress.StatusOffset[numberInTubeInterface]),
                                                       VoltageAddress = CalculateAddress(tubeInterfaceNumber, TubeBaseAddress.VoltageOffset[numberInTubeInterface]),
                                                       CurrentAddress = CalculateAddress(tubeInterfaceNumber, TubeBaseAddress.CurrentOffset[numberInTubeInterface]),
                                                       MsAddress = CalculateAddress(tubeInterfaceNumber, TubeBaseAddress.MsOffset[numberInTubeInterface]),
                                                       CaliAddress = CalculateAddress(tubeInterfaceNumber, TubeInterfaceCaliBaseAddress),
                                                       CaliValue = 1u << numberInTubeInterface,
                                                   };
                                               })
                                              .ToArray();
        }

        /// <summary>
        /// 计算校准地址
        /// </summary>
        /// <returns></returns>
        private uint CalculateAddress(int tubeInterfaceNumber, uint baseAddress)
        {
            return (uint)(TubeInterfaceBaseAddress + tubeInterfaceNumber * 0x10000 + baseAddress);
        }

        /// <summary>
        /// 关于Status、Voltage、Current、Ms，每个源板子的地址偏移量，每个数组18个是因为设计时的冗余，目前仅用到4个
        /// </summary>
        private static class TubeBaseAddress
        {
            public static readonly uint[] StatusOffset =
            {
                0x0268,
                0x026C,
                0x0270,
                0x0274,
                0x0278,
                0x027C,
                0x0280,
                0x0284,
                0x0288,
                0x028C,
                0x0290,
                0x0294,
                0x0298,
                0x029C,
                0x02A0,
                0x02A4,
                0x02A8,
                0x02AC
            };

            public static readonly uint[] VoltageOffset =
            {
                0x0124,
                0x0128,
                0x012C,
                0x0130,
                0x0134,
                0x0138,
                0x013C,
                0x0140,
                0x0144,
                0x0148,
                0x014C,
                0x0150,
                0x0154,
                0x0158,
                0x015C,
                0x0160,
                0x0164,
                0x0168,
            };

            public static readonly uint[] CurrentOffset =
            {
                0x00DC,
                0x00E0,
                0x00E4,
                0x00E8,
                0x00EC,
                0x00F0,
                0x00F4,
                0x00F8,
                0x00FC,
                0x0100,
                0x0104,
                0x0108,
                0x010C,
                0x0110,
                0x0114,
                0x0118,
                0x011C,
                0x0120,
            };

            public static readonly uint[] MsOffset =
            {
                0x016C,
                0x0170,
                0x0174,
                0x0178,
                0x017C,
                0x0180,
                0x0184,
                0x0188,
                0x018C,
                0x0190,
                0x0194,
                0x0198,
                0x019C,
                0x01A0,
                0x01A4,
                0x01A8,
                0x01AC,
                0x01B0,
            };
        }
    }
}