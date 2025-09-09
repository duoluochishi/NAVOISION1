using Microsoft.Extensions.DependencyInjection;
using NV.CT.AppService.Contract;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace NV.CT.ScreenManagement;

public partial class SecondaryWindow : Window
{
    private IScreenManagement? _screenManagement;

    public SecondaryWindow()
    {
        InitializeComponent();
        Loaded += SecondaryWindow_Loaded;

        InitEvents();
    }
    private void InitEvents()
    {
        _screenManagement = CTS.Global.ServiceProvider?.GetRequiredService<IScreenManagement>();
        if (_screenManagement != null)
        {
            _screenManagement.LockScreenStatusChanged += _screenManagement_LockScreenStatusChanged;
            _screenManagement.UnlockScreenStatusChanged += _screenManagement_UnlockScreenStatusChanged;
        }
    }
    private void SecondaryWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var screen2 = Screen.AllScreens.FirstOrDefault(s => !Equals(s, Screen.PrimaryScreen));

        if (screen2 != null)
            ScreenHelper.ShowOnMonitor(screen2, this);
    }

    private void _screenManagement_UnlockScreenStatusChanged(object? sender, string e)
    {
        Application.Current?.Dispatcher?.Invoke(Hide);
    }

    private void _screenManagement_LockScreenStatusChanged(object? sender, string e)
    {
        Application.Current?.Dispatcher?.Invoke(Show);
    }
}