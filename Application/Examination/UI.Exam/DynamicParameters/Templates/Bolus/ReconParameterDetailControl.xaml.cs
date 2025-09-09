namespace NV.CT.UI.Exam.DynamicParameters.Templates.Bolus;

public partial class ReconParameterDetailControl
{
    public ReconParameterDetailControl()
    {
        InitializeComponent();
        DataContext = Global.ServiceProvider.GetRequiredService<ReconParameterViewModel>();
    }
}