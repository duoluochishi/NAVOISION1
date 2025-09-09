using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.CTS.Encryptions;

namespace NV.CT.NanoConsole.ViewModel;

public class LockViewModel : BaseViewModel
{
	private readonly IAuthorization _authorization;
	private readonly ILayoutManager _layoutManager;
	private readonly IUserService _userService;
	private readonly IWorkflow _workflow;
	private readonly IConsoleApplicationService _consoleAppService;
	private string _loginAccount=string.Empty;
	private readonly IInputListener _inputListener;
	//private readonly int WrongPasswordMaxTryTimes = UserConfig.LoginSetting.WrongPassTryTimes.Value;
	public LockViewModel(IUserService userService, ILayoutManager layoutManager, IAuthorization authorization,IWorkflow workflow,IConsoleApplicationService consoleApplicationService,IInputListener inputListener)
	{
		_workflow=workflow;
		_userService = userService;
		_authorization = authorization;
		_layoutManager = layoutManager;
		_inputListener=inputListener;
		_consoleAppService = consoleApplicationService;

		Commands.Add("UnlockCommand", new DelegateCommand(UnlockScreen, CanUnlock));
		Commands.Add("SwitchUserCommand", new DelegateCommand(SwitchUser));

		_layoutManager.LayoutChanged += LayoutChanged;
		_userService.UserLogoutSuccess += UserLogoutSuccess;
		_workflow.LockScreenChanged += LockScreenChanged;
	}

	[UIRoute]
	private void LockScreenChanged(object? sender, EventArgs e)
	{
		Password = string.Empty;
		ValidationMsg = string.Empty;
	}

	[UIRoute]
	private void LayoutChanged(object? sender, Screens e)
	{
		if (e != Screens.LockScreen)
			return;

		var currentUser = _authorization.GetCurrentUser();
		if (currentUser == null) return;
		Username = currentUser.Account;
		_loginAccount = currentUser.Account;
		Password = string.Empty;
		ValidationMsg = string.Empty;
	}

	private bool CanUnlock()
	{
		if (Password.IsEmpty())
			return false;

		return true;
	}

	private void UserLogoutSuccess(object? sender, EventArgs e)
	{
		ValidationMsg = string.Empty;
	}

	private void SwitchUser()
	{
		var user = _authorization.GetCurrentUser();
		if (user is not null)
		{
			//注销后面的账户服务数据
			_userService.LogOut(new AuthorizationRequest(user.Account, user.Password));

			_workflow.UnlockScreen(Screens.Login.ToString());
		}
	}

	private void UnlockScreen()
	{
		//var username = Username.Trim();
		var password = Password.Trim();
		if (password.IsEmpty())
		{
			ValidationMsg = "Password is empty!";
			return;
		}

		var validationResult = _userService.Login(new AuthorizationRequest(_loginAccount, MD5Helper.Encrypt(password)));
		if (validationResult.IsSuccess)
		{
			////重置 登录次数
			//_userService.ResetLoginTimes(username);

			_workflow.UnlockScreen(string.Empty);

			if (!IsDevelopment)
			{
				_inputListener.Reset();
			}
		}
		else
		{
			ValidationMsg = "Password wrong!";
		}
	}

	#region properties

	private string _username = string.Empty;
	public string Username
	{
		get => _username;
		set => SetProperty(ref _username, value);
	}

	private string _password = string.Empty;
	public string Password
	{
		get => _password;
		set => SetProperty(ref _password, value);
	}

	private string _validationMsg = string.Empty;
	public string ValidationMsg
	{
		get => _validationMsg;
		set => SetProperty(ref _validationMsg, value);
	}


	#endregion
}