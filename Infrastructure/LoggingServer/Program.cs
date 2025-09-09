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
using Microsoft.Extensions.Logging;
using NV.CT.LoggingServer;
using NV.MPS.Environment;
using Serilog;
using Serilog.Events;
using Serilog.Filters;

public class Program
{
    public static EventWaitHandle ApplicationStarted;

    private async static Task Main(string[] args)
    {
        bool isCreateNew;
        ApplicationStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "NV.CT.LoggingServer", out isCreateNew);
        if (!isCreateNew)
        {
            ApplicationStarted.Set();
            return;
        }

        Console.Title = "Logging Service";

        var host = CreateDefaultBuilder(args);
        await host.RunAsync();
    }

    public static IHost CreateDefaultBuilder(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args);
        host.ConfigureAppConfiguration((context, config) =>
        {
            config.AddEnvironmentVariables();
        });
        host.ConfigureLogging((context, logging) =>
        {
            var logger = GetLoggerConfiguration().CreateLogger();
            logging.AddSerilog(logger);
        });
        host.ConfigureServices((context, services) => {
            services.AddHostedService<MainRunner>();
            services.AddHostedService<AuditLogRunner>();
        });
        host.UseConsoleLifetime();
        return host.Build();
    }

    public static LoggerConfiguration GetLoggerConfiguration()
    {
        var configuration = new LoggerConfiguration();

        var logLevel = LogEventLevel.Verbose;

        switch (RuntimeConfig.LogLevel)
        {
            case Microsoft.Extensions.Logging.LogLevel.Critical:
                configuration.MinimumLevel.Fatal();
                logLevel = LogEventLevel.Fatal;
                break;
            case Microsoft.Extensions.Logging.LogLevel.Error:
                configuration.MinimumLevel.Error();
                logLevel = LogEventLevel.Error;
				break;
            case Microsoft.Extensions.Logging.LogLevel.Warning:
                configuration.MinimumLevel.Warning();
                logLevel = LogEventLevel.Warning;
				break;
            case Microsoft.Extensions.Logging.LogLevel.Information:
                configuration.MinimumLevel.Information();
                logLevel = LogEventLevel.Information;
				break;
            case Microsoft.Extensions.Logging.LogLevel.Debug:
                configuration.MinimumLevel.Debug();
                logLevel = LogEventLevel.Debug;
				break;
            case Microsoft.Extensions.Logging.LogLevel.Trace:
            default:
                configuration.MinimumLevel.Verbose();
                logLevel = LogEventLevel.Verbose;
				break;
        }

        //Console.WriteLine($"current log level is {logLevel}");

        configuration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
        configuration.MinimumLevel.Override("System", LogEventLevel.Warning);

        configuration.MinimumLevel.Is(logLevel);

        configuration.WriteTo.Console();

        configuration.WriteTo.Logger(lc =>
        {
            lc.MinimumLevel.Is(logLevel);
            lc.Filter.ByExcluding(Matching.FromSource("AuditLogging"));

            lc.WriteTo.Async(o =>
            {
                o.File(path: Path.Combine(RuntimeConfig.Console.MCSLog.Path, "MCS_.log"),
                        outputTemplate: "{Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: logLevel,//LogEventLevel.Verbose,
                        rollingInterval: RollingInterval.Day,
                        fileSizeLimitBytes: 20971520,
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: null,
                        buffered: false);
            });
        });

        configuration.WriteTo.Logger(lc => {
            lc.MinimumLevel.Information();
            lc.Filter.ByIncludingOnly(Matching.FromSource("AuditLogging"));
            lc.WriteTo.Async(o => {
                o.File(path: Path.Combine(RuntimeConfig.Console.MCSLog.Path, "AuditLogging_.log"),
                        outputTemplate: "{Message:lj}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        rollingInterval: RollingInterval.Day,
                        fileSizeLimitBytes: 20971520,
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: null,
                        buffered: false);
            });
        });

        return configuration;
    }
}