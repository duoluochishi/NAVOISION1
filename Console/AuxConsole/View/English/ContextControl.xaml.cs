namespace NV.CT.AuxConsole.View.English;

public partial class ContextControl
{
	public ContextControl()
	{
		InitializeComponent();

		//ContextViewModel vm = CTS.Global.ServiceProvider.GetRequiredService<ContextViewModel>();
		//vm.RefreshContentControlSize += ContextViewModel_RefreshContentControlSize;
		//DataContext = vm;
	}
	//private void ContextViewModel_RefreshContentControlSize(object? sender, EventArgs e)
	//{
	//	if (controlContainer.Content is not UserControl)
	//	{
	//		var uc = ((FrameworkElement)controlContainer.Content);
	//		uc.Width = controlContainer.ActualWidth;
	//		uc.Height = controlContainer.ActualHeight;
	//	}
	//}
}
