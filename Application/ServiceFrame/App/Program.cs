using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NV.CT.ClientProxy;
using NV.CT.CTS.Helpers;
using NV.CT.Logging;
using NV.CT.ServiceFrame.ApplicationService.Impl.Extensions;
using NV.CT.ServiceFrame.Extensions;
using NV.CT.ServiceFrame.View.English;
using NV.CT.ServiceFramework.Contract;
using NV.CT.UI.Controls;
using NV.CT.UI.Controls.Common;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog;
using NV.MPS.UI.Dialog.Extension;
using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace NV.CT.ServiceFrame;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var host = CreateDefaultBuilder(args).Build();
        host.Start();

        CTS.Global.ServiceProvider = host.Services;
        CTS.Global.Logger = host.Services.GetRequiredService<ILogger<Program>>();
        ServiceFramework.Global.Instance.ServiceProvider = CTS.Global.ServiceProvider;

        //默认进行初始化
        var initializer = CTS.Global.ServiceProvider.GetService<IInitializer>();
        initializer?.Initialize();

        Global.Instance.Subscribe();

        var application = new Application();
        LoadingResource.LoadingInApplication();
        application.Exit += App_Exit;
        application.Startup += Application_Startup;
		GlobalErrorHandler.Handling(CTS.Global.Logger);

        if (args.Length > 0)
        {
            var serverHandle = args[0];
            ConsoleSystemHelper.WindowHwnd = IntPtr.Parse(args[1]);
            ServiceFramework.Global.Instance.MainWindowHwnd = ConsoleSystemHelper.WindowHwnd;

            Global.Instance.ModelName = args[2];
            LoadingResource.LoadingInControl();
            var view = new MainControl();

            var hwndView = ViewHelper.ToHwnd(view);
            PipeHelper.ClientStream(serverHandle, hwndView.ToInt32());
            application.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            Dispatcher.Run();
        }
    }

    private static void App_Exit(object sender, ExitEventArgs e)
    {
        Global.Instance.Unsubscribe();
    }

    public static IHostBuilder CreateDefaultBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        var moduleName = args.Length > 0 ? args[2] : "Test";
        var loaders = DynamicLoader.Instance.Load(moduleName);

        builder.ConfigureLogging(logBuilder =>
        {
            logBuilder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddNanoLogger();
        });
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var configRoot = RuntimeConfig.Console.MCSConfig.Path;
            config.AddJsonFile(Path.Combine(configRoot, "Adjustment/appsetting.json"), true, true);
            config.AddEnvironmentVariables();

            foreach (var loader in loaders)
            {
                loader?.ConfigureConfig(context, config);
            }
        });
        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.ConfigureContainer<ContainerBuilder>((context, container) =>
        {
            container.AddDialogServiceContianer();
            container.AddApplicationServiceContainer();
            container.AddViewModelContainer();

            foreach (var loader in loaders)
            {
                loader?.ConfigureContainer(context, container);
            }
        });
        builder.ConfigureServices((context, services) =>
        {
            services.AddCommunicationClientServices();
            foreach (var loader in loaders)
            {
                loader?.ConfigureServices(context, services);
            }
        });
        return builder;
    }

	private static void Application_Startup(object sender, StartupEventArgs e)
	{
		//阻止硬件加速,锁屏后会导致界面假死,material design库的问题
		RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
	}
}