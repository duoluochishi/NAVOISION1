using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Services.Universal.EventData.Abstractions;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using System.Collections.Generic;

namespace NV.CT.Service.HardwareTest.Services.Universal.EventData
{
    public class EventDataAddressService : IEventDataAddressService
    {
        public EventDataAddressService()
        {
            Initialize();
        }

        #region Fields

        /** 地址记录 **/
        private Dictionary<uint, XRaySourceDose> doseInfoAddressDictionary = new Dictionary<uint, XRaySourceDose>();
        private Dictionary<uint, XRaySourceStatusInfo> statusInfoAddressDictionary = new Dictionary<uint, XRaySourceStatusInfo>();

        /** 板的数量 **/
        private readonly uint interfaceCount = 6;
        /** 每个板的射线源数量 **/
        private readonly uint XRayItemCountPerInterface = 4;
        /** 头地址 **/
        private readonly uint headerAddress = 0x0006A000;
        /** mA地址增量 **/
        private readonly uint[] baseAddressOffsetMA = new uint[18]
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
            0x0120
        };
        /** kV地址增量 **/
        private readonly uint[] baseAddressOffsetKV = new uint[18]
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
            0x0168
        };
        /** ms地址增量 **/
        private readonly uint[] baseAddressOffsetMS = new uint[18]
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
            0x01B0
        };
        /** status地址增量 **/
        private readonly uint[] baseAddressOffsetStatus = new uint[18]
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

        public uint CTBoxStatusAddress { get; } = 0xA064;

        #endregion

        /// <summary>
        /// 初始化地址Dictionary
        /// </summary>
        private void Initialize()
        {
            for (uint i = 0; i < interfaceCount; i++)
            {
                for (uint j = 0; j < XRayItemCountPerInterface; j++)
                {
                    uint index = i * XRayItemCountPerInterface + j + 1;
                    var address_mA = CalculateAddress(i + 1, baseAddressOffsetMA[j]);
                    var address_kV = CalculateAddress(i + 1, baseAddressOffsetKV[j]);
                    var address_ms = CalculateAddress(i + 1, baseAddressOffsetMS[j]);
                    var address_status = CalculateAddress(i + 1, baseAddressOffsetStatus[j]);
                    /** DoseInfo address **/
                    doseInfoAddressDictionary.Add(address_mA, new() { Index = index, Type = XRaySourceDoseType.mA, Address = address_mA });
                    doseInfoAddressDictionary.Add(address_kV, new() { Index = index, Type = XRaySourceDoseType.kV, Address = address_kV });
                    doseInfoAddressDictionary.Add(address_ms, new() { Index = index, Type = XRaySourceDoseType.ms, Address = address_ms });
                    /** StatusInfo address **/
                    statusInfoAddressDictionary.Add(address_status, new() { Index = index, Status = XRaySourceStatus.Online, Address = address_status });
                }
            }
        }

        /// <summary>
        /// 计算每个XRaySource对应参数的地址
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="offset">增量</param>
        /// <returns></returns>
        private uint CalculateAddress(uint index, uint offset)
        {
            return headerAddress + (index - 1) * 0x10000 + offset;
        }

        /// <summary>
        /// 根据地址匹配对应的doseinfo
        /// </summary>
        /// <param name="address"></param>
        /// <param name="doseInfo"></param>
        /// <returns></returns>
        public bool MatchDoseInfoAddress(uint address, out XRaySourceDose? doseInfo)
        {
            return doseInfoAddressDictionary.TryGetValue(address, out doseInfo);
        }

        /// <summary>
        /// 根据地址匹配相应的statusinfo
        /// </summary>
        /// <param name="address"></param>
        /// <param name="xRaySourceStatusInfo"></param>
        /// <returns></returns>
        public bool MatchStatusInfoAddress(uint address, out XRaySourceStatusInfo? xRaySourceStatusInfo)
        {
            return statusInfoAddressDictionary.TryGetValue(address, out xRaySourceStatusInfo);
        }

    }

}
