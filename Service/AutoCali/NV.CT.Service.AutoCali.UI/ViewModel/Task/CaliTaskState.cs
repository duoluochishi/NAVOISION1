namespace NV.CT.Service.AutoCali.UI.Logic
{
    public enum CaliTaskState
    {
        /// <summary>
        /// 已创建
        /// </summary>
        Created = 0,

        /// <summary>
        /// 等待执行
        /// </summary>
        WaitingToRun,

        /// <summary>
        /// 运行中
        /// </summary>
        Running,

        /// <summary>
        /// 挂起（等待确认）
        /// 有些操作需要用户确认后，才可以继续运行
        /// </summary>
        Confirming,

        /// <summary>
        /// 成功
        /// </summary>
        Success,

        /// <summary>
        /// 发生错误
        /// </summary>
        Error,

        /// <summary>
        /// 已取消
        /// </summary>
        Canceled
    }
}
