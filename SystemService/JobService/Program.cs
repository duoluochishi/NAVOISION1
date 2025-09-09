//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NV.MPS.Communication;
using NV.CT.CTS;
using NV.CT.CTS.Helpers;
using NV.CT.JobService.Contract;
using NV.CT.Logging;
using NV.CT.SystemInterface.MRSIntegration.Impl.Extensions;
using NV.MPS.Environment;
using NV.CT.JobService.Extensions;
using NV.CT.JobService.Interfaces;

namespace NV.CT.JobService;

public class Program
{
    public static void Main(string[] args)
    {
        Console.Title = $"Job's server";

        var host = CreateDefaultBuilder(args).Build();
        host.Start();

        ErrorHandler.Handling(host.Services.GetRequiredService<ILogger<Program>>());

        AppDomain.CurrentDomain.ProcessExit += ProcessExit;

        var server = host.Services.GetRequiredService<ServerProxy>();
        server.StartServices(RuntimeConfig.MCSServices.JobService.IP, RuntimeConfig.MCSServices.JobService.Port);
        Console.WriteLine($"Job's server started with {RuntimeConfig.MCSServices.JobService.IP}:{RuntimeConfig.MCSServices.JobService.Port}");

        server.AttachEvents(typeof(IOfflineTaskService));
        server.AttachEvents(typeof(IDicomFileService));
        server.AttachEvents(typeof(IJobRequestService));
        server.AttachEvents(typeof(IJobManagementService));
        server.AttachEvents(typeof(IJobQueueHandler));
        Console.ReadLine();
    }

    private static void ProcessExit(object? sender, EventArgs e)
    {
        //todo:暂不实现业务
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
            config.AddJsonFile(System.IO.Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "Job/appsettings.json"), true, true);
        });
        builder.ConfigureContainer<ContainerBuilder>((_, container) =>
        {
            //container.AddMRSContainer();
            container.AddOfflineContainer();
        });
        builder.ConfigureServices((_, services) =>
        {
            services.AddJobServices();
        });

        return builder;
    }
}