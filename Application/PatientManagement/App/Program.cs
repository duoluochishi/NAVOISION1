//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------


using NV.CT.DicomImageViewer;
using NV.CT.DicomUtility.Extensions;
using NV.CT.UI.Controls.Extensions;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog;
using NV.MPS.UI.Dialog.Extension;
using System.Globalization;

namespace NV.CT.PatientManagement;

public class Program
{
    private static EventWaitHandle? _programStarted;
    public static IServiceProvider? ServiceProvider;

    [STAThread]
    public static void Main(string[] args)
    {
        SetCultureInfo();
        _programStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.PatientManagement", out var createdNew);
        //TODO:暂时不限制
        //if (!createdNew)
        //{
        //    ProgramStarted.Set();
        //    System.Environment.Exit(0);
        //    return;
        //}
        var host = CreateDefaultBuilder(args).Build();
        host.Start();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("PatientManagement is starting...");

        Global.Instance.ServiceProvider = host.Services;
        ServiceProvider = host.Services;
        CTS.Global.ServiceProvider = host.Services;
        Global.Instance.Subscribe();
        Application app = new Application();
        app.Exit += App_Exit;

        LoadingResource.LoadingInApplication();
        if (args.Length > 0)
        {
            var serverHandle = args[0];
            ConsoleSystemHelper.WindowHwnd = IntPtr.Parse(args[1]);
            try
            {
                var wih = new WindowInteropHelper(host.Services.GetRequiredService<DialogWindow>());
                wih.Owner = ConsoleSystemHelper.WindowHwnd;
                var view = new MainControl();
                var hwndView = ViewHelper.ToHwnd(view);
                PipeHelper.ClientStream(serverHandle, hwndView.ToInt32());
                GlobalErrorHandler.Handling(logger);
                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to start PatientManagement with exception:{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                var errorCode = ErrorCodes.ErrorCodeResource.MCS_Common_GRPC_NotConnected_Code;
                var errorDescription = ErrorCodes.ErrorCodeResource.MCS_Common_GRPC_NotConnected_Description;
                MessageBox.Show($"{errorCode}{Environment.NewLine}{errorDescription}", LogLevel.Error.ToString(), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            var windows = host.Services.GetRequiredService<MainWindow>();
            windows.Show();
            app.MainWindow = windows;
            logger.LogInformation("PatientManagement has started sucessfully without args.");
            app.Run();
        }
    }

    private static void App_Exit(object sender, ExitEventArgs e)
    {
        Global.Instance.Unsubscribe();
    }

    public static IHostBuilder CreateDefaultBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        builder.ConfigureLogging(logBuilder =>
        {
            logBuilder.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddNanoLogger();
        });
        builder.ConfigureAppConfiguration((_, config) =>
        {
            //TODO:配置注入
            config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "PatientManagement/appsetting.json"), true, true);
            config.AddEnvironmentVariables();
        });

        builder.ConfigureContainer<ContainerBuilder>((_, container) =>
        {
            //TODO:Autofac注入
            //container.AddDataRepositoryContainer();
            //  container.AddDataServiceContainer();
            container.AddApplicationServiceContainer();
            container.AddViewModelContainer();
            container.AddDialogServiceContianer();
            container.AddDicomImageViewerContainer();
            container.AddUIControlContainer();
            // container.AddUserConfigDataRepositoryContainer();
        });
        builder.ConfigureServices((_, services) =>
        {
            services.AddCommunicationClientServices();
            services.DicomUtilityConfigInitialization();
            services.DicomUtilityConfigInitializationForWin();

            //TODO:AutoMapper注入
            //services.AddDataMapper();
            services.AddCommonControlMapper();
            services.AddApplicationServiceMapper();
            services.AddApplicationMapper();

            services.TryAddScoped<MainWindow>();
        });
        return builder;
    }

    private static void SetCultureInfo()
    {
        //TODO : will get language from configuration file
        string language = string.Empty;

        CultureInfo currentCulture;
        if (language.ToLower() == "chinese")
        {
            currentCulture = new CultureInfo("zh-CN");
        }
        else if(language.ToLower() == "english")
        {
            currentCulture = new CultureInfo("en-US");
        }
        else
        {
            currentCulture = new CultureInfo(string.Empty);
        }

        CultureInfo.DefaultThreadCurrentUICulture = currentCulture;
    }
}