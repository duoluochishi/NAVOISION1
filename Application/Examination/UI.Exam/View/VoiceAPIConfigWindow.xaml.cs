namespace NV.CT.UI.Exam.View;

public partial class VoiceAPIConfigWindow
{
    public VoiceAPIConfigWindow()
    {
        InitializeComponent();
        this.Owner = Global.ServiceProvider.GetRequiredService<ParameterDetailWindow>();
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        DataContext = Global.ServiceProvider.GetRequiredService<VoiceAPIConfigViewModel>();
    }
}