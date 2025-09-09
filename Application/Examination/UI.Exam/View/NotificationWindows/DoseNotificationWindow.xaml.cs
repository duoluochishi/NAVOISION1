namespace NV.CT.UI.Exam.View;

public partial class DoseNotificationWindow
{
    public DoseNotificationWindow()
    {
        InitializeComponent();
        //WindowStartupLocation = WindowStartupLocation.CenterScreen;
        DataContext = Global.ServiceProvider.GetRequiredService<DoseNotificationViewModel>();
    }
}