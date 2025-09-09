using NV.CT.ConfigService.Models.SystemConfig;
using NV.CT.Console.ApplicationService.Impl.Extensions;
using NV.CT.Models.MouseKeyboard;
using NV.CT.NanoConsole.MouseKeyboardLibrary;
using NV.CT.SystemInterface.MRSIntegration.Impl.Extensions;
using NV.MPS.Exception;

namespace NV.CT.NanoConsole;

public class Program
{
	public static IServiceProvider? ServiceProvider { get; set; }
	//private static EventWaitHandle? _programStarted;

	private static int SystemLockMinutes = UserConfig.SystemSetting.SystemLockTime.Value;
	private static readonly KeyboardHook _keyboardHook = new();
	private static readonly MouseHook _mouseHook = new();
	[STAThread]
	public static void Main(string[] args)
	{
		//_programStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.NanoConsole", out var createNew);
		//if (!createNew)
		//{
		//	_programStarted.Set();
		//	System.Environment.Exit(0);
		//	return;
		//}

		//Thread.Sleep(5000);
		
		var host = CreateDefaultBuilder(args).Build();
		
		var logger = host.Services.GetRequiredService<ILogger<Program>>();
		CTS.Global.Logger = logger;
		ServiceProvider = host.Services;
		CTS.Global.ServiceProvider = host.Services;
		GlobalErrorHandler.Handling(logger);

		host.Start();

		Global.Instance.Subscribe();
		
		SetCultureInfo();
		var application = new Application();
		Global.Instance.Application = application;
		application.Exit += App_Exit;
		application.Startup += Application_Startup;

		CTS.Global.Logger?.LogInformation("NanoConsole is starting!");
		try
		{
			LoadingResource.LoadingInApplication();
			Global.Instance.Initialize();

			var mainWindow = host.Services.GetRequiredService<MainWindow>();

			mainWindow.Show();
			mainWindow.Activate();
			application.MainWindow = mainWindow;

			CTS.Global.Logger?.LogInformation($"IsDevelopment {RuntimeConfig.IsDevelopment}");

			if (!RuntimeConfig.IsDevelopment)
			{
				var inputListener = host.Services.GetRequiredService<IInputListener>();
				SetMouseKeyboardHook(inputListener);
			}

			application.Run();
		}
		catch (NanoException ex)
		{
			CTS.Global.Logger?.LogError("Console Error {0}", ex);
		}
		catch (Exception ex)
		{
			CTS.Global.Logger?.LogError($"NanoConsole error occured {ex.Message}");
		}
	}

	public static IHostBuilder CreateDefaultBuilder(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args);

		builder.ConfigureAppConfiguration((context, config) =>
		{
			var configRoot = RuntimeConfig.Console.MCSConfig.Path;
			var appConfigBuilder = config.AddJsonFile(Path.Combine(configRoot, "NanoConsole/appsetting.json"), true, true).
				//AddJsonFile(Path.Combine(configRoot, "ConfigService/HardwareConfig.json"), true, true).
				AddJsonFile(Path.Combine(configRoot, "AppService/appsetting.json"), true, true);

			config.AddEnvironmentVariables();
		}).ConfigureLogging(loggingBuilder =>
		{
			loggingBuilder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddNanoLogger();
		});

		builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

		builder.ConfigureContainer<ContainerBuilder>((context, container) =>
		{
			container.AddRealtimeContainer();
			container.AddOfflineContainer();
			container.AddNanoStatusApplicationServices();
			container.RegisterModule<ViewModelModule>();
			container.RegisterModule<UIControlModule>();
			container.AddDialogServiceContianer();
		});

