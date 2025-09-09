using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NV.CT.UI.Controls.Controls
{
	/// <summary>
	/// LoadingControl.xaml 的交互逻辑
	/// </summary>
	public partial class LoadingControl : UserControl
	{
		public LoadingControl()
		{
			InitializeComponent();

			animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher);
			animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
		}
		private readonly DispatcherTimer animationTimer;

		private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
		{
			Start();
		}
		private void HandleAnimationTick(object sender, EventArgs e)
		{
			SpinnerRotate.Angle = (SpinnerRotate.Angle + 36) % 360;
		}
		private void FrameworkElement_OnUnloaded(object sender, RoutedEventArgs e)
		{
			Stop();
		}
		private void Start()
		{
			animationTimer.Tick += HandleAnimationTick;
			animationTimer.Start();
		}

		private void Stop()
		{
			animationTimer.Stop();
			animationTimer.Tick -= HandleAnimationTick;
		}
	}
}
