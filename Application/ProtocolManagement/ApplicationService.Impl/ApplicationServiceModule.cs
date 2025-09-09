using Autofac;
using NV.CT.ProtocolManagement.ApplicationService.Contract;

namespace NV.CT.ProtocolManagement.ApplicationService.Impl
{
    public class ApplicationServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ProtocolApplicationService>().As<IProtocolApplicationService>().SingleInstance();
        }
    }
}
