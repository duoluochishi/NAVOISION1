using NV.CT.CTS.Encryptions;

namespace NV.CT.NanoConsole.ViewModel;

public class LoginViewModel : BaseViewModel
{
	private readonly ILayoutManager _layoutManager;
	private readonly IUserService _userService;
	private readonly ILoginHistoryService _loginHistoryService;
	private readonly ILogger<LoginViewModel> _logger;
	private readonly IConsoleApplicationService _consoleApplicationService;
	private readonly int WrongPasswordMaxTryTimes = UserConfig.LoginSetting.WrongPassTryTimes.Value;
	public LoginViewModel(IUserService userService, ILayoutManager layoutManager, ILogger<LoginViewModel> logger, ILoginHistoryService loginHistoryService,IConsoleApplicationService consoleApplicationService)
	{
		_logger = logger;
		_userService = userService;
		_layoutManager = layoutManager;
		_loginHistoryService = loginHistoryService;
		_consoleApplicationService=consoleApplicationService;

		Commands.Add("LoginCommand", new DelegateCommand(LoginProcess));
		Commands.Add("ChooseServiceKey", new DelegateCommand(ChooseServiceKey));
		Commands.Add("LoginWithServiceKey", new DelegateCommand(LoginWithServiceKey));
		Commands.Add("EmergencyLoginCommand", new DelegateCommand(EmergencyLogin));

		_userService.UserLogoutSuccess += UserLogoutSuccess;

		InitTestAccounts();
	}

	/// <summary>
	/// 如果是上一次登录是 [服务] 角色用户,默认加载 用户名和密码 (如果记住密码了)
	/// </summary>
	public void FillDefaultAccount()
	{
		var lastLoginUser = _loginHistoryService.GetLastLogin();
		if (lastLoginUser is null)
			return;

		var realUser = _userService.GetUserByUserName(lastLoginUser.Account);
		if (realUser is null)
			return;
		var userRoleModel = _userService.GetUserRolePermissionList(realUser.Id);
		var hasOperatorRole = IsServiceUser(userRoleModel);
		if (hasOperatorRole)
		{
			Username = realUser.Account ?? string.Empty;

			//[技师]角色用户,并且记住了密码
			if (UserConfig.LoginSetting.RememberPassword.Value)
			{
				Password = DESHelper.DecryptString(lastLoginUser.EncryptPassword);
			}
		}
	}

	private bool IsServiceUser(UserModel userModel)
	{
		//只需要判断角色即可
		if (userModel.RoleList.Any(n => string.Equals(n.Name.Trim(), "operator", StringComparison.OrdinalIgnoreCase)))
		{
			return true;
		}

		return false;
	}

	private void UserLogoutSuccess(object? sender, EventArgs e)
	{
		ValidationMsg = string.Empty;
		Username = string.Empty;
		Password = string.Empty;
	}

	private void InitTestAccounts()
	{
		TestAccounts = new ObservableCollection<TestAccount>();
		TestAccounts.Add(new TestAccount() { Account = "NanoUser", Password = "123456" });
		TestAccounts.Add(new TestAccount() { Account = "NanoEngineer", Password = "123456" });
		TestAccounts.Add(new TestAccount() { Account = "NanoService", Password = "123456" });
		TestAccounts.Add(new TestAccount() { Account = "NanoSenior", Password = "123456" });
		TestAccounts.Add(new TestAccount() { Account = "NanoAdmin", Password = "123456" });
		TestAccounts.Add(new TestAccount() { Account = "NanoDeviceManager", Password = "123456" });
	}

	/// <summary>
	/// 服务用户登录
	/// </summary>
	private void LoginWithServiceKey()
	{
		var serviceKeyContent = string.Empty;
		var validationResult = _userService.ServiceUserLogin(serviceKeyContent);
		if (validationResult.IsSuccess)
		{
			_layoutManager.Goto(Screens.Main);
		}
		else
		{
			//validation fail,show message to user
			ValidationMsg = validationResult.Message;
		}
	}

	private void ChooseServiceKey()
	{
		var openFileDialog = new Microsoft.Win32.OpenFileDialog()
		{
			Filter = "Text documents (.txt)|*.txt|All files (*.*)|*.*"
		};
		var result = openFileDialog.ShowDialog();
		if (result == true)
		{
			ChoosedServiceKey = openFileDialog.FileName;
		}
		else
		{
			//user do not choose any file
			ChoosedServiceKey = string.Empty;
		}
	}

