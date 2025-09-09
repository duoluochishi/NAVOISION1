//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/20 17:14:26           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NV.CT.CTS.Enums;

namespace NV.CT.CTS.Models
{
    public class ServiceStatusInfo
    {
        public string IP { get; set; } = string.Empty;

        public int Port { get; set; }

        public bool Connected { get; set; }
    }
}
