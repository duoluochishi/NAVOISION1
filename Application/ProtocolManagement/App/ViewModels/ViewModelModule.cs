using Autofac;

namespace NV.CT.ProtocolManagement.ViewModels
{
    public class ViewModelModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ProtocolNameEditViewModel>().SingleInstance();
            builder.RegisterType<MeasurementParameterControlViewModel>().SingleInstance();
            builder.RegisterType<ProtocolFilterControlViewModel>().SingleInstance();
            builder.RegisterType<ProtocolOperationControlViewModel>().SingleInstance();
            builder.RegisterType<ProtocolParameterControlViewModel>().SingleInstance();
            builder.RegisterType<ProtocolTreeControlViewModel>().SingleInstance();
            builder.RegisterType<ReconParameterControlViewModel>().SingleInstance();
            builder.RegisterType<ScanParameterControlViewModel>().SingleInstance();
            builder.RegisterType<SearchBoxViewModel>().SingleInstance();
            builder.RegisterType<MainControlViewModel>().SingleInstance();

        }
    }
}
