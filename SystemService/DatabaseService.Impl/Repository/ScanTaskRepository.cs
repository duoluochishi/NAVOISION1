//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using Dapper;
using Microsoft.Extensions.Logging;
using NV.CT.DatabaseService.Contract.Entities;

namespace NV.CT.DatabaseService.Impl.Repository;
public class ScanTaskRepository //: IScanTaskRepository
{
    private readonly DatabaseContext _context;
    private readonly ILogger<ScanTaskRepository> _logger;

    public ScanTaskRepository(DatabaseContext context, ILogger<ScanTaskRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public bool Insert(ScanTaskEntity entity)
    {
        //_logger.LogInformation("Insert scan task");
        string sql = "insert into t_scan_task (Id,InternalStudyId,MeasurementId,FrameOfReferenceUid,ScanId,ScanStartDate,ScanEndDate,CreateTime,Creator,IssuingParameters,Description,InternalPatientId,BodySize,TopoScanId,IsLinkScan,IsInject,ActuralParameters,BodyPart,ScanOption) " +
            "value(@Id,@InternalStudyId,@MeasurementId,@FrameOfReferenceUid,@ScanId,@ScanStartDate,@ScanEndDate,@CreateTime,@Creator,@IssuingParameters,@Description,@InternalPatientId,@BodySize,@TopoScanId,@IsLinkScan,@IsInject,@ActuralParameters,@BodyPart,@ScanOption);";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                Id = entity.Id,
                InternalStudyId = entity.InternalStudyId,
                MeasurementId = entity.MeasurementId,
                FrameOfReferenceUid = entity.FrameOfReferenceUid,
                ScanId = entity.ScanId,
                BodyPart = entity.BodyPart,
                ScanOption = entity.ScanOption,
                ScanStartDate = entity.ScanStartDate,
                ScanEndDate = entity.ScanEndDate,
                CreateTime = entity.CreateTime,
                Creator = entity.Creator,
                IssuingParameters = entity.IssuingParameters,
                Description = entity.Description,
                InternalPatientId = entity.InternalPatientId,
                BodySize = entity.BodySize,
                IsLinkScan = entity.IsLinkScan,
                IsInject = entity.IsInject,
                TopoScanId = entity.TopoScanId,
                ActuralParameters = entity.ActuralParameters,
            });
        }, _logger);
        return result == 1;
    }

    public bool Insert(List<ScanTaskEntity> list)
    {
        //_logger.LogInformation("Insert scan task");
        string sql = "insert into t_scan_task (Id,InternalStudyId,MeasurementId,FrameOfReferenceUid,ScanId,ScanStartDate,ScanEndDate,CreateTime,Creator,IssuingParameters,Description,InternalPatientId,BodySize,TopoScanId,IsLinkScan,IsInject,ActuralParameters,BodyPart,ScanOption) " +
            "value(@Id,@InternalStudyId,@MeasurementId,@FrameOfReferenceUid,@ScanId,@ScanStartDate,@ScanEndDate,@CreateTime,@Creator,@IssuingParameters,@Description,@InternalPatientId,@BodySize,@TopoScanId,@IsLinkScan,@IsInject,@ActuralParameters,@BodyPart,@ScanOption);";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, list);
        }, _logger);
        return result == 1;
    }

    public bool Update(ScanTaskEntity entity)
    {
        //_logger.LogInformation("Update scan task");
        string sql = "UPDATE t_scan_task SET InternalStudyId=@InternalStudyId,MeasurementId=@MeasurementId,FrameOfReferenceUid=@FrameOfReferenceUid,ScanId=@ScanId,BodyPart=@BodyPart,ScanOption=@ScanOption," +
            "ScanStartDate=@ScanStartDate,ScanEndDate=@ScanEndDate,CreateTime=@CreateTime,Creator=@Creator,IssuingParameters=@IssuingParameters,ActuralParameters=@ActuralParameters,Description=@Description," +
            "InternalPatientId=@InternalPatientId,BodySize=@BodySize,IsLinkScan=@IsLinkScan,IsInject=@IsInject,TopoScanId=@TopoScanId WHERE Id=@Id";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                Id = entity.Id,
                InternalStudyId = entity.InternalStudyId,
                MeasurementId = entity.MeasurementId,
                FrameOfReferenceUid = entity.FrameOfReferenceUid,
                ScanId = entity.ScanId,
                BodyPart = entity.BodyPart,
                ScanOption = entity.ScanOption,
                ScanStartDate = entity.ScanStartDate,
                ScanEndDate = entity.ScanEndDate,
                CreateTime = entity.CreateTime,
                Creator = entity.Creator,
                IssuingParameters = entity.IssuingParameters,
                Description = entity.Description,
                TaskStatus = entity.TaskStatus,
                InternalPatientId = entity.InternalPatientId,
                BodySize = entity.BodySize,
                IsLinkScan = entity.IsLinkScan,
                IsInject = entity.IsInject,
                TopoScanId = entity.TopoScanId,
                ActuralParameters = entity.ActuralParameters,
            });
        }, _logger);
        return result == 1;
    }

    public bool UpdateStatus(ScanTaskEntity entity)
    {
        //_logger.LogInformation("Insert scan task status");
        string sql = "UPDATE t_scan_task SET IssuingParameters=@IssuingParameters,ActuralParameters=@ActuralParameters WHERE InternalStudyId=@InternalStudyId and MeasurementId=@MeasurementId and FrameOfReferenceUid=@FrameOfReferenceUid and ScanId=@ScanId";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                InternalStudyId = entity.InternalStudyId,
                MeasurementId = entity.MeasurementId,
                FrameOfReferenceUid = entity.FrameOfReferenceUid,
                ScanId = entity.ScanId,
                IssuingParameters = entity.IssuingParameters,
                ActuralParameters = entity.ActuralParameters,
            });
        }, _logger);
        return result == 1;
    }

    public bool Delete(ScanTaskEntity entity)
    {
        //_logger.LogInformation("Delete scan task");
        string sql = "delete from t_scan_task  WHERE t_scan_task.Id=@ID";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                ID = entity.Id
            });
        }, _logger);
        return result == 1;
    }

    public ScanTaskEntity Get(string Id)
    {
        string sql = "select Id,InternalStudyId,MeasurementId,FrameOfReferenceUid,ScanId,BodyPart,ScanOption,ScanStartDate,ScanEndDate,CreateTime,Creator,IssuingParameters,ActuralParameters,Description,TaskStatus,InternalPatientId,BodySize,IsLinkScan,IsInject,TopoScanId from t_scan_task where Id = @ID";
        var entity = _context.Connection.ContextExecute((connection) =>
        {
            var entities = connection.Query<ScanTaskEntity>(sql, new { ID = Id });
            return entities.FirstOrDefault();
        }, _logger);
        return entity ?? new ScanTaskEntity();
    }

    public ScanTaskEntity Get(string studyID, string scanRangeID)
    {
        string sql = "select Id,InternalStudyId,MeasurementId,FrameOfReferenceUid,ScanId,BodyPart,ScanOption,ScanStartDate,ScanEndDate,CreateTime,Creator,IssuingParameters,ActuralParameters,Description,InternalPatientId,BodySize,IsLinkScan,IsInject,TopoScanId from t_scan_task where InternalStudyId=@InternalStudyId and ScanId=@ScanId";
        var entity = _context.Connection.ContextExecute((connection) =>
        {
            var entities = connection.Query<ScanTaskEntity>(sql, new
            {
                InternalStudyId = studyID,
                ScanId = scanRangeID,
            });
            return entities.FirstOrDefault();
        }, _logger);
        return entity ?? new ScanTaskEntity();
    }

    public ScanTaskEntity Get(string studyID, string measurementId, string frameOfReferenceUid, string scanRangeID)
    {
        string sql = "select Id,InternalStudyId,MeasurementId,FrameOfReferenceUid,ScanId,BodyPart,ScanOption,ScanStartDate,ScanEndDate,CreateTime,Creator,IssuingParameters,ActuralParameters,Description,InternalPatientId,BodySize,IsLinkScan,IsInject,TopoScanId from t_scan_task where InternalStudyId=@InternalStudyId and MeasurementId=@MeasurementId and FrameOfReferenceUid=@FrameOfReferenceUid and ScanId=@ScanId";
        var entity = _context.Connection.ContextExecute((connection) =>
        {
            var entities = connection.Query<ScanTaskEntity>(sql, new
            {
                InternalStudyId = studyID,
                MeasurementId = measurementId,
                FrameOfReferenceUid = frameOfReferenceUid,
                ScanId = scanRangeID,
            });
            return entities.FirstOrDefault();
        }, _logger);
        return entity ?? new ScanTaskEntity();
    }

    public List<ScanTaskEntity> GetAll(string studyId)
    {
        string sql = "select Id,InternalStudyId,MeasurementId,FrameOfReferenceUid,ScanId,BodyPart,ScanOption,ScanStartDate,ScanEndDate,CreateTime,Creator,IssuingParameters,ActuralParameters,Description,InternalPatientId,BodySize,IsLinkScan,IsInject,TopoScanId from t_scan_task where InternalStudyId=@InternalStudyId";
        var entities = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Query<ScanTaskEntity>(sql, new { InternalStudyId = studyId });
        }, _logger);
        return entities.ToList();
    }
}