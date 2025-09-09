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

using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.Language;
using NV.CT.PatientManagement.Helpers;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace NV.CT.PatientManagement.Models;

public class VStudyModel : BaseViewModel, IDataErrorInfo
{
    private Regex _reg_allow_single_quotes = new Regex(Constants.REGEX_ILLEGAL_CHARACTER_ALLOW_SINGLE_QUOTES);
    private Regex _reg_allow_underline = new Regex(Constants.REGEX_ILLEGAL_CHARACTER_ALLOW_UNDERLINE);

    private string _pid = string.Empty;
    public string Pid
    {
        get => _pid;
        set => SetProperty(ref _pid, value);
    }

    private string _firstName = string.Empty;
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    private string _patientName = string.Empty;
    public string PatientName
    {
        get => _patientName;
        set => SetProperty(ref _patientName, value);
    }

    private string _patientId = string.Empty;
    public string PatientId
    {
        get => _patientId;
        set => SetProperty(ref _patientId, value);
    }

    private DateTime? _birthday;
    public DateTime? Birthday
    {
        get => _birthday;
        set
        {
            DateTime newValue;
            if (!value.HasValue)
            {
                newValue = DateTime.Now;
            }
            else if (value.Value > DateTime.Now)
            {
                newValue = DateTime.Now;
            }
            else
            {
                newValue = value.Value;
            }

            if (SetProperty(ref _birthday, newValue) && !SetBirthday)
            {
                CalculateAge();
            }
        }
    }

    private int? _age;
    public int? Age
    {
        get => _age;
        set
        {
            if (SetProperty(ref _age, value))
            {
                CalculateBirthday();
            }
        
        } 
    }

    private AgeType _ageType;
    public AgeType AgeType
    {
        get => _ageType;
        set
        {
            if (SetProperty(ref _ageType, value))
            {
                CalculateBirthday();
            }
        }
    }

    private int _gender;
    public int Gender
    {
        get => _gender;
        set
        {
            SetProperty(ref _gender, value);
            CalcGenderString();
        } 
    }

    private string _genderString = string.Empty;
    public string GenderString
    {
        get
        {
            return _genderString;
        }
        set
        {
            SetProperty(ref _genderString, value);
        }
    }

    private int? _patientType;
    public int? PatientType
    {
        get => _patientType;
        set => SetProperty(ref _patientType, value);
    }

    public string PatientTypeString
    {
        get
        {
            string str = string.Empty;
            switch (PatientType)
            {
                case (int)NV.CT.CTS.Enums.PatientType.Emergency:
                    str = "Emergency patient";
                    break;
                case (int)NV.CT.CTS.Enums.PatientType.PreRegistration:
                    str = "Pre-registered patient";
                    break;
                case (int)NV.CT.CTS.Enums.PatientType.Local:
                default:
                    str = "Local patient";
                    break;
            }
            return str;
        }
    }

    private DateTime? _createTime;
    public DateTime? CreateTime
    {
        get => _createTime;
        set => SetProperty(ref _createTime, value);
    }

    private DateTime? _studyCreateTime;
    public DateTime? StudyCreateTime
    {
        get => _studyCreateTime;
        set => SetProperty(ref _studyCreateTime, value);
    }

    private string _studyId = string.Empty;
    public string StudyId
    {
        get => _studyId;
        set => SetProperty(ref _studyId, value);
    }

    private string _studyId_Dicom = string.Empty;
    public string StudyId_Dicom
    {
        get => _studyId_Dicom;
        set => SetProperty(ref _studyId_Dicom, value);
    }

    private string _bodyPart = string.Empty;
    public string BodyPart
    {
        get => _bodyPart is null ? string.Empty : _bodyPart;
        set => SetProperty(ref _bodyPart, value);
    }

    private int _bodyPartKey = -1;
    public int BodyPartKey
    {
        get => _bodyPartKey;
        set
        {
            if (SetProperty(ref _bodyPartKey, value))
            {
                BodyPart = ((BodyPart)_bodyPartKey).ToString();
            }
        } 
    }


    private double? _height;
    public double? Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    private string _heightUnit = string.Empty;
    public string HeightUnit
    {
        get => _heightUnit;
        set => SetProperty(ref _heightUnit, value);
    }

    private double? _weight;
    public double? Weight
    {
        get => _weight;
        set => SetProperty(ref _weight, value);
    }

    private string _weightUnit = string.Empty;
    public string WeightUnit
    {
        get => _weightUnit;
        set => SetProperty(ref _weightUnit, value);
    }

