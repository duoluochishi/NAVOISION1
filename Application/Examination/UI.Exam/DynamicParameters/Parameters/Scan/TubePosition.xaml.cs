namespace NV.CT.UI.Exam.DynamicParameters.Parameters.Scan;

public partial class TubePosition : UserControl
{
    public TubePosition()
    {
        InitializeComponent();
        this.Loaded += TubePosition_Loaded;
    }

    private void TubePosition_Loaded(object sender, RoutedEventArgs e)
    {
        this.DataContext = Global.ServiceProvider.GetRequiredService<ScanParameterViewModel>();
    }
}