	private void EmergencyLogin()
	{
		if (_userService.EmergencyLogin())
		{
			_layoutManager.Goto(Screens.Emergency);
		}
	}

	public void Login(string username, string password)
	{
		Username = username;
		Password = password;

		LoginProcess();
	}

	private void LoginProcess()
	{
		var username = Username.Trim();
		var password = Password.Trim();
		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			ValidationMsg = "username or password is empty!";
			return;
		}

		var validationResult = _userService.Login(new AuthorizationRequest(username, MD5Helper.Encrypt(password)));
		if (validationResult is not null && validationResult.IsSuccess)
		{
			//记录 登录历史
			AddLoginHistory(true, UserBehavior.Login, username, password);

			//登录成功,重置登录次数
			_userService.ResetLoginTimes(username);

			_layoutManager.Goto(Screens.Main);

			_consoleApplicationService.EnterIntoMain();
		}
		else
		{
			//登录失败 
			var userModel = _userService.GetUserByUserName(username);
			
			//如果是 [技师] 这个角色的,做锁定校验,其他角色不做锁定校验
			if (userModel is not null)
			{
				var realUser = _userService.GetUserRolePermissionList(userModel.Id);
				if (!IsServiceUser(realUser))
				{
					//不是[技师]
					ValidationMsg = $"Username or password wrong !";
					return;
				}

				//userModel存在,证明账号Account是对的 
				if (userModel.IsLocked)
				{
					ValidationMsg = "You have been locked! Please contact administrator to unlock!";

					//记录 登录历史
					AddLoginHistory(false, UserBehavior.Login, username, "user been locked");

					return;
				}

				var leftTimes = WrongPasswordMaxTryTimes - userModel.WrongPassLoginTimes - 1;
				if (leftTimes <= 0)
				{
					//lock this user and tip user
					_userService.LockUserByName(username);
					_userService.IncreWrongPassLoginTimes(username);
					ValidationMsg = "You have been locked! Please contact administrator to unlock!";
				}
				else
				{
					//增加错误次数
					_userService.IncreWrongPassLoginTimes(username);
					ValidationMsg = $"Password wrong , you have {leftTimes} times left to try.";

					//记录 登录历史
					AddLoginHistory(false, UserBehavior.Login, username, "password wrong");
				}
			}
			else
			{
				var failReason = "Username or password wrong !";
				//记录 登录历史
				AddLoginHistory(false, UserBehavior.Login, username, failReason);

				//如果账号都不对,直接提示账号或密码不正确
				ValidationMsg = validationResult is null ? failReason : validationResult.Message;
			}

		}
	}

	private void AddLoginHistory(bool isSuccess, UserBehavior behavior, string account, string plainPassword, string failReason = "")
	{
		_loginHistoryService.Insert(new LoginHistoryModel()
		{
			Account = account,
			EncryptPassword = DESHelper.EncryptString(plainPassword),
			Behavior = behavior.ToString(),
			IsSuccess = isSuccess,
			Comments = string.Empty,
			CreateTime = DateTime.Now,
			FailReason = failReason,
			Id = Guid.NewGuid().ToString()
		});
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
	/// <summary>
	/// 界面提示信息
	/// </summary>
	public string ValidationMsg
	{
		get => _validationMsg;
		set => SetProperty(ref _validationMsg, value);
	}

	private string _choosedServiceKey = string.Empty;
	public string ChoosedServiceKey
	{
		get => _choosedServiceKey;
		set => SetProperty(ref _choosedServiceKey, value);
	}

	private int? _selectedTabIndex = 0;

	public int? SelectedTabIndex
	{
		get => _selectedTabIndex;
		set
		{
			SetProperty(ref _selectedTabIndex, value);
			ValidationMsg = string.Empty;
		}
	}

	private bool _rememberPassword;
	public bool RememberPassword
	{
		get => _rememberPassword;
		set
		{
			if (_rememberPassword != value)
			{
				//仅仅修改RememberPassword
				UserConfig.LoginSetting.RememberPassword.Value = value;
				bool saveFlag = UserConfig.SaveLoginSetting();
				if (!saveFlag)
				{
					_logger.LogError($"LoginSetting SaveLoginSetting failed:{saveFlag}");
				}
			}

			SetProperty(ref _rememberPassword, value);
		}
	}

	private ObservableCollection<TestAccount>? _testAccount;
	public ObservableCollection<TestAccount>? TestAccounts
	{
		get => _testAccount;
		set => SetProperty(ref _testAccount, value);
	}

	#endregion
}