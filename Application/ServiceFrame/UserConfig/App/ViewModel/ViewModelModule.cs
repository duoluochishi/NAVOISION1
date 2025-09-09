using Autofac;
using AutoMapper;
using NV.CT.ClientProxy.ConfigService;
using NV.CT.ConfigService.Contract;
using NV.CT.UserConfig.ApplicationService.Impl;

namespace NV.CT.UserConfig.ViewModel;
public class ViewModelModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<Mapper>().As<IMapper>().SingleInstance();
        builder.RegisterType<ImageAnnotationService>().As<IImageAnnotationService>().SingleInstance();
        builder.RegisterType<PatientConfigService>().As<IPatientConfigService>().SingleInstance();
        builder.RegisterType<ImageAnnotationService>().SingleInstance();
        builder.RegisterType<ImageTextSettingService>().SingleInstance();

        builder.RegisterType<ImageTextViewSetViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<ImageTextPreviewViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<ImageTextFontSetViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<ImageTextTypeListViewModel>().AsSelf().SingleInstance();
        builder.RegisterType<ImageTextSettingViewModel>().AsSelf().SingleInstance();
    }
}