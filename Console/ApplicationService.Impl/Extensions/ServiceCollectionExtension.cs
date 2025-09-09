using Microsoft.Extensions.DependencyInjection;

namespace NV.CT.Console.ApplicationService.Impl.Extensions;

public static class ServiceCollectionExtension
{
	public static IServiceCollection AddApplicationMapper(this IServiceCollection services)
	{
		services.AddAutoMapper(typeof(ToDomainProfile));
		return services;
	}
}