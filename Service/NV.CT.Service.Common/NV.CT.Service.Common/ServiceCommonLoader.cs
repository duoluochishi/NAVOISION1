using Autofac;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Common.Models.ScanReconModels;
using NV.CT.ServiceFramework.Contract;
using NV.MPS.UI.Dialog;

namespace NV.CT.Service.Common
{
    /// <summary>
    /// 该类被服务框架通过反射调用
    /// </summary>
    public class ServiceCommonLoader : IBootLoader
    {
        public void ConfigureConfig(HostBuilderContext context, IConfigurationBuilder config)
        {
        }

        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<IInitializer, StartInit>()
                    .AddSingleton<ILogService>(_ => LogService.Instance)
                    .AddSingleton<IDialogService>(_ => DialogService.Instance)
                    .AddSingleton<IMessenger>(WeakReferenceMessenger.Default)
                    .AddAutoMapper(i => { i.AddProfile<ScanReconProfile>(); });
        }

        public void ConfigureContainer(HostBuilderContext context, ContainerBuilder container)
        {
            container.RegisterModule<DialogServiceModule>().IfNotRegistered(typeof(MPS.UI.Dialog.Service.IDialogService));
        }
    }
}