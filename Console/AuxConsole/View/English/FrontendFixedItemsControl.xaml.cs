namespace NV.CT.AuxConsole.View.English;

public partial class FrontendFixedItemsControl
{
	public FrontendFixedItemsControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<FrontendFixedItemsViewModel>();
	}
}
