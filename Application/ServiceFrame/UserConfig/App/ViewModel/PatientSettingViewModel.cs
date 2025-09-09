using Prism.Commands;
using System;
using NV.CT.CTS.Enums;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.ConfigService.Contract;
using NV.CT.UI.ViewModel;
using NV.MPS.UI.Dialog.Service;
using NV.MPS.Configuration ;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.CTS.Helpers;
using NV.CT.Language;

namespace NV.CT.UserConfig.ViewModel;
public class PatientSettingViewModel : BaseViewModel
{
    private readonly IPatientConfigService _patientConfigService;
    private IDialogService _dialogService;

    private int _adultAgeThreshold = 12;
    public int AdultAgeThreshold
    {
        get => _adultAgeThreshold;
        set => SetProperty(ref _adultAgeThreshold, value);
    }

    public PatientSettingViewModel(IDialogService dialogService,
        IPatientConfigService patientConfigService)
    {
        _patientConfigService = patientConfigService;
        _dialogService = dialogService;

        IntervalCheckStates.Add("10", false);
        IntervalCheckStates.Add("30", false);
        IntervalCheckStates.Add("60", false);
        IntervalCheckStates.Add("600", false);
        IntervalCheckStates.Add("1800", false);

        SuffixTypeCheckStates.Add(SuffixType.TimeFormat.ToString(), false);
        SuffixTypeCheckStates.Add(SuffixType.SelfAccretion.ToString(), false);

        CurrentPatientConfig = _patientConfigService.GetConfigs();

        IntervalCheckStates[CurrentPatientConfig.RefreshTimeInterval.Interval.ToString()] = true;
        SuffixTypeCheckStates[CurrentPatientConfig.PatientIdConfig.SuffixType.ToString()] = true;
        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(SavePatientConfigCommand));
        Commands.Add("SelectIntervalCommand", new DelegateCommand<string>(SelectIntervalCommand));
        Commands.Add("SelectSuffixTypeCommand", new DelegateCommand<object>(SelectSuffixTypeCommand));
        Commands.Add("LoadCommand", new DelegateCommand<PatientConfig>(LoadCommand));

