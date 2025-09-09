//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/1 9:52:11           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace NV.CT.LoggingServer;

public class LogInfo
{
    public DateTime CreateTime { get; set; }

    public string ClientName { get; set; }

    public string ClassName { get; set; }

    public string MethodName { get; set; }

    public LogLevel Level { get; set; }

    public Exception? Exception { get; set; }

    public string? Message { get; set; }
}
