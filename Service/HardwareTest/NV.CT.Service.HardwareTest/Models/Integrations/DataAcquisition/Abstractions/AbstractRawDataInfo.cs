using NV.CT.Service.HardwareTest.Share.Enums.Integrations;

namespace NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition.Abstractions
{
    public abstract class AbstractRawDataInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public PixelType PixelType { get; set; }
        public SupportInfo SupportInfo { get; set; } = new();
    }
}
