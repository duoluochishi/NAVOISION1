using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NV.CT.Logging;
using Microsoft.Extensions.Configuration;
using NV.MPS.Environment;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using NV.MPS.Communication;
using NV.CT.CTS;
using System.Reflection;
using System.Runtime.Loader;
using NV.CT.DatabaseService.Impl.Repository;
using NV.CT.ConfigService.Impl;
using NV.CT.DatabaseService.Impl;
using NV.CT.ProtocolService.Impl;
using NV.CT.WorkflowService.Impl;
using NV.CT.MessageService.Impl;
using NV.CT.SystemInterface.MCSRuntime.Impl.Extensions;
using NV.CT.SystemInterface.MRSIntegration.Impl.Extensions;
using NV.CT.AppService.Contract;
using NV.CT.AppService.Impl;
using NV.CT.CTS.Helpers;

namespace NV.CT.MCS.Services;

public class Program
{
    static void Main(string[] args)
    {
        Console.Title = "MCS Services";

        //Thread.Sleep(5000);

        var host = CreateDefaultBuilder(args).Build();
        host.StartAsync();

        CTS.Global.ServiceProvider = host.Services;
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        CTS.Global.Logger = logger;
        logger.LogInformation($"MCS Services is starting listening...");

        ErrorHandler.Handling(logger);
        var server = host.Services.GetRequiredService<ServerProxy>();

        try
        {
            server.StartServices(RuntimeConfig.MCSServices.SystemService.IP, RuntimeConfig.MCSServices.SystemService.Port);
            logger.LogInformation($"MCS Services ({RuntimeConfig.MCSServices.SystemService.IP}:{RuntimeConfig.MCSServices.SystemService.Port}) has finished listening successfully.");

            //注册这几个模块下的接口
            AttachModuleInterfaces("NV.CT.ConfigService.Contract.dll", server);
            AttachModuleInterfaces("NV.CT.DatabaseService.Contract.dll", server);
            AttachModuleInterfaces("NV.CT.ProtocolService.Contract.dll", server);
            AttachModuleInterfaces("NV.CT.WorkflowService.Contract.dll", server);
            AttachModuleInterfaces("NV.CT.MessageService.Contract.dll", server);

            //AttachModuleInterfaces("NV.CT.AppService.Contract.dll", server);

            server.AttachEvents(typeof(IApplicationCommunicationService));
            server.AttachEvents(typeof(IScreenManagement));
            server.AttachEvents(typeof(IShutdownService));
            server.AttachEvents(typeof(ISelfCheckService));

        }
        catch (Exception ex)
        {
            logger.LogError($"MCS Services failed to start with exception:{ex.Message}. Here is stacktrace:{ex.StackTrace}");
            throw;
        }

        Console.ReadLine();
    }

    private static void AttachModuleInterfaces(string moduleName, ServerProxy? server)
    {
        var databaseModule = Path.Combine(RuntimeConfig.Console.MCSBin.Path, moduleName);
        Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(databaseModule);

        var interfaceList = assembly.GetTypes().Where(n => n.IsInterface).ToList();
        foreach (var type in interfaceList)
        {
            server?.AttachEvents(type);
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
            config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "AppService/appsetting.json"), true, true);
        });
        builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        builder.ConfigureContainer<ContainerBuilder>((_, container) =>
        {
            container.AddDatabaseRepositoryContainer();
            container.AddMCSContainer();
            container.AddRealtimeContainer();
            container.AddOfflineContainer();
            container.RegisterModule<ApplicationModule>();
        });
        builder.ConfigureServices((_, services) =>
        {
            services.Configure<List<SubProcessSetting>>(_.Configuration.GetSection("SubProcesses"));

            services.AddCommunicationServerServices();

            services.AddMRSMapper();
            services.AddAutoMapper(typeof(ToDataProfile));

            services.AddDatabaseServices();
            services.AddConfigServices();
            services.AddProtocolServices();
            services.AddWorkflowServices();
            services.AddMessageServices();
        });

        return builder;
    }
}
