namespace NV.CT.AuxConsole.View.English;

public partial class DynamicCardsControl
{
	public DynamicCardsControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<DynamicCardsViewModel>();
	}
}