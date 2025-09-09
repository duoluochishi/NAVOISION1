//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/8 9:39:26    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.LoggingServer;

public class AuditLogInfo
{
    public string EventType { get; set; }

    public string UserName { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime EndTime { get; set; }

    public string ModuleName { get; set; }

    public string EntryPoint { get; set; }

    public string OriginalValues { get; set; }

    public string CurrentValues { get; set; }

    public string ReturnValues { get; set; }

    public string Resources { get; set; }

    public string Description { get; set; }

    public float ExecutionTime { get; set; }
}
