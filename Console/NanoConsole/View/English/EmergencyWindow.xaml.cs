using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using System.Windows.Forms;

namespace NV.CT.NanoConsole.View.English;

public partial class EmergencyWindow : Window
{
	private readonly IRealtimeReconProxyService? _realtimeReconProxyService;
	public EmergencyWindow()
	{
		InitializeComponent();

		_realtimeReconProxyService = CTS.Global.ServiceProvider.GetService<IRealtimeReconProxyService>();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		var primaryScreen = Screen.AllScreens.FirstOrDefault(s => s == Screen.PrimaryScreen);
		if (primaryScreen != null)
		{
			ShowOnMonitor(primaryScreen, this);
		}
	}

	public void ShowOnMonitor(Screen screen, Window window)
	{
		window.Left = double.Parse(screen.WorkingArea.Location.X.ToString());
		window.WindowStyle = WindowStyle.None;
		window.WindowState = WindowState.Maximized;
		window.Show();
	}
	
	private void BtnOk_OnClick(object sender, RoutedEventArgs e)
	{
		Hide();
	}

	private void btn_No_Click(object sender, RoutedEventArgs e)
	{

	}
	private void btn_resume_click(object sender, RoutedEventArgs e)
	{
		var resumeResult = _realtimeReconProxyService?.Resume();
		if (resumeResult != null)
		{
			if (resumeResult.Status != CommandExecutionStatus.Success)
			{
				//_logger.LogDebug($"SetCycleMessageInterval failed: {result?.Status.ToString()}, details: {JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");

				var firstResult = resumeResult.Details.FirstOrDefault();
				txtResumeInfo.Text = $"{firstResult.Code}:{firstResult.Message}";
			}
			else
			{
				//如果 resume 成功，直接关闭
				Hide();
			}
		}
	}

	private void btn_Close_Click(object sender, RoutedEventArgs e)
	{
		Hide();
	}
}