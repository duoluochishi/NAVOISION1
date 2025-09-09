namespace NV.CT.RGT.View;

public partial class HeaderControl
{
    public HeaderControl()
    {
        InitializeComponent();

        DataContext = CTS.Global.ServiceProvider.GetRequiredService<HeaderViewModel>();
    }
}