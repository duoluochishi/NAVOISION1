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
using NV.MPS.UI.Dialog.Service;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.Language;

namespace NV.CT.ConfigManagement.ViewModel;

public class LoginSettingViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<LoginSettingViewModel> _logger;

    private int _minPasswordLength;
    public int MinPasswordLength
    {
        get => _minPasswordLength;
        set => SetProperty(ref _minPasswordLength, value);
    }

    private int _maxPasswordLength;
    public int MaxPasswordLength
    {
        get => _maxPasswordLength;
        set => SetProperty(ref _maxPasswordLength, value);
    }

	private bool _applyStrongPassword ;
    public bool ApplyStrongPassword
    {
        get => _applyStrongPassword;
        set => SetProperty(ref _applyStrongPassword, value);
    }

    private int _wrongPassTryTimes;
    public int WrongPassTryTimes
    {
        get => _wrongPassTryTimes;
        set => SetProperty(ref _wrongPassTryTimes, value);
    }

	public LoginSettingViewModel(IDialogService dialogService, ILogger<LoginSettingViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;

        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void GetNodeInfo()
    {
        var node = UserConfig.LoginSetting;

        MinPasswordLength = node.MinPasswordLength.Value;
        MaxPasswordLength=node.MaxPasswordLength.Value;
        WrongPassTryTimes=node.WrongPassTryTimes.Value;
        ApplyStrongPassword = node.ApplyStrongPassword.Value;
    }

    public void Saved()
    {
        if (!CheckFormEmpty())
        {
            return;
        }

        UserConfig.LoginSetting.MinPasswordLength.Value = MinPasswordLength;
        UserConfig.LoginSetting.MaxPasswordLength.Value = MaxPasswordLength;
        UserConfig.LoginSetting.WrongPassTryTimes.Value = WrongPassTryTimes;
        UserConfig.LoginSetting.ApplyStrongPassword.Value = ApplyStrongPassword;

		bool saveFlag = UserConfig.SaveLoginSetting();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg => { }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        if (MinPasswordLength <= 0 || MaxPasswordLength<=0 || WrongPassTryTimes<=0)
        {
            sb.Append("Min password , Max password , Wrong password max try times cannot be smaller than 0!");
            flag = false;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }
}