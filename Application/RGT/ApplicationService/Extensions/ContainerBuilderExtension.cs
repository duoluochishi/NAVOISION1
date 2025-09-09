using Autofac;
using NV.CT.RGT.ApplicationService.Contract.Interfaces;

namespace NV.CT.RGT.ApplicationService.Impl.Extensions;

public static class ContainerBuilderExtension
{
    public static void AddRgtApplicationServiceContainer(this ContainerBuilder builder)
    {
        builder.RegisterType<LayoutManager>().As<ILayoutManager>().SingleInstance();
        builder.RegisterType<SelectionManager>().As<ISelectionManager>().SingleInstance();
        builder.RegisterType<StateService>().As<IStateService>().SingleInstance();
    }
}