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

public class SystemSettingViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ILogger<SystemSettingViewModel> _logger;

    private int _systemLockTime;
    public int SystemLockTime
	{
        get => _systemLockTime;
        set => SetProperty(ref _systemLockTime, value);
    }

	public SystemSettingViewModel(IDialogService dialogService, ILogger<SystemSettingViewModel> logger)
    {
        _dialogService = dialogService;
        _logger = logger;

        GetNodeInfo();
        Commands.Add("SaveCommand", new DelegateCommand(Saved));
    }

    private void GetNodeInfo()
    {
        var node = UserConfig.SystemSetting;

        SystemLockTime = node.SystemLockTime.Value;
    }

    public void Saved()
    {
        if (!CheckFormEmpty())
        {
            return;
        }

        UserConfig.SystemSetting.SystemLockTime.Value = SystemLockTime;

        bool saveFlag = UserConfig.SaveSystemSetting();
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg => { }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        //StringBuilder sb = new StringBuilder();
        //int.TryParse(SystemLockTime.ToString(),)
        //if (System <= 0 || MaxPasswordLength<=0 || WrongPassTryTimes<=0)
        //{
        //    sb.Append("Min password , Max password , Wrong password max try times cannot be smaller than 0!");
        //    flag = false;
        //}
        //if (!flag)
        //{
        //    _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
        //        arg => { }, ConsoleSystemHelper.WindowHwnd);
        //}
        return flag;
    }
}