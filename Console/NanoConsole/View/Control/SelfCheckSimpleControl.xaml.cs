namespace NV.CT.NanoConsole.View.Control;

public partial class SelfCheckSimpleControl
{
	private readonly ILayoutManager? _layoutManager;
	private readonly Random _random = new();
	private readonly SelfCheckViewModel _vm;
	private readonly DispatcherTimer _timer = new();
	private readonly DispatcherTimer _carouselTimer = new();
	private ScrollViewer? _scrollViewer;
	private double _offset;
	private readonly bool _needSimulateAnimation = false;
	public SelfCheckSimpleControl()
	{
		InitializeComponent();

		_layoutManager = CTS.Global.ServiceProvider.GetService<ILayoutManager>();
		_vm = CTS.Global.ServiceProvider.GetRequiredService<SelfCheckViewModel>();

		if (_layoutManager != null)
			_layoutManager.LayoutChanged += LayoutChanged;
		DataContext = _vm;

		Loaded += PageLoaded;
	}

	private void LayoutChanged(object? sender, Screens e)
	{
		ResetTimer();

		if (e == Screens.SelfCheckSimple)
		{
			//重新开始 自检文字滚动
			_carouselTimer.IsEnabled = true;
			_carouselTimer.Start();

			//重新开始获取自检结果 界面重新切回 自检简单页面
			_vm.GetSelfCheckResults();
		}
		else
		{
			//切换界面后, 自检文字滚动停止
			_carouselTimer.IsEnabled = false;
			_carouselTimer.Stop();
		}
	}

	private void PageLoaded(object sender, RoutedEventArgs e)
	{
		//timer based progress change simulator
		if (_needSimulateAnimation)
		{
			_timer.Interval = TimeSpan.FromMilliseconds(50);
			_timer.Tick += timer_Tick;
			_timer.Start();
		}

		_scrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this.dg, 0), 0) as ScrollViewer;
		if (_scrollViewer != null)
			_scrollViewer.ScrollChanged += ScrollChanged;

		ResetTimer();
		_carouselTimer.Start();
	}

	private void ResetTimer()
	{
		_offset = 0;
		_carouselTimer.Interval = TimeSpan.FromMilliseconds(120);
		_carouselTimer.Tick -= CarouselHandler;
		_carouselTimer.Tick += CarouselHandler;
	}

	private void ScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		if (e.VerticalOffset + e.ViewportHeight == e.ExtentHeight && e.ViewportHeight != 0)
		{
			_offset = 1;
		}
	}

	private void CarouselHandler(object? sender, EventArgs e)
	{
		_offset += 1;
		_scrollViewer?.ScrollToVerticalOffset(_offset);
	}

	private int _progress;
	private void timer_Tick(object? sender, EventArgs e)
	{
		_vm.Progress = _progress;
		_progress += _random.Next(3, 10);
		if (_progress > 100)
		{
			_timer.Stop();

			_progress = 100;
			Application.Current?.Dispatcher?.Invoke(() =>
			{
				_vm.Progress = _progress;

				////1s后自动跳转到登录页，暂时不用
				//await Task.Delay(1500);
				//_layoutManager?.Goto(Screens.Login);
			});
		}
	}

	private void SelfCheckSimpleControl_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		//不需要这样的测试逻辑了

		//Application.Current?.Dispatcher?.Invoke(() =>
		//{
		//	_progress = 0;
		//	_vm.Progress = _progress;
		//	_timer.Start();
		//});
	}

	private void BtnDetail_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_layoutManager?.Goto(Screens.SelfCheckDetail);
	}

	/// <summary>
	/// Skip跳到下一页
	/// </summary>
	private void SkipBtn_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_layoutManager?.Goto(Screens.Login);
	}
}
