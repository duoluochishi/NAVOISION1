using Autofac;
using NV.CT.AppService.Contract;
using NV.CT.ClientProxy.Application;

namespace NV.CT.ServiceFrame.ViewModel;

public class ViewModelModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ApplicationCommunicationService>().As<IApplicationCommunicationService>().SingleInstance();
        builder.RegisterType<ItemsViewModel>().SingleInstance();
        builder.RegisterType<MainViewModel>().SingleInstance();
    }
}