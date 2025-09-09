//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.Console.ApplicationService.Contract.Interfaces;
using System.Windows.Media.Imaging;

namespace NV.CT.NanoConsole.View.Control;

public partial class WelcomeControl : UserControl
{
	private readonly ILayoutManager? _layoutManager;
	public WelcomeControl()
	{
		InitializeComponent();

		_layoutManager = CTS.Global.ServiceProvider.GetService<ILayoutManager>();
		Loaded += WelcomeControl_Loaded;
	}

	private void PlayGifByDecoder()
	{
		GifBitmapDecoder decoder = new GifBitmapDecoder(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/logo.gif"), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

		DispatcherTimer timer = new DispatcherTimer();
		timer.Interval = TimeSpan.FromMilliseconds(30);
		int n = decoder.Frames.Count;
		int i = 0;
		timer.Tick += (sender, args) =>
		{
			lock (timer)
			{
				gifImage1.Source = decoder.Frames[i];
			}
			i += 1;
			if (i == n - 47)
			{
				timer.Stop();
				//动画播完，跳转页面

				_layoutManager?.Goto(Screens.SelfCheckSimple);
			}
		};
		timer.Start();
	}

	private void WelcomeControl_Loaded(object sender, RoutedEventArgs e)
	{
		try
		{
			PlayGifByDecoder();
		}
		catch (Exception ex)
		{
			CTS.Global.Logger?.LogError(ex?.Message, ex);
		}
	}

}