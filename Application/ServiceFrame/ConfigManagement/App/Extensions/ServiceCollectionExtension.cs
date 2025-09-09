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

using NV.CT.ConfigManagement.ViewModel;
using NV.CT.ConfigManagement.View;
using NV.CT.ServiceFramework.Contract;
using NV.CT.ClientProxy.ConfigService;
using NV.CT.ConfigService.Contract;

namespace NV.CT.ConfigManagement.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        AddViews(services);
        AddViewModel(services);
        services.AddSingleton<IInitializer, Initializer>();
        services.AddSingleton<IErrorCodeService, ErrorCodeService>();
        return services;
    }

    private static void AddViews(IServiceCollection services)
    {
        services.AddSingleton<RoleWindow>();
        services.AddSingleton<UserWindow>();
        services.AddSingleton<VoiceWindow>();
        services.AddSingleton<WindowingWindow>();
        services.AddSingleton<WorklistNodeWindow>();
        services.AddSingleton<PrintNodeWindow>();
        services.AddSingleton<TabletWindow>();
        services.AddSingleton<PrintProtocolWindow>();
        services.AddSingleton<KvMaCoefficientWindow>();
        services.AddSingleton<FilmSettingsControl>();
    }

    private static void AddViewModel(IServiceCollection services)
    {
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<DoseSettingViewModel>();
        services.AddSingleton<ProductInfoViewModel>();
        services.AddSingleton<ArchiveNodeViewModel>();
        services.AddSingleton<PatientSettingViewModel>();
        services.AddSingleton<UserViewModel>();
        services.AddSingleton<UserListViewModel>();
        services.AddSingleton<RoleViewModel>();
        services.AddSingleton<RoleListViewModel>();
        services.AddSingleton<TabletViewModel>();
        services.AddSingleton<TabletListViewModel>();
        services.AddSingleton<VoiceViewModel>();
        services.AddSingleton<VoiceListViewModel>();
        services.AddSingleton<ArchiveNodeWindow>();
        services.AddSingleton<ArchiveNodesViewModel>();
        services.AddSingleton<WindowingViewModel>();
        services.AddSingleton<WindowingListViewModel>();
        services.AddSingleton<WorklistNodeViewModel>();
        services.AddSingleton<WorklistNodesViewModel>();
        services.AddSingleton<PrintNodeViewModel>();
        services.AddSingleton<PrintNodesViewModel>();
        services.AddSingleton<LocalDicomNodeViewModel>();
        services.AddSingleton<DiskSpaceManagementViewModel>();
        services.AddSingleton<CollimatorSettingViewModel>();
        services.AddSingleton<HospitalSettingViewModel>();
        services.AddSingleton<ErrorCodeListViewModel>();
        services.AddSingleton<PrintProtocolViewModel>();
        services.AddSingleton<PrintProtocolListViewModel>();
        services.AddSingleton<EquipmentViewModel>();
        services.AddSingleton<SourceComponentViewModel>();
        services.AddSingleton<TableViewModel>();
        services.AddSingleton<GantryViewModel>();
        services.AddSingleton<AcquisitionViewModel>();
        services.AddSingleton<DetectorViewModel>();
        services.AddSingleton<AutoPositioningViewModel>();
        services.AddSingleton<OfflineReconParamViewModel>();
        services.AddSingleton<RTDReconParamViewModel>();
        services.AddSingleton<ScanningParamViewModel>();
        services.AddSingleton<FilmSettingsViewModel>();
        services.AddSingleton<CoefficientsViewModel>();
        services.AddSingleton<CoefficientViewModel>();
        services.AddSingleton<LaserViewModel>();
        services.AddSingleton<LoginSettingViewModel>();
        services.AddSingleton<SystemSettingViewModel>();
    }
}