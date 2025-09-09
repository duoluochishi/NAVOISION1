//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/8/30 14:31:08           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Dapper;
using Microsoft.Extensions.Logging;
using NV.CT.DatabaseService.Contract.Entities;

namespace NV.CT.DatabaseService.Impl.Repository
{
    public class DoseCheckRepository
    {
        private DatabaseContext _context;
        private readonly ILogger<DoseCheckRepository> _logger;

        public DoseCheckRepository(DatabaseContext context, ILogger<DoseCheckRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool Insert(DoseCheckEntity entity)
        {
            string sql = "INSERT INTO t_dose_check (Id,InternalStudyId,InternalPatientId,FrameOfReferenceId,MeasurementId,ScanId,DoseCheckType,WarningCTDI,WarningDLP,CurrentCTDI,CurrentDLP,Operator,Reason,CreateTime) VALUE(@Id,@InternalStudyId,@InternalPatientId,@FrameOfReferenceId,@MeasurementId,@ScanId,@DoseCheckType,@WarningCTDI,@WarningDLP,@CurrentCTDI,@CurrentDLP,@Operator,@Reason,@CreateTime);";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, new
                {
                    Id = entity.Id,
                    InternalStudyId = entity.InternalStudyId,
                    InternalPatientId = entity.InternalPatientId,
                    MeasurementId = entity.MeasurementId,
                    FrameOfReferenceId = entity.FrameOfReferenceId,
                    ScanId = entity.ScanId,
                    DoseCheckType = $"{entity.DoseCheckType}",
                    WarningCTDI = entity.WarningCTDI,
                    WarningDLP = entity.WarningDLP,
                    CurrentCTDI = entity.CurrentCTDI,
                    CurrentDLP = entity.CurrentDLP,
                    Operator = entity.Operator,
                    Reason = entity.Reason,
                    CreateTime = entity.CreateTime
                });
            }, _logger);
            return result == 1;
        }

        public bool Insert(List<DoseCheckEntity> entitys)
        {
            string sql = "INSERT INTO t_dose_check (Id,InternalStudyId,InternalPatientId,FrameOfReferenceId,MeasurementId,ScanId,DoseCheckType,WarningCTDI,WarningDLP,CurrentCTDI,CurrentDLP,Operator,Reason,CreateTime) VALUE(@Id,@InternalStudyId,@InternalPatientId,@FrameOfReferenceId,@MeasurementId,@ScanId,@DoseCheckType,@WarningCTDI,@WarningDLP,@CurrentCTDI,@CurrentDLP,@Operator,@Reason,@CreateTime);";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, from entity in entitys
                                               select new
                                               {
                                                   Id = entity.Id,
                                                   InternalStudyId = entity.InternalStudyId,
                                                   InternalPatientId = entity.InternalPatientId,
                                                   MeasurementId = entity.MeasurementId,
                                                   FrameOfReferenceId = entity.FrameOfReferenceId,
                                                   ScanId = entity.ScanId,
                                                   DoseCheckType = $"{entity.DoseCheckType}",
                                                   WarningCTDI = entity.WarningCTDI,
                                                   WarningDLP = entity.WarningDLP,
                                                   CurrentCTDI = entity.CurrentCTDI,
                                                   CurrentDLP = entity.CurrentDLP,
                                                   Operator = entity.Operator,
                                                   Reason = entity.Reason,
                                                   CreateTime = entity.CreateTime
                                               });
            }, _logger);
            return result >= 1;
        }

        public bool Update(DoseCheckEntity entity)
        {
            string sql = "UPDATE t_dose_check p SET  p.InternalStudyId=@InternalStudyId,p.InternalPatientId=@InternalPatientId,p.FrameOfReferenceId=@FrameOfReferenceId,p.MeasurementId=@MeasurementId.p.ScanId=@ScanId,p.DoseCheckType=@DoseCheckType,p.WarningCTDI=@WarningCTDI,p.WarningDLP=@WarningDLP,p.CurrentCTDI=@CurrentCTDI,p.CurrentDLP=@CurrentDLP,p.Operator=@Operator,p.Reason=@Reason WHERE p.Id=@Id";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, new
                {
                    InternalStudyId = entity.InternalStudyId,
                    InternalPatientId = entity.InternalPatientId,
                    FrameOfReferenceId = entity.FrameOfReferenceId,
                    MeasurementId = entity.MeasurementId,
                    ScanId = entity.ScanId,
                    DoseCheckType = $"{entity.DoseCheckType}",
                    WarningCTDI = entity.WarningCTDI,
                    WarningDLP = entity.WarningDLP,
                    CurrentCTDI = entity.CurrentCTDI,
                    CurrentDLP = entity.CurrentDLP,
                    Operator = entity.Operator,
                    Reason = entity.Reason,
                    Id = entity.Id,
                });
            }, _logger);
            return result == 1;
        }

        public bool Update(List<DoseCheckEntity> entitys)
        {
            string sql = "UPDATE t_dose_check p SET  p.InternalStudyId=@InternalStudyId,p.InternalPatientId=@InternalPatientId,p.FrameOfReferenceId=@FrameOfReferenceId,p.MeasurementId=@MeasurementId.p.ScanId=@ScanId,p.DoseCheckType=@DoseCheckType,p.WarningCTDI=@WarningCTDI,p.WarningDLP=@WarningDLP,p.CurrentCTDI=@CurrentCTDI,p.CurrentDLP=@CurrentDLP,p.Operator=@Operator,p.Reason=@Reason WHERE p.Id=@Id";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, from entity in entitys
                                               select new
                                               {
                                                   InternalStudyId = entity.InternalStudyId,
                                                   InternalPatientId = entity.InternalPatientId,
                                                   FrameOfReferenceId = entity.FrameOfReferenceId,
                                                   MeasurementId = entity.MeasurementId,
                                                   ScanId = entity.ScanId,
                                                   DoseCheckType = $"{entity.DoseCheckType}",
                                                   WarningCTDI = entity.WarningCTDI,
                                                   WarningDLP = entity.WarningDLP,
                                                   CurrentCTDI = entity.CurrentCTDI,
                                                   CurrentDLP = entity.CurrentDLP,
                                                   Operator = entity.Operator,
                                                   Reason = entity.Reason,
                                                   Id = entity.Id,
                                               });
            }, _logger);
            return result == 1;
        }

        public bool Delete(DoseCheckEntity entity)
        {
            string sql = "DELETE FROM t_dose_check WHERE Id =@Id";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, new
                {
                    ID = entity.Id
                });
            }, _logger);
            return result == 1;
        }

        public DoseCheckEntity Get(string doseCheckId)
        {
            var entity = _context.Connection.ContextExecute((connection) =>
            {
                return connection.QueryFirstOrDefault<DoseCheckEntity>("SELECT * FROM t_dose_check WHERE Id = @Id ", new { Id = doseCheckId });
            }, _logger);
            return entity;
        }

        public List<DoseCheckEntity> GetAll()
        {
            var entity = _context.Connection.ContextExecute((connection) =>
            {
                return connection.QueryFirstOrDefault<List<DoseCheckEntity>>("SELECT * FROM t_dose_check ORDER BY CreateTime DESC");
            }, _logger);
            return entity;
        }
    }
}