//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS.Enums;

namespace NV.CT.DatabaseService.Contract.Entities;

public class JobTaskEntity : BaseEntity
{
    public string WorkflowId { get; set; } = string.Empty;
    public string InternalPatientID { get; set; } = string.Empty;
    public string InternalStudyID { get; set; } = string.Empty;
    public JobTaskType JobType { get; set; } = JobTaskType.Unknown;
    public JobTaskStatus JobStatus { get; set; } = JobTaskStatus.Unknown;
    public string Parameter { get; set; } = string.Empty;
    public short Priority { get; set; } = 5;

    public DateTime? StartedDateTime { get; set; }
    public DateTime? FinishedDateTime { get; set; }
    public bool IsDeleted { get; set; } = false;
}