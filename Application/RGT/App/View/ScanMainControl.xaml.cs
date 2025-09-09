namespace NV.CT.RGT.View;

public partial class ScanMainControl
{
    private readonly ILogger<ScanMainControl>? _logger;

    private readonly ILayoutManager? _layoutManager;
    //private readonly IScreenSync? _screenSync;

    public ScanMainControl()
    {
        InitializeComponent();
        DataContext = CTS.Global.ServiceProvider?.GetRequiredService<ScanMainViewModel>();

        _logger = CTS.Global.ServiceProvider?.GetRequiredService<ILogger<ScanMainControl>>();

        _layoutManager = CTS.Global.ServiceProvider?.GetRequiredService<ILayoutManager>();
        if (_layoutManager != null)
            _layoutManager.LayoutChanged += LayoutChanged;

        //_screenSync = CTS.Global.ServiceProvider?.GetRequiredService<IScreenSync>();
        //if (_screenSync != null)
        //    _screenSync.ScreenChanged += ScreenChanged;

        InitContentView();
    }

    private void LayoutChanged(object? sender, EventArgs<SyncScreens> e)
    {
        var controlType =
            Type.GetType($"{nameof(NV)}.{nameof(CT)}.{nameof(RGT)}.{nameof(Layout)}.{e.Data}");
        if (controlType == null)
        {
            _logger?.LogError($"ScanMainControl controlType resolve failed");
            return;
        }

        Application.Current?.Dispatcher?.Invoke(() =>
        {
            var viewControl = CTS.Global.ServiceProvider.GetRequiredService(controlType);
            LayoutContainer.Content = viewControl;
        });
    }

    private void ScreenChanged(object? sender, string e)
    {
        var controlType =
            Type.GetType($"{nameof(NV)}.{nameof(CT)}.{nameof(RGT)}.{nameof(Layout)}.{e}");
        if (controlType == null)
        {
            _logger?.LogError($"ScanMainControl controlType resolve failed");
            return;
        }

        Application.Current?.Dispatcher?.Invoke(() =>
        {
            var viewControl = CTS.Global.ServiceProvider.GetRequiredService(controlType);
            LayoutContainer.Content = viewControl;
        });
    }

    private void InitContentView()
    {
        _logger?.LogInformation("ScanMainControl InitContentView");

        LayoutContainer.Content = CTS.Global.ServiceProvider?.GetRequiredService<PatientBrowser>();
    }
}