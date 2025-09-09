//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.SystemInterface.MCSRuntime.Contract;

namespace NV.CT.NanoConsole.View.English;

public partial class StatusControl
{
	public StatusControl()
	{
		InitializeComponent();
		DataContext = CTS.Global.ServiceProvider.GetRequiredService<StatusViewModel>();

		Loaded += StatusControl_Loaded;
	}

	private void StatusControl_Loaded(object sender, RoutedEventArgs e)
	{
		var specialDiskService = CTS.Global.ServiceProvider.GetService<ISpecialDiskService>();
		specialDiskService?.TriggerCurrentWarnLevel();
	}
}