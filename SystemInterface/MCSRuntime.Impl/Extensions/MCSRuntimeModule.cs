//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;
using NV.CT.SystemInterface.MCSRuntime.Contract;

namespace NV.CT.SystemInterface.MCSRuntime.Impl;

public class MCSRuntimeModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<DeviceService>().As<IDeviceService>().SingleInstance();
		builder.RegisterType<LogicalDiskService>().As<ILogicalDiskService>().SingleInstance();
		builder.RegisterType<USBService>().As<IUSBService>().SingleInstance();
		builder.RegisterType<CDROMService>().As<ICDROMService>().SingleInstance();
		builder.RegisterType<SpecialDiskService>().As<ISpecialDiskService>().SingleInstance();
		builder.RegisterType<ShutdownService>().As<IShutdownService>().SingleInstance();
	}
}