        LoadAllList();
    }

    private void GetNodeInfo()
    {
        PatientSettingInfo node = NV.MPS.Configuration.UserConfig.PatientSettingConfig.PatientSetting;
        AdultAgeThreshold = node.AdultAgeThreshold.Value;
    }

    private OperationType operationType;
    public OperationType OperationType
    {
        get => operationType;
        set => SetProperty(ref operationType, value);
    }

    public void SelectIntervalCommand(string interval)
    {
        IntervalCheckStates[interval] = true;
        foreach (var item in IntervalCheckStates.Keys)
        {
            if (item != interval)
            {
                IntervalCheckStates[item] = false;
            }
        }
        //清空
        CurrentPatientConfig.RefreshTimeInterval.Interval = int.Parse(interval);
        CurrentInterval = interval;
    }

    public void SelectSuffixTypeCommand(object suffixType)
    {
        SuffixTypeCheckStates[suffixType.ToString()] = true;
        //清空
        CurrentPatientConfig.PatientIdConfig.SuffixType = Enum.Parse<SuffixType>(suffixType.ToString());
        CurrentSuffixType = suffixType.ToString();
    }

    public void SavePatientConfigCommand()
    {
        //3245 【SH】【PD】【IV2.0】系统设置儿童年龄为13year ，依旧可以保存成功  来源用例-581
        if (AdultAgeThreshold > 12)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", "No more than 12 years old!", null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        PatientSettingInfo node = NV.MPS.Configuration.UserConfig.PatientSettingConfig.PatientSetting;
        node.AdultAgeThreshold.Value = AdultAgeThreshold;

        NV.MPS.Configuration.UserConfig.PatientSettingConfig.PatientSetting = node;
        NV.MPS.Configuration.UserConfig.SavePatientSetting();

        if (operationType == OperationType.Add ||
            operationType == OperationType.Edit)
        {
            CurrentPatientConfig.RefreshTimeInterval.Interval = int.Parse(CurrentInterval);
            CurrentPatientConfig.PatientIdConfig.Prefix = CurrentPrefix;
            CurrentPatientConfig.PatientIdConfig.Infix = CurrentInfix;
            CurrentPatientConfig.PatientIdConfig.SuffixType = Enum.Parse<SuffixType>(CurrentSuffixType);
            CurrentPatientConfig.DisplayItems.Items.ForEach(item =>
            {
                if (item.IsFixed)
                {
                    item.ItemName = item.ItemName.TrimStart('*');
                }
            });
            PatientConfig patientConfig = _patientConfigService.GetConfigs();

            patientConfig = CurrentPatientConfig;

            _patientConfigService.Save(patientConfig);
            CurrentPatientConfig.DisplayItems.Items.ForEach(item =>
            {
                if (item.IsFixed)
                {
                    item.ItemName = "*" + item.ItemName.TrimStart('*');
                }
            });
        }
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", LanguageResource.Message_Info_SaveSuccessfullyPara,
         arg => { }, ConsoleSystemHelper.WindowHwnd);
    }

    public void LoadCommand(PatientConfig patientConfig)
    {
        if (patientConfig is null)
        {
            return;
        }
        CurrentPatientConfig = _patientConfigService.GetConfigs();
        OperationType = OperationType.Edit;
    }

    public void LoadAllList()
    {
        CurrentPatientConfig = _patientConfigService.GetConfigs();
        CurrentPatientConfig.DisplayItems.Items.ForEach(item =>
        {
            if (item.IsFixed)
            {
                item.ItemName = "*" + item.ItemName.TrimStart('*');
            }
        });

        IntervalCheckStates[CurrentPatientConfig.RefreshTimeInterval.Interval.ToString()] = true;
        CurrentInterval = CurrentPatientConfig.RefreshTimeInterval.Interval.ToString();
        CurrentPrefix = CurrentPatientConfig.PatientIdConfig.Prefix;
        CurrentInfix = CurrentPatientConfig.PatientIdConfig.Infix;
        SuffixTypeCheckStates[CurrentPatientConfig.PatientIdConfig.SuffixType.ToString()] = true;
        CurrentSuffixType = CurrentPatientConfig.PatientIdConfig.SuffixType.ToString();

        OperationType = OperationType.Edit;
        SetCurrentPreview();
    }

    public void SetCurrentPreview()
    {
        if (CurrentPatientConfig.PatientIdConfig.SuffixType == SuffixType.TimeFormat)
        {
            CurrentPreview = CurrentPrefix + "_" + CurrentInfix + DateTime.Now.ToString("yyMMddHHmmss0000");
        }
        else
        {
            currentPrefix = "Nano";
            currentInfix = "0001";
            CurrentPreview = CurrentPrefix + "_" + CurrentInfix + "1";
        }
    }

    private PatientConfig currentPatientConfig;
    public PatientConfig CurrentPatientConfig
    {
        get => currentPatientConfig;
        set => SetProperty(ref currentPatientConfig, value);
    }

    private string currentInterval = string.Empty;
    public string CurrentInterval
    {
        get => currentInterval;
        set => SetProperty(ref currentInterval, value);
    }
    private ObservableDictionary<string, bool> intervalCheckStates = new ObservableDictionary<string, bool>();
    public ObservableDictionary<string, bool> IntervalCheckStates
    {
        get => intervalCheckStates;
        set => SetProperty(ref intervalCheckStates, value);
    }

    private string currentPrefix = string.Empty;
    public string CurrentPrefix
    {
        get => currentPrefix;
        set
        {
            SetProperty(ref currentPrefix, value);
            SetCurrentPreview();
        }
    }
    private string currentInfix = string.Empty;
    public string CurrentInfix
    {
        get => currentInfix;
        set
        {
            SetProperty(ref currentInfix, value);
            SetCurrentPreview();
        }
    }

    private string currentSuffixType = string.Empty;
    public string CurrentSuffixType
    {
        get => currentSuffixType;
        set
        {
            SetProperty(ref currentSuffixType, value);
            SetCurrentPreview();
        }
    }
    private string currentPreview = string.Empty;
    public string CurrentPreview
    {
        get => currentPreview;
        set => SetProperty(ref currentPreview, value);
    }
    private ObservableDictionary<string, bool> suffixTypeCheckStates = new ObservableDictionary<string, bool>();
    public ObservableDictionary<string, bool> SuffixTypeCheckStates
    {
        get => suffixTypeCheckStates;
        set
        {
            SetProperty(ref suffixTypeCheckStates, value);
            SetCurrentPreview();
        }
    }
}