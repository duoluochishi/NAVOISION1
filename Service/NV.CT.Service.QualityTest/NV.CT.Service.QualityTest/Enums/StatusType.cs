namespace NV.CT.Service.QualityTest.Enums
{
    public enum StatusType
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// 等待采集重建
        /// </summary>
        WaitScanRecon,
        /// <summary>
        /// 采集重建中
        /// </summary>
        ScanRecon,
        /// <summary>
        /// 等待离线重建
        /// </summary>
        WaitOfflineRecon,
        /// <summary>
        /// 离线重建中
        /// </summary>
        OfflineRecon,
        /// <summary>
        /// 完成
        /// </summary>
        Complete,
        /// <summary>
        /// 取消
        /// </summary>
        Cancel,
        /// <summary>
        /// 发生错误
        /// </summary>
        Error,
    }
}