using Autofac;
using AutoMapper;

namespace NV.CT.LogManagement.ViewModel
{
    public class ViewModelModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Mapper>().As<IMapper>().SingleInstance();

            builder.RegisterType<LogViewerViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<LogViewerOriginalLocatorViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<LogViewerSearchCriteriaViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<LogViewerSearchResultViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<LogDetailsWindowViewModel>().AsSelf().SingleInstance(); 

        }
    }
}
