using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NV.CT.ClientProxy;
using NV.CT.CTS.Helpers;
using NV.CT.JobViewer.View.English;
using NV.CT.JobViewer.ApplicationService.Impl.Extensions;
using NV.CT.JobViewer.Extensions;
using NV.CT.JobViewer.View;
using NV.CT.Logging;
using NV.CT.UI.Controls;
using NV.CT.UI.Controls.Common;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NV.MPS.UI.Dialog.Extension;
using System.Globalization;
using NV.MPS.Environment;

namespace NV.CT.JobViewer
{
    public class Program
    {
        private static EventWaitHandle? _programStarted;
        public static IServiceProvider? ServiceProvider { get; set; }
        [STAThread]
        public static void Main(string[] args)
        {
            SetCultureInfo();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            _programStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.JobViewer", out var createNew);
            if (!createNew)
            {
                _programStarted.Set();
                System.Environment.Exit(0);
                return;
            }

            var host = CreateDefaultBuilder(args).Build();
            host.Start();
            ServiceProvider = host.Services;
            if (null != Global.Instance)
            {
                Global.Instance.ServiceProvider = host.Services;
                Global.Instance.Subscribe();
            }
            //Thread.Sleep(20000);
            var app = new System.Windows.Application();
            app.Exit += App_Exit;

            LoadingResource.LoadingInApplication();
            var logger = Global.Instance?.ServiceProvider?.GetRequiredService<ILogger<Program>>();
            logger?.LogInformation("JobViewer is starting!");
            try
            {
                if (args.Length > 0)
                {
                    var serverHandle = args[0];

                    ConsoleSystemHelper.WindowHwnd = IntPtr.Parse(args[1]);
                    var view = new MainControl(LoadingResource.LoadingInControl(), IntPtr.Parse(args[1]));
                    var hwndView = ViewHelper.ToHwnd(view);
                    PipeHelper.ClientStream(serverHandle, hwndView.ToInt32());
                    Dispatcher.Run();
                }
                else
                {
                    var masterWindow = Global.Instance?.ServiceProvider?.GetRequiredService<MainWindow>();
                    app.Run(masterWindow);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError("Console Error {0}", ex);
            }
        }
        private static void App_Exit(object sender, ExitEventArgs e)
        {
            Global.Instance?.Unsubscribe();
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logger = Global.Instance?.ServiceProvider?.GetRequiredService<ILogger<Program>>();
            Exception? ex = e.ExceptionObject as Exception;//"AppDomain >>> "
            logger?.LogError(ex, ex?.Message);
        }

        public static IHostBuilder CreateDefaultBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                //TODO:配置注入
                config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "JobViewer/appsetting.json"), true, true);
                config.AddEnvironmentVariables();
            }).ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders().AddNanoLogger();
            });
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.ConfigureContainer<ContainerBuilder>((_, container) =>
            {
                container.AddApplicationServiceContainer();
                container.RegisterModule<ViewModelModule>();
                container.AddDialogServiceContianer();
            });

            builder.ConfigureServices((_, services) =>
            {
                services.AddCommunicationClientServices();

                services.AddApplicationServices();
                services.AddApplicationMapper();
                //TODO:界面窗口注入
                services.TryAddScoped<MainWindow>();
            });

            return builder;
        }

        private static void SetCultureInfo()
        {
            //TODO : will get language from configuration file
            string language = string.Empty; // string.Empty; //"chinese"

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

    }
}
