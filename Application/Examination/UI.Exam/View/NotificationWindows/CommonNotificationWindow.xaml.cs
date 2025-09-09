namespace NV.CT.UI.Exam.View;

public partial class CommonNotificationWindow
{
    public CommonNotificationWindow()
    {
        InitializeComponent();
        //WindowStartupLocation = WindowStartupLocation.CenterScreen;
        DataContext = Global.ServiceProvider.GetRequiredService<CommonNotificationViewModel>();
    }
}