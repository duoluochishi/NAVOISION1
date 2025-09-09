namespace NV.CT.RGT.View;

public partial class TimeControl
{
	public TimeControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetRequiredService<TimeViewModel>();
	}
}
