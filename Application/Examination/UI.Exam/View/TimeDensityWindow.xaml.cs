using System.Windows.Input;

namespace NV.CT.UI.Exam.View;

public partial class TimeDensityWindow
{
    public TimeDensityWindow()
    {
        InitializeComponent();
        MouseDown += (_, _) =>
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        };
        DataContext = Global.ServiceProvider.GetRequiredService<TimeDensityViewModel>();
    }
}