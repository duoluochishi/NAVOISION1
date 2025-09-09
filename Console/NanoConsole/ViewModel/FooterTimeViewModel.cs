//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
namespace NV.CT.NanoConsole.ViewModel;

public class FooterTimeViewModel : BaseViewModel
{
	private readonly DispatcherTimer _timer = new();
	public FooterTimeViewModel()
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

	private string localCurrentTime = string.Empty;
	public string LocalCurrentTime
	{
		get => localCurrentTime;
		set => SetProperty(ref localCurrentTime, value);
	}

	private string localCurrentDate = string.Empty;
	public string LocalCurrentDate
	{
		get => localCurrentDate;
		set => SetProperty(ref localCurrentDate, value);
	}
}