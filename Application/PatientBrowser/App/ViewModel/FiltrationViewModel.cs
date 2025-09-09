//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS.Enums;
using NV.CT.PatientBrowser.Models;
using NV.CT.PatientBrowser.ApplicationService.Contract.Interfaces;
using Prism.Commands;
using System.Windows;

namespace NV.CT.PatientBrowser.ViewModel;

public class FiltrationViewModel : UI.ViewModel.BaseViewModel
{
    private readonly IStudyApplicationService _studyService;
    private readonly IPatientConfigService _patientConfigService;
    private bool _isToday = true;
    public bool IsToday
    {
        get => _isToday;
        set
        {
            if (SetProperty(ref _isToday, value) && value)
            {
                searchTimeType = SearchTimeType.Today;
            }
        }
    }

    private bool _isYesterday = false;
    public bool IsYesterday
    {
        get => _isYesterday;
        set
        {
            if (SetProperty(ref _isYesterday, value) && value)
            {
                searchTimeType = SearchTimeType.Yesterday;
            }
        }
    }

    private bool _isDayBeforeYesterday = false;
    public bool IsDayBeforeYesterday
    {
        get => _isDayBeforeYesterday;
        set
        {
            if (SetProperty(ref _isDayBeforeYesterday, value) && value)
            {
                searchTimeType = SearchTimeType.DayBeforeYesterday;
            }
        }
    }

    private bool _isLast7Days = false;
    public bool IsLast7Days
    {
        get => _isLast7Days;
        set
        {
            if (SetProperty(ref _isLast7Days, value) && value)
            {
                searchTimeType = SearchTimeType.Last7Days;
            }
        }
    }

    private bool _isLast30Days = false;
    public bool IsLast30Days
    {
        get => _isLast30Days;
        set
        {
            if (SetProperty(ref _isLast30Days, value) && value)
            {
                searchTimeType = SearchTimeType.Last30Days;
            }
        }
    }

    private bool _isAll = false;
    public bool IsAll
    {
        get => _isAll;
        set
        {
            if (SetProperty(ref _isAll, value) && value)
            {
                searchTimeType = SearchTimeType.All;
            }
        }
    }

    private PatientConfig _patientConfig;
    private SearchTimeType _searchTimeType = SearchTimeType.Today;
    private SearchTimeType searchTimeType
    {
        get => searchTimeType;
        set
        {
            if (SetProperty(ref _searchTimeType, value)
                && _patientConfig is not null
                && _patientConfig.PatientQueryConfig is not null
                && !IsInitFlag)
            {
                _patientConfig.PatientQueryConfig.PatientQueryTimeType = value;
                _patientConfigService.Save(_patientConfig);
                _studyService.RefreshRearchDateTyped(_patientConfig.PatientQueryConfig.PatientQueryTimeType);
            }
        }
    }

    private bool IsInitFlag = false;

    public FiltrationViewModel(IStudyApplicationService studyService, IPatientConfigService patientConfigService)
    {
        _studyService = studyService;
        _patientConfigService = patientConfigService;       
        Commands.Add(PatientConstString.COMMAND_CLOSED, new DelegateCommand<object>(Closed, _ => true));

        _patientConfig = _patientConfigService.GetConfigs();
        IsInitFlag = true;
        InitPatientQueryTimeType(_patientConfig.PatientQueryConfig);
        IsInitFlag = false;
    }

    private void InitPatientQueryTimeType(PatientQueryConfig patientQueryConfig)
    {
        switch (patientQueryConfig.PatientQueryTimeType)
        {
            case SearchTimeType.Today:
                IsToday = true;
                break;
            case SearchTimeType.Yesterday:
                IsYesterday = true;
                break;
            case SearchTimeType.DayBeforeYesterday:
                IsDayBeforeYesterday = true;
                break;
            case SearchTimeType.Last7Days:
                IsLast7Days = true;
                break;
            case SearchTimeType.Last30Days:
                IsLast7Days = true;
                break;
            case SearchTimeType.All:
                IsAll = true;
                break;
            default:
                IsToday = true;
                break;
        }
    }
  
    public void Closed(object parameter)
    {
        if (parameter is Window window)
        {
            window.Hide();
        }
    }
}