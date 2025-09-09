//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/19 13:39:20    V1.0.0       李勇
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

namespace NV.CT.DicomUtility.Transfer
{
    public class ExecuteStatusInfo
    {
        public string JobTaskID { get; }
        public int TotalCount { get; }
        public int ProcessedCount { get; }
        public ExecuteStatus Status { get; }
        public string Tips { get; }
        public object? Data { get; }
        public string? SeriesID { get; }
        public ExecuteStatusInfo(int totalCount, int processedCount, ExecuteStatus status, string tips, object? data = null) :
             this(string.Empty, totalCount, processedCount, status, tips, data)
        {
        }

        public ExecuteStatusInfo(string jobTaskID, int totalCount, int processedCount, ExecuteStatus status, string tips,  object? data = null, string? seriesID = null)
        {
            JobTaskID = jobTaskID;
            TotalCount = totalCount;
            ProcessedCount = processedCount;
            Status = status;
            Tips = tips;
            Data = data;
            SeriesID = seriesID;
        }
    }
}
