//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------


using NV.CT.DicomUtility.Extensions;

namespace NV.CT.Examination.ApplicationService.Impl.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.DicomUtilityConfigInitialization();
        services.DicomUtilityConfigInitializationForWin();
        services.AddAutoMapper(typeof(ToDomainProfile));
        services.AddHostedService<RTDReconHandler>();
        services.AddHostedService<ScanControlHandler>();
        services.AddHostedService<MeasurementHandler>();
        services.AddHostedService<OfflineReconHandler>();
        services.AddHostedService<AdjustTomoScanReconLengthByTopoDoneHandler>();
        services.AddHostedService<ProtocolAdjustmentOnFORChange>();
        return services;
    }
}