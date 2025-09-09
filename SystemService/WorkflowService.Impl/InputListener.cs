using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.Models.MouseKeyboard;
using NV.CT.WorkflowService.Contract;
using NV.MPS.Configuration;
using Timer = System.Threading.Timer;

namespace NV.CT.WorkflowService.Impl;

public class InputListener:IInputListener
{
	//private ListenerStartParameter _originalParameter;
	private readonly Timer _timer;
	private DateTime _lastActiveTime = DateTime.Now;
	private int _dueTime=0;
	private int _period=1000;
	public event EventHandler<ListenerStatus>? IdleTimeOccured;
	public event EventHandler<ListenerStatus>? StatusChanged;
	private readonly TimeSpan _thresholdTime = TimeSpan.FromMinutes(UserConfig.SystemSetting.SystemLockTime.Value);

	private bool _isIdleOccured;
	private readonly ILogger<InputListener>? _logger=CTS.Global.ServiceProvider.GetService<ILogger<InputListener>>();

	public InputListener()
	{
		//_originalParameter = new ListenerStartParameter()
		//{
		//	DueTime = Timeout.Infinite,Period = Timeout.Infinite
		//};
		_timer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);

		_logger?.LogInformation($"InputListener ctor");
	}

	private void TimerCallback(object? state)
	{
		var idleTotal = (int)(DateTime.Now - _lastActiveTime).TotalSeconds;

		//_logger?.LogInformation($"InputListener timer callback idleTotal={idleTotal}/{_thresholdTime}m , duetime={_dueTime},period={_period}");

		StatusChanged?.Invoke(this, new ListenerStatus()
		{
			DueTime = _dueTime,
			Period = _period,
			ThresholdTime = _thresholdTime,
			IdledSeconds = idleTotal,
			Status = TimerStatus.Start
		});

		if (idleTotal >= _thresholdTime.TotalSeconds)
		{
			IdleTimeOccured?.Invoke(this, new ListenerStatus()
			{
				DueTime = _dueTime,
				Period = _period,
				ThresholdTime = _thresholdTime,
				IdledSeconds = idleTotal,
				Status = TimerStatus.Start
			});

			_isIdleOccured = true;

			//一旦达到预定时间,立即停止
			Stop();
		}
	}

	private void SetToNormal()
	{
		_dueTime = 0;
		_period = 1000;
	}

	private void SetToUnNormal()
	{
		_dueTime = Timeout.Infinite;
		_period = Timeout.Infinite;
	}

	public void Start()
	{
		SetToNormal();

		_lastActiveTime = DateTime.Now;

		_timer.Change(_dueTime, _period);
		
		_isIdleOccured = false;

		StatusChanged?.Invoke(this, new ListenerStatus()
		{
			DueTime = _dueTime,
			Period = _period,
			Status = TimerStatus.Start
		});
	}

	public void Stop()
	{
		SetToUnNormal();

		_timer.Change(_dueTime, _period);

		_isIdleOccured = true;

		StatusChanged?.Invoke(this, new ListenerStatus()
		{
			DueTime = _dueTime,
			Period = _period,
			Status = TimerStatus.Stop
		});
	}

	public void Dispose()
	{
		Stop();
		_timer.Dispose();

		StatusChanged?.Invoke(this, new ListenerStatus()
		{
			DueTime = _dueTime,
			Period = _period,
			Status = TimerStatus.Dispose
		});
	}

	public void Reset()
	{
		//_logger?.LogInformation($"InputListener reset called _isIdleOccured={_isIdleOccured},_dueTime={_dueTime},_period={_period}");

		_lastActiveTime = DateTime.Now;
		
		if (_isIdleOccured)
			return;

		if (_dueTime == Timeout.Infinite || _period == Timeout.Infinite)
		{
			Start();
		}
	}
}