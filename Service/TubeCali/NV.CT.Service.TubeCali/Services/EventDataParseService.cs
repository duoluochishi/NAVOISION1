using System;
using System.Buffers.Binary;
using System.Linq;
using NV.CT.Service.TubeCali.Enums;
using NV.CT.Service.TubeCali.Models;

namespace NV.CT.Service.TubeCali.Services
{
    public class EventDataParseService(AddressService addressService)
    {
        public bool ParseTubeStatus(byte[] data, out (int TubeNumber, ComponentCaliStatus? Status) tube)
        {
            var parseRes = Parse(data, (item, address) => item.StatusAddress == address);

            if (parseRes == null)
            {
                tube = (0, 0);
                return false;
            }

            var status = ParseCalibrationStatus(parseRes.Value.Value);
            tube = (parseRes.Value.Number, status);
            return true;
        }

        public bool ParseVoltageStatus(byte[] data, out (int TubeNumber, uint Value) tube)
        {
            var parseRes = Parse(data, (item, address) => item.VoltageAddress == address);

            if (parseRes == null)
            {
                tube = (0, 0);
                return false;
            }

            tube = (parseRes.Value.Number, parseRes.Value.Value);
            return true;
        }

        public bool ParseCurrentStatus(byte[] data, out (int TubeNumber, uint Value) tube)
        {
            var parseRes = Parse(data, (item, address) => item.CurrentAddress == address);

            if (parseRes == null)
            {
                tube = (0, 0);
                return false;
            }

            tube = (parseRes.Value.Number, parseRes.Value.Value);
            return true;
        }

        public bool ParseMsStatus(byte[] data, out (int TubeNumber, uint Value) tube)
        {
            var parseRes = Parse(data, (item, address) => item.MsAddress == address);

            if (parseRes == null)
            {
                tube = (0, 0);
                return false;
            }

            tube = (parseRes.Value.Number, parseRes.Value.Value);
            return true;
        }

        private (int Number, uint Value)? Parse(byte[] data, Func<TubeAddress, uint, bool> func)
        {
            var address = BinaryPrimitives.ReadUInt32BigEndian(data);
            var currentItem = addressService.TubeAddressCollection.FirstOrDefault(i => func(i, address));
            return currentItem == null ? null : (currentItem.Number, BinaryPrimitives.ReadUInt32BigEndian(data[4..8]));
        }

        private ComponentCaliStatus? ParseCalibrationStatus(uint statusValue)
        {
            if (BitEqual(statusValue, 6))
            {
                return ComponentCaliStatus.Working;
            }

            if (BitEqual(statusValue, 7))
            {
                return ComponentCaliStatus.Success;
            }

            if (BitEqual(statusValue, 8))
            {
                return ComponentCaliStatus.Failed;
            }

            return null;

            bool BitEqual(uint value, int index) => (value >> index) % 2 == 1;
        }
    }
}