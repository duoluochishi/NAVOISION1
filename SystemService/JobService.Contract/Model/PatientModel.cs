//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
namespace NV.CT.JobService.Contract.Model;

public class PatientModel
{
    public string Id { get; set; }

    public string PatientName { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string PatientId { get; set; }

    public Gender PatientSex { get; set; }

    public DateTime CreateTime { get; set; }
}
