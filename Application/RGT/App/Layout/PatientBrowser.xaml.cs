namespace NV.CT.RGT.Layout;

public partial class PatientBrowser
{
    public PatientBrowser()
    {
        InitializeComponent();

        DataContext = CTS.Global.ServiceProvider.GetRequiredService<PatientBrowserViewModel>();
    }
}