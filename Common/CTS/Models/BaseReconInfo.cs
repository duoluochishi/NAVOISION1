//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/2/15 9:46:22           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NV.CT.CTS.Enums;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.OfflineMachineEnums;

namespace NV.CT.CTS.Models
{
    public class BaseReconInfo
    {
        public string StudyUID { get; set; } = string.Empty;

        public string PatientId { get; set; } = string.Empty;

        public string ScanId { get; set; } = string.Empty;

        public string ReconId { get; set; } = string.Empty;

        public string SeriesUID { get; set; } = string.Empty;

        public int TotalCount { get; set; }

        public int FinishCount { get; set; }

        public float Progress { get; set; }

        public string RawDataPath { get; set; } = string.Empty;

        public string ImagePath { get; set; } = string.Empty;

        public string LastImage { get; set; } = string.Empty;

        public bool IsOver { get; set; }

        public int SeriesNumber { get; set; }
    }

    public class RealtimeReconInfo : BaseReconInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public RealtimeReconStatus Status { get; set; }
    }

    public class OfflineTaskInfo : BaseReconInfo
    {
        public string TaskId { get; set; }

        public TaskPriority Priority { get; set; }

        public OfflineTaskStatus Status { get; set; }

        public OfflineMachineTaskStatus TaskStatus { get; set; }

        public int Index { get; set; }

        public string MachineName { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public string SeriesDescription { get; set; } = string.Empty;

        public DateTime ReconTaskDateTime { get; set; }

        public List<string> ErrorCodes { get; set; }

        public int ProgressStep { get; set; }

        public int TotalStep { get; set; }

        public bool IsOfflineRecon { get; set; }
    }
}
