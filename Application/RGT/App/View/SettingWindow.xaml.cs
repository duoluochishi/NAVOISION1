using System.Windows.Input;

namespace NV.CT.RGT.View;

public partial class SettingWindow
{
    public SettingWindow()
    {
        InitializeComponent();

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        MouseDown += (_, _) =>
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        };
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}