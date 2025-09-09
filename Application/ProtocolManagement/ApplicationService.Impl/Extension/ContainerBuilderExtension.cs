using Autofac;

namespace NV.CT.ProtocolManagement.ApplicationService.Impl.Extension
{
    public static class ContainerBuilderExtension
    {
        public static void AddApplicationServiceContainer(this ContainerBuilder builder)
        {
            builder.RegisterModule<ApplicationServiceModule>();
        }
    }
}
