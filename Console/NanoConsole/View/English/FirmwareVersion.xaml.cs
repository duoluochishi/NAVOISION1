namespace NV.CT.NanoConsole.View.English;

public partial class FirmwareVersion
{
	public FirmwareVersion()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<FirmwareVersionViewModel>();
	}
}
