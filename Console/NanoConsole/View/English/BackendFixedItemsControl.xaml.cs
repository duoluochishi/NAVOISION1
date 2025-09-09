//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.View.English;

public partial class BackendFixedItemsControl
{
	public BackendFixedItemsControl()
	{
		InitializeComponent();
		DataContext = CTS.Global.ServiceProvider?.GetRequiredService<BackendFixedItemsViewModel>();
	}

	private void BtnLogin_OnClick(object sender, RoutedEventArgs e)
	{
		PopLoginMenu.PlacementTarget = BtnLogin;
		PopLoginMenu.IsOpen = true;
	}

	private void BtnLogin_OnInitialized(object? sender, EventArgs e)
	{
		BtnLogin.ContextMenu = null;
	}
}