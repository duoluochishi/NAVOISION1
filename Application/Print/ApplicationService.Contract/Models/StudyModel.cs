//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.Print.ApplicationService.Contract.Models
{
    public class StudyModel
    {
        public string Id { get; set; } = string.Empty;
        public string InternalPatientId { get; set; } = string.Empty;
        public string StudyInstanceUID { get; set; } = string.Empty;
        public string BodyPart { get; set; } = string.Empty;
        public DateTime StudyDate { get; set; }
        public DateTime? ExamStartTime { get; set; }
        public DateTime? ExamEndTime { get; set; }

    }
}
