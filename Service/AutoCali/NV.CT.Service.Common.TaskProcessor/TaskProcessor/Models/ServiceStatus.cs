namespace NV.CT.Service.Common.TaskProcessor.Models
{
    public enum ServiceStatus
    {
        Idle,
        Starting,

        /// <summary>
        /// 正在运行
        /// </summary>
        Running,
        
        /// <summary>
        /// 正在取消
        /// </summary>
        Cancelling,

        /// <summary>
        /// 被取消了
        /// </summary>
        Cancelled,
        
        /// <summary>
        /// 已成功完成
        /// </summary>
        Completed,

        /// <summary>
        /// 发生错误
        /// </summary>
        Error
    }
}
