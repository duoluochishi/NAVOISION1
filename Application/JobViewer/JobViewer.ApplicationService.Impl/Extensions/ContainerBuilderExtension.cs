using Autofac;

using NV.CT.JobViewer.ApplicationService.Contract;

namespace NV.CT.JobViewer.ApplicationService.Impl.Extensions
{
    public static class ContainerBuilderExtension
    {
        public static void AddApplicationServiceContainer(this ContainerBuilder builder)
        {

            builder.RegisterType<OfflineReconTaskService>().As<IOfflineReconTaskService>().SingleInstance();

            builder.RegisterModule<ApplicationServiceModule>();
        }
    }
}
