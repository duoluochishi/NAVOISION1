namespace NV.CT.UI.Exam.View;

public partial class CommonWarningWindow
{
    public CommonWarningWindow()
    {
        InitializeComponent();
       // WindowStartupLocation = WindowStartupLocation.CenterScreen;
        DataContext = Global.ServiceProvider.GetRequiredService<CommonWarningViewModel>();
    }
}