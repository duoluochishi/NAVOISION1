//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.DatabaseService.Contract.Models;
public class ReconTaskModel
{
    public string Id { get; set; } = string.Empty;

    public string InternalStudyId { get; set; } = string.Empty;

    public string ScanId { get; set; } = string.Empty;

    public string FrameOfReferenceUid { get; set; } = string.Empty;

    public bool IsRTD { get; set; }

    public string ReconId { get; set; } = string.Empty;

    public int SeriesNumber { get; set; }

    public string SeriesDescription { get; set; } = string.Empty;

    public string WindowWidth { get; set; } = string.Empty;

    public string WindowLevel { get; set; } = string.Empty;

    public DateTime ReconStartDate { get; set; }

    public DateTime ReconEndDate { get; set; }

    public string IssuingParameters { get; set; } = string.Empty;

    public string ActuralParameters { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string InternalPatientId { get; set; } = string.Empty;

    public int TaskStatus { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public string Creator { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;
}