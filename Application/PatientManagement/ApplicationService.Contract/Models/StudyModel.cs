//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.PatientManagement.ApplicationService.Contract.Models;

public class StudyModel
{
    public string Id { get; set; } = string.Empty;
    public string InternalPatientId { get; set; } = string.Empty;
    public string StudyInstanceUID { get; set; } = string.Empty;
    public int Age { get; set; }
    public int AgeType { get; set; }
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
    public int PatientType { get; set; }
    public string StudyStatus { get; set; } = string.Empty;
    public bool IsProtected { get; set; }
    public int ArchiveStatus { get; set; }
    public int PrintStatus { get; set; }

    public int CorrectStatus { get; set; }


    public string StudyDescription { get; set; } = string.Empty;


    public DateTime? ExamStartTime { get; set; }

    public DateTime? ExamEndTime { get; set; }
    
    public DateTime? StudyDate { get; set; }
    public DateTime? StudyTime { get; set; }
    public DateTime? Birthday { get; set; }

    public DateTime? CreateTime { get; set; }
}