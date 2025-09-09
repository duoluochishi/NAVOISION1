//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.Recon.View;

public partial class ScanMainControl
{
	private readonly ILogger<ScanMainControl>? _logger;
	private readonly ILayoutManager? _layoutManager;

	public ScanMainControl()
	{
		InitializeComponent();

		//DataContext = CTS.Global.ServiceProvider?.GetRequiredService<ScanMainViewModel>();

		_logger = CTS.Global.ServiceProvider?.GetRequiredService<ILogger<ScanMainControl>>();
		_layoutManager = CTS.Global.ServiceProvider?.GetRequiredService<ILayoutManager>();
		if (_layoutManager != null)
			_layoutManager.LayoutChanged += LayoutChanged;

		Loaded += ScanMainControl_Loaded;
	}

	private void ScanMainControl_Loaded(object sender, RoutedEventArgs e)
	{
		_logger?.LogInformation("Init content view");

		//默认加载 recon界面 布局
		LayoutContainer.Content = CTS.Global.ServiceProvider?.GetRequiredService<ReconControl>();
		//LayoutContainer.Content = CTS.Global.ServiceProvider.GetRequiredService<ScanDefaultControl>();
	}

	private void LayoutChanged(object? sender, EventArgs<ScanTaskAvailableLayout> e)
	{
		object? uc = null;
		switch (e.Data)
		{
			case ScanTaskAvailableLayout.ScanDefault:
				uc = CTS.Global.ServiceProvider.GetRequiredService<ScanDefaultControl>();
				break;
			case ScanTaskAvailableLayout.Recon:
				uc = CTS.Global.ServiceProvider.GetRequiredService<ReconControl>();
				break;
		}

		if (uc is not null)
		{
			Application.Current?.Dispatcher.Invoke(() =>
			{
				LayoutContainer.Content = uc;
			});
		}

		//var controlType = Type.GetType($"{nameof(NV)}.{nameof(CT)}.{nameof(Examination)}.{nameof(App)}.{nameof(View)}.{nameof(Layout)}.{page}Control");
		//if (controlType == null)
		//{
		//    _logger?.LogError($"ScanMainControl controlType resolve failed");
		//    return;
		//}
		//var viewControl = _serviceProvider?.GetRequiredService(controlType);
		//LayoutContainer.Content = viewControl;
	}

}