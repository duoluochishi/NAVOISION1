//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/21 9:51:00           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.CTS.Models
{
    public class RealtimeInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RealtimeStatus Status { get; set; }

        /// <summary>
        /// 仅在Status为Error的时候有效
        /// </summary>
        public List<string> ErrorCodes { get; set; } = new();

        /// <summary>
        /// 在Status暂未定，注射器信息，暂时不用
        /// </summary>
        public InjectorInfo? InjectorInfo { get; set; }

        /// <summary>
        /// 在Status暂未定，曝光状态，暂时不用
        /// </summary>
        public ExposureInfo? ExposureInfo { get; set; }

        /// <summary>
        /// 仅在Status为ExposureDelay时有效，曝光延迟
        /// </summary>
        public uint? ExposureDelay { get; set; }

        public string ScanId { get; set; } = string.Empty;
    }
}
