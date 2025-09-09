using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.HardwareTest.Attachments.Extensions;
using System;

namespace NV.CT.Service.HardwareTest.Initializer
{
    public static class ContainerInitializer
    {
        public static void ConfigureServices()
        {
            /** 注入Services **/
            Ioc.Default.ConfigureServices(Configure());
        }

        private static IServiceProvider Configure()
        {
            ServiceCollection services = new ServiceCollection();

            /** Models & Viewmodels **/
            services.AddModelToViewModelMappings();
            /** Services **/
            services.AddServices();
            /** Configuration **/
            services.AddConfiguration();

            return services.BuildServiceProvider();
        }
    }
}
