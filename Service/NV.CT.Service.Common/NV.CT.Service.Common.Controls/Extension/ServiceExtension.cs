using Microsoft.Extensions.DependencyInjection;
using NV.CT.Service.Common.Controls.Integrations;
using NV.CT.Service.Common.Controls.Universal;
using NV.CT.Service.Common.Controls.ViewModels;
using NV.CT.Service.Common.Controls.ViewModels.Universal;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.Universal.PrintMessage;
using NV.CT.Service.Universal.PrintMessage.Abstractions;

namespace NV.CT.Service.Common.Controls.Extension
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddModelToViewModelMappings(this IServiceCollection services)
        {
            services.AddTransient<ImageViewerViewModel>();
            services.AddTransient<ImageSortView>();

            /** Common **/
            services.AddTransient<UniversalPopUpView>();
            services.AddTransient<UniversalPopUpViewModel>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            /** Log Services **/
            services.AddSingleton<ILogService>(LogService.Instance);
            services.AddTransient<IMessagePrintService, MessagePrintService>();

            return services;
        }

        public static IServiceCollection AddConfiguration(this IServiceCollection services)
        {
            return services;
        }
    }
}