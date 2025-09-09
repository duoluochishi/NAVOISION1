using NV.MPS.Environment;

namespace NV.CT.RGT;

public class Program
{
	//private static EventWaitHandle? _programStarted;

	[STAThread]
	public static void Main(string[] args)
	{
		//_programStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.RGT", out var createNew);
		//if (!createNew)
		//{
		//	_programStarted.Set();
		//	System.Environment.Exit(0);
		//	return;
		//}

		var host = CreateDefaultBuilder(args).Build();
		host.Start();

		CTS.Global.ServiceProvider = host.Services;
		var logger = host.Services.GetRequiredService<ILogger<Program>>();
		CTS.Global.Logger = logger;

		GlobalErrorHandler.Handling(logger);
		//Thread.Sleep(7000);

		try
		{
			var app = new Application();

			LoadResources();

			app.Exit += App_Exit;

			Global.Instance.Initialize();
			Global.Instance.Subscribe();

			try
			{
				var mainWindow = host.Services.GetRequiredService<MainWindow>();
				mainWindow.Show();
				app.MainWindow = mainWindow;
				app.Run(mainWindow);
			}
			catch (Exception ex)
			{
				CTS.Global.Logger?.LogError("RGT Error {0}", ex);
			}
		}
		catch (Exception ex)
		{
			logger?.LogError(ex, $"RGT exception:{ex.Message}");
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
			config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "RGT/appsettings.json"), true, true);
			config.AddEnvironmentVariables();
		});

		builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

		builder.ConfigureContainer<ContainerBuilder>((_, container) =>
		{
			container.AddRgtAppContainer();
			container.AddDialogServiceContianer();
			container.AddRgtApplicationServiceContainer();
		});

		builder.ConfigureServices((_, services) =>
		{
			services.AddCommunicationClientServices();

			services.AddSingleton<FooterViewModel>();
			services.AddSingleton<HeaderViewModel>();
			services.AddSingleton<PatientBrowserViewModel>();
			services.AddSingleton<ProtocolSelectMainViewModel>();
			services.AddSingleton<ProtocolViewModel>();
			services.AddSingleton<ScanControlsViewModel>();
			services.AddSingleton<ScanDefaultViewModel>();
			services.AddSingleton<ScanMainViewModel>();
			services.AddSingleton<ScanRangeViewModel>();
			services.AddSingleton<TimeViewModel>();

			services.AddSingleton<MainWindow>();
			services.AddSingleton<SettingWindow>();
		});

		return builder;
	}

	private static void LoadResources()
	{
		BundledTheme bundledTheme = new BundledTheme();
		bundledTheme.BaseTheme = BaseTheme.Dark;
		bundledTheme.PrimaryColor = PrimaryColor.LightBlue;
		bundledTheme.SecondaryColor = SecondaryColor.LightGreen;
		Application.Current.Resources.MergedDictionaries.Add(bundledTheme);

		ResourceDictionary languageResDic = new ResourceDictionary();
		languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml", UriKind.RelativeOrAbsolute);
		Application.Current.Resources.MergedDictionaries.Add(languageResDic);

		languageResDic = new ResourceDictionary();
		languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.PopupBox.xaml", UriKind.RelativeOrAbsolute);
		Application.Current.Resources.MergedDictionaries.Add(languageResDic);

		languageResDic = new ResourceDictionary();
		languageResDic.Source = new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Nanovision.xaml", UriKind.RelativeOrAbsolute);
		Application.Current.Resources.MergedDictionaries.Add(languageResDic);

		var resource = new ResourceDictionary();
		resource.Source = new Uri("/Resources/Style.xaml", UriKind.RelativeOrAbsolute);
		Application.Current.Resources.MergedDictionaries.Add(resource);
	}

}