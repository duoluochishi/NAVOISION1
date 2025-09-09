using Autofac;

namespace NV.CT.ServiceFrame.ApplicationService.Impl.Extensions;

public static class ContainerBuilderExtension
{
    public static void AddApplicationServiceContainer(this ContainerBuilder builder)
    {
        builder.RegisterModule<ApplicationServiceModule>();
    }
}
