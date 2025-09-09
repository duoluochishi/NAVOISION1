//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.Examination.ApplicationService.Contract.Models;
using NV.CT.Examination.ApplicationService.Impl.Extensions;
using NV.CT.InterventionScan.ApplicationService.Impl.Extensions;
using NV.CT.InterventionScan.Extensions;
using NV.CT.InterventionScan.View;
using NV.CT.SystemInterface.MCSRuntime.Impl.Extensions;
using NV.CT.UI.Controls.Common;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog;
using NV.MPS.UI.Dialog.Extension;
using System.Threading;

namespace NV.CT.InterventionScan;

public class Program
{
    private static EventWaitHandle? _programStarted;
    [STAThread]
    public static void Main(string[] args)
    {
        _programStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.InterventionScan", out var createNew);
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
            var app = new Application();
            LoadingResource.LoadingInApplication();

            app.Exit += App_Exit;
            Global.Instance.Subscribe();

            if (args.Length > 1)
            {
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var serverHandle = args[0];
                ConsoleSystemHelper.WindowHwnd = IntPtr.Parse(args[1]);
                var wih = new WindowInteropHelper(host.Services.GetRequiredService<DialogWindow>());
                wih.Owner = ConsoleSystemHelper.WindowHwnd;               
                var view = new InterventionScanControl();
                var hwndView = ViewHelper.ToHwnd(view);
                PipeHelper.ClientStream(serverHandle, hwndView.ToInt32());

                Dispatcher.Run();
            }
            else
            {               
                Window mainWindow = host.Services.GetRequiredService<MainWindow>();
                app.Run(mainWindow);
                ConsoleSystemHelper.WindowHwnd = ViewHelper.ToHwnd(mainWindow);
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"Intervention Scan exception:{ex.Message}");
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
            config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "InterventionScan/appsettings.json"), true, true);
            config.AddEnvironmentVariables();
        });

        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        builder.ConfigureContainer<ContainerBuilder>((_, container) =>
        {
            container.AddMCSContainer();
            //container.AddMRSContainer();
            container.AddRealtimeContainer();
            container.AddApplicationServiceContainer();
            container.AddReconAppContainer();
            container.AddDialogServiceContianer();
            container.AddInterventionApplicationServiceContainer();
        });

        builder.ConfigureServices((context, services) =>
        {
            services.Configure<DeviceMonitorConfig>(context.Configuration.GetSection("DeviceMonitorConfig"));
            services.AddCommunicationClientServices();
            services.AddInterventionScanAppServices();

            services.AddMRSMapper();
            services.AddApplicationServices();

            services.TryAddScoped<MainWindow>();
        });
        return builder;
    }
}