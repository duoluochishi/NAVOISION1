namespace NV.CT.AuxConsole.ViewModel;

public class ViewModelModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<ConsoleApplicationService>().As<IConsoleApplicationService>().SingleInstance();

		builder.RegisterType<FrontendFixedItemsViewModel>().SingleInstance();
		builder.RegisterType<DynamicCardsViewModel>().SingleInstance();
		builder.RegisterType<BackendFixedItemsViewModel>().SingleInstance();
		builder.RegisterType<HeaderViewModel>().SingleInstance();
		//builder.RegisterType<ContextViewModel>().SingleInstance();
		builder.RegisterType<MainViewModel>().SingleInstance();
	}
}
