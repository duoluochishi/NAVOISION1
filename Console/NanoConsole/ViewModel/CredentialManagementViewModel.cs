using NV.CT.CTS.Encryptions;
using NV.CT.Models.User;
using System.Text.RegularExpressions;

namespace NV.CT.NanoConsole.ViewModel;

public class CredentialManagementViewModel : BaseViewModel
{
	private readonly ILogger<CredentialManagementViewModel> _logger;
	private readonly IConsoleApplicationService _consoleApplicationService;
	private readonly IAuthorization _authorization;
	private readonly IUserService _userService;
	public CredentialManagementViewModel(ILogger<CredentialManagementViewModel> logger, IConsoleApplicationService consoleApplicationService, IAuthorization authorization,IUserService userService)
	{
		_logger = logger;
		_authorization = authorization;
		_userService = userService;
		_consoleApplicationService = consoleApplicationService;
		_consoleApplicationService.ChangePasswordStarted += ChangePasswordStarted;

		Commands.Add("CloseCommand", new DelegateCommand<object>(CloseClicked, _ => true));
		Commands.Add("CancelCommand", new DelegateCommand<object>(CancelClicked, _ => true));
		Commands.Add("DragMoveCommand", new DelegateCommand<object>(DragMove, _ => true));

		Commands.Add("ConfirmChangeCommand", new DelegateCommand(ConfirmChange,  CanConfirm));
	}

	private void ChangePasswordStarted(object? sender, EventArgs e)
	{
		OldPassword = string.Empty;
		NewPassword = string.Empty;
		ConfirmPassword = string.Empty;
		ErrorMsg = string.Empty;
	}

	public bool CanConfirm()
	{
		return !OldPassword.IsEmpty() && !NewPassword.IsEmpty() && !ConfirmPassword.IsEmpty();
	}

	/// <summary>
	/// 确认修改密码
	/// </summary>
	public void ConfirmChange()
	{
		OldPassword = OldPassword.Trim();
		NewPassword = NewPassword.Trim();
		ConfirmPassword = ConfirmPassword.Trim();

		//validate old password 
		var currentUser = _authorization.GetCurrentUser();
		if (currentUser is null)
		{
			ErrorMsg = "Please login first!";
			return;
		}

		var compareResult=_userService.IsPasswordCorrect(new IsPasswordCorrectRequest( currentUser.Password, OldPassword));
		if (!compareResult)
		{
			ErrorMsg = "Old password is not correct";
			return;
		}

		//compare two password
		if(NewPassword!=ConfirmPassword)
		{
			ErrorMsg = "Confirm password not equal to new password!";
			return;
		}

		//如果使用了, 强密码策略验证
		if (UserConfig.LoginSetting.ApplyStrongPassword.Value)
		{
			var minLen = UserConfig.LoginSetting.MinPasswordLength.Value;
			var maxLen = UserConfig.LoginSetting.MaxPasswordLength.Value;
			if (NewPassword.Length > maxLen || NewPassword.Length < minLen)
			{
				ErrorMsg = $"Strong password length should between {minLen} to {maxLen} !";
				return;
			}

			var validateOk = IsAlphaNumeric(NewPassword);
			if (!validateOk)
			{
				ErrorMsg = "Strong password should include at least a uppercase letter,a lowercase letter and a number !";
				return;
			}
		}

		//update password to db
		var updateResult=_userService.UpdatePassword(new UpdatePasswordRequest(currentUser.Id, MD5Helper.Encrypt(NewPassword)));
		if (updateResult)
		{
			ErrorMsg = "Update success!";
		}
		else
		{
			ErrorMsg = "Update failed! Please contact system administrator!";
		}
	}

	public void DragMove(object parameter)
	{
		if ((parameter as Window) is not null)
		{
			((Window)parameter).DragMove();
		}
	}

	public void CloseClicked(object parameter)
	{
		if (parameter is Window window)
		{
			window.Hide();
		}
	}

	public void CancelClicked(object parameter)
	{
		if (parameter is Window window)
		{
			window.Hide();
		}
	}


	private string _oldPassword = string.Empty;
	public string OldPassword
	{
		get => _oldPassword;
		set => SetProperty(ref _oldPassword, value);
	}

	private string _newPassword = string.Empty;
	public string NewPassword
	{
		get => _newPassword;
		set => SetProperty(ref _newPassword, value);
	}

	private string _confirmPassword = string.Empty;
	public string ConfirmPassword
	{
		get => _confirmPassword;
		set => SetProperty(ref _confirmPassword, value);
	}

	private string _errorMsg = string.Empty;
	public string ErrorMsg
	{
		get => _errorMsg;
		set => SetProperty(ref _errorMsg, value);
	}

	public bool IsAlphaNumeric(string input)
	{
		// 正则表达式匹配 最少一个大写,一个小写,一个数字
		string pattern = @"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])[a-zA-Z0-9]+$";
		Regex regex = new Regex(pattern);
		return regex.IsMatch(input);
	}
}