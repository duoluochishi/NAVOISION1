//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.DatabaseService.Contract.Entities;

public class StudyEntity : BaseEntity
{
    public string StudyInstanceUID { get; set; } = string.Empty;
    public string InternalPatientId { get; set; } = string.Empty;
    public int Age { get; set; }

    public AgeType AgeType { get; set; }

    public string AdmittingDiagnosisDescription { get; set; } = string.Empty;

    public string Ward { get; set; } = string.Empty;

    public string ReferringPhysicianName { get; set; } = string.Empty;

    public string BodyPart { get; set; } = string.Empty;

    public string AccessionNo { get; set; } = string.Empty;

    public string StudyId { get; set; } = string.Empty;

    public string Comments { get; set; } = string.Empty;

    public string InstitutionName { get; set; } = string.Empty;

    public string InstitutionAddress { get; set; } = string.Empty;

    public double? PatientWeight { get; set; }

    public double? PatientSize { get; set; }

    public string StudyStatus { get; set; } = string.Empty;

    public int PatientType { get; set; }

    public DateTime RegistrationDate { get; set; }

    /// <summary>
    /// 检查时间
    /// </summary>
    public DateTime StudyTime { get; set; }

    /// <summary>
    /// 检查日期
    /// </summary>
    public DateTime? ExamStartTime { get; set; }

    /// <summary>
    /// 检查时间
    /// </summary>
    public DateTime? ExamEndTime { get; set; }

    /// <summary>
    /// 检查技师
    /// </summary>
    public string Technician { get; set; } = string.Empty;

    /// <summary>
    /// 检查描述
    /// </summary>
    public string StudyDescription { get; set; } = string.Empty;

    /// <summary>
    /// 检查协议
    /// </summary>
    public string Protocol { get; set; } = string.Empty;

    /// <summary>
    /// 是否锁定
    /// </summary>
    public bool IsProtected { get; set; } = false;

    public bool IsDeleted { get; set; } = false;

    public Gender PatientSex { get; set; }
    public int ArchiveStatus { get; set; }

    public int PrintStatus { get; set; }

    public int CorrectStatus { get; set; }

    public string PrintConfigPath { get; set; } = string.Empty;

    public bool IsLocalModified { get; set; } = false;

}
