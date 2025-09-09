//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/10/17 8:38:34           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NV.CT.ServiceFramework.Contract;
using NV.CT.UserConfig.Extensions;
using NV.CT.UserConfig.ViewModel;
using NV.CT.SystemInterface.MRSIntegration.Impl.Extensions;

namespace NV.CT.UserConfig;

public class BootLoader : IBootLoader
{
    public void ConfigureConfig(HostBuilderContext context, IConfigurationBuilder config)
    {
        //todo: 添加需要加载的配置
    }

    public void ConfigureContainer(HostBuilderContext context, ContainerBuilder container)
    {
        //todo: 添加通过Autofac实现的依赖注入       
        container.AddRealtimeContainer();
        container.RegisterModule<ViewModelModule>();
    }

    public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        //todo: 添加通过默认依赖注入，添加需要绑定的类型参数
        services.AddMRSMapper();
        services.AddUIUserConfigServices();
    }
}