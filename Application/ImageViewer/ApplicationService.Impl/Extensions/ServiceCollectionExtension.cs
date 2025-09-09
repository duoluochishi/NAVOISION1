using Microsoft.Extensions.DependencyInjection;

namespace NV.CT.ImageViewer.ApplicationService.Impl.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServiceMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(ToDomainProfile));
        return services;
    }
}