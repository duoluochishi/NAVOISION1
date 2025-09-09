namespace NV.CT.UI.Exam.DynamicParameters.Templates.AxialDefault;

public partial class ReconParameterControl : UserControl
{
    public ReconParameterControl()
    {
        InitializeComponent();
        DataContext = Global.ServiceProvider.GetRequiredService<ReconParameterViewModel>();
    }
}