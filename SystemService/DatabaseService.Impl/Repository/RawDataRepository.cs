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
using static Dapper.SqlMapper;

namespace NV.CT.DatabaseService.Impl.Repository
{
    public class RawDataRepository //: IRawDataRepository
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<RawDataRepository> _logger;

        public RawDataRepository(DatabaseContext context, ILogger<RawDataRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool Add(RawDataEntity entity)
        {
            string sql = "INSERT INTO t_raw_data (Id,InternalStudyId,FrameOfReferenceUID,ScanId,ScanName,BodyPart,ProtocolName,PatientPosition,ScanModel,ScanEndTime,Path,IsExported,IsDeleted,Creator,CreateTime) VALUE (@Id,@InternalStudyId,@FrameOfReferenceUID,@ScanId,@ScanName,@BodyPart,@ProtocolName,@PatientPosition,@ScanModel,@ScanEndTime,@Path,@IsExported,@IsDeleted,@Creator,@CreateTime);";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, new
                {
                    entity.Id,
                    entity.InternalStudyId,
                    entity.FrameOfReferenceUID,
                    entity.ScanId,
                    entity.ScanName,
                    entity.BodyPart,
                    entity.ProtocolName,
                    entity.PatientPosition,
                    entity.ScanModel,
                    entity.ScanEndTime,
                    entity.Path,
                    entity.IsExported,
                    entity.IsDeleted,
                    entity.Creator,
                    entity.CreateTime,
                });
            }, _logger);
            return result > 0;
        }

        public bool Update(RawDataEntity entity)
        {
            string sql = "UPDATE t_raw_data SET InternalStudyId=@InternalStudyId,FrameOfReferenceUID=@FrameOfReferenceUID,ScanId=@ScanId,ScanName=@ScanName,BodyPart=@BodyPart,ProtocolName=@ProtocolName,PatientPosition=@PatientPosition,ScanModel=@ScanModel,ScanEndTime=@ScanEndTime,Path=@Path,IsExported=@IsExported,IsDeleted=@IsDeleted,Creator=@Creator,CreateTime=@CreateTime WHERE Id=@Id";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, new
                {
                    entity.Id,
                    entity.InternalStudyId,
                    entity.FrameOfReferenceUID,
                    entity.ScanId,
                    entity.ScanName,
                    entity.BodyPart,
                    entity.ProtocolName,
                    entity.PatientPosition,
                    entity.ScanModel,
                    entity.ScanEndTime,
                    entity.Path,
                    entity.IsExported,
                    entity.IsDeleted,
                    entity.Creator,
                    entity.CreateTime,
                });
            }, _logger);
            return result > 0;
        }

        public bool Delete(string Id)
        {
            string sql = "DELETE FROM t_raw_data WHERE Id=@Id";
            var result = _context.Connection.ContextExecute((connection, transaction) =>
            {                
                return connection.Execute(sql, new
                {
                    Id = Id,

                }, transaction);

            });

            return result > 0;
        }

        public List<RawDataEntity> GetRawDataListByStudyId(string studyId)
        {
            var entityList = _context.Connection.ContextExecute((connection) =>
            {
                string fetchSql = "SELECT Id,InternalStudyId,FrameOfReferenceUID,ScanId,ScanName,BodyPart,ProtocolName,PatientPosition,ScanModel,ScanEndTime,Path,IsExported,IsDeleted,Creator,CreateTime FROM t_raw_data WHERE IsDeleted=0 AND InternalStudyId=@StudyId ORDER BY ScanEndTime ASC;";

                return connection.Query<RawDataEntity>(fetchSql, new { StudyId = studyId });
            }, _logger);

            return new List<RawDataEntity>(entityList);
        }

        public bool UpdateExportStatusById(string id, bool isExported)
        {  
            var result = _context.Connection.ContextExecute((connection, transaction) =>
            {
                string sql = "UPDATE t_raw_data SET IsExported=@IsExported WHERE Id=@Id";
                return connection.Execute(sql, new
                {
                    Id = id,
                    IsExported = isExported
                });
            }, _logger);
            return result > 0;
        }

    }
}
