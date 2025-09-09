namespace NV.CT.RGT.ViewModel;
public class TimeViewModel : BaseViewModel
{
	private readonly DispatcherTimer _timer = new();
	public TimeViewModel()
	{
		_timer.Interval = TimeSpan.FromSeconds(1);
		_timer.IsEnabled = true;
		_timer.Tick += _timer_Tick;
		_timer.Start();
	}

	private void _timer_Tick(object? sender, EventArgs e)
	{
		DateTime dt = DateTime.Now;
		LocalCurrentTime = dt.ToTimeString();
		LocalCurrentDate = dt.ToDateString();
	}

	private string _localCurrentTime = string.Empty;
	public string LocalCurrentTime
	{
		get => _localCurrentTime;
		set => SetProperty(ref _localCurrentTime, value);
	}

	private string _localCurrentDate = string.Empty;
	public string LocalCurrentDate
	{
		get => _localCurrentDate;
		set => SetProperty(ref _localCurrentDate, value);
	}
}
