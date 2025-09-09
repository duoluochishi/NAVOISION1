//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;
using NV.CT.CTS.Helpers;

namespace NV.CT.Examination.ApplicationService.Contract.Models;
public class StudyModel
{
    public string ID { get; set; } = string.Empty;

    public string StudyInstanceUID { get; set; } = string.Empty;

    public string InternalPatientId { get; set; } = string.Empty;

    public int Age { get; set; }

    public int AgeType { get; set; }

    public string AdmittingDiagnosisDescription { get; set; } = string.Empty;

    public string Ward { get; set; } = string.Empty;

    public string BodyPart { get; set; } = string.Empty;

    public string AccessionNo { get; set; } = string.Empty;

    public string StudyID { get; set; } = string.Empty;

    public string Comments { get; set; } = string.Empty;

    public string InstitutionName { get; set; } = string.Empty;

    public string InstitutionAddress { get; set; } = string.Empty;

    public double? PatientWeight { get; set; }

    public double? PatientSize { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string PatientID { get; set; } = string.Empty;

    public Gender PatientSex { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime StudyDate { get; set; }

    public DateTime RegistrationDate { get; set; }

    /// <summary>
    /// 检查时间
    /// </summary>
    public DateTime StudyTime { get; set; }

    /// <summary>
    /// 检查日期
    /// </summary>
    public DateTime ExamStartTime { get; set; }

    /// <summary>
    /// 检查时间
    /// </summary>
    public DateTime ExamEndTime { get; set; }

    /// <summary>
    /// 检查技师
    /// </summary>
    public string Technician { get; set; } = string.Empty;

    /// <summary>
    /// 检查描述
    /// </summary>
    public string StudyDescription { get; set; } = string.Empty;

    public DateTime Birthday => AgeHelper.GetBirthday((AgeType)AgeType, Age);

    public string StudyUID { get; set; } = string.Empty;

    /// <summary>
    /// 检查协议
    /// </summary>
    public string Protocol { get; set; } = string.Empty;
}