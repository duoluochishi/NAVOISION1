//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/8 9:50:42    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.TP.NamedPipeWrapper;

namespace NV.CT.Logging;

public class AuditLogPipelineClient
{
    private readonly static Lazy<AuditLogPipelineClient> _instance = new Lazy<AuditLogPipelineClient>(() => new AuditLogPipelineClient());

    private readonly NamedPipeClient<string> _client;

    private AuditLogPipelineClient()
    {
        _client = new NamedPipeClient<string>("AuditLogging");
        _client.Start();
    }

    public static AuditLogPipelineClient Instance => _instance.Value;

    public void PushMessage(string message)
    {
        _client.PushMessage(message);
    }
}
