using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;

namespace NV.CT.UI.Controls.Controls;

public partial class LoadingWindow
{
	public LoadingWindow()
	{
		InitializeComponent();
	}

	//[UIRoute]
	public void ShowLoading(bool onPrimaryScreen = true)
	{
		System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
		{
			Screen? screen;
			if (onPrimaryScreen)
			{
				screen = Screen.AllScreens.FirstOrDefault(s => s == Screen.PrimaryScreen);
			}
			else
			{
				screen = Screen.AllScreens.FirstOrDefault(s => !Equals(s, Screen.PrimaryScreen));
			}

			if (screen is null)
				return;

			Left = screen.WorkingArea.Location.X + (screen.WorkingArea.Width / 2.0 - ActualWidth / 2);
			Top = screen.WorkingArea.Location.Y + (screen.WorkingArea.Height / 2.0 - ActualHeight / 2);
			Show();
		}, DispatcherPriority.Render);
	}

	//[UIRoute]
	public void HideLoading()
	{
		System.Windows.Application.Current?.Dispatcher?.Invoke(Hide, DispatcherPriority.Render);

		//Hide();
	}
}
