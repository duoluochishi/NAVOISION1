//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.DatabaseService.Contract.Entities;

public class PatientEntity : BaseEntity
{
    public string PatientName { get; set; } = string.Empty;

    public string PatientId { get; set; } = string.Empty;

    public Gender PatientSex { get; set; }

    public DateTime PatientBirthDate { get; set; }

    public string BodyPart { get; set; } = string.Empty;

    public string Editor { get; set; } = string.Empty;
}