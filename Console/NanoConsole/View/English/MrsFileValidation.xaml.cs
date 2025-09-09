namespace NV.CT.NanoConsole.View.English;

public partial class MrsFileValidation
{
	public MrsFileValidation()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<FileValidationViewModel>();
	}
}
