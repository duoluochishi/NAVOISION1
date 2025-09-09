using NV.CT.DicomUtility.Extensions;
using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.View;
using NV.CT.ImageViewer.ViewModel;
using NV.CT.UI.Controls.Extensions;
using NV.CT.UI.Controls.Keyboard;
using NV.MPS.Configuration;
using System.Runtime.InteropServices;
using System.Threading;
using Application = System.Windows.Application;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using MessageBox = System.Windows.MessageBox;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;

namespace NV.CT.ImageViewer;

public class Program
{
	[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
	protected static extern short GetKeyState(int vKey);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

	private static readonly KeyboardHook keyboardHook = new();
	private static IApplicationCommunicationService? _applicationCommunicationService;

	private static EventWaitHandle? ProgramStarted;
	public static IServiceProvider? ServiceProvider;
	private static IntPtr MainPtr { get; set; }
	[STAThread]
	public static void Main(string[] args)
	{
		ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.ImageViewer", out var createNew);
		if (!createNew)
		{
			ProgramStarted.Set();
			Environment.Exit(0);
			return;
		}

		//System.Threading.Thread.Sleep(6000);

		//TODO:need to replace from config service
		LanguageResource.Culture = new System.Globalization.CultureInfo("");

		var host = CreateDefaultBuilder(args).Build();
		host.Start();

		CTS.Global.ServiceProvider = host.Services;

		Global.Instance.Subscribe();

		Application app = new Application();
		app.Exit += App_Exit;

		var logger = host.Services.GetRequiredService<ILogger<Program>>();
		CTS.Global.Logger = logger;
		GlobalErrorHandler.Handling(logger);

		LoadingResource.LoadingInApplication();
		if (args.Length > 0)
		{
			var serverHandle = args[0];
			//if (args[2].Contains(","))
			//{
			//             Global.Instance.StudyId = args[2].Split(",")[0];
			//             Global.Instance.SeriesId = args[2].Split(",")[1];
			//         }
			//         else
			//{
			//             Global.Instance.StudyId = args[2];
			//         }
			ConsoleSystemHelper.WindowHwnd = IntPtr.Parse(args[1]);
			logger.LogInformation($"start ImageViewer with study id {Global.Instance.StudyId}");
			try
			{
				var wih = new WindowInteropHelper(host.Services.GetRequiredService<DialogWindow>());
				wih.Owner = ConsoleSystemHelper.WindowHwnd;

				_applicationCommunicationService = CTS.Global.ServiceProvider.GetService<IApplicationCommunicationService>();

				var view = new MainControl();
				var hwndView = ViewHelper.ToHwnd(view);

				MainPtr = hwndView;
				PipeHelper.ClientStream(serverHandle, hwndView.ToInt32());

				RegisterHotKeys(app, hwndView, logger);

				Dispatcher.Run();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + System.Environment.NewLine + ex.StackTrace);
			}
		}
		else
		{
			Global.Instance.StudyId = "1ca4b02e-8595-472c-900d-36e1c04812f9";
			var windows = host.Services.GetRequiredService<MainWindow>();
			windows.Show();
			app.MainWindow = windows;
			app.Run();
			ConsoleSystemHelper.WindowHwnd = ViewHelper.ToHwnd(windows);
		}
	}

	private static void App_Exit(object sender, ExitEventArgs e)
	{
		Global.Instance.Unsubscribe();

		keyboardHook.Stop();
	}

	public static void Log(string message)
	{
		var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff");
		var content = $"{timestamp}:{message}\r\n";
		File.AppendAllText(@"D:\ym_log.txt", content);
	}

	public static IHostBuilder CreateDefaultBuilder(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args);
		builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

		builder.ConfigureLogging(logBuilder =>
		{
			logBuilder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddNanoLogger();
		});

		//耗时 613ms 这里不需要加载任何配置相关东西
		//builder.ConfigureAppConfiguration((_, config) =>
		//{
		//	config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "Viewer/appsettings.json"), true, true);
		//	config.AddEnvironmentVariables();
		//});

