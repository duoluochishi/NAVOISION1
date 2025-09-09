using System.Windows.Media.Imaging;

namespace NV.CT.RGT.View;

public partial class WelcomeControl
{
    private readonly ILogger<WelcomeControl>? _logger;
    private readonly IStateService? _stateService;
    public WelcomeControl()
    {
        InitializeComponent();
        if (_logger == null)
        {
            _logger = CTS.Global.ServiceProvider.GetRequiredService<ILogger<WelcomeControl>>();
        }

        _stateService = CTS.Global.ServiceProvider.GetService<IStateService>();

        // ReSharper disable once StringLiteralTypo
        txtCompany.Text = $"Nanovision Technology @{DateTime.Now.Year} COMPOUND EYE CT ALL RIGHTS RESERVED.";
        Loaded += WelcomeControl_Loaded;
    }

    private void WelcomeControl_Loaded(object sender, RoutedEventArgs e)
    {
        PlayGifByDecoder();
    }

    private void PlayGifByDecoder()
    {
        var decoder = new GifBitmapDecoder(new Uri("pack://application:,,,/Resources/icons/logo.gif"), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

        var timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(20);
        int n = decoder.Frames.Count;
        int i = 0;
        timer.Tick += (_, _) =>
        {
            lock (timer)
            {
                gifImage.Source = decoder.Frames[i];
            }
            i += 1;
            if (i == n - 47)
            {
                timer.Stop();

                Thread.Sleep(500);
                _stateService?.AnimationComplete();
            }
        };
        timer.Start();
    }
}