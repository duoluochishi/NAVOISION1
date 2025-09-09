using System.Windows.Input;
using Point = System.Windows.Point;

namespace NV.CT.UI.Exam.View;

public partial class ProtocolSaveAsWindow
{
    public ProtocolSaveAsWindow()
    {
        InitializeComponent();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;       
        DataContext = Global.ServiceProvider.GetRequiredService<ProtocolSaveAsViewModel>();
    }
}