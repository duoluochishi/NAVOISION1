namespace NV.CT.NanoConsole.View.English;

public partial class McsFileValidation
{
	public McsFileValidation()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<FileValidationViewModel>();
	}
}
