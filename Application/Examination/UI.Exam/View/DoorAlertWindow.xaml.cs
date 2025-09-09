using System.Windows.Input;

namespace NV.CT.UI.Exam.View;

public partial class DoorAlertWindow : Window
{
	public DoorAlertWindow()
	{
		InitializeComponent();
		DataContext = Global.ServiceProvider.GetService<DoorAlertViewModel>();
		//WindowStartupLocation = WindowStartupLocation.CenterScreen;
		CenterWindowOnPrimaryScreen();
		Topmost = true;
		MouseDown += (_, _) =>
		{
			if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				DragMove();
			}
		};
	}

	private void CenterWindowOnPrimaryScreen()
	{
		double screenWidth = SystemParameters.PrimaryScreenWidth;
		double screenHeight = SystemParameters.PrimaryScreenHeight;
		double windowWidth = Width;
		double windowHeight = Height;
		Left = (screenWidth / 2) - (windowWidth / 2);
		Top = (screenHeight / 2) - (windowHeight / 2);
	}

	private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
	{
		var vm = Global.ServiceProvider.GetService<ScanControlsViewModel>();
		vm?.Cancel();

		Hide();
	}
}