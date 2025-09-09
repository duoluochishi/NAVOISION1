using Microsoft.Extensions.DependencyInjection;
using NV.CT.ClientProxy;
using NV.CT.UserConfig.ViewModel;

namespace NV.CT.UserConfig.Extensions;
public static class ServiceCollectionExtension
{
    public static IServiceCollection AddUIUserConfigServices(this IServiceCollection services)
    {
        services.AddCommunicationClientServices();
        services.AddSingleton<ImageTextTypeListViewModel>();
        services.AddSingleton<ImageTextViewSetViewModel>();
        services.AddSingleton<ImageTextPreviewViewModel>();
        services.AddSingleton<ImageTextFontSetViewModel>();
        services.AddSingleton<ImageTextSettingViewModel>();
        services.AddSingleton<PatientSettingViewModel>();
        return services;
    }
}