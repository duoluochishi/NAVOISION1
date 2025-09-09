using Microsoft.Extensions.Configuration;
using NV.CT.Logging;
using NV.CT.UI.Controls;
using NV.CT.UI.Controls.Common;
using NV.CT.UI.Controls.Controls;
using NV.CT.UI.Controls.Extensions;
using NV.CT.UI.Controls.Keyboard;
using NV.MPS.Configuration;

namespace NV.CT.Recon;

public class Program
{
	private static readonly KeyboardHook KeyboardHook = new();
	private static IApplicationCommunicationService? _applicationCommunicationService;
	private static ILayoutManager? _layoutManager;
	private static ReconControlViewModel? _reconControlViewModel;
	private static IDicomImageViewModel? _reconDicomImageViewModel;

	[STAThread]
	public static void Main(string[] args)
	{
		// 424ms
		var host = CreateDefaultBuilder(args).Build();
		
		// 350ms
		host.Start();

		CTS.Global.ServiceProvider = host.Services;
		var logger = host.Services.GetRequiredService<ILogger<Program>>();

		CTS.Global.Logger = logger;
		GlobalErrorHandler.Handling(logger);

		//System.Threading.Thread.Sleep(6000);
		//logger.LogInformation($"recon process with args:{args.ToJson()}");

		try
		{
			var app = new Application();
			
			// 184ms
			LoadingResource.LoadingInApplication();
			
			app.Exit += App_Exit;

			if (args.Length > 1)
			{
				app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
				var serverHandle = args[0];
				ConsoleSystemHelper.WindowHwnd = IntPtr.Parse(args[1]);
				Global.Instance.StudyId = args[2].Trim();

				// 138ms
				Global.Instance.ResumeReconStates();
				
				// 1711ms
				Global.Instance.Initialize();
	
				// 8ms
				Global.Instance.Subscribe();

				// 5ms
				_applicationCommunicationService = CTS.Global.ServiceProvider.GetService<IApplicationCommunicationService>();
				_layoutManager = CTS.Global.ServiceProvider.GetService<ILayoutManager>();
				_reconControlViewModel = CTS.Global.ServiceProvider.GetService<ReconControlViewModel>();
				_reconDicomImageViewModel = CTS.Global.ServiceProvider.GetService<IDicomImageViewModel>();
				
				// 60ms
				var view = new ScanMainControl();
				
				// 801ms
				var hwndView = ViewHelper.ToHwnd(view);
				PipeHelper.ClientStream(serverHandle, hwndView.ToInt32());
				
				// 172ms
				RegisterHotKeys(app, hwndView, logger);
				
				Dispatcher.Run();
			}
			else
			{
				//for test only
				Global.Instance.StudyId = "5bf8455b-217e-48b5-8d5d-791e4d6272de";
				Global.Instance.ResumeReconStates();
				Global.Instance.Initialize();
				Global.Instance.Subscribe();

				Window mainWindow = host.Services.GetRequiredService<MainWindow>();
				app.Run(mainWindow);
				ConsoleSystemHelper.WindowHwnd = ViewHelper.ToHwnd(mainWindow);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, $"Recon exception:{ex.Message}");
		}
	}

	private static void App_Exit(object sender, ExitEventArgs e)
	{
		Global.Instance.Unsubscribe();
	}

	public static IHostBuilder CreateDefaultBuilder(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args);
		builder.ConfigureLogging(logBuilder =>
		{
			logBuilder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddNanoLogger();
		});
		builder.ConfigureAppConfiguration((_, config) =>
		{
			//config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "Recon/appsettings.json"), true, true);
			config.AddEnvironmentVariables();
		});

		builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

		builder.ConfigureContainer<ContainerBuilder>((_, container) =>
		{
			container.AddUIControlContainer();
			container.AddMCSContainer();
			container.AddRealtimeContainer();
			container.AddOfflineContainer();

			container.AddDicomImageViewerContainer();
			container.AddReconAppContainer();
			container.AddDialogServiceContianer();
			container.AddReconApplicationServiceContainer();
		});

		builder.ConfigureServices((context, services) =>
		{
			//services.Configure<DeviceMonitorConfig>(context.Configuration.GetSection("DeviceMonitorConfig"));
			services.AddCommunicationClientServices();
			services.AddUIExamServices();
			services.AddReconAppServices();

			services.AddMRSMapper();
			services.AddReconApplicationServices();

			services.TryAddScoped<MainWindow>();
		});

