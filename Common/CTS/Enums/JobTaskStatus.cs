namespace NV.CT.CTS.Enums
{
    public enum JobTaskStatus
    {
        Unknown = 0,
        Queued,
        Processing,
        Completed,
        PartlyCompleted,
        Failed,
        Cancelled,
        Paused,
    }
}
