using Autofac;

namespace NV.CT.ImageViewer.ApplicationService.Impl.Extensions;

public static class ContainerBuilderExtension
{
    public static void AddApplicationServiceContainer(this ContainerBuilder builder)
    {
        builder.RegisterModule<ApplicationServiceModule>();
    }
}