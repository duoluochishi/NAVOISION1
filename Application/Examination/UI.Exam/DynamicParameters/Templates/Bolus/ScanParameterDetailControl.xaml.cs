namespace NV.CT.UI.Exam.DynamicParameters.Templates.Bolus;

public partial class ScanParameterDetailControl
{
    public ScanParameterDetailControl()
    {
        InitializeComponent();
        DataContext = Global.ServiceProvider.GetRequiredService<ScanParameterViewModel>();
    }
}