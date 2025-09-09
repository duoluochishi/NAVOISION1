using NV.CT.ImageViewer.ViewModel;

namespace NV.CT.ImageViewer.View;
/// <summary>
/// SeriesDescription
/// </summary>
public partial class SeriesDescription : UserControl
{
    public SeriesDescription()
    {
        InitializeComponent();
        DataContext = CTS.Global.ServiceProvider.GetRequiredService<SeriesViewModel>();
    }
}