using NV.CT.CTS.Enums;

namespace NV.CT.Models;

public class StudyModel : BaseInfo
{
    public string StudyInstanceUID { get; set; } = string.Empty;

    public string InternalPatientId { get; set; } = string.Empty;
    public string RequestProcedure { get; set; } = string.Empty;
    public int Age { get; set; }
    public AgeType AgeType { get; set; }

    public string AdmittingDiagnosisDescription { get; set; } = string.Empty;

    public string Ward { get; set; } = string.Empty;

    /// <summary>
    /// 检查部位
    /// </summary>
    public string BodyPart { get; set; } = string.Empty;

    /// <summary>
    /// 登记号
    /// </summary>
    public string AccessionNo { get; set; } = string.Empty;

    public string StudyId { get; set; } = string.Empty;

    public string Comments { get; set; } = string.Empty;

    public string InstitutionName { get; set; } = string.Empty;

    public string InstitutionAddress { get; set; } = string.Empty;

    public double? PatientWeight { get; set; }

    public double? PatientSize { get; set; }

    public string StudyStatus { get; set; } = string.Empty;
    public int PatientType { get; set; }

    /// <summary>
    /// 检查日期
    /// </summary>
    public DateTime StudyDate { get; set; }

    /// <summary>
    /// 登记日期
    /// </summary>
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

    public DateTime Birthday
    {
        get
        {
            switch (AgeType)
            {
                case AgeType.Year:
                    return DateTime.Now.AddYears(-Age);
                case AgeType.Month:
                    if (Age % 12 == 0 && Age != 0)
                    {
                        int years = (Age / 12);
                        return DateTime.Now.AddYears(-years);
                    }
                    else
                    {
                        return DateTime.Now.AddMonths(-Age);
                    }
                case AgeType.Week:
                    return DateTime.Now.AddDays((-Age) * 7);
                case AgeType.Day:
                    return DateTime.Now.AddDays(-Age);
                default:
                    return default;
            }
        }
    }

    public string Protocol { get; set; } = string.Empty;
    public bool IsProtected { get; set; }

    public bool IsDeleted { get; set; }

    public Gender PatientSex { get; set; }
    public int ArchiveStatus { get; set; }
}