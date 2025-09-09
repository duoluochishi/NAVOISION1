using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NV.CT.AppService.Contract;
using NV.CT.LogManagement.Extensions;
using NV.CT.LogManagement.ViewModel;
using NV.CT.ServiceFramework.Contract;
using NV.MPS.Environment;
using System.Collections.Generic;
using System.IO;

namespace NV.CT.LogManagement
{
    public class BootLoader : IBootLoader
    {
        public void ConfigureConfig(HostBuilderContext context, IConfigurationBuilder config)
        {
            //todo: 添加需要加载的配置
            config.AddJsonFile(Path.Combine(RuntimeConfig.Console.MCSConfig.Path, "Adjustment/LogManagement.json"), true, true);
        }

        public void ConfigureContainer(HostBuilderContext context, ContainerBuilder container)
        {
            //todo: 添加通过Autofac实现的依赖注入
            container.RegisterModule<ViewModelModule>();
        }

        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            //todo: 添加通过默认依赖注入，添加需要绑定的类型参数
            services.AddUIUserConfigServices();
            services.Configure<List<LogLevelSettings>>(context.Configuration.GetSection("LogLevelsConfig"));
            services.Configure<List<ModuleNameConfig>>(context.Configuration.GetSection("ModuleNamesConfig"));
        }
    }
}
