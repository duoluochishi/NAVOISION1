//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Autofac;

namespace NV.CT.DatabaseService.Impl.Repository;
public class DatabaseRepositoryModule : Module
{
	protected override void Load(ContainerBuilder builder)
	{
		builder.RegisterType<DatabaseContext>().SingleInstance();

		builder.RegisterType<PatientRepository>().SingleInstance();
		builder.RegisterType<StudyRepository>().SingleInstance();
		builder.RegisterType<SeriesRepository>().SingleInstance();
		builder.RegisterType<ReconTaskRepository>().SingleInstance();
		builder.RegisterType<ScanTaskRepository>().SingleInstance();
		builder.RegisterType<JobTaskRepository>().SingleInstance();
		builder.RegisterType<DoseCheckRepository>().SingleInstance();
		builder.RegisterType<VoiceRepository>().SingleInstance();
		builder.RegisterType<RoleRepository>().SingleInstance();
		builder.RegisterType<UserRepository>().SingleInstance();
		builder.RegisterType<LoginHistoryRepository>().SingleInstance();
		builder.RegisterType<PermissionRepository>().SingleInstance();
		builder.RegisterType<RawDataRepository>().SingleInstance();
	}
}