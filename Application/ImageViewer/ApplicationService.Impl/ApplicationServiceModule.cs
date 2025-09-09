using Autofac;

using NV.CT.ImageViewer.ApplicationService.Contract.Interfaces;

namespace NV.CT.ImageViewer.ApplicationService.Impl;

public class ApplicationServiceModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<ViewerService>().As<IViewerService>().SingleInstance();
    }
}