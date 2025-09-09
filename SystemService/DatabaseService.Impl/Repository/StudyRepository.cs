//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Dapper;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract.Entities;
using Org.BouncyCastle.Crypto;
using System.Text;
using static Dapper.SqlMapper;

namespace NV.CT.DatabaseService.Impl.Repository
{
	public class StudyRepository //: IStudyRepository
	{
		private readonly DatabaseContext _context;
		private readonly ILogger<StudyRepository> _logger;
		private readonly PatientRepository _patientRepository;
		private readonly SeriesRepository _seriesRepository;

		public StudyRepository(DatabaseContext context,
							   ILogger<StudyRepository> logger,
							   PatientRepository patientRepository,
							   SeriesRepository seriesRepository)
		{
			_context = context;
			_logger = logger;
			_patientRepository = patientRepository;
			_seriesRepository = seriesRepository;
		}

		public bool Insert(StudyEntity entity)
		{
			//_logger.LogInformation("Insert study");
			string sql = "insert into t_study (Id,InternalPatientId,Age,AgeType,PatientSize,StudyInstanceUID,PatientWeight,StudyDescription,AdmittingDiagnosisDescription,Ward,ReferringPhysicianName,BodyPart,AccessionNo,StudyId,Comments,InstitutionName,InstitutionAddress,StudyStatus,PatientType,StudyTime,RegistrationDate,ExamStartTime,ExamEndTime,Technician,Protocol,CreateTime,Creator,UpdateTime,Updater,PrintConfigPath,IsLocalModified) " +
						 " value (@Id,@InternalPatientId,@Age,@AgeType,@PatientSize,@StudyInstanceUID,@PatientWeight,@StudyDescription,@AdmittingDiagnosisDescription,@Ward,@ReferringPhysicianName,@BodyPart,@AccessionNo,@StudyId,@Comments,@InstitutionName,@InstitutionAddress,@StudyStatus,@PatientType,@StudyTime,@RegistrationDate,@ExamStartTime,@ExamEndTime,@Technician,@Protocol,@CreateTime,@Creator,@UpdateTime,@Updater,@PrintConfigPath,@IsLocalModified);";
			var result = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Execute(sql, new
				{
					Id = entity.Id,
					InternalPatientId = entity.InternalPatientId,
					Age = entity.Age,
					AgeType = entity.AgeType,
					PatientSize = entity.PatientSize,
					PatientWeight = entity.PatientWeight,
					StudyInstanceUID = entity.StudyInstanceUID,
					AdmittingDiagnosisDescription = entity.AdmittingDiagnosisDescription,
					Ward = entity.Ward,
					ReferringPhysicianName = entity.ReferringPhysicianName,
					BodyPart = entity.BodyPart,
					AccessionNo = entity.AccessionNo,
					StudyId = entity.StudyId,
					Comments = entity.Comments,
					InstitutionName = entity.InstitutionName,
					InstitutionAddress = entity.InstitutionAddress,
					StudyStatus = entity.StudyStatus,
					PatientType = entity.PatientType,
					StudyDescription = entity.StudyDescription,
					StudyTime = entity.StudyTime,
					RegistrationDate = entity.RegistrationDate,
					ExamStartTime = entity.ExamStartTime,
					ExamEndTime = entity.ExamEndTime,
					Technician = entity.Technician,
					Protocol = entity.Protocol,
					CreateTime = entity.CreateTime,
					Creator = entity.Creator,
					UpdateTime = entity.UpdateTime,
					Updater = entity.Updater,
					IsLocalModified = entity.IsLocalModified,
					PrintConfigPath = entity.PrintConfigPath,
				});
			}, _logger);
			return result == 1;
		}
		public bool Update(StudyEntity entity)
		{
			//_logger.LogInformation("Update study");
			string sql = "UPDATE t_study t SET t.Age=@Age,AgeType=@AgeType,PatientSize=@PatientSize,PatientWeight=@PatientWeight,StudyDescription=@StudyDescription,AdmittingDiagnosisDescription=@AdmittingDiagnosisDescription," +
						 " Ward=@Ward,ReferringPhysicianName=@ReferringPhysicianName,BodyPart=@BodyPart,AccessionNo=@AccessionNo,StudyId=@StudyId,Comments=@Comments,InstitutionName=@InstitutionName," +
						 " InstitutionAddress=@InstitutionAddress,StudyStatus=@StudyStatus,PatientType=@PatientType,StudyTime=@StudyTime," +
						 " ExamStartTime=@ExamStartTime,ExamEndTime=@ExamEndTime,RegistrationDate=@RegistrationDate,Technician=@Technician,Protocol=@Protocol,CreateTime=@CreateTime,Creator=@Creator,UpdateTime=@UpdateTime,Updater=@Updater,PrintConfigPath=@PrintConfigPath,IsLocalModified=@IsLocalModified WHERE t.Id=@Id";
			var result = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Execute(sql, new
				{
					Id = entity.Id,
					Age = entity.Age,
					AgeType = entity.AgeType,
					PatientSize = entity.PatientSize,
					PatientWeight = entity.PatientWeight,
					AdmittingDiagnosisDescription = entity.AdmittingDiagnosisDescription,
					Ward = entity.Ward,
					ReferringPhysicianName = entity.ReferringPhysicianName,
					BodyPart = entity.BodyPart,
					AccessionNo = entity.AccessionNo,
					StudyId = entity.StudyId,
					Comments = entity.Comments,
					InstitutionName = entity.InstitutionName,
					InstitutionAddress = entity.InstitutionAddress,
					StudyStatus = entity.StudyStatus,
					PatientType = entity.PatientType,
					StudyDescription = entity.StudyDescription,
					StudyTime = entity.StudyTime,
					ExamStartTime = entity.ExamStartTime,
					ExamEndTime = entity.ExamEndTime,
					RegistrationDate = entity.RegistrationDate,
					Technician = entity.Technician,
					Protocol = entity.Protocol,
					CreateTime = entity.CreateTime,
					Creator = entity.Creator,
					UpdateTime = entity.UpdateTime,
					Updater = entity.Updater,
					PrintConfigPath = entity.PrintConfigPath,
					IsLocalModified = true,
				});
			}, _logger);
			return result == 1;
		}

		public bool Delete(StudyEntity entity)
		{
			//考虑Resume exam场景时存在1个Patient对应多个Study的情况
			string sql = "SELECT COUNT(id) from t_study WHERE InternalPatientId = @InternalPatientId";
			var countOfPatientReference = _context.Connection.ContextExecute((connection) =>
			{
				var result = connection.QuerySingle<int>(sql, new { InternalPatientId = entity.InternalPatientId });
				return result;
			});

			//_logger.LogInformation("Delete study");
			return _context.Connection.ContextExecute((connection, transaction) =>
			{
				StringBuilder sqlBuilder = new StringBuilder();
				if (countOfPatientReference == 1)
				{
					sqlBuilder.Append("DELETE FROM t_patient WHERE id = (SELECT t.InternalPatientId FROM t_study t WHERE t.ID = @Id);");
				}
				sqlBuilder.Append("DELETE FROM t_series WHERE InternalStudyId = @Id;");
				sqlBuilder.Append("DELETE FROM t_study WHERE Id = @Id;");

				connection.Execute(sqlBuilder.ToString(), new
				{
					Id = entity.Id
				}, transaction);
				return true;
			});
		}

		public bool DeleteByGuid(StudyEntity entity)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("delete from t_study_correction where InternalStudyId = @StudyGuid;");
			sb.AppendLine("delete from t_study where Id = @StudyGuid");
			_context.Connection.ContextExecute((connection) => {
				var effectRows = connection.Execute(sb.ToString(), new {
					StudyGuid = entity.Id
				});
				return effectRows > 0;
			}, _logger);
			return true;
		}

		public (PatientEntity Patient, StudyEntity Study) Get(string studyId)
		{
			string sql = "select p.Id,p.PatientName,p.PatientId,p.PatientSex,p.CreateTime,p.PatientBirthDate,t.Age,t.InternalPatientId,t.Id,t.PatientWeight,t.PatientType,t.PatientSize,t.AgeType,t.StudyInstanceUID,t.StudyTime,t.ExamStartTime,t.ExamEndTime,t.StudyDescription,t.AdmittingDiagnosisDescription,t.Technician,t.Ward,t.ReferringPhysicianName,t.BodyPart,t.AccessionNo,t.StudyId,t.Comments,t.InstitutionName,t.InstitutionAddress,t.StudyStatus,t.Protocol,t.IsProtected,t.ArchiveStatus,t.PrintStatus,t.CorrectStatus,t.CreateTime,t.Creator,t.UpdateTime,t.Updater,t.PrintConfigPath,t.IsLocalModified from t_study t join t_patient p on t.InternalPatientId=p.ID where t.Id = @StudyId";
			var entity = _context.Connection.ContextExecute((connection) =>
			{
				var entities = connection.Query<PatientEntity, StudyEntity, (PatientEntity, StudyEntity)>(sql,
							   (patientEntity, studyEntity) => { return (patientEntity, studyEntity); }, new { StudyId = studyId }, splitOn: "Age");
				return entities.FirstOrDefault();
			}, _logger);
			return entity;
		}
		public StudyEntity GetStudyById(string studyId)
		{
			string sql = "SELECT * FROM t_study t  where t.Id = @StudyId";
			var entity = _context.Connection.ContextExecute((connection) =>
			{
				var entities = connection.Query<StudyEntity>(sql, new { StudyId = studyId });
				return entities.FirstOrDefault();
			});
			return entity ?? new StudyEntity();
		}

		public (PatientEntity Patient, StudyEntity Study) GetWithUID(string studyInstanceUID)
		{
			string sql = "select p.Id,p.PatientName,p.PatientId,p.PatientSex,p.CreateTime,p.PatientBirthDate,t.Age,t.PatientType,t.InternalPatientId,t.Id,t.PatientWeight,t.PatientSize,t.AgeType,t.StudyInstanceUID,t.StudyTime,t.StudyDescription,t.AdmittingDiagnosisDescription,t.Technician,t.Ward,t.ReferringPhysicianName,t.BodyPart,t.AccessionNo,t.StudyId,t.Comments,t.InstitutionName,t.InstitutionAddress,t.StudyStatus,t.Protocol,t.ExamStartTime,t.ExamEndTime,t.ArchiveStatus,t.PrintStatus,t.CorrectStatus,t.CreateTime,t.Creator,t.UpdateTime,t.Updater,t.PrintConfigPath,t.IsLocalModified from t_study t join t_patient p on t.InternalPatientId=p.ID where t.StudyInstanceUID = @StudyInstanceUID";
			var entity = _context.Connection.ContextExecute((connection) =>
			{
				var entities = connection.Query<PatientEntity, StudyEntity, (PatientEntity, StudyEntity)>(sql, (patientEntity, studyEntity) => { return (patientEntity, studyEntity); }, new { StudyInstanceUID = studyInstanceUID }, splitOn: "Age");
				return entities.FirstOrDefault();
			}, _logger);
			return entity;
		}

		public StudyEntity? GetStudyByUId(string studyInstanceUID)
		{
			string sql = "SELECT * FROM t_study t  where t.StudyInstanceUID = @StudyInstanceUID";
			var entity = _context.Connection.ContextExecute((connection) =>
			{
				var entities = connection.Query<StudyEntity>(sql, new { StudyInstanceUID = studyInstanceUID });
				return entities.FirstOrDefault();
			});
			return entity;
		}

		public List<(PatientEntity, StudyEntity)> GetPatientStudyListByStatus(string[] statusList)
		{
			string sql = "select p.Id,p.PatientName,p.PatientId,p.PatientSex,p.CreateTime,p.PatientBirthDate,t.Age,t.PatientType,t.InternalPatientId,t.Id,t.PatientWeight,t.PatientSize,t.AgeType,t.StudyInstanceUID,t.StudyTime,t.StudyDescription,t.AdmittingDiagnosisDescription,t.Technician,t.Ward,t.ReferringPhysicianName,t.BodyPart,t.AccessionNo,t.StudyId,t.Comments,t.InstitutionName,t.InstitutionAddress,t.StudyStatus,t.IsProtected,t.ArchiveStatus,t.PrintStatus,t.CorrectStatus,t.ExamStartTime,t.ExamEndTime,t.CreateTime,t.Creator,t.UpdateTime,t.Updater,t.PrintConfigPath,t.IsLocalModified from t_study t join t_patient p on t.InternalPatientId=p.ID WHERE t.IsDeleted = 0 And t.StudyStatus in @StudyStatusList ORDER BY t.StudyTime DESC";
			var entities = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Query<PatientEntity, StudyEntity, (PatientEntity, StudyEntity)>
										(sql, (patientEntity, studyEntity) => { return (patientEntity, studyEntity); }, new { StudyStatusList = statusList }, splitOn: "Age");
			}, _logger);
			return entities.ToList();
		}

		public List<(PatientEntity, StudyEntity)> GetPatientStudyListWithEndStudyDate(string startDate, string endDate)
		{
			string sql = "select p.Id,p.PatientName,p.PatientId,p.PatientSex,p.CreateTime,p.PatientBirthDate,t.Age,t.PatientType,t.InternalPatientId,t.Id,t.PatientWeight,t.PatientSize,t.AgeType,t.StudyInstanceUID,t.StudyTime,t.StudyDescription,t.AdmittingDiagnosisDescription,t.Technician,t.Ward,t.ReferringPhysicianName,t.BodyPart,t.AccessionNo,t.StudyId,t.Comments,t.InstitutionName,t.InstitutionAddress,t.StudyStatus,t.IsProtected,t.ExamStartTime,t.ExamEndTime,t.ArchiveStatus,t.PrintStatus,t.CorrectStatus,t.CreateTime,t.Creator,t.UpdateTime,t.Updater,t.PrintConfigPath,t.IsLocalModified from t_study t join t_patient p on t.InternalPatientId=p.ID WHERE (t.StudyStatus='ExaminationClosed' Or t.StudyStatus='Examinating' Or t.StudyStatus='ExaminationStarting' Or t.StudyStatus='ExaminationDiscontinue') ";
			if (!string.IsNullOrEmpty(startDate))
			{
				sql += $" and (t.StudyTime is NULL || t.StudyTime>='{startDate}')";
			}
			if (!string.IsNullOrEmpty(endDate))
			{
				sql += $" and (t.StudyTime is NULL || t.StudyTime <='{endDate}')";
			}
			sql += " ORDER BY t.StudyTime DESC";
			var entities = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Query<PatientEntity, StudyEntity, (PatientEntity, StudyEntity)>(sql, (patientEntity, studyEntity) => { return (patientEntity, studyEntity); }, splitOn: "Age");
			}, _logger);
			return entities.ToList();
		}

		public List<(PatientEntity, StudyEntity)> GetPatientStudyListWithNotStartedStudyDate(string startDate, string endDate)
		{
			string sql = "select p.Id,p.PatientName,p.PatientId,p.PatientSex,p.CreateTime,p.PatientBirthDate,t.Age,t.PatientType,t.InternalPatientId,t.Id,t.PatientWeight,t.PatientSize,t.StudyInstanceUID,t.AgeType,t.StudyTime,t.StudyDescription,t.AdmittingDiagnosisDescription,t.Technician,t.Ward,t.ReferringPhysicianName,t.BodyPart,t.AccessionNo,t.StudyId,t.Comments,t.InstitutionName,t.ArchiveStatus,t.PrintStatus,t.CorrectStatus,t.InstitutionAddress,t.ExamStartTime,t.ExamEndTime,t.CreateTime,t.Creator,t.UpdateTime,t.Updater,t.PrintConfigPath,t.IsLocalModified from t_study t join t_patient p on t.InternalPatientId=p.ID WHERE t.IsDeleted=0 and t.StudyStatus='NotStarted'";
			if (!string.IsNullOrEmpty(startDate))
			{
				sql += $" and (p.CreateTime>='{startDate}')";
			}
			if (!string.IsNullOrEmpty(endDate))
			{
				sql += $" and (p.CreateTime <='{endDate}')";
			}
			sql += " ORDER BY p.CreateTime ASC";
			var entities = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Query<PatientEntity, StudyEntity, (PatientEntity, StudyEntity)>(sql, (patientEntity, studyEntity) => { return (patientEntity, studyEntity); }, splitOn: "Age");
			}, _logger);
			return entities.ToList();
		}

		public List<(PatientEntity, StudyEntity)> GetPatientStudyListByFilter(StudyListFilterModel filter)
		{
			var sqlWithFilter = this.BuildStudyListQuerySQL(filter);
			var entities = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Query<PatientEntity, StudyEntity, (PatientEntity, StudyEntity)>(sqlWithFilter, (patientEntity, studyEntity) => { return (patientEntity, studyEntity); }, splitOn: "Age");
			}, _logger);
			return entities.ToList();
		}

		public List<(PatientEntity, StudyEntity)> GetPatientStudyListByFilterSimple(StudyQueryModel queryModel)
		{
			var sqlWithFilter = BuildStudyListQuerySql(queryModel);
			var entities = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Query<PatientEntity, StudyEntity, (PatientEntity, StudyEntity)>(sqlWithFilter, (patientEntity, studyEntity) => { return (patientEntity, studyEntity); }, splitOn: "Age");
			}, _logger);
			return entities.ToList();
		}

		private string BuildStudyListQuerySql(StudyQueryModel queryModel)
		{
			var sqlBuilder = new StringBuilder();
			sqlBuilder.Append(" SELECT p.Id,p.PatientName,p.PatientId,p.PatientSex,p.CreateTime,p.PatientBirthDate,t.Age,t.PatientType,t.InternalPatientId,t.Id,t.PatientWeight,t.PatientSize,t.AgeType,t.StudyInstanceUID,t.StudyTime,t.StudyDescription,t.AdmittingDiagnosisDescription,t.Technician,t.Ward,t.ReferringPhysicianName,t.BodyPart,t.AccessionNo,t.StudyId,t.Comments,t.InstitutionName,t.InstitutionAddress,t.StudyStatus,t.IsProtected,t.ExamStartTime,t.ExamEndTime,t.ArchiveStatus,t.PrintStatus,t.CorrectStatus,t.CreateTime,t.Creator,t.UpdateTime,t.Updater,t.PrintConfigPath,t.IsLocalModified ");
			sqlBuilder.Append(" FROM t_study t JOIN t_patient p ON t.InternalPatientId = p.ID WHERE t.IsDeleted = 0 ");

			//Apply Date range filter
			if (queryModel.StartDate.HasValue && queryModel.StartDate.Value != DateTime.MinValue)
			{
				sqlBuilder.Append($" AND (t.StudyTime is NULL || t.StudyTime>='{queryModel.StartDate.Value.ToString("yyyy-MM-dd")} 00:00:00') ");
			}
			if (queryModel.EndDate.HasValue && queryModel.EndDate.Value != DateTime.MinValue)
			{
				sqlBuilder.Append($" AND (t.StudyTime is NULL || t.StudyTime<'{queryModel.EndDate.Value.AddDays(1).ToString("yyyy-MM-dd")} 00:00:00') ");
			}

			//PatientId
			if (!string.IsNullOrEmpty(queryModel.PatientId.Trim()))
			{
				sqlBuilder.Append($" AND (p.PatientId like '%{queryModel.PatientId.Trim()}%') ");
			}

			//BodyPart
			if (!string.IsNullOrEmpty(queryModel.BodyPart.Trim()))
			{
				sqlBuilder.Append($" AND (t.BodyPart='{queryModel.BodyPart.Trim()}') ");
			}

			//Apply Sort
			string sortedColumnName = "t.CreateTime";
			string sortDirection = "asc";
			sqlBuilder.Append($" ORDER BY {sortedColumnName} {sortDirection}");

			return sqlBuilder.ToString();
		}

		private string BuildStudyListQuerySQL(StudyListFilterModel filter)
		{
			var sqlBuilder = new StringBuilder();
			sqlBuilder.Append(" SELECT p.Id,p.PatientName,p.PatientId,p.PatientSex,p.CreateTime,p.PatientBirthDate,t.Age,t.PatientType,t.InternalPatientId,t.Id,t.PatientWeight,t.PatientSize,t.AgeType,t.StudyInstanceUID,t.StudyTime,t.StudyDescription,t.AdmittingDiagnosisDescription,t.Technician,t.Ward,t.ReferringPhysicianName,t.BodyPart,t.AccessionNo,t.StudyId,t.Comments,t.InstitutionName,t.InstitutionAddress,t.StudyStatus,t.IsProtected,t.ExamStartTime,t.ExamEndTime,t.ArchiveStatus,t.PrintStatus,t.CorrectStatus,t.CreateTime,t.Creator,t.UpdateTime,t.Updater,t.PrintConfigPath,t.IsLocalModified ");
			sqlBuilder.Append(" FROM t_study t JOIN t_patient p ON t.InternalPatientId = p.ID WHERE t.IsDeleted = 0 ");

			//Apply Date range filter
			if (filter.DateRangeBeginDate.HasValue && filter.DateRangeBeginDate.Value != DateTime.MinValue)
			{
				sqlBuilder.Append($" AND (t.StudyTime is NULL || t.StudyTime>='{filter.DateRangeBeginDate.Value.ToString("yyyy-MM-dd")} 00:00:00') ");
			}
			if (filter.DateRangeEndDate.HasValue && filter.DateRangeEndDate.Value != DateTime.MinValue)
			{
				sqlBuilder.Append($" AND (t.StudyTime is NULL || t.StudyTime<'{filter.DateRangeEndDate.Value.AddDays(1).ToString("yyyy-MM-dd")} 00:00:00') ");
			}

			//Apply StudyStatus
			if (!filter.IsInProgressCheckedOfStudyStatus || !filter.IsFinishedCheckedOfStudyStatus || !filter.IsAbnormalCheckedOfStudyStatus)
			{
				var studyStatusList = new List<WorkflowStatus>();
				if (filter.IsInProgressCheckedOfStudyStatus)
				{
					studyStatusList.Add(WorkflowStatus.Examinating);
					studyStatusList.Add(WorkflowStatus.ExaminationStarting);
				}
				if (filter.IsFinishedCheckedOfStudyStatus)
				{
					studyStatusList.Add(WorkflowStatus.ExaminationClosing);
					studyStatusList.Add(WorkflowStatus.ExaminationClosed);
				}
				if (filter.IsAbnormalCheckedOfStudyStatus)
				{
					studyStatusList.Add(WorkflowStatus.ExaminationDiscontinue);
				}

				string statusFilter = string.Join(",", studyStatusList.Select(s => $"'{s.ToString()}'").ToArray());
				sqlBuilder.Append($" AND (t.StudyStatus in ({statusFilter})) ");
			}
			else
			{
				sqlBuilder.Append($" AND (t.StudyStatus <> 'NotStarted') ");
			}

			//Apply PrintStatus
			if (!filter.IsNotYetCheckedOfPrintStatus || !filter.IsFinishedCheckedOfPrintStatus || !filter.IsFailedCheckedOfPrintStatus)
			{
				var printStatusList = new List<JobTaskStatus>();
				if (filter.IsNotYetCheckedOfPrintStatus)
				{
					printStatusList.Add(JobTaskStatus.Queued);
					printStatusList.Add(JobTaskStatus.Processing);
					printStatusList.Add(JobTaskStatus.Paused);
				}
				if (filter.IsFinishedCheckedOfPrintStatus)
				{
					printStatusList.Add(JobTaskStatus.Completed);
					printStatusList.Add(JobTaskStatus.PartlyCompleted);
				}
				if (filter.IsFailedCheckedOfPrintStatus)
				{
					printStatusList.Add(JobTaskStatus.Failed);
					printStatusList.Add(JobTaskStatus.Cancelled);
				}

				string statusFilter = string.Join(",", printStatusList.Select(s => $"'{(int)s}'").ToArray());
				sqlBuilder.Append($" AND (t.PrintStatus in ({statusFilter})) ");
			}

			//Apply ArchiveStatus
			if (!filter.IsNotYetCheckedOfArchiveStatus || !filter.IsPartlyFinishedCheckedOfArchiveStatus || !filter.IsFinishedCheckedOfArchiveStatus || !filter.IsFailedCheckedOfArchiveStatus)
			{
				var archiveStatusList = new List<JobTaskStatus>();
				if (filter.IsNotYetCheckedOfArchiveStatus)
				{
					archiveStatusList.Add(JobTaskStatus.Queued);
					archiveStatusList.Add(JobTaskStatus.Processing);
					archiveStatusList.Add(JobTaskStatus.Paused);
				}
				if (filter.IsPartlyFinishedCheckedOfArchiveStatus)
				{
					archiveStatusList.Add(JobTaskStatus.PartlyCompleted);
				}
				if (filter.IsFinishedCheckedOfArchiveStatus)
				{
					archiveStatusList.Add(JobTaskStatus.Completed);
				}
				if (filter.IsFailedCheckedOfArchiveStatus)
				{
					archiveStatusList.Add(JobTaskStatus.Failed);
					archiveStatusList.Add(JobTaskStatus.Cancelled);
				}

				string statusFilter = string.Join(",", archiveStatusList.Select(s => $"'{s.ToString()}'").ToArray());
				sqlBuilder.Append($" AND (t.ArchiveStatus in ({statusFilter})) ");
			}

			//Apply CorrectionStatus
			if (!filter.IsCorrectedChecked || !filter.IsUncorrectedChecked)
			{
				if (filter.IsCorrectedChecked)
				{
					sqlBuilder.Append($" AND (t.CorrectStatus = '1') ");
				}

				if (filter.IsUncorrectedChecked)
				{
					sqlBuilder.Append($" AND (t.CorrectStatus is null OR t.CorrectStatus = '0') ");
				}
			}

			//Apply LockStatus
			if (!filter.IsLockedChecked || !filter.IsUnlockedChecked)
			{
				if (filter.IsLockedChecked)
				{
					sqlBuilder.Append($" AND (t.IsProtected = '1') ");
				}

				if (filter.IsUnlockedChecked)
				{
					sqlBuilder.Append($" AND (t.IsProtected is null OR t.IsProtected = '0') ");
				}
			}

			//Apply Sex
			if (!filter.IsMaleChecked || !filter.IsFemaleChecked || !filter.IsOtherChecked)
			{
				var sexList = new List<Gender>();
				if (filter.IsMaleChecked)
				{
					sexList.Add(Gender.Male);
				}
				if (filter.IsFemaleChecked)
				{
					sexList.Add(Gender.Female);
				}
				if (filter.IsOtherChecked)
				{
					sexList.Add(Gender.Other);
				}

				string sexFilter = string.Join(",", sexList.Select(s => $"'{(int)s}'").ToArray());
				sqlBuilder.Append($" AND (p.PatientSex in ({sexFilter})) ");
			}

			//Apply PatientType
			if (!filter.IsLocalChecked || !filter.IsPreRegChecked || !filter.IsEmergencyChecked)
			{
				var sourceList = new List<PatientType>();
				if (filter.IsLocalChecked)
				{
					sourceList.Add(PatientType.Local);
				}
				if (filter.IsPreRegChecked)
				{
					sourceList.Add(PatientType.PreRegistration);
				}
				if (filter.IsEmergencyChecked)
				{
					sourceList.Add(PatientType.Emergency);
				}

				string sourceFilter = string.Join(",", sourceList.Select(s => $"'{(int)s}'").ToArray());
				sqlBuilder.Append($" AND (t.PatientType in ({sourceFilter})) ");
			}

			//Apply BirthdayDateRange
			if (filter.BirthdayRangeBeginDate.HasValue && filter.BirthdayRangeBeginDate.Value != DateTime.MinValue)
			{
				sqlBuilder.Append($" AND (p.PatientBirthDate is NULL || p.PatientBirthDate>='{filter.BirthdayRangeBeginDate.Value.ToString("yyyy-MM-dd")} 00:00:00') ");
			}
			if (filter.BirthdayRangeEndDate.HasValue && filter.BirthdayRangeEndDate.Value != DateTime.MinValue)
			{
				sqlBuilder.Append($" AND (p.PatientBirthDate<'{filter.BirthdayRangeEndDate.Value.AddDays(1).ToString("yyyy-MM-dd")} 00:00:00') ");
			}

			//Apply Sort
			string sortedColumnName = this.GetSortedColumnName(filter.SortedColumnName);
			string sortDirection = filter.IsAscendingSort ? "ASC" : "DESC";
			sqlBuilder.Append($" ORDER BY {sortedColumnName} {sortDirection}");

			return sqlBuilder.ToString();
		}

		private string GetSortedColumnName(StudyListColumn column)
		{
			string sortedColumnName = string.Empty;
			switch (column)
			{
				case StudyListColumn.PatientName:
					sortedColumnName = "p.PatientName";
					break;
				case StudyListColumn.PatientID:
					sortedColumnName = "p.PatientId";
					break;
				case StudyListColumn.BodyPartExam:
					sortedColumnName = "t.BodyPart";
					break;
				case StudyListColumn.Birthday:
					sortedColumnName = "p.PatientBirthDate";
					break;
				case StudyListColumn.Sex:
					sortedColumnName = "p.PatientSex";
					break;
				case StudyListColumn.StudyStatus:
					sortedColumnName = "t.StudyStatus";
					break;
				case StudyListColumn.ArchiveStatus:
					sortedColumnName = "t.ArchiveStatus";
					break;
				case StudyListColumn.PrintStatus:
					sortedColumnName = "t.PrintStatus";
					break;
				case StudyListColumn.CorrectionStatus:
					sortedColumnName = "t.CorrectStatus";
					break;
				case StudyListColumn.StudyTime:
					sortedColumnName = "t.StudyTime";
					break;
				case StudyListColumn.Technician:
					sortedColumnName = "t.Technician";
					break;
				case StudyListColumn.Ward:
					sortedColumnName = "t.Ward";
					break;
				case StudyListColumn.ReferringPhysician:
					sortedColumnName = "t.ReferringPhysicianName";
					break;
				case StudyListColumn.CreateTime:
					sortedColumnName = "t.CreateTime";
					break;
				case StudyListColumn.IsLocked:
					sortedColumnName = "t.IsProtected";
					break;
				default:
					sortedColumnName = "t.CreateTime";
					break;
			}
			return sortedColumnName;
		}

		public int GetStudyCountByPatientID(string patientId)
		{
			var result = _context.Connection.ContextExecute<int>((connection) =>
			{
				return connection.ExecuteScalar<int>("SELECT COUNT(*) FROM t_study t WHERE t.InternalPatientId=@Id", new { Id = patientId });
			});
			return result;
		}

		public bool UpdateProtocol(StudyEntity entity)
		{
			//_logger.LogInformation("Update study protocol");
			string sql = "UPDATE t_study t SET StudyDescription=@StudyDescription, Protocol=@Protocol, StudyId=@StudyId, BodyPart=@BodyPart, UpdateTime=@UpdateTime, Updater=@Updater, IsLocalModified=@IsLocalModified WHERE t.Id=@Id";
			var result = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Execute(sql, new
				{
					Id = entity.Id,
					StudyId = entity.StudyId,
                    StudyDescription = entity.StudyDescription,
					Protocol = entity.Protocol,
					BodyPart = entity.BodyPart,
					UpdateTime = entity.UpdateTime,
					Updater = entity.Updater,
					IsLocalModified = true,
				});
			}, _logger);
			return result == 1;
		}

		public bool UpdateProtocol(string id,string protocol)
		{
			//_logger.LogInformation("Update study protocol");
			string sql = "UPDATE t_study t SET Protocol=@Protocol WHERE t.Id=@Id";
			var result = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Execute(sql, new
				{
					Id = id,
					Protocol = protocol
				});
			}, _logger);
			return result == 1;
		}

		public bool UpdateStudyStatus(string studyId, WorkflowStatus examStatus)
		{
			//_logger.LogInformation("Update study exam status");
			string sql = "";
			var now = DateTime.Now;
			var result = 0;
			if (examStatus == WorkflowStatus.ExaminationClosed || examStatus == WorkflowStatus.ExaminationClosing || examStatus == WorkflowStatus.ExaminationDiscontinue)
			{
				sql = "UPDATE t_study t SET StudyStatus=@StudyStatus,ExamEndTime=@ExamEndTime,UpdateTime=@UpdateTime,Updater=@Updater,IsLocalModified=@IsLocalModified WHERE t.Id=@Id";
				result = _context.Connection.ContextExecute(connection => connection.Execute(sql, new
				{
					Id = studyId,
					ExamEndTime = now,
					StudyStatus = examStatus.ToString(),
					UpdateTime = now,
					Updater = string.Empty,
					IsLocalModified = true,
				}), _logger);
			}
			//Exam进程将状态设置为 检查中的时候 也就是点击confirm的时候，这时是真正开始检查
			else if (examStatus == WorkflowStatus.Examinating)
			{
				sql = "UPDATE t_study t SET StudyStatus=@StudyStatus,ExamStartTime=@ExamStartTime,UpdateTime=@UpdateTime,Updater=@Updater,IsLocalModified=@IsLocalModified WHERE t.Id=@Id";
				result = _context.Connection.ContextExecute(connection => connection.Execute(sql, new
				{
					Id = studyId,
					ExamStartTime = now,
					StudyStatus = examStatus.ToString(),
					UpdateTime = now,
					Updater = string.Empty,
					IsLocalModified = true,
				}), _logger);
			}
			else
			{
				sql = "UPDATE t_study t SET StudyStatus=@StudyStatus,UpdateTime=@UpdateTime,Updater=@Updater,IsLocalModified=@IsLocalModified WHERE t.Id=@Id";
				result = _context.Connection.ContextExecute(connection => connection.Execute(sql, new
				{
					Id = studyId,
					StudyStatus = examStatus.ToString(),
					UpdateTime = now,
					Updater = string.Empty,
					IsLocalModified = true,
				}), _logger);
			}

			return result == 1;
		}

		public string GetStudyIdWithAbnoramlClosed()
		{
			var sql = $"SELECT id FROM t_study WHERE StudyStatus != '{WorkflowStatus.NotStarted}' and StudyStatus != '{WorkflowStatus.ExaminationClosed}' and StudyStatus != '{WorkflowStatus.ExaminationDiscontinue}'";
			return _context.Connection.ContextExecute(connection => connection.ExecuteScalar<string>(sql), _logger);
		}

		public void UpdateStudyClosedWithAbnormalStatus()
		{
			var sql = $"UPDATE t_study t SET StudyStatus='{WorkflowStatus.ExaminationDiscontinue}',ExamEndTime=CURRENT_TIMESTAMP(),UpdateTime=CURRENT_TIMESTAMP(),IsLocalModified=1 WHERE StudyStatus != '{WorkflowStatus.NotStarted}' and StudyStatus != '{WorkflowStatus.ExaminationClosed}' and StudyStatus != '{WorkflowStatus.ExaminationDiscontinue}'";
			_context.Connection.ContextExecute(connection => connection.Execute(sql), _logger);
		}

		public bool UpdateStudyExaming(string studyId, DateTime studyTime, WorkflowStatus workflowStatus)
		{
			var parameters = new { StudyId = studyId, StudyTime = studyTime, StudyStatus = workflowStatus.ToString() };
			_logger.LogInformation($"Update study info: {JsonConvert.SerializeObject(parameters)}");
			string sql = "UPDATE t_study SET StudyStatus = @StudyStatus,StudyTime = @StudyTime,UpdateTime=CURRENT_TIMESTAMP(),IsLocalModified=1 WHERE Id=@StudyId";
			var result = _context.Connection.ContextExecute(connection => connection.Execute(sql, parameters), _logger);
			return result == 1;
		}
		public StudyEntity GetStudyEntityByPatientIdAndStudyStatus(string patientId, string studyStatus)
		{
			var entity = _context.Connection.ContextExecute((connection) =>
			{
				return connection.QueryFirstOrDefault<StudyEntity>("SELECT * FROM t_study WHERE InternalPatientId=@InternalPatientId AND StudyStatus=@StudyStatus", new { InternalPatientId = patientId, StudyStatus = studyStatus });
			});
			return entity;
		}

		public PatientEntity GetPatientEntityById(string patientId)
		{
			var entity = _context.Connection.ContextExecute((connection) =>
			{
				return connection.QueryFirstOrDefault<PatientEntity>("select * from t_patient p where p.Id = @PatientId", new { PatientId = patientId });
			});
			return entity;
		}
		public bool ResumeExamination(StudyEntity studyEntity, string StudyId)
		{
			return _context.Connection.ContextExecute((connection, transaction) =>
			{
				string sql = "UPDATE t_study SET StudyStatus=@StudyStatus,UpdateTime=CURRENT_TIMESTAMP(),IsLocalModified=1 WHERE Id=@Id";
				var res = connection.Execute(sql, new
				{
					Id = StudyId,
					StudyStatus = WorkflowStatus.ExaminationClosed.ToString(),
				}, transaction);
				sql = "insert into t_study (Id,InternalPatientId,Age,AgeType,PatientSize,StudyInstanceUID,PatientWeight,StudyDescription,AdmittingDiagnosisDescription,Ward,ReferringPhysicianName,BodyPart,AccessionNo,StudyId,Comments,InstitutionName,InstitutionAddress,StudyStatus,PatientType,CreateTime,Creator,UpdateTime,Updater,IsLocalModified) value(@Id,@InternalPatientId,@Age,@AgeType,@PatientSize,@StudyInstanceUID,@PatientWeight,@StudyDescription,@AdmittingDiagnosisDescription,@Ward,@ReferringPhysicianName,@BodyPart,@AccessionNo,@StudyId,@Comments,@InstitutionName,@InstitutionAddress,@StudyStatus,@PatientType,@CreateTime,@Creator,@UpdateTime,@Updater,@IsLocalModified);";
				res = connection.Execute(sql, new
				{
					Id = studyEntity.Id,
					InternalPatientId = studyEntity.InternalPatientId,
					Age = studyEntity.Age,
					AgeType = studyEntity.AgeType,
					PatientSize = studyEntity.PatientSize,
					StudyInstanceUID = studyEntity.StudyInstanceUID,
					PatientWeight = studyEntity.PatientWeight,
					AdmittingDiagnosisDescription = studyEntity.AdmittingDiagnosisDescription,
					Ward = studyEntity.Ward,
					ReferringPhysicianName = studyEntity.ReferringPhysicianName,
					BodyPart = studyEntity.BodyPart,
					AccessionNo = studyEntity.AccessionNo,
					StudyId = studyEntity.StudyId,
					Comments = studyEntity.Comments,
					InstitutionName = studyEntity.InstitutionName,
					InstitutionAddress = studyEntity.InstitutionAddress,
					StudyStatus = studyEntity.StudyStatus,
					PatientType = studyEntity.PatientType,
					StudyDescription = studyEntity.StudyDescription,
					CreateTime = studyEntity.CreateTime,
					Creator = studyEntity.Creator,
					UpdateTime = studyEntity.UpdateTime,
					Updater = studyEntity.Updater,
					IsLocalModified = true,
				}, transaction);
				return true;
			}, _logger);
		}

		public bool SwitchLockStatus(string studyId)
		{
			//_logger.LogInformation("SwitchLockStatus");
			string sql = "UPDATE t_study t SET IsProtected=NOT IsProtected,UpdateTime=CURRENT_TIMESTAMP() WHERE t.Id=@Id";
			var result = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Execute(sql, new
				{
					Id = studyId
				});
			}, _logger);
			return result == 1;
		}

		public bool UpdatePatientAndStudy(PatientEntity patientEntity, StudyEntity studyEntity)
		{
			return _context.Connection.ContextExecute((connection, transaction) =>
			{
				string sql = "UPDATE t_patient p SET  p.PatientId=@PatientId,p.PatientSex=@PatientSex,p.PatientName=@PatientName,PatientBirthDate=@PatientBirthDate,UpdateTime=@UpdateTime,Updater=@Updater WHERE p.Id=@Id";
				connection.Execute(sql, new
				{
					PatientID = patientEntity.PatientId,
					PatientSex = patientEntity.PatientSex,
					PatientName = patientEntity.PatientName,
					PatientBirthDate = patientEntity.PatientBirthDate.Date,
					UpdateTime = patientEntity.UpdateTime,
					Updater = patientEntity.Updater,
					Id = patientEntity.Id,
				}, transaction);
				sql = "UPDATE t_study t SET t.Age=@Age,AgeType=@AgeType,PatientSize=@PatientSize,PatientWeight=@PatientWeight,StudyDescription=@StudyDescription, AdmittingDiagnosisDescription=@AdmittingDiagnosisDescription,Ward=@Ward,ReferringPhysicianName=@ReferringPhysicianName,BodyPart=@BodyPart,AccessionNo=@AccessionNo,StudyId=@StudyId,Comments=@Comments,InstitutionName=@InstitutionName,InstitutionAddress=@InstitutionAddress,PatientType=@PatientType,CorrectStatus=@CorrectStatus,ExamStartTime=@ExamStartTime,ExamEndTime=@ExamEndTime,UpdateTime=@UpdateTime,Updater=@Updater,IsLocalModified=1 WHERE t.Id=@Id";
				connection.Execute(sql, new
				{
					Id = studyEntity.Id,
					Age = studyEntity.Age,
					AgeType = studyEntity.AgeType,
					PatientSize = studyEntity.PatientSize,
					PatientWeight = studyEntity.PatientWeight,
					AdmittingDiagnosisDescription = studyEntity.AdmittingDiagnosisDescription,
					Ward = studyEntity.Ward,
					ReferringPhysicianName = studyEntity.ReferringPhysicianName,
					BodyPart = studyEntity.BodyPart,
					AccessionNo = studyEntity.AccessionNo,
					StudyId = studyEntity.StudyId,
					Comments = studyEntity.Comments,
					InstitutionName = studyEntity.InstitutionName,
					InstitutionAddress = studyEntity.InstitutionAddress,
					PatientType = studyEntity.PatientType,
					StudyTime = studyEntity.StudyTime,
					StudyDescription = studyEntity.StudyDescription,
					CorrectStatus = studyEntity.CorrectStatus,
					ExamStartTime = studyEntity.ExamStartTime,
					ExamEndTime = studyEntity.ExamEndTime,
					UpdateTime = studyEntity.UpdateTime,
					Updater = studyEntity.Updater,
				}, transaction);

				sql = "UPDATE t_series s SET s.BodyPart=@BodyPart WHERE s.InternalStudyId=@StudyId";
				connection.Execute(sql, new
				{
					BodyPart = studyEntity.BodyPart,
					StudyId = studyEntity.Id,
				}, transaction);

				return true;
			}, _logger);
		}

		public bool InsertPatientAndStudy(PatientEntity patientEntity, StudyEntity studyEntity)
		{
			return _context.Connection.ContextExecute((connection, transaction) =>
			{
				string sql = "insert into t_patient (Id,PatientId,PatientName,PatientSex,PatientBirthDate,CreateTime,Creator,UpdateTime,Updater) value(@Id,@PatientId,@PatientName,@PatientSex,@PatientBirthDate,@CreateTime,@Creator,@UpdateTime,@Updater);";
				connection.Execute(sql, new
				{
					ID = patientEntity.Id,
					PatientID = patientEntity.PatientId,
					PatientSex = patientEntity.PatientSex,
					PatientName = patientEntity.PatientName,
					PatientBirthDate = patientEntity.PatientBirthDate.Date,
					CreateTime = patientEntity.CreateTime,
					Creator = patientEntity.Creator,
					UpdateTime = patientEntity.UpdateTime,
					Updater = patientEntity.Updater,
				}, transaction);
				sql = "insert into t_study (Id,InternalPatientId,Age,AgeType,PatientSize,StudyInstanceUID,PatientWeight,StudyDescription,AdmittingDiagnosisDescription,Ward,ReferringPhysicianName,BodyPart,AccessionNo,StudyId,Comments,InstitutionName,InstitutionAddress,StudyStatus,PatientType,StudyTime,CreateTime,Creator,UpdateTime,Updater,PrintConfigPath,IsLocalModified,Protocol) value (@Id,@InternalPatientId,@Age,@AgeType,@PatientSize,@StudyInstanceUID,@PatientWeight,@StudyDescription,@AdmittingDiagnosisDescription,@Ward,@ReferringPhysicianName,@BodyPart,@AccessionNo,@StudyId,@Comments,@InstitutionName,@InstitutionAddress,@StudyStatus,@PatientType,@StudyTime,@CreateTime,@Creator,@UpdateTime,@Updater,@PrintConfigPath,@IsLocalModified,@Protocol);";
				connection.Execute(sql, new
				{
					Id = studyEntity.Id,
					InternalPatientId = studyEntity.InternalPatientId,
					Age = studyEntity.Age,
					StudyInstanceUID = studyEntity.StudyInstanceUID,
					AgeType = studyEntity.AgeType,
					PatientSize = studyEntity.PatientSize,
					PatientWeight = studyEntity.PatientWeight,
					AdmittingDiagnosisDescription = studyEntity.AdmittingDiagnosisDescription,
					Ward = studyEntity.Ward,
					ReferringPhysicianName = studyEntity.ReferringPhysicianName,
					BodyPart = studyEntity.BodyPart,
					AccessionNo = studyEntity.AccessionNo,
					StudyId = studyEntity.StudyId,
					Comments = studyEntity.Comments,
					InstitutionName = studyEntity.InstitutionName,
					InstitutionAddress = studyEntity.InstitutionAddress,
					StudyStatus = studyEntity.StudyStatus,
					PatientType = studyEntity.PatientType,
					StudyTime = studyEntity.StudyTime,
					StudyDescription = studyEntity.StudyDescription,
					CreateTime = studyEntity.CreateTime,
					Creator = studyEntity.Creator,
					UpdateTime = studyEntity.UpdateTime,
					Updater = studyEntity.Updater,
					PrintConfigPath = studyEntity.PrintConfigPath,
					IsLocalModified = studyEntity.IsLocalModified,
					Protocol = studyEntity.Protocol
				}, transaction);
				return true;
			}, _logger);
		}

		public bool SavePatientStudySeries(List<PatientEntity> patientEntities, List<StudyEntity> studyEntities, List<SeriesEntity> seriesEntities)
		{
			bool result = true;

			//save patients
			foreach (var patient in patientEntities)
			{
				var patientInDB = _patientRepository.Get(patient.PatientId);
				//update related studies in memory
				var relatedStudies = studyEntities.Where(s => s.InternalPatientId == patient.Id).ToArray();

				if (patientInDB is null)
				{
					//insert if the patient does not exist
					var executedResult = _patientRepository.Insert(patient);
					if (!executedResult)
					{
						//如果失败，则跳过该患者关联的检查和序列，继续下一个患者导入
						result = false;
						continue;
					}
				}
				else
				{
					foreach (var relatedStudy in relatedStudies)
					{
						relatedStudy.InternalPatientId = patientInDB.Id;
					}

					patient.Id = patientInDB.Id;
					var executedResult = _patientRepository.Update(patient);
					if (!executedResult)
					{
						//如果失败，则跳过该患者关联的检查和序列，继续下一个患者导入
						result = false;
						continue;
					}
				}

				//save studies and series
				foreach (var study in relatedStudies)
				{
					var relatedSeries = seriesEntities.Where(s => s.InternalStudyId == study.Id).ToList();
					var executedResult = SaveStudyWithRelatedSeries(study, relatedSeries);
					if (!executedResult)
					{
						//如果失败，则跳过该检查和序列，继续下一个患者导入
						result = false;
						continue;
					}
				}

			}// end foreach of patient

			return result;
		}

		public bool SaveStudyWithRelatedSeries(StudyEntity study, List<SeriesEntity> relatedSeries)
		{
			//如果已存在序列，则仅覆盖会造成更新不完全的情况：比如原Study下有100个Series，但新Study下仅有80个Series，
			//此时对原Series覆盖会出现问题，预期更新结果是数据库存在80个相关Series，实际更新结果是数据库中存在超过80个相关Series。
			//基于上述场景的考虑，首先基于Study删除所有该检查相关的Series，再插入新Series。 
			var studyInDB = this.GetStudyByUId(study.StudyInstanceUID);
			if (studyInDB is null)
			{
				//insert if the study does not exist
				var executedResult = this.Insert(study);
				if (!executedResult)
				{
					//如果失败，则跳过该检查和序列
					return false;
				}
			}
			else
			{
				//update related series in memory
				foreach (var series in relatedSeries)
				{
					series.InternalStudyId = studyInDB.Id;
				}
				study.Id = studyInDB.Id;
				var executedResult = this.Update(study);
				if (!executedResult)
				{
					//如果失败，则跳过该检查和序列
					return false;
				}

				//注释掉该方法保留Study下已经存在的图像序列;
				//delete all realted series by studyId  
				//_seriesRepository.DeleteByStudyId(studyInDB.Id);
			}

			//insert all realted series by studyId
			var result = _seriesRepository.AddSeriesList(relatedSeries);
			if (!result)
			{
				//如果失败，则跳过
				return false;
			}

			return true;
		}

		public bool UpdateArchiveStatus(List<StudyEntity> studyModels)
		{
			return _context.Connection.ContextExecute((connection, transaction) =>
			{
				string sql = "Update t_study set ArchiveStatus=@ArchiveStatus,UpdateTime=@UpdateTime,Updater=@Updater,IsLocalModified=1 Where Id=@Id";
				connection.Execute(sql, studyModels, transaction);
				return true;
			}, _logger);
		}

		public bool UpdatePrintStatus(List<StudyEntity> studyModels)
		{
			return _context.Connection.ContextExecute((connection, transaction) =>
			{
				string sql = "Update t_study set PrintStatus=@PrintStatus,UpdateTime=@UpdateTime,Updater=@Updater,IsLocalModified=1 Where Id=@Id";
				connection.Execute(sql, studyModels, transaction);
				return true;
			}, _logger);
		}

		public bool SetStudyArchiveFail()
		{
			return _context.Connection.ContextExecute((connection, transaction) =>
			{
				string sql = "UPDATE t_study SET ArchiveStatus=5,UpdateTime=CURRENT_TIMESTAMP(),IsLocalModified=1 WHERE ArchiveStatus=1 OR ArchiveStatus=2";
				connection.Execute(sql, transaction);
				return true;
			}, _logger);
		}

		public StudyEntity[] GetStudiesByIds(string[] ids)
		{
			string sql = "select Id,InternalPatientId,StudyInstanceUID, Age, AgeType,PatientType,RequestProcedure,Technician,Ward, ReferringPhysicianName, BodyPart, AccessionNo, RegistrationDate, StudyTime, ArchiveStatus,StudyStatus, CorrectStatus, PrintStatus, StudyPath, StudyId, Protocol, IsProtected,StudyDescription,CreateTime,Creator,UpdateTime,Updater,PrintConfigPath,IsLocalModified,PatientSize,PatientWeight from t_study where Id in @Ids";
			var entities = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Query<StudyEntity>(sql, new { Ids = ids });
			});
			return entities.ToArray();
		}

		public List<StudyEntity> GetStudiesByPatient(string patientGuid)
		{
            string sql = "select Id,InternalPatientId,StudyInstanceUID, Age, AgeType,PatientType,RequestProcedure,Technician,Ward, ReferringPhysicianName, BodyPart, AccessionNo, RegistrationDate, StudyTime, ArchiveStatus,StudyStatus, CorrectStatus, PrintStatus, StudyPath, StudyId, Protocol, IsProtected,StudyDescription,CreateTime,Creator,UpdateTime,Updater,PrintConfigPath,IsLocalModified,PatientSize,PatientWeight from t_study where InternalPatientId = @PatientGuid";
            var entities = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Query<StudyEntity>(sql, new { PatientGuid = patientGuid });
            });
            return entities.ToList();
        }

        public List<(PatientEntity, StudyEntity)> GetCorrectionHistoryList(string studyId)
		{
			string sql = "SELECT p.Id,p.PatientName,p.PatientId,p.PatientSex,p.CreateTime,p.PatientBirthDate,p.Editor,t.PatientType,t.Age,t.InternalPatientId,t.Id,t.PatientWeight,t.PatientSize,t.StudyInstanceUID,t.AgeType,t.StudyDescription,t.AdmittingDiagnosisDescription,t.Technician,t.Ward,t.ReferringPhysicianName,t.BodyPart,t.AccessionNo,t.StudyId,t.Comments,t.InstitutionName,t.ArchiveStatus,t.PrintStatus,t.CorrectStatus,t.InstitutionAddress,t.ExamStartTime,t.ExamEndTime,t.PrintConfigPath,t.CreateTime from t_study_correction t join t_patient_correction p on t.Id=p.Id WHERE InternalStudyId = @StudyId ORDER BY t.CreateTime DESC";
			var entities = _context.Connection.ContextExecute((connection) =>
			{
				return connection.Query<PatientEntity, StudyEntity, (PatientEntity, StudyEntity)>(sql, (patientEntity, studyEntity) => { return (patientEntity, studyEntity); }, new { StudyId = studyId }, splitOn: "Age");
			}, _logger);
			return entities.ToList();
		}

		public bool CorrectPatientAndStudy(PatientEntity patientEntity, StudyEntity studyEntity, string editor)
		{
			return _context.Connection.ContextExecute((connection, transaction) =>
			{
				string correctionId = Guid.NewGuid().ToString();
				//首先把患者原数据备份到对应校正表中
				string backupPatientSql = "INSERT INTO t_patient_correction (Id, InternalPatientId, PatientId, PatientName, PatientSex, PatientBirthDate, Editor, CreateTime, Creator,UpdateTime,Updater, PatientComments) SELECT @NewCorrectPatientId, Id, PatientId, PatientName, PatientSex, PatientBirthDate, @Editor, CreateTime, @CurrentAccount,UpdateTime, Updater, PatientComments FROM t_patient WHERE Id = @Id;";
				connection.Execute(backupPatientSql, new
				{
					NewCorrectPatientId = correctionId,
					Editor = editor,
					CurrentAccount = editor,
					Id = patientEntity.Id,
				}, transaction);

				//把检查原数据备份到对应校正表中
				string backupStudySql = "INSERT INTO t_study_correction (Id,InternalStudyId,InternalPatientId,StudyInstanceUID,Age,AgeType,PatientType,RequestProcedure,BodyPart,AccessionNo,RegistrationDate,StudyTime,ExamStartTime,ExamEndTime,DeviceId,Technician,LoginId,StudyStatus,PatientSize,PatientWeight,ReferringPhysicianName,Ward,FieldStrenght,AdmittingDiagnosisDescription,MedicalAlerts,Allergies,SmokingStatus,PregnancyStatus,StudyDescription,PatientResidency,PatientAddress,BodyParts,PerformingPhysician,ArchiveStatus,StudyPath,StudyId,PrintStatus,CorrectStatus,IsProtected,OriginStudyId,MedicalHistory,InstitutionName,InstitutionAddress,Comments,Protocol,Editor,CreateTime,Creator,UpdateTime,Updater,PrintConfigPath,IsLocalModified) SELECT @NewCorrectStudyId,Id,InternalPatientId,StudyInstanceUID,Age,AgeType,PatientType,RequestProcedure,BodyPart,AccessionNo,RegistrationDate,StudyTime,ExamStartTime,ExamEndTime,DeviceId,Technician,LoginId,StudyStatus,PatientSize,PatientWeight,ReferringPhysicianName,Ward,FieldStrenght,AdmittingDiagnosisDescription,MedicalAlerts,Allergies,SmokingStatus,PregnancyStatus,StudyDescription,PatientResidency,PatientAddress,BodyParts,PerformingPhysician,ArchiveStatus,StudyPath,StudyId,PrintStatus,CorrectStatus,IsProtected,OriginStudyId,MedicalHistory,InstitutionName,InstitutionAddress,Comments,Protocol,@Editor,CreateTime, @CurrentAccount,UpdateTime,Updater,PrintConfigPath,IsLocalModified FROM t_study WHERE Id = @Id;";
				connection.Execute(backupStudySql, new
				{
					NewCorrectStudyId = correctionId,
					Editor = editor,
					CurrentAccount = editor, //等集成账号服务后，再去取当前登录账号
					Id = studyEntity.Id,
				}, transaction);

				//更新原患者数据
				string sql = "UPDATE t_patient p SET p.PatientId=@PatientId,p.PatientSex=@PatientSex,p.PatientName=@PatientName,PatientBirthDate=@PatientBirthDate,UpdateTime=@UpdateTime,Updater=@Updater WHERE p.Id=@Id;";
				connection.Execute(sql, new
				{
					PatientID = patientEntity.PatientId,
					PatientSex = patientEntity.PatientSex,
					PatientName = patientEntity.PatientName,
					PatientBirthDate = patientEntity.PatientBirthDate,
					UpdateTime = patientEntity.UpdateTime,
					Updater = patientEntity.Updater,
					Id = patientEntity.Id,
				}, transaction);

				//更新原检查数据
				sql = "UPDATE t_study t SET t.Age=@Age,AgeType=@AgeType,PatientSize=@PatientSize,PatientWeight=@PatientWeight,StudyDescription=@StudyDescription,ReferringPhysicianName=@ReferringPhysicianName,AccessionNo=@AccessionNo,CorrectStatus=@CorrectStatus,UpdateTime=@UpdateTime,Updater=@Updater,IsLocalModified=@IsLocalModified WHERE t.Id=@Id;";
				connection.Execute(sql, new
				{
					Id = studyEntity.Id,
					Age = studyEntity.Age,
					AgeType = studyEntity.AgeType,
					PatientSize = studyEntity.PatientSize,
					PatientWeight = studyEntity.PatientWeight,
					ReferringPhysicianName = studyEntity.ReferringPhysicianName,
					AccessionNo = studyEntity.AccessionNo,
					StudyDescription = studyEntity.StudyDescription,
					CorrectStatus = (int)CorrectStatus.Corrected,
					UpdateTime = studyEntity.UpdateTime,
					Updater = studyEntity.Updater,
					IsLocalModified = true,
				}, transaction);
				return true;

			}, _logger);
		}

		public (bool, bool) UpdateWorklistByStudy(PatientEntity patientEntity, StudyEntity studyEntity)
		{
			return _context.Connection.ContextExecute((connection, transaction) =>
			{
				bool executionResult = false;
				bool needToRefreshWorkList = false;

				//判断是否存在本地Study
				string sqlCheckStudy = "SELECT Id,InternalPatientId,IsLocalModified FROM t_study WHERE StudyInstanceUID=@StudyInstanceUID;";
				var studyDbResult = _context.Connection.ContextExecute((connection) =>
				{
					return connection.Query<string, string, UInt64, (string, string, UInt64)>(sqlCheckStudy,
						   (studyId, internalPatientId, isLocalModified) => { return (studyId, internalPatientId, isLocalModified); }, new { StudyInstanceUID = studyEntity.StudyInstanceUID }, splitOn: "Id, InternalPatientId,IsLocalModified");
				}, _logger);

				//如果本地存在已被修改的Study，则直接返回，不覆盖本地数据
				if (studyDbResult.Any() && studyDbResult.First().Item3 > 0)
				{
					executionResult = true;
					needToRefreshWorkList = false;
					return (executionResult, needToRefreshWorkList);
				}

				if (studyDbResult.Any())
				{
					//如果本地已存在尚未修改的Study，则对Patient和Study进行覆盖
					this._logger.LogDebug($"Update worklist for {patientEntity.PatientName}");

					studyEntity.Id = studyDbResult.First().Item1;
					patientEntity.Id = studyDbResult.First().Item2;
					this.UpdateWorkList(connection, transaction, patientEntity, studyEntity);
				}
				else
				{
					//如果本地不存在该Study,则新增Patient和Study
					this._logger.LogDebug($"Add worklist for {patientEntity.PatientName}");
					this.AddWorkList(connection, transaction, patientEntity, studyEntity);
				}

				executionResult = true;
				needToRefreshWorkList = true;
				return (executionResult, needToRefreshWorkList);
			}, _logger);
		}

		private void AddWorkList(MySqlConnection connection, MySqlTransaction transaction, PatientEntity patientEntity, StudyEntity studyEntity)
		{
			//Step1:新增患者数据
			string sqlAddPatient = "INSERT INTO t_patient (Id, PatientId, PatientName, PatientSex, PatientBirthDate, CreateTime,Creator,UpdateTime,Updater) VALUE( @Id, @PatientId, @PatientName, @PatientSex, @PatientBirthDate, @CreateTime, @Creator, @UpdateTime, @Updater);";
			connection.Execute(sqlAddPatient, new
			{
				PatientID = patientEntity.PatientId,
				PatientSex = patientEntity.PatientSex,
				PatientName = patientEntity.PatientName,
				PatientBirthDate = patientEntity.PatientBirthDate,
				CreateTime = patientEntity.CreateTime,
				Creator = patientEntity.Creator,
				UpdateTime = patientEntity.UpdateTime,
				Updater = patientEntity.Updater,
				Id = patientEntity.Id,
			}, transaction);

			//Step2:新增检查数据
			string sqlInsertStudy = "INSERT INTO t_study (Id,InternalPatientId,Age,AgeType,PatientSize,StudyInstanceUID,PatientWeight,StudyDescription,AdmittingDiagnosisDescription,Ward,ReferringPhysicianName,BodyPart,AccessionNo,StudyId,Comments,InstitutionName,InstitutionAddress,StudyStatus,PatientType,StudyTime,CreateTime,Creator,UpdateTime,Updater,PrintConfigPath,IsLocalModified) VALUE (@Id,@InternalPatientId,@Age,@AgeType,@PatientSize,@StudyInstanceUID,@PatientWeight,@StudyDescription,@AdmittingDiagnosisDescription,@Ward,@ReferringPhysicianName,@BodyPart,@AccessionNo,@StudyId,@Comments,@InstitutionName,@InstitutionAddress,@StudyStatus,@PatientType,@StudyTime,@CreateTime,@Creator,@UpdateTime,@Updater,@PrintConfigPath,@IsLocalModified);";
			connection.Execute(sqlInsertStudy, new
			{
				Id = studyEntity.Id,
				InternalPatientId = patientEntity.Id,
				Age = studyEntity.Age,
				StudyInstanceUID = studyEntity.StudyInstanceUID,
				AgeType = studyEntity.AgeType,
				PatientSize = studyEntity.PatientSize,
				PatientWeight = studyEntity.PatientWeight,
				AdmittingDiagnosisDescription = studyEntity.AdmittingDiagnosisDescription,
				Ward = studyEntity.Ward,
				ReferringPhysicianName = studyEntity.ReferringPhysicianName,
				BodyPart = studyEntity.BodyPart,
				AccessionNo = studyEntity.AccessionNo,
				StudyId = studyEntity.StudyId,
				Comments = studyEntity.Comments,
				InstitutionName = studyEntity.InstitutionName,
				InstitutionAddress = studyEntity.InstitutionAddress,
				StudyStatus = studyEntity.StudyStatus,
				PatientType = studyEntity.PatientType,
				StudyTime = studyEntity.StudyTime,
				StudyDescription = studyEntity.StudyDescription,
				CreateTime = studyEntity.CreateTime,
				Creator = studyEntity.Creator,
				UpdateTime = studyEntity.UpdateTime,
				Updater = studyEntity.Updater,
				PrintConfigPath = studyEntity.PrintConfigPath,
				IsLocalModified = false,

			}, transaction);
		}

		private void UpdateWorkList(MySqlConnection connection, MySqlTransaction transaction, PatientEntity patientEntity, StudyEntity studyEntity)
		{
			//Step1:更新检查数据            
			string sqlUpdateStudy = "UPDATE t_study t SET t.Age=@Age,AgeType=@AgeType,PatientSize=@PatientSize,PatientWeight=@PatientWeight,StudyDescription=@StudyDescription, AdmittingDiagnosisDescription=@AdmittingDiagnosisDescription,Ward=@Ward,ReferringPhysicianName=@ReferringPhysicianName,BodyPart=@BodyPart,AccessionNo=@AccessionNo,StudyId=@StudyId,Comments=@Comments,InstitutionName=@InstitutionName,InstitutionAddress=@InstitutionAddress,PatientType=@PatientType,CorrectStatus=@CorrectStatus WHERE t.Id=@Id";
			connection.Execute(sqlUpdateStudy, new
			{
				Id = studyEntity.Id,
				Age = studyEntity.Age,
				AgeType = studyEntity.AgeType,
				PatientSize = studyEntity.PatientSize,
				PatientWeight = studyEntity.PatientWeight,
				AdmittingDiagnosisDescription = studyEntity.AdmittingDiagnosisDescription,
				Ward = studyEntity.Ward,
				ReferringPhysicianName = studyEntity.ReferringPhysicianName,
				BodyPart = studyEntity.BodyPart,
				AccessionNo = studyEntity.AccessionNo,
				StudyId = studyEntity.StudyId,
				Comments = studyEntity.Comments,
				InstitutionName = studyEntity.InstitutionName,
				InstitutionAddress = studyEntity.InstitutionAddress,
				PatientType = studyEntity.PatientType,
				IsDeleted = studyEntity.IsDeleted,
				StudyTime = studyEntity.StudyTime,
				StudyDescription = studyEntity.StudyDescription,
				CorrectStatus = studyEntity.CorrectStatus,
			}, transaction);

			//Step2:更新患者数据
			string sqlUpdatePatient = "UPDATE t_patient p SET p.PatientId=@PatientId,p.PatientName=@PatientName,p.PatientSex=@PatientSex,PatientBirthDate=@PatientBirthDate WHERE p.Id=@Id;";
			connection.Execute(sqlUpdatePatient, new
			{
				PatientID = patientEntity.PatientId,
				PatientSex = patientEntity.PatientSex,
				PatientName = patientEntity.PatientName,
				PatientBirthDate = patientEntity.PatientBirthDate,
				Id = patientEntity.Id,
			}, transaction);
		}

		public bool UpdatePrintConfigPath(string studyId, string printConfigPath)
		{
			return _context.Connection.ContextExecute((connection, transaction) =>
			{
				string sql = "UPDATE t_study SET PrintConfigPath=@PrintConfigPath WHERE Id=@StudyId";
				connection.Execute(sql, new { StudyId = studyId, PrintConfigPath = printConfigPath }, transaction);
				return true;
			}, _logger);
		}

	}
}