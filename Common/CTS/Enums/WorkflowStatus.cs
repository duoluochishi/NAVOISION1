namespace NV.CT.CTS.Enums
{
    /// <summary>
    /// 检查流程状态
    /// </summary>
    public enum WorkflowStatus
    {
        /// <summary>
        /// 未开始
        /// </summary>
        NotStarted,

        /// <summary>
        /// 启动检查
        /// </summary>
        ExaminationStarting,

        /// <summary>
        /// 检查中
        /// </summary>
        Examinating,

        /// <summary>
        /// 检查结束...
        /// </summary>
        ExaminationClosing,

        /// <summary>
        /// 检查结束
        /// </summary>
        ExaminationClosed,

        /// <summary>
        /// 检查异常结束，无法继续
        /// </summary>
        ExaminationDiscontinue
    }
}
