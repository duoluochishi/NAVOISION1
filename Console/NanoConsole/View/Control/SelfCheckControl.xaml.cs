namespace NV.CT.NanoConsole.View.Control;

public partial class SelfCheckControl
{
	private readonly ILayoutManager? _layoutManager;
	public SelfCheckControl()
	{
		InitializeComponent();

		_layoutManager = CTS.Global.ServiceProvider.GetService<ILayoutManager>();
		
		DataContext = CTS.Global.ServiceProvider.GetService<SelfCheckViewModel>();
	}

	///// <summary>
	///// 重做自检
	///// </summary>
	//private void RedoBtn_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	//{
	//	CTS.Global.ServiceProvider.GetService<ISelfCheckService>()?.StartSelfChecking();
	//}

	/// <summary>
	/// 跳过自检页面
	/// </summary>
	private void SkipBtn_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_layoutManager?.Goto(Screens.Login);
	}

	/// <summary>
	/// 回到 自检简版页面
	/// </summary>
	private void BtnBack_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_layoutManager?.Goto(Screens.SelfCheckSimple);
	}

	/// <summary>
	/// 急诊
	/// </summary>
	private void BtnEmergency_OnClick(object sender, RoutedEventArgs e)
	{
		_layoutManager?.Goto(Screens.Emergency);
	}

}

