using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NV.CT.ClientProxy;
using NV.CT.CTS.Helpers;
using NV.CT.Logging;
using NV.CT.ProtocolManagement.Views.English;
using NV.CT.ProtocolManagement.ApplicationService.Impl.Extension;
using NV.CT.ProtocolManagement.ViewModels;
using NV.CT.UI.Controls;
using NV.CT.UI.Controls.Common;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NV.MPS.UI.Dialog.Extension;
using NV.MPS.Environment;

namespace NV.CT.ProtocolManagement;

public class Program
{
    private static EventWaitHandle _eventWaitHandle;

    [STAThread]
    public static void Main(string[] args)
    {
        _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.ProtocolManagement", out bool creatNew);
        if (!creatNew)
        {
            _eventWaitHandle.Set();
            System.Environment.Exit(0);
            return;
        }
        var host = CreateDefaultBuilder(args).Build();
        host.Start();
        var logger = host.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
        CTS.Global.Logger = logger;
        GlobalErrorHandler.Handling(logger);

        Global.Instance.ServiceProvider = host.Services;
        Global.Instance.Subscribe();

        var app = new Application();
        LoadingResource.LoadingInApplication();
        app.Exit += App_Exit;

        if (args.Length > 1)
        {
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var serverHandle = args[0];
            ConsoleSystemHelper.WindowHwnd = IntPtr.Parse(args[1]);//LoadingResource.LoadingInControl()
            var view = new MainUserControl();

            var hwndView = ViewHelper.ToHwnd(view);
            PipeHelper.ClientStream(serverHandle, hwndView.ToInt32());

            Dispatcher.Run();
        }
        else
        {
            Window mainWindow = host.Services.GetRequiredService<MainWindow>();
            app.Run(mainWindow);
        }
    }

    private static void App_Exit(object sender, ExitEventArgs e)
    {
        Global.Instance.Unsubscribe();
    }

    public static IHostBuilder CreateDefaultBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.ConfigureAppConfiguration((context, config) =>
        {
            //TODO:配置注入
            config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "ProtocolManagement/appsetting.json"), true, true);
            config.AddEnvironmentVariables();
        });
        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        builder.ConfigureLogging(logBuilder =>
        {
            logBuilder.ClearProviders().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace).AddNanoLogger();
        });

        builder.ConfigureContainer<ContainerBuilder>((context, container) =>
        {
            container.AddApplicationServiceContainer();
            container.RegisterModule<ViewModelModule>();
            container.AddDialogServiceContianer();

        });

        builder.ConfigureServices((context, services) =>
        {
            services.AddCommunicationClientServices();
            //TODO:界面窗口注入
            services.TryAddScoped<MainWindow>();
        });

        return builder;
    }
}