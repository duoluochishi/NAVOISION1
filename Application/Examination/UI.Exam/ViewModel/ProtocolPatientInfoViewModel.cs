//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.Examination.ApplicationService.Contract.Models;
using AgeType = NV.CT.CTS.Enums.AgeType;

namespace NV.CT.UI.Exam.ViewModel;

public class ProtocolPatientInfoViewModel : BaseViewModel
{
    private int _patientAge = 30;
    public int PatientAge
    {
        get => _patientAge;
        set => SetProperty(ref _patientAge, value);
    }

    private string _patientAgeType = "Year";
    public string PatientAgeType
    {
        get => _patientAgeType;
        set => SetProperty(ref _patientAgeType, value);
    }

    private double? _patientWeight = 75;
    public double? PatientWeight
    {
        get => _patientWeight;
        set => SetProperty(ref _patientWeight, value);
    }

    private string _patientGender = "Male";
    public string PatientGender
    {
        get => _patientGender;
        set => SetProperty(ref _patientGender, value);
    }

    private double? _patientHeight = 165;
    public double? PatientHeight
    {
        get => _patientHeight;
        set => SetProperty(ref _patientHeight, value);
    }

    public ProtocolPatientInfoViewModel(IStudyHostService studyHostService)
    {
        GetPatientInfo(studyHostService.Instance);      
    }

    private void GetPatientInfo(StudyModel studyModel)
    {
        if (studyModel is null)
        {
            return;
        }
        PatientAge = studyModel.Age;
        PatientAgeType = ((AgeType)studyModel.AgeType).GetDescription();
        PatientWeight = studyModel.PatientWeight;
        PatientHeight = studyModel.PatientSize;
        PatientGender = studyModel.PatientSex.GetDescription();
    }
}