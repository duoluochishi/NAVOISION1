//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="Study.cs" company="纳米维景">
// 版权所有 (C)2020,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------

namespace NV.CT.PatientManagement.Models;

public partial class StudyModel : BaseViewModel
{
    private bool isChecked;
    private int locked = 0;
    private int printStatus = 0;
    private int correctStatus = 0;
    private DateTime? examStartDate = default(DateTime);
    private DateTime? examEndDate = default(DateTime);
    private string bodyPart = string.Empty;
    private string deviceId = string.Empty;
    private string technician = string.Empty;
    private int storeState = 0;
    private int archiveStatus = 0;
    private string studyPath = string.Empty;
    private string loginId = string.Empty;
    private string pregnancyStatus = string.Empty;
    private string allergies = string.Empty;
    private string medicalAlerts = string.Empty;
    private string admittingDiagnosis = string.Empty;
    private string fieldStrenght = string.Empty;
    private string ward = string.Empty;
    private string referringPhysician = string.Empty;
    private string hw_Type = string.Empty;
    private string weightUnit = string.Empty;
    private string studyStatus = string.Empty;
    private double? weight = 0;
    private string heightUnit = string.Empty;
    private double? height = 0;
    private string accessionNo = string.Empty;
    private string studyInstanceUID = string.Empty;
    private string studyId_Dicom = string.Empty;
    private string id = string.Empty;
    private string internalPatientId = string.Empty;
    private string studyId = string.Empty;
    private string originStudyId = string.Empty;
    private string medicalHistory = string.Empty;
    private string institutionName = string.Empty;
    private string institutionAddress = string.Empty;
    private string comments = string.Empty;
    private int? age;
    private int? ageType;
    private DateTime? studyDate;
    private DateTime? studyTime;
    private int _patientType = (int)(NV.CT.CTS.Enums.PatientType.Local);
    private string _studyDescription = string.Empty;

    public StudyModel()
    {
    }

    public string Id
    {
        get
        {
            return id;
        }
        set
        {
            id = value;
        }
    }

    public string StudyId_Dicom
    {
        get
        {
            return studyId_Dicom;
        }
        set
        {
            studyId_Dicom = value;
        }
    }

