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

using NV.CT.CTS.Enums;
using NV.CT.CTS.Helpers;
using NV.CT.Language;
using NV.CT.UI.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;

namespace NV.CT.PatientBrowser.Models;

public partial class VStudyModel : BaseViewModel, INotifyDataErrorInfo
{
    public VStudyModel()
    {
        func_validateLastName = () =>
        {
            if (string.IsNullOrEmpty(lastName))
            {
                return LanguageResource.Message_Error_LastNameNotEmpty;
            }
            Regex reg = new Regex(Global.Instance.IllegalcharacterAllowQuotes);
            if (!reg.IsMatch(lastName))
            {
                return LanguageResource.Message_Error_IllegalCharachersNotAllowed;
            }
            return string.Empty;
        };
        func_validateFirstName = () =>
        {
            if (string.IsNullOrEmpty(firstName))
            {
                return string.Empty;
            }
            Regex reg = new Regex(Global.Instance.IllegalcharacterAllowQuotes);
            if (!reg.IsMatch(firstName))
            {
                return LanguageResource.Message_Error_IllegalCharachersNotAllowed;
            }
            return string.Empty;
        };
        func_validatePatientID = () =>
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return LanguageResource.Message_Error_PatientIdNotEmpty;
            }
            Regex reg = new Regex(Global.Instance.IllegalcharacterAllowUnderline);
            if (!reg.IsMatch(patientId))
            {
                return LanguageResource.Message_Error_IllegalCharachersNotAllowed;
            }
            return string.Empty;
        };
        func_validateAge = () =>
        {
            if (string.IsNullOrEmpty(age))
            {
                return LanguageResource.Message_Error_AgeCannotBeEmpty;
            }
            int val = 0;
            if (!int.TryParse(age, out val))
            {
                return LanguageResource.Message_Error_AgeMustBeInteger;
            }
            if (val < 0 || val > 162)
            {
                return LanguageResource.Message_Error_AgeRange;
            }
            return string.Empty;
        };
        func_validateHeight = () =>
        {
            if (!string.IsNullOrEmpty(height))
            {
                double val = 0;
                if (!double.TryParse(height, out val))
                {
                    return LanguageResource.Message_Error_HeightMustBeNumber;
                }
                if (val < 0 || val > 280)
                {
                    return LanguageResource.Message_Error_HeightRange;
                }
                return string.Empty;
            }
            return string.Empty;
        };
        func_validateWeight = () =>
        {
            if (!string.IsNullOrEmpty(weight))
            {
                double val = 0;
                if (!double.TryParse(weight, out val))
                {
                    return LanguageResource.Message_Error_WeightMustBeNumber;
                }
                if (val < 0 || val > 230)
                {
                    return LanguageResource.Message_Error_WeightRange;
                }
                return string.Empty;
            }
            return string.Empty;
        };
    }
    private string pid = string.Empty;
    private string firstName = string.Empty;
    private string lastName = string.Empty;
    private string patientName = string.Empty;
    private string patientId = string.Empty;
    private DateTime? birthday;
    private AgeType ageType = AgeType.Year;
    private string age = string.Empty;
    private int gender = 0;
    private string studyId = string.Empty;
    private string studyID_Dicom = string.Empty;
    private int? patientType;
    private string bodyPart = string.Empty;
    private string? height = string.Empty;
    private string heightUnit = string.Empty;
    private string? weight = string.Empty;
    private string weightUnit = string.Empty;
    private string admittingDiagnosis = string.Empty;
    private string ward = string.Empty;
    private string fieldStrenght = string.Empty;
    private string accessionNo = string.Empty;
    private DateTime? examStartDate;
    private DateTime? examEndDate;
    private string technician = string.Empty;
    private string referringPhysicianName = string.Empty;
    private DateTime createTime;
    private string studyStatus = string.Empty;
    private string patientStatus = string.Empty;
    private string patientAddress = string.Empty;
    private string medicalAlerts = string.Empty;
    private string performingPhysician = string.Empty;
    private string studyInstanceUID = string.Empty;
    private string studyDescription = string.Empty;
    private bool isSelected;
    private int locked;
    private int archiveStatus;
    private int printStatus;
    private int correctStatus;
    private string institutionName = string.Empty;
    private string institutionAddress = string.Empty;
    private string comments = string.Empty;
    private string hisStudyID = string.Empty;
    private bool examButtonStatus;
    readonly Func<string> func_validateLastName;
    readonly Func<string> func_validateFirstName;
    readonly Func<string> func_validatePatientID;
    readonly Func<string> func_validateAge;
    readonly Func<string> func_validateHeight;
    readonly Func<string> func_validateWeight;

    //病人表的主键
    public string Pid
    {
        get => pid;
        set => SetProperty(ref pid, value);
    }

    public string FirstName
    {
        get => firstName;
        set
        {
            SetProperty(ref firstName, value);
            ValidateFirstName();
        }
    }

    public string LastName
    {
        get => lastName;
        set
        {
            if (lastName != value)
            {
                lastName = value;
                ValidateLastName();
            }
        }
    }
    private void ValidateLastName()
    {
        Validate(func_validateLastName, nameof(LastName));
    }
    private void ValidateFirstName()
    {
        Validate(func_validateFirstName, nameof(FirstName));
    }
    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>           
    public string PatientName
    {
        get => patientName;
        set => SetProperty(ref patientName, value);
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:False
    /// </summary>      
    public string PatientId
    {
        get => patientId;
        set
        {
            if (patientId != value)
            {
                SetProperty(ref patientId, value);
                ValidatePatientID();
            }
        }
    }

    private void ValidatePatientID()
    {
        Validate(func_validatePatientID, nameof(PatientId));
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>    
    public DateTime? Birthday
    {
        get => birthday;
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

            if (SetProperty(ref birthday, newValue) && !SetBirthday)
            {
                CalculateAge();
                this.SetExamButtonStatus();
            }
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>  

    private string ValidateAge()
    {
        return Validate(func_validateAge, nameof(Age));
    }

    public AgeType AgeType
    {
        get => ageType;
        set
        {
            if (SetProperty(ref ageType, value))
            {
                CalculateBirthday();
            }
        }
    }

    public string Age
    {
        get => age;
        set
        {
            if (SetProperty(ref age, value) 
                && ValidateAge() == string.Empty)
            {
                CalculateBirthday();
            }
        }
    }

    /// <summary>
    /// Desc:
    /// Default:
    /// Nullable:True
    /// </summary>      
    public int Gender
    {
        get => gender;
        set => SetProperty(ref gender, value);
    }

    public string GenderString
    {
        get
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
            return str;
        }
    }

    public int? PatientType
    {
        get => patientType;
        set => SetProperty(ref patientType, value);
    }

    public BitmapImage PatientTypeImage
    {
        get
        {
            BitmapImage bitmapImage = new BitmapImage();
            switch (PatientType)
            {
                case (int)NV.CT.CTS.Enums.PatientType.Emergency:
                    bitmapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/patient3.png", UriKind.RelativeOrAbsolute));
                    break;
                case (int)NV.CT.CTS.Enums.PatientType.PreRegistration:
                    bitmapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/patient1.png", UriKind.RelativeOrAbsolute));
                    break;
                case (int)NV.CT.CTS.Enums.PatientType.Local:
                default:
                    bitmapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/patient2.png", UriKind.RelativeOrAbsolute));
                    break;
            }
            return bitmapImage;
        }
    }

    public string PatientTypeString
    {
        get
        {
            string str = string.Empty;
            switch (PatientType)
            {
                case (int)NV.CT.CTS.Enums.PatientType.Emergency:
                    str = LanguageResource.Content_EmergencyPatient;
                    break;
                case (int)NV.CT.CTS.Enums.PatientType.PreRegistration:
                    str = LanguageResource.Content_PreRegisteredPatient;
                    break;
                case (int)NV.CT.CTS.Enums.PatientType.Local:
                default:
                    str = LanguageResource.Content_LocalPatient;
                    break;
            }
            return str;
        }
    }

    public DateTime CreateTime
    {
        get => createTime;
        set => SetProperty(ref createTime, value);
    }

    public string StudyId
    {
        get => studyId;
        set => SetProperty(ref studyId, value);
    }
    public string StudyID_Dicom
    {
        get => studyID_Dicom;
        set => SetProperty(ref studyID_Dicom, value);
    }

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
    public string? Height
    {
        get => height;
        set
        {
            if (height != value)
            {
                height = value;
                ValidateHeight(height);
            }
        }
    }
    private void ValidateHeight(string value)
    {
        Validate(func_validateHeight, nameof(Height));
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
    public string? Weight
    {
        get => weight;
        set
        {
            if (weight != value)
            {
                weight = value;
                ValidateWeight(weight);
            }
        }
    }
    private void ValidateWeight(string value)
    {
        Validate(func_validateWeight, nameof(Weight));
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
    public string AccessionNo
    {
        get => accessionNo;
        set => SetProperty(ref accessionNo, value);
    }

    public DateTime? ExamStartDate
    {
        get => examStartDate;
        set => SetProperty(ref examStartDate, value);
    }
    public DateTime? ExamEndDate
    {
        get => examEndDate;
        set => SetProperty(ref examEndDate, value);
    }
    public string Technician
    {
        get => technician;
        set => SetProperty(ref technician, value);
    }
    public string ReferringPhysicianName
    {
        get => referringPhysicianName;
        set => SetProperty(ref referringPhysicianName, value);
    }

    public string StudyStatus
    {
        get => studyStatus;
        set => SetProperty(ref studyStatus, value);
    }
    public string PatientStatus
    {
        get => patientStatus;
        set => SetProperty(ref patientStatus, value);
    }

    public string PatientAddress
    {
        get => patientAddress;
        set => SetProperty(ref patientAddress, value);
    }
    public string MedicalAlerts
    {
        get => medicalAlerts;
        set => SetProperty(ref medicalAlerts, value);
    }

    public string PerformingPhysician
    {
        get => performingPhysician;
        set => SetProperty(ref performingPhysician, value);
    }

    public string StudyInstanceUID
    {
        get => studyInstanceUID;
        set => SetProperty(ref studyInstanceUID, value);
    }

    public string StudyDescription
    {
        get => studyDescription;
        set => SetProperty(ref studyDescription, value);
    }
    public bool IsSelected
    {
        get => isSelected;
        set => SetProperty(ref isSelected, value);
    }

    public int Locked
    {
        get => locked;
        set => SetProperty(ref locked, value);
    }
    public int ArchiveStatus
    {
        get => archiveStatus;
        set => SetProperty(ref archiveStatus, value);
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

    public string InstitutionName
    {
        get => institutionName;
        set => SetProperty(ref institutionName, value);
    }
    public string InstitutionAddress
    {
        get => institutionAddress;
        set => SetProperty(ref institutionAddress, value);
    }
    public string Comments
    {
        get => comments;
        set => SetProperty(ref comments, value);
    }

    public string HisStudyID
    {
        get => hisStudyID;
        set => SetProperty(ref hisStudyID, value);
    }
    private readonly IDictionary<string, IList<string>> m_errors = new Dictionary<string, IList<string>>();

    public bool HasErrors
    {
        get => m_errors.Count != 0;
    }

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool ExamButtonStatus
    {
        get => examButtonStatus;
        set => SetProperty(ref examButtonStatus, value);
    }

    public void Validate(Func<bool> valid, string error, [CallerMemberName] string prop = "")
    {
        if (valid())
        {
            ClearError(prop);
        }
        else
        {
            SetError(new[] { error }, prop);
        }
        RaisePropertyChanged(prop);
        if (HasErrors)
        {
            ExamButtonStatus = false;
        }
        else
        {
            ExamButtonStatus = true;
        }
    }
    public string Validate(Func<string> valid, [CallerMemberName] string prop = "")
    {
        string error = valid();
        if (string.IsNullOrEmpty(error))
            ClearError(prop);
        else
            SetError(new[] { error }, prop);
        RaisePropertyChanged(prop);
        SetExamButtonStatus();
        return error;
    }

    private void SetExamButtonStatus()
    {
        if (func_validateFirstName() != string.Empty)
        {
            ExamButtonStatus = false;
            return;
        }
        if (func_validateLastName() != string.Empty)
        {
            ExamButtonStatus = false;
            return;
        }
        if (func_validatePatientID() != string.Empty)
        {
            ExamButtonStatus = false;
            return;
        }
        if (func_validateAge() != string.Empty)
        {
            ExamButtonStatus = false;
            return;
        }
        if (func_validateHeight() != string.Empty)
        {
            ExamButtonStatus = false;
            return;
        }
        if (func_validateWeight() != string.Empty)
        {
            ExamButtonStatus = false;
            return;
        }
        ExamButtonStatus = true;
    }
    public void ValidateAll()
    {
        ValidateLastName();
        ValidateFirstName();
        ValidatePatientID();
        ValidateAge();
        ValidateHeight(height);
        ValidateWeight(weight);
    }
    // 该方法由 wpf 调用。
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return "";
        }
        // SelectMany 方法很有用，若没有它，错误消息不能正确显示，除非你重写集合，并重写其 ToString 方法。它帮你省了很多麻烦。
        // var errors = m_errors.SelectMany(e => e.Value.FirstOrDefault(r=>r==propertyName));
        if (m_errors.ContainsKey(propertyName))
        {
            return m_errors[propertyName];
        }
        else
        {
            return "";
        }
    }
    // 添加错误消息。
    public void SetError(IList<string> errors, string prop)
    {
        m_errors.Remove(prop);
        m_errors.Add(prop, errors);
        OnError(prop);

    }
    // 清除错误消息。
    public void ClearError(string prop)
    {
        m_errors.Remove(prop);
        OnError(prop);

    }
    // 验证属性时调用。
    private void OnError(string prop)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
    }

    private bool SetBirthday = false;
    private void CalculateBirthday()
    {
        int age = 0;
        if (int.TryParse(Age, out age))
        {
            SetBirthday = true;
            Birthday = AgeHelper.GetBirthday(AgeType, age);
            SetBirthday = false;
        }
    }

    private void CalculateAge()
    {
        if (Birthday is null)
        {
            return;
        }

        var ageInfo = AgeHelper.CalculateAgeByBirthday(Birthday.Value);
        age = ageInfo.Item1.ToString();
        ageType = ageInfo.Item2;

        ClearError("Age");
        RaisePropertyChanged("Age");
        RaisePropertyChanged("AgeType");
    }
}