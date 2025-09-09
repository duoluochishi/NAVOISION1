namespace NV.CT.UI.Exam.DynamicParameters.Templates.AxialDefault;

public partial class ReconParameterDetailControl
{
    public ReconParameterDetailControl()
    {
        InitializeComponent();
        DataContext = Global.ServiceProvider.GetRequiredService<ReconParameterViewModel>();
    }
}