//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;
using NV.CT.Console.ApplicationService.Contract.Interfaces;

namespace NV.CT.Console.ApplicationService.Impl.Extensions;

public static class ContainerBuilderExtension
{
	public static void AddNanoStatusApplicationServices(this ContainerBuilder builder)
	{
		builder.RegisterType<ConsoleApplicationService>().As<IConsoleApplicationService>().SingleInstance();
		builder.RegisterType<LayoutManager>().As<ILayoutManager>().SingleInstance();
	}
}
