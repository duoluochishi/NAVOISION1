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
    public class QueryJobRequest
    {
        public JobTaskType JobType { get; set; }

        public List<JobTaskStatus> JobTaskStatusList { get; set; }
        
        public DateTime BeginDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public string PatientName { get; set; } = string.Empty;

        public string BodyPart { get; set; } = string.Empty;

        public QueryJobRequest()
        {
            JobTaskStatusList = new List<JobTaskStatus>();
        }

    }
}
