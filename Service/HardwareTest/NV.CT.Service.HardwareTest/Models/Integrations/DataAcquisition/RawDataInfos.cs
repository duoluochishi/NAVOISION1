using NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition.Abstractions;

namespace NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition
{
    public class UshortRawDataInfo : AbstractRawDataInfo
    {
        public UshortRawDataInfo(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.PixelType = Share.Enums.Integrations.PixelType.Ushort;
            this.Data = new ushort[width * height];
        }

        public ushort[] Data { get; set; } = null!;
    }

    public class FloatRawDataInfo : AbstractRawDataInfo
    {
        public FloatRawDataInfo(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.PixelType = Share.Enums.Integrations.PixelType.Float;
            this.Data = new float[width * height];
        }

        public float[] Data { get; set; } = null!;
    }
}
