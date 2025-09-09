//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:28    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;

namespace NV.CT.PatientManagement.ApplicationService.Impl.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServiceMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ToDomainProfile));
        return services;
    }
}