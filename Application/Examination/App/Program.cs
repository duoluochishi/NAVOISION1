//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.AppService.Contract;
using NV.CT.CTS;
using NV.CT.DicomImageViewer;
using NV.CT.Examination.ApplicationService.Contract.Models;
using NV.CT.Examination.Extensions;
using NV.CT.Examination.View;
using NV.CT.SystemInterface.MCSRuntime.Impl.Extensions;
using NV.CT.UI.Controls.Controls;
using NV.CT.UI.Controls.Keyboard;
using NV.CT.UI.Exam.Contract;
using NV.MPS.Configuration;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Extension;

namespace NV.CT.Examination;

public class Program
{
	private static EventWaitHandle? _programStarted;
	private static readonly KeyboardHook KeyboardHook = new();
	private static IApplicationCommunicationService? _applicationCommunicationService;
	private static IDicomImageViewModel? _reconDicomImageViewModel;

	[STAThread]
	public static void Main(string[] args)
	{
		//Thread.Sleep(15000);
		_programStarted = new EventWaitHandle(false, EventResetMode.AutoReset, ApplicationParameterNames.PROCESSNAME_EXAMINATION, out var createNew);
		if (!createNew)
		{
			_programStarted.Set();
			System.Environment.Exit(0);
			return;
		}
		var host = CreateDefaultBuilder(args).Build();
		host.Start();

		CTS.Global.ServiceProvider = host.Services;
		var logger = host.Services.GetRequiredService<ILogger<Program>>();
		CTS.Global.Logger = logger;

		GlobalErrorHandler.Handling(logger);
		try
		{
			Global.Instance.Initialize();
			Global.Instance.Subscribe();

			var app = new Application();
			LoadingResource.LoadingInApplication();

			Language.LanguageResource.Culture = new System.Globalization.CultureInfo("");

			app.Exit += App_Exit;

			//Sync screen
			CTS.Global.ServiceProvider.GetService<IScreenSync>()?.Go();

			if (args.Length > 1)
			{
				app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
				var serverHandle = args[0];
				ConsoleSystemHelper.WindowHwnd = IntPtr.Parse(args[1]);

				_applicationCommunicationService = CTS.Global.ServiceProvider.GetService<IApplicationCommunicationService>();
				_reconDicomImageViewModel = CTS.Global.ServiceProvider.GetService<IDicomImageViewModel>();

				var view = new ScanMainControl();

				var hwndView = ViewHelper.ToHwnd(view);
				PipeHelper.ClientStream(serverHandle, hwndView.ToInt32());

				RegisterHotKeys(app, hwndView, logger);

				Dispatcher.Run();
			}
			else
			{
				var mainWindow = host.Services.GetRequiredService<MainWindow>();
				var window = (Window)mainWindow;
				app.Run(window);
				ConsoleSystemHelper.WindowHwnd = ViewHelper.ToHwnd(window);
			}
		}
		catch (Exception ex)
		{
			logger?.LogError(ex, $"Examination exception:{ex.Message}");
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
			//TODO:配置注入
			config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "Examination/appsetting.json"), true, true);
			config.AddEnvironmentVariables();
		});

		builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

		builder.ConfigureContainer<ContainerBuilder>((_, container) =>
		{
			//container.AddMRSContainer();
			container.AddUIControlContainer();
			container.AddRealtimeContainer();
			container.AddOfflineContainer();
			container.AddMCSContainer();
			container.AddApplicationServiceContainer();

			container.AddDicomImageViewerContainer();
			container.AddExaminationAppContainer();

			container.AddDialogServiceContianer();
		});

		builder.ConfigureServices((context, services) =>
		{
			services.Configure<DeviceMonitorConfig>(context.Configuration.GetSection("DeviceMonitorConfig"));

			services.AddCommunicationClientServices();
			services.AddUIExamServices();
			services.AddExaminationAppServices();

			services.AddMRSMapper();
			services.AddApplicationServices();

			services.TryAddScoped<MainWindow>();
		});

		return builder;
	}

	private static void RegisterHotKeys(Application app, IntPtr intPtr, ILogger<Program> logger)
	{
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

			if (currentActiveApplication != null &&
				currentActiveApplication.ItemName != ApplicationParameterNames.APPLICATIONNAME_EXAMINATION)
			{
				//logger.LogInformation($"RegisterHotKeys Recon current active app : [{currentActiveApplication?.ItemName}],do not process");
				return;
			}

			var keycode = e.KeyCode.ToString().ToUpper();

			//logger.LogInformation($"RegisterHotKeys Examination current active app : [{currentActiveApplication?.ItemName}_{currentActiveApplication?.Parameters}],KeyCode={keycode}]");

			if (presetKeys.Contains(keycode) || keycode.ToUpper() == "F12")
			{
				var choosePreset = wwwlSets.FirstOrDefault(n => n.Shortcut == keycode);
				if (choosePreset is null)
					return;

				if (choosePreset.Shortcut == "F12")
				{
					double ww = 0, wl = 0;
					//_reconDicomImageViewModel?.TomoImageViewer?.GetWWWL(ref ww, ref wl);
					ShowCustomWWWLWindow(ww, wl, (newWw, newWl) =>
					{
						_reconDicomImageViewModel?.TomoImageViewer?.SetWWWL(newWw, newWl);
					});
				}
				else
				{
					_reconDicomImageViewModel?.TomoImageViewer?.SetWWWL(choosePreset.Width.Value, choosePreset.Level.Value);
				}

				//logger.LogInformation($"set ww/wl with shortcut {keycode},with preset {choosePreset.Width.Value},{choosePreset.Level.Value}");
			}
		};

		KeyboardHook.Start();
	}

	public static void ShowCustomWWWLWindow(double ww, double wl, Action<double, double> action)
	{
		var _customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();

		if (_customWwwlWindow is null)
		{
			_customWwwlWindow = CTS.Global.ServiceProvider?.GetRequiredService<CustomWWWLWindow>();
		}

		if (_customWwwlWindow != null)
		{
			_customWwwlWindow.SetDefaultValue(ww, wl);
			_customWwwlWindow.SetOkCallback(action);
			WindowDialogShow.Show(_customWwwlWindow);
		}
	}
}