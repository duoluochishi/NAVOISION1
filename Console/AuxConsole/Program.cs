using System.Windows.Media;
using Application = System.Windows.Application;

namespace NV.CT.AuxConsole;

public class Program
{
	private static EventWaitHandle? _programStarted;

	[STAThread]
	public static void Main(string[] args)
	{
		_programStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.AuxConsole", out var createNew);
		if (!createNew)
		{
			_programStarted.Set();
			Environment.Exit(0);
			return;
		}

		//Thread.Sleep(8000);
		var host = CreateDefaultBuilder(args).Build();
		CTS.Global.ServiceProvider = host.Services;
		var logger = host.Services.GetRequiredService<ILogger<Program>>();
		CTS.Global.Logger = logger;
		GlobalErrorHandler.Handling(logger);
		
		host.Start();

		Global.Instance.Subscribe();

		var application = new Application();
		application.Startup += Application_Startup;
		application.Exit += App_Exit;

		try
		{
			LoadingResource.LoadingInApplication();
			Global.Instance.Initialize();

			var windows = host.Services.GetRequiredService<MainWindow>();

			windows.Show();
			application.MainWindow = windows;
			application.Run();
		}
		catch (Exception ex)
		{
			logger.LogError(ex, ex.Message);
		}
	}

	private static void App_Exit(object sender, ExitEventArgs e)
	{
		Global.Instance?.Unsubscribe();
	}

	public static IHostBuilder CreateDefaultBuilder(string[] args)
	{
		var builder = Host.CreateDefaultBuilder(args);

		builder.ConfigureAppConfiguration((context, config) =>
		{
			//读取子进程配置信息
			config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "AppService/appsetting.json"), true, true);
			config.AddEnvironmentVariables();
		}).ConfigureLogging(loggingBuilder =>
		{
			loggingBuilder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddNanoLogger();
		});

		builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

		builder.ConfigureContainer<ContainerBuilder>((context, container) =>
		{
			container.AddRealtimeContainer();
			container.AddNanoStatusApplicationServices();
			container.RegisterModule<ViewModelModule>();
			container.RegisterModule<UIControlModule>();
		});

		builder.ConfigureServices((context, services) =>
		{
			services.Configure<List<SubProcessSetting>>(context.Configuration.GetSection("SubProcesses"));

			services.AddCommunicationClientServices();
			services.AddApplicationMapper();

			//services.AddSingleton<LockControl>();
			services.TryAddScoped<MainWindow>();
		});

		return builder;
	}

	private static void Application_Startup(object sender, StartupEventArgs e)
	{
		//阻止硬件加速,锁屏后会导致界面假死,material design库的问题
		RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
	}
}
