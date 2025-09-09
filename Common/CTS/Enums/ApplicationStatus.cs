namespace NV.CT.CTS.Enums
{
    /// <summary>
    /// 进程状态（主控台进程控制）
    /// </summary>
    public enum ApplicationStatus
    {
        /// <summary>
        /// 进程启动中
        /// </summary>
        ApplicationStarting = 1,
        /// <summary>
        /// 进程运行中
        /// </summary>
        ApplicationRunning = 2,
        /// <summary>
        /// 进程关闭中
        /// </summary>
        ApplicationClosing = 3,
        /// <summary>
        /// 进程已关闭
        /// </summary>
        ApplicationClosed = 4,
        /// <summary>
        /// 无
        /// </summary>
        None = 5,
    }
}
