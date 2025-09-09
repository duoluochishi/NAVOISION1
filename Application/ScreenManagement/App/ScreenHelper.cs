using System.Windows;
using System.Windows.Forms;

namespace NV.CT.ScreenManagement;

public class ScreenHelper
{
    public static void ShowOnMonitor(Screen screen, Window window)
    {
        window.Left = double.Parse(screen.WorkingArea.Location.X.ToString());
        window.WindowStyle = WindowStyle.None;
        window.WindowState = WindowState.Maximized;
        window.Show();
    }
}
