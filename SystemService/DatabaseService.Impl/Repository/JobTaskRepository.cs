//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Dapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract.Entities;
using System.Text;
using static Dapper.SqlMapper;

namespace NV.CT.DatabaseService.Impl.Repository
{
    public class JobTaskRepository //: IJobTaskRepository
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<JobTaskRepository> _logger;

        public JobTaskRepository(DatabaseContext context, ILogger<JobTaskRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool Insert(JobTaskEntity entity)
        {
            string sql = "INSERT INTO t_job_task (Id,WorkflowId,InternalPatientID,InternalStudyID,JobType,JobStatus,Parameter,Priority,CreateTime,Creator,UpdateTime,Updater,StartedDateTime,FinishedDateTime,IsDeleted) VALUE (@Id,@WorkflowId,@InternalPatientID,@InternalStudyID,@JobType,@JobStatus,@Parameter,@Priority,@CreateTime,@Creator,@UpdateTime,@Updater,@StartedDateTime,@FinishedDateTime,@IsDeleted);";
            
            var result = _context.Connection.ContextExecute((connection) =>
            {
                var taskTypeName = Enum.GetName(typeof(JobTaskType), entity.JobType);
                var taskStatusName = Enum.GetName(typeof(JobTaskStatus), entity.JobStatus);
                return connection.Execute(sql, new
                {
                    entity.Id,
                    entity.WorkflowId,
                    entity.InternalPatientID,
                    entity.InternalStudyID,
                    JobType = taskTypeName,
                    JobStatus = taskStatusName,
                    entity.Parameter,
                    entity.Priority,
                    entity.CreateTime,
                    entity.Creator,
                    entity.UpdateTime,
                    entity.Updater,
                    entity.StartedDateTime,
                    entity.FinishedDateTime,
                    entity.IsDeleted,
                });
            }, _logger);
            return result > 0;
        }

        public bool Update(JobTaskEntity entity)
        {
            string sql = "UPDATE t_job_task SET WorkflowId=@WorkflowId,InternalPatientID=@InternalPatientID,InternalStudyID=@InternalStudyID,JobType=@JobType,JobStatus=@JobStatus,Parameter=@Parameter,Priority=@Priority,UpdateTime=@UpdateTime,Updater=@Updater,StartedDateTime=@StartedDateTime,FinishedDateTime=@FinishedDateTime,IsDeleted=@IsDeleted WHERE Id=@Id";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                var taskTypeName = Enum.GetName(typeof(JobTaskType), entity.JobType);
                var taskStatusName = Enum.GetName(typeof(JobTaskStatus), entity.JobStatus);
                return connection.Execute(sql, new
                {
                    entity.Id,
                    entity.WorkflowId,
                    entity.InternalPatientID,
                    entity.InternalStudyID,
                    JobType = taskTypeName,
                    JobStatus = taskStatusName,
                    entity.Parameter,
                    entity.Priority,
                    entity.UpdateTime,
                    entity.Updater,
                    entity.StartedDateTime,
                    entity.FinishedDateTime,
                    entity.IsDeleted,
                });
            }, _logger);
            return result > 0;
        }

        public bool Delete(string jobId, string jobTaskType)
        {
            string sql = "DELETE FROM t_job_task WHERE Id=@Id and JobStatus=@JobStatus";
            var result = _context.Connection.ContextExecute((connection, transaction) =>
            {                
                return connection.Execute(sql, new
                {
                    Id = jobId,
                    JobStatus = jobTaskType,

                }, transaction);

            });

            return result > 0;
        }

        public JobTaskEntity? GetJobById(string jobId, string jobTaskType)
        {
            string fetchSql = "SELECT Id,WorkflowId,InternalPatientID,InternalStudyID,JobType,JobStatus,Parameter,Priority,CreateTime,Creator,UpdateTime,Updater,StartedDateTime,FinishedDateTime,IsDeleted FROM t_job_task WHERE IsDeleted=0 AND JobType=@JobType AND Id=@Id;";
            var entity = _context.Connection.ContextExecute((connection) =>
            {
                return connection.QueryFirstOrDefault<JobTaskEntity>(fetchSql, new { JobType = jobTaskType, Id = jobId });
            }, _logger);

            return entity;   
        }

        /// <summary>
        /// 无需指定JobId直接找下个可用job
        /// </summary>
        /// <param name="jobTaskType"></param>
        /// <returns></returns>
        public JobTaskEntity? FetchNextAvailableJob(string jobTaskType)
        {
            //TODO：后续将把所有SQL语句合并到同一次数据库连接中执行，以提高性能
            string fetchSql = "SELECT Id,WorkflowId,InternalPatientID,InternalStudyID,JobType,JobStatus,Parameter,Priority,CreateTime,Creator,UpdateTime,Updater,StartedDateTime,FinishedDateTime,IsDeleted FROM t_job_task WHERE IsDeleted=0 AND JobType=@JobType AND JobStatus=@JobStatus AND StartedDateTime IS NULL ORDER BY CreateTime,Priority LIMIT 1;";
            var entity = _context.Connection.ContextExecute((connection) =>
            {
                return connection.QueryFirstOrDefault<JobTaskEntity>(fetchSql, new { JobType = jobTaskType, JobStatus = JobTaskStatus.Queued.ToString() });
            }, _logger);
            if (entity is null)
            {
                return null;
            }

            //按指定条件进行"加锁",如果更新后影响行数为1，则"加锁"成功；如果更新后影响行数为0，则"加锁"失败
            string setStartedDateTimeSql = "UPDATE t_job_task SET StartedDateTime = @StartedDateTime WHERE IsDeleted=0 AND Id = @Id AND StartedDateTime IS NULL;";
            var startedDatetime = DateTime.Now;
            var affectedCount = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(setStartedDateTimeSql, new { StartedDateTime = startedDatetime, Id = entity.Id });
            }, _logger);

            if (affectedCount == 1)
            {
                entity.StartedDateTime = startedDatetime;
                return entity;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 通过指定JobId找可用Job
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public JobTaskEntity? FetchAvailableJobById(string jobId , string jobTaskType)
        {
            //TODO：后续将把所有SQL语句合并到同一次数据库连接中执行，以提高性能
            string fetchSql = "SELECT Id,WorkflowId,InternalPatientID,InternalStudyID,JobType,JobStatus,Parameter,Priority,CreateTime,Creator,UpdateTime,Updater,StartedDateTime,FinishedDateTime,IsDeleted FROM t_job_task WHERE IsDeleted=0 AND JobType=@JobType AND Id=@Id ORDER BY CreateTime,Priority LIMIT 1;";
            var entity = _context.Connection.ContextExecute((connection) =>
            {
                return connection.QueryFirstOrDefault<JobTaskEntity>(fetchSql, new { JobType = jobTaskType, Id = jobId });
            }, _logger);

            if (entity is null)
            {
                return null;
            }

            //按指定条件进行"加锁",如果更新后影响行数为1，则"加锁"成功；如果更新后影响行数为0，则"加锁"失败
            string setStartedDateTimeSql = "UPDATE t_job_task SET StartedDateTime = @StartedDateTime WHERE IsDeleted=0 AND Id = @Id AND StartedDateTime IS NULL;";
            var startedDatetime = DateTime.Now;
            var affectedCount = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(setStartedDateTimeSql, new { StartedDateTime = startedDatetime, Id = entity.Id });
            }, _logger);

            if (affectedCount == 1)
            {
                entity.StartedDateTime = startedDatetime;
                return entity;
            }
            else
            {
                return null;
            }
        }

        public List<JobTaskEntity> GetJobTasksByTypeAndStatus(string jobType, List<string> jobTaskStatusList, string beginDatetime, string endDatetime)
        {
            if (!DateTime.TryParse(beginDatetime, out var datetimeOfBegin))
            { 
                return new List<JobTaskEntity>();
            }

            if (!DateTime.TryParse(endDatetime, out var datetimeOfEnd))
            {
                return new List<JobTaskEntity>();
            }

            string sql = "SELECT Id,WorkflowId,InternalPatientID,InternalStudyID,JobType,JobStatus,Parameter,Priority,CreateTime,Creator,UpdateTime,Updater,StartedDateTime,FinishedDateTime FROM t_job_task WHERE IsDeleted=0 AND JobType=@JobType AND JobStatus in @JobStatusList AND CreateTime >= @BeginDateTime AND CreateTime < @EndDateTime ORDER BY CreateTime DESC;";
            var entities = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Query<JobTaskEntity>(sql, new { JobType = jobType,
                                                                  JobStatusList = jobTaskStatusList,
                                                                  BeginDateTime = datetimeOfBegin,
                                                                  EndDateTime = datetimeOfEnd
                                                                 });
            }, _logger);

            return entities.ToList();
        }

        public List<JobTaskEntity> GetJobTasksByTypeAndStatusWithConditions(string jobType, List<string> jobTaskStatusList, string beginDatetime, string endDatetime, string patientName, string bodyPart)
        {
            if (!DateTime.TryParse(beginDatetime, out var datetimeOfBegin))
            {
                return new List<JobTaskEntity>();
            }

            if (!DateTime.TryParse(endDatetime, out var datetimeOfEnd))
            {
                return new List<JobTaskEntity>();
            }

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT t.Id,t.WorkflowId,t.InternalPatientID,t.InternalStudyID,t.JobType,t.JobStatus,t.Parameter,t.Priority,t.CreateTime,t.Creator,t.UpdateTime,t.Updater,t.StartedDateTime,t.FinishedDateTime ");
            sqlBuilder.Append("FROM t_job_task t INNER JOIN t_patient p ON t.InternalPatientID=p.id INNER JOIN t_Study s ON t.InternalStudyID=s.id ");
            sqlBuilder.Append("WHERE t.IsDeleted=0 AND t.JobType=@JobType AND t.JobStatus in @JobStatusList AND t.CreateTime >= @BeginDateTime AND t.CreateTime < @EndDateTime ");
            if (!string.IsNullOrEmpty(patientName))
            {
                sqlBuilder.Append($"AND p.PatientName like '%{patientName}%' ");
            }
            if (!string.IsNullOrEmpty(bodyPart))
            {
                sqlBuilder.Append($"AND s.BodyPart like '%{bodyPart}%' ");
            }
            sqlBuilder.Append("ORDER BY CreateTime DESC;");
            
            try
            {
                var entities = _context.Connection.ContextExecute((connection) =>
                {
                    return connection.Query<JobTaskEntity>(sqlBuilder.ToString(), new
                    {
                        JobType = jobType,
                        JobStatusList = jobTaskStatusList,
                        BeginDateTime = datetimeOfBegin,
                        EndDateTime = datetimeOfEnd,
                    });
                }, _logger);

                return entities.ToList();
            }
            catch (Exception ex)
            {
                var info = ex.Message;
                return new List<JobTaskEntity>();
            }
        }

        public bool UpdateTaskStatusByWorkflowId(string workflowId, string jobStatus)
        {
            var result = _context.Connection.ContextExecute((connection, transaction) =>
            {
                string sql = "UPDATE t_job_task SET JobStatus=@JobStatus WHERE WorkflowId=@WorkflowId";
                return connection.Execute(sql, new
                {
                    WorkflowId = workflowId,
                    JobStatus = jobStatus
                });
            }, _logger);
            return result > 0;
        }

        public bool UpdateTaskStatusByJobId(string jobId, string jobStatus)
        {
            DateTime? finishedDateTime = null;
            if (jobStatus == JobTaskStatus.Cancelled.ToString() || 
                jobStatus == JobTaskStatus.Failed.ToString() ||
                jobStatus == JobTaskStatus.PartlyCompleted.ToString() ||
                jobStatus == JobTaskStatus.Completed.ToString() )
            {
                finishedDateTime = DateTime.Now;
            }

            string sql = "UPDATE t_job_task SET JobStatus=@JobStatus, FinishedDateTime=@FinishedDateTime WHERE Id=@Id";
            var result = _context.Connection.ContextExecute((connection, tran) =>
            {
                return connection.Execute(sql, new
                {
                    JobStatus = jobStatus,
                    FinishedDateTime = finishedDateTime,
                    Id = jobId
                },tran);
            }, _logger);
            return result > 0;
        }

        public JobTaskEntity GetJobTaskByWorkflowId(string workflowId)
        {
            string sql = "SELECT Id,WorkflowId,InternalPatientID,InternalStudyID,JobType,JobStatus,Parameter,Priority,CreateTime,Creator,UpdateTime,Updater,StartedDateTime,FinishedDateTime FROM t_job_task WHERE WorkflowId=@WorkflowId";
            var entities = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Query<JobTaskEntity>(sql, new { WorkflowId = workflowId });
            }, _logger);
            return entities.FirstOrDefault() ?? new();
        }

        public int GetCountOfJobs(string jobType, string jobTaskStatus)
        {
            string fetchSql = "SELECT count(1) FROM t_job_task WHERE IsDeleted=0 AND JobType=@JobType AND JobStatus=@JobStatus;";
            var count = _context.Connection.ContextExecute((connection) =>
            {
                return connection.ExecuteScalar<int>(fetchSql, new { JobType = jobType.ToString(), JobStatus = jobTaskStatus });
            }, _logger);

            return count;
        }

        public bool UpdatePriority(string jobId, string jobType, int priority)
        {
            string sql = "UPDATE t_job_task SET Priority=@Priority WHERE IsDeleted=0 AND Id=@Id AND JobType=@JobType";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, new { Id = jobId, JobType = jobType, Priority = priority, });
            }, _logger);
            return result > 0;
        }

    }
}
