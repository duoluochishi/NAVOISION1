//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS.Enums;

namespace NV.CT.JobService.Contract.Model;

public class StudyModel
{
    public string Id { get; set; }
    public string InternalPatientId { get; set; }
    public string StudyInstanceUID { get; set; }

    public int Age { get; set; }

    public AgeType AgeType { get; set; }

    public string AdmittingDiagnosisDescription { get; set; }
    public string Ward { get; set; }

    public string BodyPart { get; set; }

    public string AccessionNo { get; set; }

    public string StudyId { get; set; }

    public string Comments { get; set; }

    public string InstitutionName { get; set; }

    public string InstitutionAddress { get; set; }

    public double PatientWeight { get; set; }

    public double PatientSize { get; set; }
    public int PatientType { get; set; }
    public string StudyStatus { get; set; }
    public bool IsProtected { get; set; }
}
