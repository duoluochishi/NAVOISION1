using Microsoft.Extensions.DependencyInjection;
using NV.CT.MessageService.Contract;

namespace NV.CT.MessageService.Impl;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddMessageServices(this IServiceCollection services)
    {
        services.AddSingleton<IMessageService, MessageService>();
        return services;
    }
}