		builder.ConfigureContainer<ContainerBuilder>((_, container) =>
		{
			container.AddViewModelContainer();
			container.AddDialogServiceContianer();
			container.AddApplicationServiceContainer();
			container.AddUIControlContainer();
		});
		builder.ConfigureServices((_, services) =>
		{
			services.AddCommonControlMapper();
			services.AddApplicationServiceMapper();
			services.AddApplicationMapper();
			services.AddCommunicationClientServices();
			services.DicomUtilityConfigInitialization();
			services.DicomUtilityConfigInitializationForWin();
			services.AddSingleton<MainWindow>();
			services.AddSingleton<CustomWWWLWindow>();
			services.AddSingleton<CustomRotateDegreeWindow>();
			services.AddSingleton<DicomTagWindow>();
			services.AddSingleton<FilmWindow>();
			services.AddSingleton<TextInputWindow>();
			services.AddSingleton<CustomLayoutWindow>();
			services.AddSingleton<BatchSettingWindow>();
			services.AddSingleton<StudyFilterWindow>();
			services.AddSingleton<ImageViewerScrollBar>();
			services.AddSingleton<PostProcessParametersWindow>();
			services.AddSingleton<QRDialog>();
			services.AddAutoMapper(typeof(ToDataProfile));
		});
		return builder;
	}

	private static void RegisterHotKeys(Application app, IntPtr intPtr, ILogger<Program> logger)
	{
		var image2dViewModel = CTS.Global.ServiceProvider.GetService<Image2DViewModel>();
		var image3dViewModel = CTS.Global.ServiceProvider.GetService<Image3DViewModel>();
		var maincontrolViewModel = CTS.Global.ServiceProvider.GetRequiredService<MainControlViewerModel>();
		var wwwlSets = UserConfig.WindowingConfig.Windowings;
		var presetKeys = wwwlSets.Select(n => n.Shortcut?.ToUpper().Trim()).ToList();

		//如果当前图像浏览进程不在焦点范围内，快捷键不起作用！
		keyboardHook.KeyUp += (sender, e) =>
		{
			var currentActiveApplication = _applicationCommunicationService?.GetCurrentActiveApplication();
			if (currentActiveApplication != null &&
			    currentActiveApplication.ItemName != ApplicationParameterNames.APPLICATIONNAME_VIEWER)
			{
				//logger.LogInformation($"RegisterHotKeys ImageViewer current active app : [{currentActiveApplication?.ItemName}],do not process");
				return;
			}

			//var mainWindow = Application.Current.MainWindow;
			//if (mainWindow is not null && mainWindow.IsEnabled)
			if (true)
			{
				var keycode = e.KeyCode.ToString().ToUpper();
				
				logger.LogInformation($"RegisterHotKeys ImageViewer current active app : [{currentActiveApplication?.ItemName}_{currentActiveApplication?.Parameters}],KeyCode={keycode}]");

				if (presetKeys.Contains(keycode))
				{
					var choosePreset = wwwlSets.FirstOrDefault(n => n.Shortcut == keycode);
					if (choosePreset is null)
						return;
					if (choosePreset.Shortcut == "F12")
					{
						CommonMethod.ShowCustomWWWLWindow(maincontrolViewModel?._2DVisibility == Visibility.Visible ? ViewScene.View2D : ViewScene.View3D);
					}
					else
					{
						if (maincontrolViewModel?._2DVisibility == Visibility.Visible)
						{
							image2dViewModel?.CurrentImageViewer.SetWWWL(choosePreset.Width.Value, choosePreset.Level.Value);
                            EventAggregator.Instance.GetEvent<Update2DHotKeyEvent>().Publish(Switch2DButtonType.txtWWWL.GetDisplayName());
                        }
						else
						{
							image3dViewModel?.CurrentImageViewer.SetWWWL3D(choosePreset.Width.Value, choosePreset.Level.Value);
                            EventAggregator.Instance.GetEvent<Update3DHotKeyEvent>().Publish(Switch3DButtonType.txtWWWL.GetDisplayName());
                        }
                        
                    }
					
					
					//Log($"set ww/wl with shortcut {keycode},with preset {choosePreset.Width.Value},{choosePreset.Level.Value}");
				}
			}

		};

		keyboardHook.Start();
	}

}
