//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/8/30 14:31:08           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.DatabaseService.Contract.Entities;

public class DoseCheckEntity
{
    public string Id { get; set; } = string.Empty;

    public string InternalStudyId { get; set; } = string.Empty;

    public string InternalPatientId { get; set; } = string.Empty;

    public string FrameOfReferenceId { get; set; } = string.Empty;

    public string MeasurementId { get; set; } = string.Empty;

    public string ScanId { get; set; } = string.Empty;

    public DoseCheckType DoseCheckType { get; set; } = DoseCheckType.Notification;

    public float WarningCTDI { get; set; }

    public float WarningDLP { get; set; }

    public float CurrentCTDI { get; set; }

    public float CurrentDLP { get; set; }

    public string Operator { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public DateTime CreateTime { get; set; }
}