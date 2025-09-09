//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
namespace NV.CT.PatientManagement.ApplicationService.Contract.Models;

public class PatientModel
{
    public string Id { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string PatientId { get; set; } = string.Empty;

    public Gender PatientSex { get; set; }

    public DateTime PatientBirthDate { get; set; }

    public DateTime CreateTime { get; set; }

    public string Editor { get; set; } = string.Empty;
}