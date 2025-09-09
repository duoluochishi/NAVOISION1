//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:54:59    V1.0.0       胡安
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
// <summary>
//     修改日期                  版本号       创建人
// 2020/4/28 16:58:27               V0.0.1                   liujian
// </summary>
// ---------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS.Enums;
using NV.CT.UI.ViewModel;
using System;

namespace NV.CT.PatientBrowser.Models;

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
    private string studyID_Dicom = string.Empty;
    private string id = string.Empty;
    private string internalPatientId = string.Empty;
    private string studyId = string.Empty;
    private string originStudyID = string.Empty;
    private string medicalHistory = string.Empty;
    private string institutionName = string.Empty;
    private string institutionAddress = string.Empty;
    private string comments = string.Empty;
    private int age;
    private AgeType ageType;
    private int patientType;

    public string Id
    {
        get => id;
        set => SetProperty(ref id, value);
    }

    public string StudyID_Dicom
    {
        get => studyID_Dicom;
        set => SetProperty(ref studyID_Dicom, value);
    }

    public string StudyInstanceUID
    {
        get => studyInstanceUID;
        set => SetProperty(ref studyInstanceUID, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string InternalPatientId
    {
        get => internalPatientId;
        set => SetProperty(ref internalPatientId, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string BodyPart
    {
        get => bodyPart;
        set => SetProperty(ref bodyPart, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string AccessionNo
    {
        get => accessionNo;
        set => SetProperty(ref accessionNo, value);
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
        get => examStartDate;
        set => SetProperty(ref examStartDate, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public DateTime? ExamEndDate
    {
        get => examEndDate;
        set => SetProperty(ref examEndDate, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string DeviceID
    {
        get => deviceId;
        set => SetProperty(ref deviceId, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string Technician
    {
        get => technician;
        set => SetProperty(ref technician, value);
    }

    /// <summary>
    /// Desc:
    /// Default:1
    /// Nullable:False
    /// </summary>
    public int StoreState
    {
        get => storeState;
        set => SetProperty(ref storeState, value);
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
        get => studyStatus;
        set => SetProperty(ref studyStatus, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public double? Height
    {
        get => height;
        set => SetProperty(ref height, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string HeightUnit
    {
        get => heightUnit;
        set => SetProperty(ref heightUnit, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public double? Weight
    {
        get => weight;
        set => SetProperty(ref weight, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string WeightUnit
    {
        get => weightUnit;
        set => SetProperty(ref weightUnit, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string HW_Type
    {
        get => hw_Type;
        set => SetProperty(ref hw_Type, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string ReferringPhysician
    {
        get => referringPhysician;
        set => SetProperty(ref referringPhysician, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string Ward
    {
        get => ward;
        set => SetProperty(ref ward, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string FieldStrenght
    {
        get => fieldStrenght;
        set => SetProperty(ref fieldStrenght, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string AdmittingDiagnosis
    {
        get => admittingDiagnosis;
        set => SetProperty(ref admittingDiagnosis, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string MedicalAlerts
    {
        get => medicalAlerts;
        set => SetProperty(ref medicalAlerts, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string Allergies
    {
        get => allergies;
        set => SetProperty(ref allergies, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>
    public string PregnancyStatus
    {
        get => pregnancyStatus;
        set => SetProperty(ref pregnancyStatus, value);
    }

    public string LoginID
    {
        get => loginId;
        set => SetProperty(ref loginId, value);
    }

    public int ArchiveStatus
    {
        get => archiveStatus;
        set => SetProperty(ref archiveStatus, value);
    }

    public string StudyPath
    {
        get => studyPath;
        set => SetProperty(ref studyPath, value);
    }

    public bool IsChecked
    {
        get => isChecked;
        set => SetProperty(ref isChecked, value);
    }

    public int Locked
    {
        get => locked;
        set => SetProperty(ref locked, value);
    }

    public int PrintStatus
    {
        get => printStatus;
        set => SetProperty(ref printStatus, value);
    }

    public int CorrectStatus
    {
        get => correctStatus;
        set => SetProperty(ref correctStatus, value);
    }

    public string StudyId
    {
        get => studyId;
        set => SetProperty(ref studyId, value);
    }

    public string OriginStudyID
    {
        get => originStudyID;
        set => SetProperty(ref originStudyID, value);
    }

    public string MedicalHistory
    {
        get => medicalHistory;
        set => SetProperty(ref medicalHistory, value);
    }

    public string InstitutionAddress
    {
        get => institutionAddress;
        set => SetProperty(ref institutionAddress, value);
    }

    public string InstitutionName
    {
        get => institutionName;
        set => SetProperty(ref institutionName, value);
    }

    public string Comments
    {
        get => comments;
        set => SetProperty(ref comments, value);
    }

    public int Age
    {
        get => age;
        set => SetProperty(ref age, value);
    }
    public AgeType AgeType
    {
        get => ageType;
        set => SetProperty(ref ageType, value);
    }
    public int PatientType
    {
        get => patientType;
        set => SetProperty(ref patientType, value);
    }
}