namespace NV.CT.Service.Common.Enums
{
    public enum ScanReconStatus
    {
        /// <summary>
        /// 未启动
        /// </summary>
        UnStarted,

        /// <summary>
        /// 运行中
        /// </summary>
        Inprogress,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled,

        /// <summary>
        /// 已完成
        /// </summary>
        Finished,

        /// <summary>
        /// 发生错误
        /// </summary>
        Error,

        /// <summary>
        /// 急停
        /// </summary>
        Emergency
    }

    public enum ScanReconStatusType
    {
        RealtimeStatus,
        AcqReconStatus,
    }
}