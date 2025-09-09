namespace NV.CT.NanoConsole.View.Control;

public partial class ShutdownControl
{
	public ShutdownControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<ShutdownViewModel>();
	}
}