    private string _admittingDiagnosis = string.Empty;
    public string AdmittingDiagnosis
    {
        get => _admittingDiagnosis;
        set => SetProperty(ref _admittingDiagnosis, value);
    }

    private string _ward = string.Empty;
    public string Ward
    {
        get => _ward;
        set => SetProperty(ref _ward, value);
    }

    private string _fieldStrenght = string.Empty;
    public string FieldStrenght
    {
        get => _fieldStrenght;
        set => SetProperty(ref _fieldStrenght, value);
    }

    private string _accessionNo = string.Empty;
    public string AccessionNo
    {
        get => _accessionNo;
        set => SetProperty(ref _accessionNo, value);
    }

    private DateTime? _examStartTime;
    public DateTime? ExamStartTime
    {
        get => _examStartTime;
        set => SetProperty(ref _examStartTime, value);
    }

    private DateTime? _examEndTime;
    public DateTime? ExamEndTime
    {
        get => _examEndTime;
        set => SetProperty(ref _examEndTime, value);
    }

    private string _technician = string.Empty;
    public string Technician
    {
        get => _technician;
        set => SetProperty(ref _technician, value);
    }

    private string _referringPhysician = string.Empty;
    public string ReferringPhysician
    {
        get => _referringPhysician;
        set => SetProperty(ref _referringPhysician, value);
    }

    private string _studyStatus = string.Empty;
    public string StudyStatus
    {
        get => _studyStatus;
        set
        {
            SetProperty(ref _studyStatus, value);
        } 
    }

    private string _patientStatus = string.Empty;
    public string PatientStatus
    {
        get => _patientStatus;
        set => SetProperty(ref _patientStatus, value);
    }

    private string _patientAddress = string.Empty;
    public string PatientAddress
    {
        get => _patientAddress;
        set => SetProperty(ref _patientAddress, value);
    }

    private string _medicalAlerts = string.Empty;
    public string MedicalAlerts
    {
        get => _medicalAlerts;
        set => SetProperty(ref _medicalAlerts, value);
    }

    private string _performingPhysician = string.Empty;
    public string PerformingPhysician
    {
        get => _performingPhysician;
        set => SetProperty(ref _performingPhysician, value);
    }

    private string _studyInstanceUID = string.Empty;
    public string StudyInstanceUID
    {
        get => _studyInstanceUID;
        set => SetProperty(ref _studyInstanceUID, value);
    }

