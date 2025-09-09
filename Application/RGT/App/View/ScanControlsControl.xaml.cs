namespace NV.CT.RGT.View;

public partial class ScanControlsControl
{
    public ScanControlsControl()
    {
        InitializeComponent();

        DataContext = CTS.Global.ServiceProvider.GetRequiredService<ScanControlsViewModel>();
    }
}