namespace NV.CT.WorkingBoxThread.WB
{
    /// <summary>
    /// WB信息，封装当前WB处理信息。
    /// </summary>
    public class WorkingBoxMessage
    {
        /// <summary>
        /// 工作
        /// </summary>
        public Action ActionValue { get; }
        public string WorkingBoxName { get; }
        public string MethodName { get; }
        public Guid Guid { get;  }
        public WorkingBoxMessage(Action action,string workingBoxName,string methodName)
        {
            ActionValue = action;
            WorkingBoxName = workingBoxName;
            MethodName = methodName;
            Guid = Guid.NewGuid();
        }
    }
}
