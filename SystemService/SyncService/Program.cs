using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NV.CT.ClientProxy;
using NV.MPS.Communication;
using NV.CT.CTS;
using NV.CT.CTS.Helpers;
using NV.CT.Logging;
using NV.CT.SyncService.Contract;
using NV.CT.SystemInterface.MRSIntegration.Impl.Extensions;
using NV.MPS.Environment;

namespace NV.CT.SyncService;

public class Program
{
    private static MCSServiceClientProxy? _serviceClientProxy;
    private static ClientInfo? _clientInfo;

    public static void Main(string[] args)
    {
        Console.Title = "SyncService server";

        var tag = $"[SyncService]-{DateTime.Now:yyyyMMddHHmmss}";
        _clientInfo = new() { Id = tag };

        var host = CreateDefaultBuilder(args).Build();
        host.Start();

        CTS.Global.ServiceProvider = host.Services;

        ErrorHandler.Handling(host.Services.GetRequiredService<ILogger<Program>>());

        ConnectToOtherGrpcService(host.Services);

		var server = host.Services.GetRequiredService<ServerProxy>();
        server.StartServices(RuntimeConfig.MCSServices.SyncService.IP, RuntimeConfig.MCSServices.SyncService.Port);
        Console.WriteLine($"SyncService server started with {RuntimeConfig.MCSServices.SyncService.IP}:{RuntimeConfig.MCSServices.SyncService.Port}");

        AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

        //附加业务接口事件
        server.AttachEvents(typeof(IScreenSync));
        server.AttachEvents(typeof(IDataSync));

        Console.ReadLine();
    }

    private static void CurrentDomain_ProcessExit(object? sender, EventArgs e)
    {
        if (_clientInfo != null)
        {
            _serviceClientProxy?.Unsubscribe(_clientInfo);
        }
    }

    private static void ConnectToOtherGrpcService(IServiceProvider serviceProvider)
    {
        if (_clientInfo != null)
        {
            _serviceClientProxy = serviceProvider.GetRequiredService<MCSServiceClientProxy>();
        }
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
            config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "SyncService/appsettings.json"), true, true);
        });

        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        builder.ConfigureContainer<ContainerBuilder>((context, container) =>
        {
            //container.AddMRSContainer();
            container.AddRealtimeContainer();
        });
        builder.ConfigureServices((_, services) =>
        {
            services.AddMRSMapper();

            services.AddCommunicationClientServices();
            services.AddCommunicationServerServices();

            services.AddSingleton<IDataSync, DataSyncService>();
            services.AddSingleton<IScreenSync, ScreenSyncService>();
        });

        return builder;
    }
}