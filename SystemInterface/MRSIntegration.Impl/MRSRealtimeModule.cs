//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/8 17:31:16    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Autofac;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class MRSRealtimeModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<RealtimeProxyService>().As<IRealtimeProxyService>().SingleInstance();
		builder.RegisterType<RealtimeConnectionService>().As<IRealtimeConnectionService>().SingleInstance();
		builder.RegisterType<RealtimeStatusProxyService>().As<IRealtimeStatusProxyService>().SingleInstance();
		builder.RegisterType<RealtimeReconProxyService>().As<IRealtimeReconProxyService>().SingleInstance();
		builder.RegisterType<RealtimeVoiceService>().As<IRealtimeVoiceService>().SingleInstance();

		builder.RegisterType<TablePositionService>().As<ITablePositionService>().SingleInstance();
		builder.RegisterType<HeatCapacityService>().As<IHeatCapacityService>().SingleInstance();
		builder.RegisterType<DoseEstimateService>().As<IDoseEstimateService>().SingleInstance();

		builder.RegisterType<DoorStatusService>().As<IDoorStatusService>().SingleInstance();
		builder.RegisterType<FrontRearCoverStatusService>().As<IFrontRearCoverStatusService>().SingleInstance();
		builder.RegisterType<ControlBoxStatusService>().As<IControlBoxStatusService>().SingleInstance();
		builder.RegisterType<CTBoxStatusService>().As<ICTBoxStatusService>().SingleInstance();

		builder.RegisterType<DetectorTemperatureService>().As<IDetectorTemperatureService>().SingleInstance();

		builder.RegisterType<ShutdownProxyService>().As<IShutdownProxyService>().SingleInstance();
		builder.RegisterType<SelfCheckingProxyService>().As<ISelfCheckingProxyService>().SingleInstance();

		builder.RegisterType<ComponentStatusProxyService>().As<IComponentStatusProxyService>().SingleInstance();

		base.Load(builder);
	}
}
