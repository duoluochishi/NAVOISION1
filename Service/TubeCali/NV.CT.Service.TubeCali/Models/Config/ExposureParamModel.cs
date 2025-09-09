namespace NV.CT.Service.TubeCali.Models.Config
{
    public class ExposureParamModel
    {
        /// <summary>
        /// 扫描管电压
        /// <para>单位：千伏</para>
        /// </summary>
        public uint KV { get; init; }

        /// <summary>
        /// 扫描管电流
        /// <para>单位：毫安</para>
        /// </summary>
        public uint MA { get; init; }

        /// <summary>
        /// 曝光时间
        /// <para>单位：毫秒(ms)</para>
        /// </summary>
        public float ExposureTime { get; init; }

        /// <summary>
        /// 帧时间
        /// <para>单位：毫秒(ms)</para>
        /// </summary>
        public float FrameTime { get; init; }

        /// <summary>
        /// 扫描圈数
        /// </summary>
        public uint CycleCount { get; init; }
    }
}