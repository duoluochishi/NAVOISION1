//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using NV.CT.SystemInterface.MRSIntegration.Impl.Profiles;

namespace NV.CT.SystemInterface.MRSIntegration.Impl.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddMRSMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ToInternalProfile));
        services.AddAutoMapper(typeof(ToProxyProfile));
        return services;
    }
}