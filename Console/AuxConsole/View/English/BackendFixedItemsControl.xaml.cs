namespace NV.CT.AuxConsole.View.English;

public partial class BackendFixedItemsControl
{
	public BackendFixedItemsControl()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<BackendFixedItemsViewModel>();
	}
}