    private string _studyDescription = string.Empty;
    public string StudyDescription
    {
        get => _studyDescription;
        set => SetProperty(ref _studyDescription, value);
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    private JobTaskStatus _archiveStatus;
    public JobTaskStatus ArchiveStatus
    {
        get => _archiveStatus;
        set
        {
            SetProperty(ref _archiveStatus, value);
        } 
    }

    private JobTaskStatus _printStatus;
    public JobTaskStatus PrintStatus
    {
        get => _printStatus;
        set
        {
            SetProperty(ref _printStatus, value);
        } 
    }

    private int _correctStatus;
    public int CorrectStatus
    {
        get => _correctStatus;
        set
        {
            SetProperty(ref _correctStatus, value);
        } 
    }

    private string _institutionName = string.Empty;
    public string InstitutionName
    {
        get => _institutionName;
        set => SetProperty(ref _institutionName, value);
    }

    private string _institutionAddress = string.Empty;
    public string InstitutionAddress
    {
        get => _institutionAddress;
        set => SetProperty(ref _institutionAddress, value);
    }

    private string _comments = string.Empty;
    public string Comments
    {
        get => _comments;
        set => SetProperty(ref _comments, value);
    }

    private string _hisStudyId = string.Empty;
    public string HisStudyId
    {
        get => _hisStudyId;
        set => SetProperty(ref _hisStudyId, value);
    }

    private bool _isProtected;
    public bool IsProtected
    {
        get => _isProtected;
        set
        {
            SetProperty(ref _isProtected, value);
        }
    }

    private bool _isValid = false;
    public bool IsValid
    {
        get => _isValid;
        set => SetProperty(ref _isValid, value);
    }

    private string _editor = string.Empty;
    public string Editor
    {
        get => _editor;
        set => SetProperty(ref _editor, value);
    }

    private DateTime? _studyDate;
    public DateTime? StudyDate
    {
        get => _studyDate;
        set => SetProperty(ref _studyDate, value);
    }

    private DateTime? _studyTime;
    public DateTime? StudyTime
    {
        get => _studyTime;
        set => SetProperty(ref _studyTime, value);
    }
    public string Error => string.Empty;

    [JsonIgnore]
    private bool _isOriginalRecord = false;
    public bool IsOriginalRecord
    {
        get => _isOriginalRecord;
        set => SetProperty(ref _isOriginalRecord, value);
    }

    [JsonIgnore]
    private Style? _originalRecordStyle;
    public Style? OriginalRecordStyle
    {
        get => _originalRecordStyle;
        set => SetProperty(ref _originalRecordStyle, value);
    }


    public string this[string columnName]
    {
        get
        {
            string result = string.Empty;
            switch (columnName)
            {
                case "LastName":
                    if (string.IsNullOrEmpty(LastName))
                    {
                        result = LanguageResource.Message_Error_LastNameNotEmpty;
                        break;
                    }

                    if (!this._reg_allow_single_quotes.IsMatch(LastName))
                    {
                        result = LanguageResource.Message_Error_IllegalCharachersNotAllowed;
                        break;
                    }

                    break;

                case "FirstName":
                    if (!string.IsNullOrEmpty(FirstName) && !this._reg_allow_single_quotes.IsMatch(FirstName))
                    {
                        result = LanguageResource.Message_Error_IllegalCharachersNotAllowed;
                        break;
                    }
                    break;

                case "PatientId":
                    if (string.IsNullOrEmpty(PatientId))
                    {
                        result = LanguageResource.Message_Error_PatientIdNotEmpty;
                        break;
                    }

                    if (!this._reg_allow_underline.IsMatch(PatientId))
                    {
                        result = LanguageResource.Message_Error_IllegalCharachersNotAllowed;
                        break;
                    }

                    break;
                case "Age":
                    if (Age< 0 || Age>162)
                    {
                        result = LanguageResource.Message_Error_AgeRange;
                        break;
                    }

                    break;

                case "Height":
                    if (Height < 0 || Height > 280)
                    {
                        result = LanguageResource.Message_Error_HeightRange;
                        break;
                    }

                    break;

                case "Weight":
                    if (Weight < 0 || Weight > 230)
                    {
                        result = LanguageResource.Message_Error_WeightRange;
                        break;
                    }
                    break;

                case "Editor":
                    if (string.IsNullOrEmpty(Editor))
                    {
                        result = LanguageResource.Message_Error_EditorNotEmpty;
                        break;
                    }

                    break;
            }

            this.CalcIsValid();

            return result;
        }
    }

    private bool SetBirthday = false;
    private void CalculateBirthday()
    {
        SetBirthday = true;
        Birthday = AgeHelper.GetBirthday(AgeType, this.Age.HasValue ? this.Age.Value: 0);
        SetBirthday = false;
    }

    private void CalculateAge()
    {
        if (Birthday is null || Birthday == DateTime.MinValue)
        {
            return;
        }
        var ageInfo = AgeHelper.CalculateAgeByBirthday(Birthday.Value);
        this.Age = ageInfo.Item1;
        this.AgeType = ageInfo.Item2;
    }

    private void CalcGenderString()
    {
        string str = string.Empty;
        switch (Gender)
        {
            case (int)NV.CT.CTS.Enums.Gender.Male:
                str = LanguageResource.Content_GenderMale;
                break;
            case (int)NV.CT.CTS.Enums.Gender.Female:
                str = LanguageResource.Content_GenderFemale;
                break;
            case (int)NV.CT.CTS.Enums.Gender.Other:
                str = LanguageResource.Content_GenderOther;
                break;
            default:
                str = string.Empty;
                break;
        }
        this.GenderString = str;
    }

    public void CalcIsValid()
    {
        if (string.IsNullOrEmpty(LastName))
        {
            IsValid = false;
            return;
        }

        if (!this._reg_allow_single_quotes.IsMatch(LastName))
        {
            IsValid = false;
            return;
        }

        if (!string.IsNullOrEmpty(FirstName) && !this._reg_allow_single_quotes.IsMatch(FirstName))
        {
            IsValid = false;
            return;
        }

        if (string.IsNullOrEmpty(PatientId))
        {
            IsValid = false;
            return;
        }

        if (!this._reg_allow_underline.IsMatch(PatientId))
        {
            IsValid = false;
            return;
        }

        if (Age < 0 || Age > 162)
        {
            IsValid = false;
            return;
        }

        if (Height < 0 || Height > 280)
        {
            IsValid = false;
            return;
        }

        if (Weight < 0 || Weight > 230)
        {
            IsValid = false;
            return;
        }

        if (string.IsNullOrEmpty(Editor))
        {
            IsValid = false;
            return;
        }

        IsValid = true;

    }


}