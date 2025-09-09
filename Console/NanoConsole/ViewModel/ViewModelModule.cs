//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.NanoConsole.ViewModel;

public class ViewModelModule : Module
{

	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<LayoutManager>().SingleInstance();

		builder.RegisterType<ConsoleApplicationService>().As<IConsoleApplicationService>().SingleInstance();

		//注册所有viewmodel
		builder.RegisterType<CopyrightViewModel>().SingleInstance();
		builder.RegisterType<FrontendFixedItemsViewModel>().SingleInstance();
		builder.RegisterType<ScanListViewModel>().SingleInstance();
		builder.RegisterType<DynamicCardsViewModel>().SingleInstance();
		builder.RegisterType<BackendFixedItemsViewModel>().SingleInstance();	
		builder.RegisterType<TableControlViewModel>().SingleInstance();
		builder.RegisterType<GantryViewModel>().SingleInstance();
		builder.RegisterType<StatusViewModel>().SingleInstance();
		builder.RegisterType<TableMoveWindowViewModel>().SingleInstance();
		builder.RegisterType<SelfCheckWindowViewModel>().SingleInstance();
		builder.RegisterType<FooterTimeViewModel>().SingleInstance();
		builder.RegisterType<FooterViewModel>().SingleInstance();
		builder.RegisterType<SystemHomeViewModel>().SingleInstance();
		builder.RegisterType<SettingHomeViewModel>().SingleInstance();
		builder.RegisterType<LoginViewModel>().SingleInstance();
		builder.RegisterType<SelfCheckViewModel>().SingleInstance();
		builder.RegisterType<ShutdownViewModel>().SingleInstance();
		builder.RegisterType<MainViewModel>().SingleInstance();
		builder.RegisterType<CalibrationTemplateViewModel>().SingleInstance();
		builder.RegisterType<FirmwareVersionViewModel>().SingleInstance();
		builder.RegisterType<FileValidationViewModel>().SingleInstance();
		builder.RegisterType<LockViewModel>().SingleInstance();

		builder.RegisterType<MessagesWindow>().SingleInstance();
		builder.RegisterType<MessagesViewModel>().SingleInstance();

		builder.RegisterType<CredentialManagementViewModel>().SingleInstance();
		builder.RegisterType<CredentialManagementWindow>().SingleInstance();

	}
}