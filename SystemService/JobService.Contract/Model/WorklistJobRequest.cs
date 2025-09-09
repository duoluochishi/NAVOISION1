//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/7/11 13:45:36    V1.0.0        胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.Job.Contract.Model
{
    public class WorklistJobRequest : BaseJobRequest
    {

        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string AECaller { get; set; } = string.Empty;

        public string AETitle { get; set; } = string.Empty;

        /// <summary>
        /// for search filter
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        public string PatientId { get; set; } = string.Empty;

        public string PatientSex { get; set; } = string.Empty;

        public string AccessionNumber { get; set; } = string.Empty;

        public string ReferringPhysicianName { get; set; } = string.Empty;

        public DateTime StudyDateStart { get; set; } = DateTime.Today;

        public DateTime StudyDateEnd { get; set; } = DateTime.Today;




    }
}
