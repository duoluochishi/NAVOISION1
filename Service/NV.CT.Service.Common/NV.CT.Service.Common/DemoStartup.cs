using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NV.CT.Logging;
using NV.CT.ServiceFramework.Contract;

namespace NV.CT.Service.Common
{
    public static class DemoStartup
    {
        public static void Startup()
        {
            Startup(null);
        }

        public static void Startup(IBootLoader? moduleLoader)
        {
            var commonLoader = new ServiceCommonLoader();
            var builder = Host.CreateDefaultBuilder();
            var host = builder.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                              .ConfigureLogging(b => b.ClearProviders().SetMinimumLevel(LogLevel.Trace).AddNanoLogger())
                              .ConfigureAppConfiguration((c, b) =>
                               {
                                   commonLoader.ConfigureConfig(c, b);
                                   moduleLoader?.ConfigureConfig(c, b);
                               })
                              .ConfigureServices((c, b) =>
                               {
                                   commonLoader.ConfigureServices(c, b);
                                   moduleLoader?.ConfigureServices(c, b);
                               })
                              .ConfigureContainer<ContainerBuilder>((c, b) =>
                               {
                                   commonLoader.ConfigureContainer(c, b);
                                   moduleLoader?.ConfigureContainer(c, b);
                               })
                              .Build();
            // host.Start();
            ServiceFramework.Global.Instance.ServiceProvider = host.Services;
            host.Services.GetService<IInitializer>()?.Initialize();
        }
    }
}