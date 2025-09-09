//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.Print.ApplicationService.Contract.Models
{
    public class SeriesModel
    {
        public string Id { get; set; } = string.Empty;
        public string InternalStudyId { get; set; } = string.Empty;
        public string SeriesInstanceUID { get; set; } = string.Empty;
        public string BodyPart { get; set; } = string.Empty;
        public string Modality { get; set; } = string.Empty;
        public string SeriesType { get; set; } = string.Empty;
        public string ImageType { get; set; } = string.Empty;
        public string SeriesDescription { get; set; } = string.Empty;
        public string PatientPosition { get; set; } = string.Empty;
        public string SeriesPath { get; set; } = string.Empty;
        public DateTime? ReconStartDate { get; set; }
        public DateTime? ReconEndDate { get; set; }

    }
}
