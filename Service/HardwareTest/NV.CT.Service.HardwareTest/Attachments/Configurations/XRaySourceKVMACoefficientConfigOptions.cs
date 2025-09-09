using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.Service.HardwareTest.Attachments.Configurations
{
    public class XRaySourceKVMACoefficientConfigOptions
    {
        /// <summary>
        /// 曝光时间，单位ms
        /// </summary>
        public float ExposureTime { get; set; }

        /// <summary>
        /// 帧时间，单位ms
        /// </summary>
        public float FrameTime { get; set; }

        /// <summary>
        /// 大/小焦点
        /// </summary>
        public FocalType Focus { get; set; }

        /// <summary>
        /// 扫描张数
        /// </summary>
        public uint FramesCount { get; set; }
    }
}