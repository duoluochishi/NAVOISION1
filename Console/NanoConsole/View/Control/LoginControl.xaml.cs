using NV.CT.Console.ApplicationService.Contract.Interfaces;
using NV.CT.NanoConsole.Model;

namespace NV.CT.NanoConsole.View.Control;

public partial class LoginControl
{
	private readonly IAuthorization? _authorization;
	private readonly ILayoutManager? _layoutManager;
	public LoginControl()
	{
		InitializeComponent();

		_authorization = CTS.Global.ServiceProvider.GetService<IAuthorization>();
		_layoutManager = CTS.Global.ServiceProvider.GetService<ILayoutManager>();
		DataContext = CTS.Global.ServiceProvider.GetRequiredService<LoginViewModel>();

		if (_authorization != null)
			_authorization.CurrentUserChanged += Authorization_CurrentUserChanged;

		Loaded += LoginControl_Loaded;
		Unloaded += LoginControl_Unloaded;
	}

	private void LoginControl_Unloaded(object sender, RoutedEventArgs e)
	{
	}

	private void LoginControl_Loaded(object sender, RoutedEventArgs e)
	{
		if (DataContext is LoginViewModel vm)
		{
			//确保加载后,记住密码这个checkbox持久化
			vm.RememberPassword=UserConfig.LoginSetting.RememberPassword.Value;

			//[服务]用户,加载默认记录
			vm.FillDefaultAccount();
		}
	}

	/// <summary>
	/// 用户注销或登陆，获取输入焦点
	/// </summary>
	private void Authorization_CurrentUserChanged(object? sender, Models.UserModel e)
	{
		SetDefaultInputFocus();
	}

	private void SetDefaultInputFocus()
	{
		Dispatcher.BeginInvoke(DispatcherPriority.Render,
			new Action(() =>
			{
				userNameTextBox.Focus();
			}));
	}

	/// <summary>
	/// Skip跳转登录 
	/// </summary>
	private void SkipBtn_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (DataContext is LoginViewModel vm)
		{
			vm.Login("NanoAdmin", "123456");
		}
	}

	/// <summary>
	/// 回到自检页面
	/// </summary>
	private void BtnPrev_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_layoutManager?.Goto(Screens.SelfCheckSimple);
	}

	/// <summary>
	/// for simplicity 双击登录
	/// </summary>
	private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if ((sender as DataGrid)?.SelectedItem is TestAccount selectedAccount && DataContext is LoginViewModel vm)
		{
			vm.Login(selectedAccount.Account, selectedAccount.Password);
		}
	}
}