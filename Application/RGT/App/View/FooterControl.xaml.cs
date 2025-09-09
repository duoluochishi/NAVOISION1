namespace NV.CT.RGT.View;

public partial class FooterControl
{
    public FooterControl()
    {
        InitializeComponent();

        DataContext = CTS.Global.ServiceProvider.GetRequiredService<FooterViewModel>();
    }
}