    public string StudyInstanceUID
    {
        get
        {
            return studyInstanceUID;
        }
        set
        {
            studyInstanceUID = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string InternalPatientId
    {
        get
        {
            return internalPatientId;
        }
        set
        {
            internalPatientId = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string BodyPart
    {
        get
        {
            return bodyPart;
        }
        set
        {
            SetProperty(ref bodyPart, value);
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string AccessionNo
    {
        get
        {
            return accessionNo;
        }
        set
        {
            accessionNo = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public DateTime? ExamStartDate
    {
        get
        {
            return examStartDate;
        }
        set
        {
            SetProperty(ref examStartDate, value);
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public DateTime? ExamEndDate
    {
        get
        {
            return examEndDate;
        }
        set
        {
            SetProperty(ref examEndDate, value);
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string DeviceId
    {
        get
        {
            return deviceId;
        }
        set
        {
            deviceId = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string Technician
    {
        get
        {
            return technician;
        }
        set
        {
            technician = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:1
    /// Nullable:False
    /// </summary>
    public int StoreState
    {
        get
        {
            return storeState;
        }
        set
        {
            storeState = value;
        }
    }

    /// <summary>
    ///
    /// 待检查 Waiting,
    /// 检查结束 End,
    /// 进行中Processing,
    /// 中断Suspend,
    /// 异常 Abnormal,
    /// 离线重建中OffLineReconing,
    /// Default:
    /// Nullable:True
    /// </summary>
    public string StudyStatus
    {
        get
        {
            return studyStatus;
        }
        set
        {
            studyStatus = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public double? Height
    {
        get
        {
            return height;
        }
        set
        {
            height = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string HeightUnit
    {
        get
        {
            return heightUnit;
        }
        set
        {
            heightUnit = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public double? Weight
    {
        get
        {
            return weight;
        }
        set
        {
            weight = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string WeightUnit
    {
        get
        {
            return weightUnit;
        }
        set
        {
            weightUnit = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string HW_Type
    {
        get
        {
            return hw_Type;
        }
        set
        {
            hw_Type = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string ReferringPhysician
    {
        get
        {
            return referringPhysician;
        }
        set
        {
            referringPhysician = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string Ward
    {
        get
        {
            return ward;
        }
        set
        {
            ward = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string FieldStrenght
    {
        get
        {
            return fieldStrenght;
        }
        set
        {
            fieldStrenght = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string AdmittingDiagnosis
    {
        get
        {
            return admittingDiagnosis;
        }
        set
        {
            admittingDiagnosis = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string MedicalAlerts
    {
        get
        {
            return medicalAlerts;
        }
        set
        {
            medicalAlerts = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string Allergies
    {
        get
        {
            return allergies;
        }
        set
        {
            allergies = value;
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string PregnancyStatus
    {
        get
        {
            return pregnancyStatus;
        }
        set
        {
            pregnancyStatus = value;
        }
    }

    public string LoginID
    {
        get
        {
            return loginId;
        }
        set
        {
            loginId = value;
        }
    }

    public int ArchiveStatus
    {
        get
        {
            return archiveStatus;
        }
        set
        {
            archiveStatus = value;
        }
    }

    public string StudyPath
    {
        get
        {
            return studyPath;
        }
        set
        {
            studyPath = value;
        }
    }

    public bool IsChecked
    {
        get
        {
            return isChecked;
        }
        set
        {
            SetProperty(ref isChecked, value);
        }
    }

    public int Locked
    {
        get
        {
            return locked;
        }
        set
        {
            SetProperty(ref locked, value);
        }
    }

    public int PrintStatus
    {
        get
        {
            return printStatus;
        }
        set
        {
            SetProperty(ref printStatus, value);
        }
    }

    public int CorrectStatus
    {
        get
        {
            return correctStatus;
        }
        set
        {
            SetProperty(ref correctStatus, value);
        }
    }

    public string StudyId { get => studyId; set => studyId = value; }

    public string OriginStudyId
    {
        get
        {
            return originStudyId;
        }
        set
        {
            SetProperty(ref originStudyId, value);
        }
    }

    public string MedicalHistory
    {
        get
        {
            return medicalHistory;
        }
        set
        {
            SetProperty(ref medicalHistory, value);
        }
    }

    public string InstitutionAddress
    {
        get
        {
            return institutionAddress;
        }
        set
        {
            SetProperty(ref institutionAddress, value);
        }
    }

    public string InstitutionName
    {
        get
        {
            return institutionName;
        }
        set
        {
            SetProperty(ref institutionName, value);
        }
    }

    public string Comments
    {
        get
        {
            return comments;
        }
        set
        {
            SetProperty(ref comments, value);
        }
    }

    public int? Age
    {
        get
        {
            return age;
        }
        set
        {
            SetProperty(ref age, value);
        }
    }
    public int? AgeType
    {
        get
        {
            return ageType;
        }
        set
        {
            SetProperty(ref ageType, value);
        }
    }
    public DateTime? StudyDate
    {
        get
        {
            return studyDate;
        }
        set
        {
            SetProperty(ref studyDate, value);
        }
    }
    public DateTime? StudyTime
    {
        get
        {
            return studyTime;
        }
        set
        {
            SetProperty(ref studyTime, value);
        }
    }

    public int PatientType
    {
        get
        {
            return _patientType;
        }
        set
        {
            SetProperty(ref _patientType, value);
        }
    }

    public string StudyDescription
    {
        get
        {
            return _studyDescription;
        }
        set
        {
            SetProperty(ref _studyDescription, value);
        }
    }
    public DateTime CreateTime { get; set; } = DateTime.Now;

}