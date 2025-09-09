//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Dapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract.Entities;
using System.Text;
using static Dapper.SqlMapper;

namespace NV.CT.DatabaseService.Impl.Repository;
public class ReconTaskRepository //: IReconTaskRepository
{
    private readonly DatabaseContext _context;
    private readonly ILogger<ReconTaskRepository> _logger;

    public ReconTaskRepository(DatabaseContext context, ILogger<ReconTaskRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public bool Insert(ReconTaskEntity entity)
    {
        return Insert(new List<ReconTaskEntity> { entity });
    }

    public bool Insert(List<ReconTaskEntity> list)
    {
        _logger.LogInformation("Insert recon task with list");
        string sql = "insert into t_recon_task (Id,InternalStudyId,ScanId,FrameOfReferenceUid,IsRTD,ReconId,SeriesNumber,SeriesDescription,WindowWidth,WindowLevel,ReconStartDate,ReconEndDate,IssuingParameters,Description,InternalPatientId,TaskStatus,CreateTime,Creator,Remark) " +
                     "value(@Id,@InternalStudyId,@ScanId,@FrameOfReferenceUid,@IsRTD,@ReconId,@SeriesNumber,@SeriesDescription,@WindowWidth,@WindowLevel,@ReconStartDate,@ReconEndDate,@IssuingParameters,@Description,@InternalPatientId,@TaskStatus,@CreateTime,@Creator,@Remark);";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, list);
        }, _logger);
        return result == 1;
    }

    public bool Update(ReconTaskEntity entity)
    {
        //_logger.LogInformation("Update recon task");
        string sql = "UPDATE t_recon_task SET InternalStudyId=@InternalStudyId,ScanId=@ScanId,FrameOfReferenceUid=@FrameOfReferenceUid,IsRTD=@IsRTD,ReconId=@ReconId,SeriesNumber=@SeriesNumber,SeriesDescription=@SeriesDescription,WindowWidth=@WindowWidth," +
            "WindowLevel=@WindowLevel,ReconStartDate=@ReconStartDate,ReconEndDate=@ReconEndDate,IssuingParameters=@IssuingParameters,Description=@Description,InternalPatientId=@InternalPatientId,TaskStatus=@TaskStatus,CreateTime=@CreateTime," +
            "Creator=@Creator,Remark=@Remark WHERE t.Id = @ID And IsDeleted = 0";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                ID = entity.Id,
                InternalStudyId = entity.InternalStudyId,
                ScanId = entity.ScanId,
                FrameOfReferenceUid = entity.FrameOfReferenceUid,
                IsRTD = entity.IsRTD,
                ReconId = entity.ReconId,
                SeriesNumber = entity.SeriesNumber,
                SeriesDescription = entity.SeriesDescription,
                WindowWidth = entity.WindowWidth,
                WindowLevel = entity.WindowLevel,
                ReconStartDate = entity.ReconStartDate,
                ReconEndDate = entity.ReconEndDate,
                IssuingParameters = entity.IssuingParameters,
                Description = entity.Description,
                InternalPatientId = entity.InternalPatientId,
                TaskStatus = entity.TaskStatus,
                CreateTime = entity.CreateTime,
                Creator = entity.Creator,
                Remark = entity.Remark,
            });
        }, _logger);
        return result == 1;
    }

    public bool UpdateStatus(ReconTaskEntity entity)
    {
        //_logger.LogInformation("Update recon task status");
        string sql = "UPDATE t_recon_task SET IssuingParameters=@IssuingParameters,TaskStatus=@TaskStatus WHERE InternalStudyId=@InternalStudyId and ScanId = @ScanId and FrameOfReferenceUid = @FrameOfReferenceUid and ReconId = @ReconId";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                InternalStudyId = entity.InternalStudyId,
                ScanId = entity.ScanId,
                FrameOfReferenceUid = entity.FrameOfReferenceUid,
                ReconId = entity.ReconId,
                IssuingParameters = entity.IssuingParameters,
                TaskStatus = entity.TaskStatus,
            });
        }, _logger);
        return result == 1;
    }

    public bool UpdateTaskStatus(string studyId, string reconId, OfflineTaskStatus taskStatus, DateTime startTime, DateTime endTime)
    {
        var sb = new StringBuilder();
        sb.Append("UPDATE t_recon_task SET TaskStatus = @TaskStatus");

        if (startTime != default(DateTime))
        {
            sb.Append(", ReconStartDate = @StartTime");
        }
        if (endTime != default(DateTime))
        {
            sb.Append(", ReconEndDate = @EndTime");
        }
        sb.Append(" WHERE InternalStudyId = @StudyId AND ReconId = @ReconId");
        if (taskStatus != OfflineTaskStatus.Cancelled && taskStatus != OfflineTaskStatus.Error)
        {
            sb.Append(" And IsDeleted = 0");
        }
        var affectedRows = _context.Connection.ContextExecute(c => c.Execute(sb.ToString(), new
        {
            StudyId = studyId,
            ReconId = reconId,
            TaskStatus = (int)taskStatus,
            StartTime = startTime,
            EndTime = endTime
        }));
        return affectedRows == 1;
    }

    public bool UpdateReconTaskStatus((string ScanId, string ReconId) condtionFields, (OfflineTaskStatus TaskStatus, DateTime StartTime, DateTime EndTime) updateFields)
    {
        var sb = new StringBuilder();
        sb.Append("UPDATE t_recon_task SET TaskStatus = @TaskStatus");

        if (updateFields.StartTime != default(DateTime))
        {
            sb.Append(", ReconStartDate = @StartTime");
        }
        if (updateFields.EndTime != default(DateTime))
        {
            sb.Append(", ReconEndDate = @EndTime");
        }
        sb.Append(" WHERE ScanId = @ScanId AND ReconId = @ReconId");
        if (updateFields.TaskStatus != OfflineTaskStatus.Cancelled && updateFields.TaskStatus != OfflineTaskStatus.Error)
        {
            sb.Append(" And IsDeleted = 0");
        }
        var affectedRows = _context.Connection.ContextExecute(c => c.Execute(sb.ToString(), new
        {
            ScanId = condtionFields.ScanId,
            ReconId = condtionFields.ReconId,
            TaskStatus = (int)updateFields.TaskStatus,
            StartTime = updateFields.StartTime,
            EndTime = updateFields.EndTime
        }));
        return affectedRows == 1;
    }

    public bool Delete(string studyId, string scanId, string reconId)
    {
        string sql = "update t_recon_task set IsDeleted = 1 WHERE InternalStudyId = @StudyId and ScanId = @ScanId and ReconId = @ReconId";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                StudyId = studyId,
                ScanId = scanId,
                ReconId = reconId
            });
        }, _logger);
        return result == 1;
    }

    public bool Delete(ReconTaskEntity entity)
    {
        //_logger.LogInformation("Delete recon task");
        string sql = "update t_recon_task set IsDeleted = 1 WHERE Id = @ID";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                ID = entity.Id
            });
        }, _logger);
        return result == 1;
    }

    public bool DeleteByGuid(string reconGuid)
    {
        string sql = "delete from t_recon_task WHERE Id = @ID";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                ID = reconGuid
            });
        }, _logger);
        return result == 1;
    }

    public ReconTaskEntity Get(string Id)
    {
        string sql = "select Id,InternalStudyId,ScanId,FrameOfReferenceUid,IsRTD,ReconId,SeriesNumber,SeriesDescription,WindowWidth,WindowLevel,ReconStartDate,ReconEndDate,IssuingParameters,ActuralParameters,Description,InternalPatientId,TaskStatus,CreateTime,Creator,Remark from t_recon_task where Id = @ID";
        var entity = _context.Connection.ContextExecute((connection) =>
        {
            return connection.QueryFirstOrDefault<ReconTaskEntity>(sql, new { ID = Id });
        }, _logger);
        return entity;
    }
    public ReconTaskEntity GetOfflineRecon(string reconId)
    {
        string sql = "select t.Id,InternalStudyId,t.ScanId,t.FrameOfReferenceUid,t.IsRTD,t.ReconId,t.SeriesNumber,t.SeriesDescription,t.WindowWidth,t.WindowLevel,t.ReconStartDate,t.ReconEndDate,t.IssuingParameters,t.ActuralParameters,t.Description,t.InternalPatientId,t.TaskStatus,t.CreateTime,t.Creator,t.Remark,t2.PatientName from t_recon_task t left join t_patient t2 on t.InternalPatientId=t2.Id where ReconId = @ReconID";
        var entity = _context.Connection.ContextExecute((connection) =>
        {
            return connection.QueryFirstOrDefault<ReconTaskEntity>(sql, new { ReconID = reconId });
        }, _logger);
        return entity;
    }

    public ReconTaskEntity? Get(string studyId, string scanId, string reconId)
    {
        var sb = new StringBuilder();
        if (string.IsNullOrEmpty(scanId))
        {
            sb.Append("select Id,InternalStudyId,ScanId,FrameOfReferenceUid,IsRTD,ReconId,SeriesNumber,SeriesDescription,WindowWidth,WindowLevel,ReconStartDate,ReconEndDate,IssuingParameters,ActuralParameters,Description,InternalPatientId,TaskStatus,CreateTime,Creator,Remark from t_recon_task where InternalStudyId = @StudyId and ReconId = @ReconId And IsDeleted = 0");
        }
        else
        {
            sb.Append("select Id,InternalStudyId,ScanId,FrameOfReferenceUid,IsRTD,ReconId,SeriesNumber,SeriesDescription,WindowWidth,WindowLevel,ReconStartDate,ReconEndDate,IssuingParameters,ActuralParameters,Description,InternalPatientId,TaskStatus,CreateTime,Creator,Remark from t_recon_task where InternalStudyId = @StudyId and ScanId = @ScanId and ReconId = @ReconId And IsDeleted = 0");
        }

        var entity = _context.Connection.ContextExecute((connection) =>
        {
            return connection.QueryFirstOrDefault<ReconTaskEntity>(sb.ToString(), new
            {
                StudyId = studyId,
                ScanId = scanId,
                ReconId = reconId,
            });
        }, _logger);
        
        return entity;
    }

    public List<ReconTaskEntity> GetOfflineList()
    {
        var sql = "select t.Id,InternalStudyId,t.ScanId,t.FrameOfReferenceUid,t.IsRTD,t.ReconId,t.SeriesNumber,t.SeriesDescription,t.WindowWidth,t.WindowLevel,t.ReconStartDate,t.ReconEndDate,t.IssuingParameters,t.ActuralParameters,t.Description,t.InternalPatientId,t.TaskStatus,t.CreateTime,t.Creator,t.Remark,(case when t.TaskStatus=5 then 1 else 0 end) as Progress,t3.StudyInstanceUID,t2.PatientId,t2.PatientName from t_recon_task t left join t_patient t2 on t.InternalPatientId=t2.Id left join t_study t3 on t.InternalStudyId=t3.Id where t.IsRTD = 0 And t.IsDeleted = 0 and t.TaskStatus<>7 and t.TaskStatus<>5 and t.TaskStatus<>0 order by t.CreateTime";
        var entitys = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Query<ReconTaskEntity>(sql);
        }, _logger);
        return entitys.ToList();
    }

    public List<ReconTaskEntity> GetAll(string studyId)
    {
        string sql = "select Id,InternalStudyId,ScanId,FrameOfReferenceUid,IsRTD,ReconId,SeriesNumber,SeriesDescription,WindowWidth,WindowLevel,ReconStartDate,ReconEndDate,IssuingParameters,ActuralParameters,Description,InternalPatientId,TaskStatus,CreateTime,Creator,Remark from t_recon_task where InternalStudyId = @InternalStudyId";
        var entities = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Query<ReconTaskEntity>(sql, new { InternalStudyId = studyId });
        }, _logger);
        return entities.ToList();
    }

    public int GetLatestSeriesNumber(string studyId, int originalSeriesNumber)
    {
        string sql = "select max(SeriesNumber) from t_recon_task where InternalStudyId = @InternalStudyId and SeriesNumber % 1000 = @OriginalSeriesNumber;";
        var latestSeriesNumber = _context.Connection.ContextExecute((connection) =>
        {
            return connection.QueryFirstOrDefault<int>(sql, new { InternalStudyId = studyId, OriginalSeriesNumber = originalSeriesNumber });
        }, _logger);
        return latestSeriesNumber;
    }
}