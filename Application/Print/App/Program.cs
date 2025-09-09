//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/8 8:59:39     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------

using NV.CT.DatabaseService.Contract;
using NV.CT.DicomImageViewer;
using NV.CT.Print.ApplicationService.Impl.Extensions;
using NV.CT.Print.Extensions;
using NV.CT.Print.View;
using NV.CT.Print.ViewModel;
using NV.CT.UI.Controls.Keyboard;
using NV.MPS.Configuration;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Extension;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Print;

public class Program
{
    private static readonly KeyboardHook keyboardHook = new();
    private static IApplicationCommunicationService? _applicationCommunicationService;
    private static EventWaitHandle? _programStarted;

    [STAThread]
    public static void Main(string[] args)
    {
        _programStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.Print", out var createNew);
        if (!createNew)
        {
            _programStarted.Set();
            System.Environment.Exit(0);
            return;
        }

        var host = CreateDefaultBuilder(args).Build();
        host.Start();
        CTS.Global.ServiceProvider = host.Services;
        //Global.Instance.ServiceProvider = host.Services;
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        CTS.Global.Logger = logger;
        GlobalErrorHandler.Handling(logger);

        try
        {
            //开发阶段暂时不打开全局捕获异常
            //GlobalHandleError(logger);
            var app = new System.Windows.Application();
            app.Exit += App_Exit;

            LoadingResource.LoadingInApplication();
            Global.Instance.Subscribe();

            //加载打印配置文件
            var printService = host.Services.GetRequiredService<IPrint>();
            string studyId = printService.GetCurrentStudyId();
            var printConfigManager = CTS.Global.ServiceProvider.GetRequiredService<IPrintConfigManager>();
            printConfigManager.LoadConfig(studyId);

            if (args.Length > 1)
            {
                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                var serverHandle = args[0];
                ConsoleSystemHelper.WindowHwnd = IntPtr.Parse(args[1]);
                logger?.LogDebug($"args length of Print:{args.Length}");

                var view = new MainPrintControl();
                var hwndView = ViewHelper.ToHwnd(view);
                PipeHelper.ClientStream(serverHandle, hwndView.ToInt32());

                logger?.LogDebug($"recevied StudyId of Print is:{studyId}");
                var mainPrintControlViewModel = CTS.Global.ServiceProvider.GetRequiredService<MainPrintControlViewModel>();
                mainPrintControlViewModel.LoadStudyDataById(studyId);

                //采用异步方式避免堵塞进程Print的启动
                Task.Run(() => {
                    var server = host.Services.GetRequiredService<ServerProxy>();
                    StartPrintConfigService(server);
                    logger?.LogDebug($"PrintConfigService's server started with {RuntimeConfig.MCSServices.PrintConfigService.IP}:{RuntimeConfig.MCSServices.PrintConfigService.Port}");
                });
                
                _applicationCommunicationService = CTS.Global.ServiceProvider.GetService<IApplicationCommunicationService>();
                RegisterHotKeys(app, hwndView, logger);

                Dispatcher.Run();
            }
            else
            {
                var mainWindow = host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
                app.MainWindow = mainWindow;
                ConsoleSystemHelper.WindowHwnd = ViewHelper.ToHwnd(mainWindow);
                app.Run();
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, $"Print exception:{ex.Message}");
        }
    }

    private static void StartPrintConfigService(ServerProxy server)
    {        
        server.StartServices(RuntimeConfig.MCSServices.PrintConfigService.IP, RuntimeConfig.MCSServices.PrintConfigService.Port);
        server.AttachEvents(typeof(IPrintConfigService));
        Console.WriteLine($"PrintConfigService's server started with {RuntimeConfig.MCSServices.PrintConfigService.IP}:{RuntimeConfig.MCSServices.PrintConfigService.Port}");
    }

