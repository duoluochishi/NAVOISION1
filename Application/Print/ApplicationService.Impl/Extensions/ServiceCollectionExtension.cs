//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

namespace NV.CT.Print.ApplicationService.Impl.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServiceMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(ToDomainProfile));
            return services;
        }
    }
}
