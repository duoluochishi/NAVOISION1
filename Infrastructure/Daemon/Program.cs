//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:44:32    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NV.CT.Daemon.Models;
using NV.CT.Daemon.Services;
using NV.MPS.Environment;
using Serilog.Events;
using Serilog;
using System.Diagnostics;
using Newtonsoft.Json;
using NV.CT.Daemon.Helpers;
using NV.CT.Daemon.Natives;
namespace NV.CT.Daemon;

public class Program
{
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

        var host = CreateHostBuilder(args).Build();

        var launchService = host.Services.GetRequiredService<LaunchService>();
        launchService.Start();

        host.Run();
    }

    private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
    {
        var process = new Process();
        process.StartInfo.FileName = "taskkill.exe";
        process.StartInfo.Arguments = string.Format(" /f /t /im NV.CT.*");
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();
        process.WaitForExit();
        process.Close();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "Daemon/appsetting.json"), true, true);
            config.AddEnvironmentVariables();
        });
        builder.ConfigureLogging((context, logging) => {
            logging.AddSerilog(
                new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .WriteTo
                    .Async(o => {
                        o.Console();
                        o.File(path: Path.Combine(RuntimeConfig.Console.MCSLog.Path, "daemon_.log"),
                                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] ({SourceContext}) {Message:lj}{NewLine}{Exception}",
                                    restrictedToMinimumLevel: LogEventLevel.Verbose,
                                    rollingInterval: RollingInterval.Day,
                                    fileSizeLimitBytes: 10485760,
                                    rollOnFileSizeLimit: true,
                                    buffered: false);
                    })
                    .CreateLogger(),
                dispose: true);
        });
        builder.ConfigureServices((context, services) =>
        {
            services.Configure<List<AppInfo>>(context.Configuration.GetSection("Processes"));
            services.AddSingleton<LaunchService>();
            services.AddHostedService<WatchService>();
            services.AddHostedService<PerformanceService>();
        });

        return builder;
    }
}