//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Autofac;
using NV.CT.ConfigManagement.ApplicationService.Contract;

namespace NV.CT.ConfigManagement.ApplicationService.Impl.Extensions;
public static class ContainerBuilderExtension
{
    public static void AddServiceContainer(this ContainerBuilder builder)
    {       
        builder.RegisterType<UserApplicationService>().As<IUserApplicationService>().SingleInstance();
        builder.RegisterType<TabletApplicationService>().As<ITabletApplicationService>().SingleInstance();
        builder.RegisterType<VoiceApplicationService>().As<IVoiceApplicationService>().SingleInstance();
        builder.RegisterType<ArchiveNodeApplicationService>().As<IArchiveNodeApplicationService>().SingleInstance();
        builder.RegisterType<WindowingApplicationService>().As<IWindowingApplicationService>().SingleInstance();
        builder.RegisterType<WorklistNodeApplicationService>().As<IWorklistNodeApplicationService>().SingleInstance();
        builder.RegisterType<PrintNodeApplicationService>().As<IPrintNodeApplicationService>().SingleInstance();
        builder.RegisterType<PrintProtocolApplicationService>().As<IPrintProtocolApplicationService>().SingleInstance();
        builder.RegisterType<KvMaCoefficientApplicationService>().As<IKvMaCoefficientApplicationService>().SingleInstance();
        builder.RegisterType<FilmSettingsApplicationService>().As<IFilmSettingsApplicationService>().SingleInstance();
    }
}