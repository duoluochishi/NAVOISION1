using Autofac;
using NV.CT.UI.Controls.Archive;
using NV.CT.UI.Controls.Controls;
using NV.CT.UI.Controls.Export;

namespace NV.CT.UI.Controls;
public class UIControlModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<CustomWWWLWindow>().SingleInstance();
		builder.RegisterType<ArchiveWindow>().SingleInstance();
		builder.RegisterType<ArchiveWindowViewModel>().SingleInstance();
		builder.RegisterType<ExportWindow>().SingleInstance();
		builder.RegisterType<ExportWindowViewModel>().SingleInstance();
		builder.RegisterType<AddEditDirectoryWindow>().SingleInstance();
		builder.RegisterType<AddEditDirectoryViewModel>().SingleInstance();

		builder.RegisterType<LoadingWindow>().SingleInstance();
	}
}