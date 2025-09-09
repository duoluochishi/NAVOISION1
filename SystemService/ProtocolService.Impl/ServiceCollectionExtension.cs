//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/6 17:27:07     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using NV.CT.ProtocolService.Contract;

namespace NV.CT.ProtocolService.Impl;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddProtocolServices(this IServiceCollection services)
    {
        services.AddSingleton<ProtocolRepository>();
        services.AddSingleton<ProtocolService>();

        services.AddSingleton<IProtocolOperation, ProtocolOperationService>();

        return services;
    }
}
