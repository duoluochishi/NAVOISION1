//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NV.CT.ClientProxy;
using NV.CT.Logging;
using NV.MPS.Environment;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace NV.CT.ScreenManagement;

public class Program
{
    private static EventWaitHandle? _programStarted;

    [STAThread]
    public static void Main(string[] args)
    {
        _programStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.ScreenManagement", out var createNew);
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

        //GlobalErrorHandler.Handling(logger);
        //Thread.Sleep(7000);

        try
        {
            var app = new Application();

            app.Exit += App_Exit;

            if (args.Length > 1)
            {
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                Global.Instance.Subscribe();

                new MainWindow();

                Dispatcher.Run();
            }
            else
            {
                Global.Instance.Subscribe();

                Window mainWindow = host.Services.GetRequiredService<MainWindow>();
                mainWindow.Hide();

                //var secondaryWindow = host.Services.GetRequiredService<SecondaryWindow>();
                //secondaryWindow.Hide();

                Dispatcher.Run();
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"ScreenManagement exception:{ex.Message}");
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
            config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "ScreenManagement/appsettings.json"), true, true);
            config.AddEnvironmentVariables();
        });

        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        builder.ConfigureContainer<ContainerBuilder>((_, container) =>
        {
        });

        builder.ConfigureServices((_, services) =>
        {
            services.AddCommunicationClientServices();

            services.AddSingleton<MainWindow>();
            //services.AddSingleton<SecondaryWindow>();
        });

        return builder;
    }
}