		return builder;
	}

	private static void RegisterHotKeys(Application app, IntPtr intPtr, ILogger<Program> logger)
	{
		//var image2dViewModel = CTS.Global.ServiceProvider.GetService<Image2DViewModel>();
		//var image3dViewModel = CTS.Global.ServiceProvider.GetService<Image3DViewModel>();
		//var maincontrolViewModel = CTS.Global.ServiceProvider.GetRequiredService<MainControlViewerModel>();
		var wwwlSets = UserConfig.WindowingConfig.Windowings;
		wwwlSets.Add(new WindowingInfo()
		{
			Width = new MPS.Configuration.ItemField<int> { Value = 350, Default = 350 },
			Level = new MPS.Configuration.ItemField<int> { Value = 20, Default = 20 },
			BodyPart = "Custom",
			Shortcut = "F12",
			Description = "Custom"
		});
		var presetKeys = wwwlSets.Select(n => n.Shortcut?.ToUpper().Trim()).ToList();

		//如果当前图像浏览进程不在焦点范围内，快捷键不起作用！
		KeyboardHook.KeyUp += (sender, e) =>
		{
			//logger.LogInformation($"RegisterHotKeys Recon keyup : {e.KeyCode} ");

			var currentActiveApplication = _applicationCommunicationService?.GetCurrentActiveApplication();

			//logger.LogInformation($"RegisterHotKeys current active app 1:{currentActiveApplication?.ItemName}");

			if (currentActiveApplication != null &&
				currentActiveApplication.ItemName != ApplicationParameterNames.APPLICATIONNAME_RECON)
			{
				//logger.LogInformation($"RegisterHotKeys Recon current active app : [{currentActiveApplication?.ItemName}],do not process");
				return;
			}

			//logger.LogInformation($"RegisterHotKeys current active app 2:{currentActiveApplication?.ItemName}");

			var keycode = e.KeyCode.ToString().ToUpper();

			//logger.LogInformation($"RegisterHotKeys Recon current active app : [{currentActiveApplication?.ItemName}_{currentActiveApplication?.Parameters}],KeyCode={keycode}]");

			if (presetKeys.Contains(keycode) || keycode.ToUpper()=="F12")
			{
				var choosePreset = wwwlSets.FirstOrDefault(n => n.Shortcut == keycode);
				if (choosePreset is null)
					return;

				var currentLayout = _layoutManager?.CurrentLayout;
				//logger.LogInformation($"RegisterHotKeys Recon current layout is {currentLayout}");

				if (currentLayout != null)
				{
					//MPR高级重建界面
					if (currentLayout == ScanTaskAvailableLayout.Recon)
					{
						if (choosePreset.Shortcut == "F12")
						{
							double ww = 0, wl = 0;
							_reconControlViewModel?.AdvancedReconControl?.GetWWWL(ref ww,ref wl);
							ShowCustomWWWLWindow(ww,wl, (newWw, newWl) =>
							{
								_reconControlViewModel?.AdvancedReconControl?.SetWWWL(newWw, newWl);
							});
						}
						else
						{
							_reconControlViewModel?.AdvancedReconControl?.SetWWWL(choosePreset.Width.Value, choosePreset.Level.Value);
						}
					}
					//默认检查页面
					else if (currentLayout == ScanTaskAvailableLayout.ScanDefault)
					{
						if (choosePreset.Shortcut == "F12")
						{
							double ww = 0, wl = 0;
							//_reconDicomImageViewModel?.TomoImageViewer?.setw(ref ww, ref wl);
							ShowCustomWWWLWindow(ww, wl, (newWw, newWl) =>
							{
								_reconDicomImageViewModel?.TomoImageViewer.SetWWWL(newWw, newWl);
							});
						}
						else
						{
							_reconDicomImageViewModel?.TomoImageViewer.SetWWWL(choosePreset.Width.Value, choosePreset.Level.Value);
						}
					}
				}


				//logger.LogInformation($"set ww/wl with shortcut {keycode},with preset {choosePreset.Width.Value},{choosePreset.Level.Value}");
			}
		};

		KeyboardHook.Start();
	}

	public static void ShowCustomWWWLWindow(double ww,double wl,Action<double,double> action)
	{
		var _customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();

		if (_customWwwlWindow is null)
		{
			_customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();
		}

		if (_customWwwlWindow != null)
		{
			_customWwwlWindow.SetDefaultValue(ww,wl);
			_customWwwlWindow.SetOkCallback(action);
			WindowDialogShow.Show(_customWwwlWindow);
		}
	}
}

public static class LogManager
{
	public static void Log(string msg)
	{
		//File.AppendAllText(@"G:\log.txt",msg+Environment.NewLine);
	}
}