namespace NV.CT.UI.Exam.DynamicParameters.Templates.Bolus;

public partial class ScanParameterControl : UserControl
{
    public ScanParameterControl()
    {
        InitializeComponent();
        DataContext = Global.ServiceProvider.GetRequiredService<ScanParameterViewModel>();
    }
}