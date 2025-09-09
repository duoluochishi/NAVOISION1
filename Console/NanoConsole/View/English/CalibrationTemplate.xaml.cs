using CalibrationTemplateViewModel = NV.CT.NanoConsole.ViewModel.CalibrationTemplateViewModel;

namespace NV.CT.NanoConsole.View.English;

public partial class CalibrationTemplate
{
	public CalibrationTemplate()
	{
		InitializeComponent();

		DataContext = CTS.Global.ServiceProvider.GetService<CalibrationTemplateViewModel>();
	}
}
