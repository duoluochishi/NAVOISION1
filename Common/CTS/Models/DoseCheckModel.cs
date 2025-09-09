//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.CTS.Models;

public class DoseCheckModel
{
    public string Id { get; set; } = string.Empty;

    public string InternalStudyId { get; set; } = string.Empty;

    public string InternalPatientId { get; set; } = string.Empty;

    public string MeasurementId { get; set; } = string.Empty;

    public string FrameOfReferenceId { get; set; } = string.Empty;

    public string ScanId { get; set; } = string.Empty;

    public DoseCheckType DoseCheckType { get; set; } = DoseCheckType.Notification;

    public double WarningCTDI { get; set; }

    public double WarningDLP { get; set; }

    public double CurrentCTDI { get; set; }

    public double CurrentDLP { get; set; }

    public string Operator { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public DateTime CreateTime { get; set; }
}