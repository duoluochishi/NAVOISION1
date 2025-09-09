//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.DatabaseService.Contract.Models;
public class ScanTaskModel
{
    public string Id { get; set; } = string.Empty;

    public string InternalStudyId { get; set; } = string.Empty;

    public string MeasurementId { get; set; } = string.Empty;

    public string FrameOfReferenceUid { get; set; } = string.Empty;

    public string ScanId { get; set; } = string.Empty;

    public string BodyPart { get; set; } = string.Empty;

    public string ScanOption { get; set; } = string.Empty;

    public DateTime ScanStartDate { get; set; }

    public DateTime ScanEndDate { get; set; }

    public DateTime CreateTime { get; set; } = DateTime.Now;

    public string Creator { get; set; } = string.Empty;

    public string IssuingParameters { get; set; } = string.Empty;

    public string ActuralParameters { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int TaskStatus { get; set; }

    public string InternalPatientId { get; set; } = string.Empty;

    public string BodySize { get; set; } = string.Empty;

    public bool IsLinkScan { get; set; }

    public bool IsInject { get; set; }

    public string TopoScanId { get; set; } = string.Empty;
}