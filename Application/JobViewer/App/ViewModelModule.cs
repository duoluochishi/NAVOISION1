using Autofac;
using AutoMapper;
using NV.CT.JobViewer.ViewModel;

namespace NV.CT.JobViewer;

public class ViewModelModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Mapper>().As<IMapper>().SingleInstance();
        builder.RegisterType<TaskViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<ImportTaskViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<ExportTaskViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<ArchiveTaskViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<PrintTaskViewModel>().AsSelf().SingleInstance();
        

    }
}
