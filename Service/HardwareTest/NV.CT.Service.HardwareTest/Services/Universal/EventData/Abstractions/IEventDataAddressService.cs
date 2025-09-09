using NV.CT.Service.HardwareTest.Models.Components.XRaySource;

namespace NV.CT.Service.HardwareTest.Services.Universal.EventData.Abstractions
{
    public interface IEventDataAddressService
    {
        public uint CTBoxStatusAddress { get; }

        bool MatchDoseInfoAddress(uint address, out XRaySourceDose? doseInfo);

        bool MatchStatusInfoAddress(uint address, out XRaySourceStatusInfo? xRaySourceStatusInfo);
    }
}