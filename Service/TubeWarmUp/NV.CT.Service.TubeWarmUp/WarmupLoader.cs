using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.TubeWarmUp.DependencyInject;
using NV.CT.Service.TubeWarmUp.Interfaces;
using NV.CT.Service.TubeWarmUp.Services;
using NV.CT.Service.TubeWarmUp.Services.Adapter;
using NV.CT.Service.TubeWarmUp.ViewModels;
using NV.CT.ServiceFramework.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp
{
    public class WarmupLoader : IBootLoader
    {
        public void ConfigureConfig(HostBuilderContext context, IConfigurationBuilder config)
        {
        }

        public void ConfigureContainer(HostBuilderContext context, ContainerBuilder container)
        {
        }

        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            ServiceLocator.Instance.AddSingleton<IDataService, DataService>();
            ServiceLocator.Instance.AddSingleton<IDialogService>(() => DialogService.Instance);
            ServiceLocator.Instance.AddSingleton<ILogService>(() => LogService.Instance);
            ServiceLocator.Instance.AddSingleton<WarmUpTaskViewModel>();
            ServiceLocator.Instance.AddSingleton<WarmUpProgressViewModel>();
            //ServiceLocator.Instance.AddSingleton<IWarmUpAdapter, WarmUpProxyAdapter>();
            ServiceLocator.Instance.AddSingleton<IWarmUpAdapter, WarmUpProxyAdapter1>();
            ServiceLocator.Instance.AddSingleton<IConfigService, ConfigService>();
            ServiceLocator.Instance.AddSingleton<WarmUpService>();
            ServiceLocator.Instance.AddSingleton<WarmUpViewModel>();
            ServiceLocator.Instance.AddSingleton<MiniWarmUpViewModel>();
        }
    }
}