//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.AppService.Impl;

public class ApplicationModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<ScreenManagementService>().As<IScreenManagement>().SingleInstance();
		builder.RegisterType<ApplicationCommunicationService>().As<IApplicationCommunicationService>().SingleInstance();
		builder.RegisterType<ProcessService>().SingleInstance();
		builder.RegisterType<ShutdownService>().As<IShutdownService>().SingleInstance();

		builder.Register(c =>
		{
			var checkingService = new SelfCheckService();
			checkingService.AddSelfCheckingExecutor(new EmbeddedSystemSelfCheckingExecutor());
			return checkingService;
		}).As<ISelfCheckService>().SingleInstance();
	}
}
