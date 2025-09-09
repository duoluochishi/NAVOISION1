using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using NV.CT.FacadeProxy.Extensions;
using NV.CT.FacadeProxy.Registers.Helpers;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Helper;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Models;
using NV.CT.Service.TubeCali.Models;

namespace NV.CT.Service.TubeCali.Services
{
    public class TubeCaliService(ILogService logger)
    {
        public GenericResponse StartTubeCali(IList<SourceComponent> sourceComponents)
        {
            return GenerateAndSendCommand(sourceComponents, false);
        }

        public GenericResponse StopTubeCali(IList<SourceComponent> sourceComponents)
        {
            return GenerateAndSendCommand(sourceComponents, true);
        }

        private GenericResponse GenerateAndSendCommand(IList<SourceComponent> sourceComponents, bool isStop)
        {
            if (sourceComponents.Count == 0)
            {
                return new GenericResponse(true, string.Empty);
            }

            var command = sourceComponents.GroupBy(i => i.Address.CaliAddress)
                                          .SelectMany(i =>
                                           {
                                               var value = isStop ? 0 : i.Aggregate(0u, (v, item) => v | item.Address.CaliValue);
                                               return GenerateRegisterAddressValue(i.Key, value);
                                           })
                                          .ToArray();
            logger.Info(ServiceCategory.SourceComponentCali, $"[TubeCali] {(isStop ? "Stop" : "Start")} TubeCali, Tube: {string.Join(' ', sourceComponents.Select(i => $" {i.Number}({i.NumberOfTubeInterface}-{i.NumberInTubeInterface})"))}, SendCommand: {HexStringHelper.BytesToHexString(command)}");
            var sendRes = CommandHelper.SendWriteCommand(command);
            return sendRes.Success() ? new GenericResponse(true, string.Empty) : new GenericResponse(false, sendRes.ErrorCodes.Codes.First());
        }

        private byte[] GenerateRegisterAddressValue(uint address, uint value)
        {
            Span<byte> result = new byte[8];
            BinaryPrimitives.WriteUInt32BigEndian(result[..4], address);
            BinaryPrimitives.WriteUInt32BigEndian(result[4..], value);
            return result.ToArray();
        }
    }
}