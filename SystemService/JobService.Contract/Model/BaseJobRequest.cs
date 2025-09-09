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

namespace NV.CT.Job.Contract.Model
{
    public class BaseJobRequest
    {
        public string Id { get; set; } = string.Empty;

        public string WorkflowId { get; set; } = string.Empty;

        public string InternalPatientID { get; set; } = string.Empty;

        public string InternalStudyID { get; set; } = string.Empty;

        public bool IsRawDataImport { get; set; } = false;

        public int Priority { get; set; } = 5;

        public string Creator { get; set; } = string.Empty;

        public JobTaskType JobTaskType { get; set; }

        /// <summary>
        /// Json格式
        /// </summary>
        public string Parameter { get; set; } = string.Empty;

    }
}
