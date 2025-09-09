//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using Autofac;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using NV.CT.Protocol.Interfaces;
using NV.CT.Protocol.Services;

namespace NV.CT.PatientManagement.ApplicationService.Impl;

public class ApplicationServiceModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<StudyApplicationService>().As<IStudyApplicationService>().SingleInstance();
		builder.RegisterType<SeriesApplicationService>().As<ISeriesApplicationService>().SingleInstance();

		builder.RegisterType<ProtocolModificationService>().As<IProtocolModificationService>().SingleInstance();
	}
}