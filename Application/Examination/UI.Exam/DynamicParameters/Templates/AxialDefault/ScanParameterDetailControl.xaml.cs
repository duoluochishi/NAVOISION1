namespace NV.CT.UI.Exam.DynamicParameters.Templates.AxialDefault;

public partial class ScanParameterDetailControl
{
    public ScanParameterDetailControl()
    {
        InitializeComponent();
        DataContext = Global.ServiceProvider.GetRequiredService<ScanParameterViewModel>();
    }
}