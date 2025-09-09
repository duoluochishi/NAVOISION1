namespace NV.CT.UI.Exam.View;

public partial class DoseAlertWindow
{
    public DoseAlertWindow()
    {
        InitializeComponent();
       // WindowStartupLocation = WindowStartupLocation.CenterScreen;
        DataContext = Global.ServiceProvider.GetRequiredService<DoseAlertViewModel>();
    }
}