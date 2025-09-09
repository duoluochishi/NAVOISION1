//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------


using NV.CT.DicomUtility.Extensions;
using NV.CT.Examination.ApplicationService.Impl;
using NV.CT.Examination.ApplicationService.Impl.ProtocolExtension;

namespace NV.CT.Recon.ApplicationService.Impl.Extensions;

public static class ServiceCollectionExtension
{
	public static IServiceCollection AddReconApplicationServices(this IServiceCollection services)
	{
		services.DicomUtilityConfigInitialization();
        services.DicomUtilityConfigInitializationForWin();
        services.AddAutoMapper(typeof(ToDomainProfile));

		//这三个不需要
		services.AddHostedService<RTDReconHandler>();
		services.AddHostedService<ScanControlHandler>();
		services.AddHostedService<MeasurementHandler>();

		services.AddHostedService<OfflineReconHandler>();
		services.AddHostedService<AdjustTomoScanReconLengthByTopoDoneHandler>();
		services.AddHostedService<ProtocolAdjustmentOnFORChange>();
		return services;
	}
}