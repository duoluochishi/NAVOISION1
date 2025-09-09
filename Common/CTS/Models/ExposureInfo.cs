//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/21 9:41:39           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.CTS.Models
{
    /// <summary>
    /// 曝光状态下实时状态信息的数据部分
    /// </summary>
    public class ExposureInfo
    {
        /// <summary>
        /// 总时间
        /// </summary>
        public uint TotalTime { get; set; }

        /// <summary>
        /// 剩余时间
        /// </summary>
        public uint RemainedTime { get; set; }

        /// <summary>
        /// 总时间
        /// </summary>
        public uint TotalSeries { get; set; }

        /// <summary>
        /// 剩余次数
        /// </summary>
        public uint RemainedSeries { get; set; }
    }
}
