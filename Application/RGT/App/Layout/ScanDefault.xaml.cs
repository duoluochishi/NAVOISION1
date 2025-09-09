using NV.CT.FacadeProxy.Common.Enums;
using System.Windows.Input;

namespace NV.CT.RGT.Layout;

public partial class ScanDefault : UserControl
{
    private readonly IDataSync? _dataSync;
    public ScanDefault()
    {
        InitializeComponent();

        DataContext = CTS.Global.ServiceProvider.GetRequiredService<ScanDefaultViewModel>();

        _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(IntervalMilliSeconds);
        _dispatcherTimer.Tick += DispatcherTimerTick;

        _dataSync = CTS.Global.ServiceProvider.GetService<IDataSync>();
        if (_dataSync != null)
            _dataSync.RealtimeStatusChanged += DataSync_RealtimeStatusChanged;
    }

    private void DataSync_RealtimeStatusChanged(object? sender, EventArgs<CTS.Models.RealtimeInfo> e)
    {
        if (e.Data.Status == RealtimeStatus.ExposureStarted)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                Clockwise = false;
                if (DataContext is ScanDefaultViewModel vm)
                    StartAnimation((int)vm.DelayTime);
            });
        }
    }

    private readonly DispatcherTimer _dispatcherTimer = new();

    double percent;


    private void btnDecrease_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Clockwise = false;
        StartAnimation(5);
    }

    private void BtnWork_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Clockwise = true;
        StartAnimation(5);
    }

    private double CurrentSecond = 0;
    private int TotalSeconds = 0;
    private bool Clockwise = false;
    private readonly int IntervalMilliSeconds = 10;

    //private int EventOccuredTimes = 0;

    private void StartAnimation(int totalSeconds)
    {
        if (Clockwise)
        {
            //顺时针
            circleProgressBar.CurrentSecond = 0;
            circleProgressBar.CurrentValue = 0;
            circleProgressBar.TotalSeconds = totalSeconds;
            circleProgressBar.EyeStickSecond = 0;
            circleProgressBar.Clockwise = Clockwise;
            percent = 0;
            CurrentSecond = 0;

            circleProgressBar.UpdateStartLabel();
        }
        else
        {
            //逆时针
            circleProgressBar.TotalSeconds = totalSeconds;
            circleProgressBar.CurrentSecond = totalSeconds;
            circleProgressBar.EyeStickSecond = totalSeconds;
            circleProgressBar.CurrentValue = 1;
            circleProgressBar.Clockwise = Clockwise;
            percent = 1;
            CurrentSecond = totalSeconds;

            circleProgressBar.UpdateStartLabel();
        }

        TotalSeconds = totalSeconds;

        _dispatcherTimer.Start();

    }

    private void DispatcherTimerTick(object? sender, EventArgs e)
    {
        //EventOccuredTimes++;

        if (Clockwise)
        {
            if (percent > 1)
            {
                percent = 1;

                _dispatcherTimer.Stop();

                circleProgressBar.UpdateEndLabel();

                return;
            }
        }
        else
        {
            if (percent < 0)
            {
                percent = 0;

                _dispatcherTimer.Stop();
                circleProgressBar.UpdateEndLabel();
                return;
            }
        }

        var percentStep = IntervalMilliSeconds * 1.0 / (TotalSeconds * 1000);
        if (Clockwise)
        {
            percent += percentStep;
            CurrentSecond += TotalSeconds * percentStep;
        }
        else
        {
            percent -= percentStep;
            CurrentSecond = TotalSeconds * percent;
        }

        circleProgressBar.CurrentValue = percent;
        circleProgressBar.CurrentSecond = CurrentSecond;
    }

}