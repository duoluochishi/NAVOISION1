using Autofac;
using NV.CT.ServiceFrame.ViewModel;

namespace NV.CT.ServiceFrame.Extensions
{
    public static class ContainerBuilderExtension
    {
        public static void AddViewModelContainer(this ContainerBuilder builder)
        {
            builder.RegisterModule<ViewModelModule>();
        }
    }
}
