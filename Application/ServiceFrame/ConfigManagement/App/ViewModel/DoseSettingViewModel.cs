//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.MPS.UI.Dialog.Service;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.Language;

namespace NV.CT.ConfigManagement.ViewModel;

public class DoseSettingViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<DoseSettingViewModel> _logger;

    private bool isNotice = false;
    public bool IsNotice
    {
        get => isNotice;
        set => SetProperty(ref isNotice, value);
    }

    private string childCTDIVal;
    public string ChildCTDIVal
    {
        get => childCTDIVal;
        set => SetProperty(ref childCTDIVal, value);
    }

    private string childDLPVal;
    public string ChildDLPVal
    {
        get => childDLPVal;
        set => SetProperty(ref childDLPVal, value);
    }

    private string adultCTDIVal;
    public string AdultCTDIVal
    {
        get => adultCTDIVal;
        set => SetProperty(ref adultCTDIVal, value);
    }

    private string adultDLPVal;
    public string AdultDLPVal
    {
        get => adultDLPVal;
        set => SetProperty(ref adultDLPVal, value);
    }

    public DoseSettingViewModel(IDialogService dialogService,
        ILogger<DoseSettingViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;
        Commands.Add("SaveCommand", new DelegateCommand(SaveCommand));
        GetDoseSetting();
    }

    public void GetDoseSetting()
    {
        DoseSettingInfo setting = UserConfig.DoseSettingConfig.DoseSetting;

        IsNotice = setting.NotificationEnabled.Value;
        AdultCTDIVal = Math.Round(setting.AdultAlertCTDIThreshold.Value, 4).ToString();
        AdultDLPVal = Math.Round(setting.AdultAlertDLPThreshold.Value, 4).ToString();
        ChildCTDIVal = Math.Round(setting.ChildAlertCTDIThreshold.Value, 4).ToString();
        ChildDLPVal = Math.Round(setting.ChildAlertDLPThreshold.Value, 4).ToString();
    }

    public void SaveCommand()
    {
        double AdultC, AdultD, ChildC, ChildD = 0;

        if (!double.TryParse(AdultCTDIVal, out AdultC) || AdultC < 0)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", "Please enter a number greater than or equal to 0 for Adult CTDI!",
            arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        if (!double.TryParse(AdultDLPVal, out AdultD) || AdultD < 0)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", "Please enter a number greater than or equal to 0 for Adult DLP!",
            arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        if (!double.TryParse(ChildCTDIVal, out ChildC) || ChildC < 0)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", "Please enter a number greater than or equal to 0 for Child CTDI!",
            arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        if (!double.TryParse(ChildDLPVal, out ChildD) || ChildD < 0)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", "Please enter a number greater than or equal to 0 for Child DLP!",

            arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        DoseSettingInfo doseSettingInfo = UserConfig.DoseSettingConfig.DoseSetting;
        doseSettingInfo.NotificationEnabled.Value = IsNotice;
        doseSettingInfo.AdultAlertCTDIThreshold.Value = AdultC;
        doseSettingInfo.AdultAlertDLPThreshold.Value = AdultD;
        doseSettingInfo.ChildAlertCTDIThreshold.Value = ChildC;
        doseSettingInfo.ChildAlertDLPThreshold.Value = ChildD;

        UserConfig.DoseSettingConfig.DoseSetting = doseSettingInfo;
        bool saveFlag = UserConfig.SaveDoseSetting();

        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
            arg => { }, ConsoleSystemHelper.WindowHwnd);
    }
}