		builder.ConfigureServices((context, services) =>
		{
			services.Configure<List<SettingLinkItem>>(context.Configuration.GetSection("SettingLinkList"));
			services.Configure<List<SubProcessSetting>>(context.Configuration.GetSection("SubProcesses"));
			//services.Configure<List<HeatCapacityItem>>(context.Configuration.GetSection("TubeHeatCapacity"));
			services.AddCommunicationClientServices();

			services.AddApplicationMapper();

			services.AddMRSMapper();

			//注册控件
			services.AddSingleton<WelcomeControl>();
			services.AddSingleton<SelfCheckSimpleControl>();
			services.AddSingleton<SelfCheckControl>();
			services.AddSingleton<LoginControl>();
			services.AddSingleton<SystemHomeControl>();
			services.AddSingleton<SettingHomeControl>();
			services.AddSingleton<ShutdownControl>();
			services.AddSingleton<LockControl>();

			services.AddSingleton<TableMoveWindow>();
			services.AddSingleton<SelfCheckWindow>();
			services.AddSingleton<MainWindow>();
			services.AddSingleton<EmergencyWindow>();

			services.AddHostedService<HeatCapacityLoggerService>();
		});

		return builder;
	}

	private static void SetCultureInfo()
	{
		string language = "";

		CultureInfo currentCulture;
		if (language.ToLower() == "chinese")
		{
			currentCulture = new CultureInfo("zh-CN");
		}
		else if (language.ToLower() == "english")
		{
			currentCulture = new CultureInfo("en-US");
		}
		else
		{
			currentCulture = new CultureInfo("");
		}

		CultureInfo.DefaultThreadCurrentUICulture = currentCulture;
	}
	private static void Application_Startup(object sender, StartupEventArgs e)
	{
		//阻止硬件加速,锁屏后会导致界面假死,material design库的问题
		RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
	}

	private static void App_Exit(object sender, ExitEventArgs e)
	{
		Global.Instance?.Unsubscribe();

		_mouseHook.Stop();
		_keyboardHook.Stop();

		var userService = CTS.Global.ServiceProvider.GetService<IUserService>();
		var authorization = CTS.Global.ServiceProvider.GetService<IAuthorization>();
		var currentUser = authorization?.GetCurrentUser();
		if (currentUser != null)
		{
			userService?.LogOut(new AuthorizationRequest(currentUser.Account, currentUser.Password));
		}
	}

	private static void SetMouseKeyboardHook(IInputListener inputListener)
	{
        _mouseHook.MouseMove += (_, e) => HandleMouseEvent("MouseMove", inputListener);
        _mouseHook.MouseDown += (_, e) => HandleMouseEvent("MouseDown", inputListener);
        _mouseHook.MouseUp += (_, e) => HandleMouseEvent("MouseUp", inputListener);
        _mouseHook.MouseWheel += (_, e) => HandleMouseEvent("MouseWheel", inputListener);

        _keyboardHook.KeyDown += (_, e) => HandleKeyboardEvent("KeyDown", inputListener);
        _keyboardHook.KeyUp += (_, e) => HandleKeyboardEvent("KeyUp", inputListener);
        _keyboardHook.KeyPress += (_, e) => HandleKeyboardEvent("KeyPress", inputListener);

        _mouseHook.Start();
        _keyboardHook.Start();

        inputListener.IdleTimeOccured += (sender, e) =>
        {
            CTS.Global.Logger.LogInformation("[InputListener] idle time occured ");
        };

        inputListener.StatusChanged += (sender, e) =>
        {				
	        //reduce log output frequency
	        if (e.IdledSeconds % 60 == 0 && e.IdledSeconds!=0)
	        {
		        CTS.Global.Logger.LogInformation(
			        $"[InputListener] status changed to {e.Status}, IdledSeconds/LockMinutes : {e.IdledSeconds}/{SystemLockMinutes}m");
	        }
        };
    }

    private static void HandleKeyboardEvent(string eventType,IInputListener inputListener)
	{
		//CTS.Global.Logger.LogInformation($"[InputListener] keyboard event : {eventType}");
		inputListener.Reset();
	}

	private static void HandleMouseEvent(string eventType,IInputListener inputListener)
	{
		//CTS.Global.Logger.LogInformation($"[InputListener] mouse event : {eventType} will reset timer");
		inputListener.Reset();
	}
}