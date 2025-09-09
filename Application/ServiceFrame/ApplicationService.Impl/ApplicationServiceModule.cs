using Autofac;
using NV.CT.ServiceFrame.ApplicationService.Contract.Interfaces;

namespace NV.CT.ServiceFrame.ApplicationService.Impl;

public class ApplicationServiceModule: Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ServiceAppControlManager>().As<IServiceAppControlManager>().SingleInstance();
    }
}
