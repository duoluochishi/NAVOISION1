using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NV.CT.Service.TubeHistory.Service.Implements;
using NV.CT.Service.TubeHistory.Service.Interfaces;
using NV.CT.Service.TubeHistory.ViewModels;
using NV.CT.ServiceFramework.Contract;

namespace NV.CT.Service.TubeHistory
{
    public class BootLoader : IBootLoader
    {
        public void ConfigureConfig(HostBuilderContext context, IConfigurationBuilder config)
        {
        }

        public void ConfigureContainer(HostBuilderContext context, ContainerBuilder container)
        {
        }

        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddTransient<TubeHistoryViewModel>()
                    .AddSingleton<ITubeLog, TubeLog>();
        }
    }
}