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
using System.Collections.Generic;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.Language;
using System.Net;
using Autofac.Core;

namespace NV.CT.ConfigManagement.ViewModel;

public class TabletViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly ITabletApplicationService _tabletApplicationService;

    private BaseTabletViewModel _currentTablet = new BaseTabletViewModel();
    public BaseTabletViewModel CurrentTablet
    {
        get => _currentTablet;
        set => SetProperty(ref _currentTablet, value);
    }

    private bool _isEdit = false;
    public bool IsEdit
    {
        get => _isEdit;
        set => SetProperty(ref _isEdit, value);
    }

    public OperationType OperationType { get; set; } = OperationType.Add;

    public TabletViewModel(ITabletApplicationService tabletApplicationService,
        IDialogService dialogService)
    {
        _dialogService = dialogService;
        _tabletApplicationService = tabletApplicationService;

        Commands.Add("SaveCommand", new DelegateCommand<object>(Saved, _ => true));
        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => true));
        _tabletApplicationService.TabletInfoChanged += TabletApplicationService_TabletInfoChanged;
    }

    [UIRoute]
    private void TabletApplicationService_TabletInfoChanged(object? sender, EventArgs<(OperationType operation, TabletInfo tabletModel)> e)
    {
        if (e is null)
        {
            return;
        }
        OperationType = e.Data.operation;
        SetRoleInfo(e.Data.tabletModel);
        if (OperationType == OperationType.Edit)
        {
            IsEdit = false;
        }
        if (OperationType == OperationType.Add)
        {
            IsEdit = true;
        }
    }

    private void SetRoleInfo(TabletInfo tabletInfo)
    {
        CurrentTablet = new BaseTabletViewModel();
        CurrentTablet.ID = tabletInfo.Id;
        CurrentTablet.IP = tabletInfo.IP;
        CurrentTablet.SN = tabletInfo.SerialNumber;
    }

    public void Saved(object parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }
        if (!CheckFormEmpty() || !CheckIpPort() || CheckNameRepeat() || !CheckNumAndEnChForm())
        {
            return;
        }
		IPAddress ip = IPAddress.None;
		IPAddress.TryParse(CurrentTablet.IP, out ip);
		CurrentTablet.IP = ip.ToString();

		TabletInfo tabletInfo = new TabletInfo();
        tabletInfo.Id = CurrentTablet.ID;
        tabletInfo.IP = CurrentTablet.IP;
        tabletInfo.SerialNumber = CurrentTablet.SN;

        bool saveFlag = false;
        switch (OperationType)
        {
            case OperationType.Add:
                saveFlag = _tabletApplicationService.Add(tabletInfo);
                break;
            case OperationType.Edit:
            default:
                saveFlag = _tabletApplicationService.Update(tabletInfo);
                break;
        }
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg =>
          {
              if (saveFlag)
              {
                  _tabletApplicationService.ReloadTabletList();
                  window.Hide();
              }
          }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} can't be empty!";

        if (string.IsNullOrEmpty(CurrentTablet.IP))
        {
            sb.Append(string.Format(message, "IP"));
            flag = false;
        }
        if (string.IsNullOrEmpty(CurrentTablet.SN))
        {
            sb.Append(string.Format(message, "SN"));
            flag = false;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

	private bool CheckIpPort()
	{
		bool flag = true;
		StringBuilder sb = new StringBuilder();
		string message = "{0} Incorrect input!";
		if (!VerificationExtension.IpFormatVerification(CurrentTablet.IP))
		{
			sb.Append(string.Format(message, "IP"));
			flag = false;
		}		
		if (!flag)
		{
			_dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
				arg => { }, ConsoleSystemHelper.WindowHwnd);
		}
		return flag;
	}

	private bool CheckNameRepeat()
    {
        bool flag = false;
        List<TabletInfo> tablets = _tabletApplicationService.GetAllTabletInfo();
        switch (OperationType)
        {
            case OperationType.Add:
                flag = tablets.Any(t => t.IP == CurrentTablet.IP);
                break;
            case OperationType.Edit:
                flag = tablets.Any(t => t.IP != CurrentTablet.IP && t.SerialNumber == CurrentTablet.SN);
                break;
            default: break;
        }
        if (flag)
        {
            var message = "The name is duplicated!";
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

	private bool CheckNumAndEnChForm()
	{
		bool flag = true;
		string message = "";
		if (VerificationExtension.IsSpecialCharacters(CurrentTablet.SN))
		{
			flag = false;
			message += $"SN:Special characters are not allowed!";
		}
		if (!flag)
		{
			_dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
				arg => { }, ConsoleSystemHelper.WindowHwnd);
		}
		return flag;
	}

	public void Closed(object parameter)
    {
        if (parameter is Window window)
        {
            _tabletApplicationService.ReloadTabletList();
            window.Hide();
        }
    }
}