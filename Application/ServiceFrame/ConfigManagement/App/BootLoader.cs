//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.ConfigManagement.Extensions;
using NV.CT.ServiceFramework.Contract;
using NV.CT.ConfigManagement.ApplicationService.Impl.Extensions;

namespace NV.CT.ConfigManagement;

public class BootLoader : IBootLoader
{
    public void ConfigureConfig(HostBuilderContext context, IConfigurationBuilder config)
    {
        //todo: 添加需要加载的配置
    }

    public void ConfigureContainer(HostBuilderContext context, ContainerBuilder container)
    {       
        container.AddServiceContainer();
    }

    public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {       
        services.AddAppServices();
    }
}