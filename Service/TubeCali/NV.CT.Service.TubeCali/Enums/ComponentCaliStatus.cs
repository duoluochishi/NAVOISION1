namespace NV.CT.Service.TubeCali.Enums
{
    /// <summary>
    /// 组件状态
    /// </summary>
    public enum ComponentCaliStatus
    {
        /// <summary>
        /// 表示可以下发参数的射线源
        /// </summary>
        Waiting,

        /// <summary>
        /// 训管中
        /// </summary>
        Working,

        /// <summary>
        /// 训管成功
        /// </summary>
        Success,

        /// <summary>
        /// 训管失败
        /// </summary>
        Failed,

        /// <summary>
        /// 手动停止，或训管中检测到开门后将其它Waiting状态的置为取消
        /// </summary>
        Cancelled
    }
}