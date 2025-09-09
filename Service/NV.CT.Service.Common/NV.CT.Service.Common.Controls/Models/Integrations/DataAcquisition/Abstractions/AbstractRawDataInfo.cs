using NV.CT.Service.Common.Controls.Enums;

namespace NV.CT.Service.Common.Controls.Models.Integrations.DataAcquisition.Abstractions
{
    public abstract class AbstractRawDataInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public PixelType PixelType { get; set; }
        public SupportInfo SupportInfo { get; set; } = new();
    }
}
