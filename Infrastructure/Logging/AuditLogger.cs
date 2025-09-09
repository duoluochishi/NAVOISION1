using Newtonsoft.Json;

namespace NV.CT.Logging;

public static class AuditLogger
{
    public static void Log(AuditLogInfo logInfo)
    {
        AuditLogPipelineClient.Instance.PushMessage(JsonConvert.SerializeObject(logInfo));
    }
}
