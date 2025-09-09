namespace NV.CT.NanoConsole.View.English;

public partial class DeviceStatus
{
	public DeviceStatus()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<SelfCheckViewModel>();
	}
}