    private static void App_Exit(object sender, ExitEventArgs e)
    {
        Global.Instance.Unsubscribe();
        keyboardHook.Stop();
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
            config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "Print/appsettings.json"), true, true);
            config.AddEnvironmentVariables();
        });

        builder.ConfigureContainer<ContainerBuilder>((_, container) =>
        {
            container.AddApplicationServiceContainer();
            container.AddViewModelContainer();
            container.AddDialogServiceContianer();
            container.AddDicomImageViewerContainer();
        });

        builder.ConfigureServices((_, services) =>
        {
            services.AddCommunicationServerServices();
            services.AddCommunicationClientServices();
            services.AddApplicationMapper();
            services.AddApplicationServiceMapper();
            services.TryAddScoped<MainWindow>();
        });

        return builder;
    }

    private static void RegisterHotKeys(Application app, IntPtr intPtr, ILogger<Program> logger)
    {
        var operateCommandsViewModel = CTS.Global.ServiceProvider.GetRequiredService<OperateCommandsViewModel>();
        var wwwlSets = UserConfig.WindowingConfig.Windowings;
        var presetKeys = wwwlSets.Select(n => n.Shortcut?.ToUpper().Trim()).ToList();

        //如果当前图像浏览进程不在焦点范围内，快捷键不起作用！
        keyboardHook.KeyUp += (sender, e) =>
        {
            var currentActiveApplication = _applicationCommunicationService?.GetCurrentActiveApplication();
            if (currentActiveApplication != null &&
                currentActiveApplication.ItemName != ApplicationParameterNames.APPLICATIONNAME_PRINT)
            {
                //logger.LogInformation($"RegisterHotKeys Print module current active app : [{currentActiveApplication?.ItemName}],do not process");
                return;
            }

            var keycode = e.KeyCode.ToString().ToUpper();
            var isCtrlPressed = (e.Modifiers & System.Windows.Forms.Keys.Control) == System.Windows.Forms.Keys.Control;
            var isShiftPressed = (e.Modifiers & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift;
            var isAltPressed = (e.Modifiers & System.Windows.Forms.Keys.Alt) == System.Windows.Forms.Keys.Alt;

            logger.LogInformation($"RegisterHotKeys for Print module current active app : [{currentActiveApplication?.ItemName}_{currentActiveApplication?.Parameters}], KeyCode={keycode}, isCtrlPressed={isCtrlPressed} isShiftPressed ={isShiftPressed}, isAltPressed ={isAltPressed}]");

            if (operateCommandsViewModel.IsEditable)
            {
                // Deal with the predefined hotkeys
                if (presetKeys.Contains(keycode))
                {
                    var choosePreset = wwwlSets.FirstOrDefault(n => n.Shortcut == keycode);
                    if (choosePreset is null)
                        return;

                    operateCommandsViewModel?.OnClickWWWLMenuItem(choosePreset);

                    //Log($"set ww/wl with shortcut {keycode},with preset {choosePreset.Width.Value},{choosePreset.Level.Value}");
                }

                // Deal with special hotkeys (Ctrl+C, Ctrl+V)
                if (isCtrlPressed && !isShiftPressed && !isAltPressed)
                {
                    switch (e.KeyCode)
                    {
                        case System.Windows.Forms.Keys.C:
                            //logger.LogInformation("Target Ctrl+C");
                            Global.Instance.ImageViewer.Copy();
                            return;
                        case System.Windows.Forms.Keys.V:
                            //logger.LogInformation("Target Ctrl+V");
                            Global.Instance.ImageViewer.Paste();
                            return;
                    }
                }

                // Deal with Delete hotkey
                if (e.KeyCode == System.Windows.Forms.Keys.Delete && !isCtrlPressed && !isShiftPressed && !isAltPressed)
                {
                    //logger.LogInformation("Target Delete");
                    Global.Instance.ImageViewer.Delete();
                    return;
                }
            }
        };
        keyboardHook.Start();
    }
}