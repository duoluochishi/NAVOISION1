namespace NV.CT.Service.HardwareTest.Attachments.Configurations
{
    public class DataAcquisitionConfigOptions
    {
        /** 生数据读取间隔（即多少帧存一次） **/
        public int RawDataReadInterval { get; set; } = 24;
        /** 射线源数量 **/
        public uint XRaySourceCount { get; set; } = 24;
        /** 限束器数量 **/
        public uint CollimatorSourceCount { get; set; } = 24;
        /** 探测器模组数量 **/
        public uint DetectorSourceCount { get; set; } = 16;
        /** GrayLevel计算范围宽度 **/
        public uint GrayLevelCalculateWidth { get; set; } = 256;
        /** GrayLevel计算范围高度 **/
        public uint GrayLevelCalculateHeight { get; set; } = 100;
    }
}
