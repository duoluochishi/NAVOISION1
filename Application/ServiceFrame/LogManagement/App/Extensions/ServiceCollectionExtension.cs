using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using NV.CT.ClientProxy;
using NV.CT.LogManagement.ViewModel;

namespace NV.CT.LogManagement.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddUIUserConfigServices(this IServiceCollection services)
        {
            services.AddCommunicationClientServices();
            services.AddSingleton<IMapper, Mapper>();
            services.AddSingleton<LogViewerSearchCriteriaViewModel>();
            services.AddSingleton<LogViewerSearchResultViewModel>();
            services.AddSingleton<LogViewerOriginalLocatorViewModel>();
            return services;
        }
    }
}
