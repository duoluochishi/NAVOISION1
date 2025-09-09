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
using FellowOakDicom.Network;

namespace NV.CT.DicomUtility.Transfer.CStoreScu
{
    public class StorageResult
    {
        public string WorkflowID { get;  }

        public string SeriesID { get; }

        public int TotalCount { get; }

        public int ProcessedCount { get; }

        public bool IsSuccess { get; }

        public DicomStatus LastStatus { get; }

        public string Tips { get; }

        public StorageResult(string workflowID, string seriesID, bool isSuccess, int processedCount,int totalCount, DicomStatus lastStatus,string tips)
        {
            WorkflowID = workflowID;
            SeriesID = seriesID;
            IsSuccess = isSuccess;
            ProcessedCount = processedCount;
            TotalCount = totalCount;
            LastStatus = lastStatus;
            Tips = tips;
        }
    }
}
