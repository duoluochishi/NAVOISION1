namespace NV.CT.Service.AutoCali.Model
{
    /// <summary>
    /// 校准历史条目
    /// 何时，何人，做了什么校准
    /// </summary>
    public class CaliHistoryItem
    {
        public string? CreatedTime { get; set; }

        public string? OperationUser { get; set; }

        public string? OperationObject { get; set; }
    }
}
