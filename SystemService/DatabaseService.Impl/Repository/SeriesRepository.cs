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

using Dapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.CTS.Enums;
using System.Text;
using static Dapper.SqlMapper;
using System.Transactions;

namespace NV.CT.DatabaseService.Impl.Repository
{
    public class SeriesRepository //: ISeriesRepository
    {
        private readonly DatabaseContext _context;
        private readonly ILogger<SeriesRepository> _logger;

        public SeriesRepository(DatabaseContext context, ILogger<SeriesRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public bool Delete(string studyId, string scanId, string reconId)
        {
            //_logger.LogInformation("Delete series");
            string sql = "update t_series set IsDeleted = 1 WHERE InternalStudyId = @StudyId and ScanId = @ScanId and ReconId = @ReconId";
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

        public bool Delete(SeriesEntity entity)
        {
            //_logger.LogInformation("Delete series");
            string sql = "delete from t_series WHERE t_series.Id=@Id";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, new
                {
                    Id = entity.Id
                });
            }, _logger);

            return result == 1;
        }
        public async void DeleteDicomSeries(SeriesEntity entity)
        {
            if (entity.SeriesType == "image" && Directory.Exists(entity.SeriesPath))
            {
              await  SafeParallelDelete(entity.SeriesPath);
            }
        }
        public async Task SafeParallelDelete(string path, int maxDegree = 4)
        {
            await Task.Run(() =>
            {
                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxDegree
                };
                try
                {
                    Parallel.ForEach(Directory.GetFiles(path), options, file =>
                    {
                        File.Delete(file);
                    });
                }
                catch (AggregateException ae)
                {
                    foreach (var e in ae.InnerExceptions)
                    {
                        _logger?.LogError($"Failed to delete series {e.Message} . ");
                    }
                }
            });
        }

        public void DeleteByStudyId(string studyId)
        {
            //_logger.LogInformation("Delete series");
            string sql = "delete from t_series where InternalStudyId=@StudyId";
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Execute(sql, new
                {
                    StudyId = studyId
                });
            }, _logger);
        }

        public bool Add(SeriesEntity entity)
        {
            _logger.LogInformation($"Insert series: {JsonConvert.SerializeObject(entity)}");
            string sql = "insert into t_series (Id,InternalStudyId,BodyPart,SeriesInstanceUID,Modality,SeriesType,FrameOfReferenceUID,ImageType,SeriesNumber,SeriesDescription,WindowType,WindowWidth,WindowLevel,ReconStartDate,ReconEndDate,IsDeleted,PatientPosition,ProtocolName,ImageCount,ScanId,ReconId,ReportPath,ArchiveStatus,PrintStatus,IsProtected,CorrectStatus,SeriesPath) value(@Id,@InternalStudyId,@BodyPart,@SeriesInstanceUID,@Modality,@SeriesType,@FrameOfReferenceUID,@ImageType,@SeriesNumber,@SeriesDescription,@WindowType,@WindowWidth,@WindowLevel,@ReconStartDate,@ReconEndDate,@IsDeleted,@PatientPosition,@ProtocolName,@ImageCount,@ScanId,@ReconId,@ReportPath,@ArchiveStatus,@PrintStatus,@IsProtected,@CorrectStatus,@SeriesPath);";
            var result = _context.Connection.ContextExecute(connection => connection.Execute(sql, new
            {
                entity.Id,
                entity.InternalStudyId,
                entity.BodyPart,
                entity.SeriesInstanceUID,
                entity.Modality,
                entity.SeriesType,
                entity.FrameOfReferenceUID,
                entity.ImageType,
                entity.SeriesNumber,
                entity.SeriesDescription,
                entity.WindowType,
                entity.WindowWidth,
                entity.WindowLevel,
                entity.ReconStartDate,
                entity.ReconEndDate,
                entity.IsDeleted,
                entity.PatientPosition,
                entity.ProtocolName,
                entity.ImageCount,
                entity.ScanId,
                entity.ReconId,
                entity.ReportPath,
                entity.ArchiveStatus,
                entity.PrintStatus,
                entity.IsProtected,
                entity.CorrectStatus,
                entity.SeriesPath,
            }), _logger);
            return result == 1;
        }

        public bool AddSeriesList(List<SeriesEntity>? entityList)
        {

            if(entityList is null)
                return false;

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" INSERT INTO t_series (Id,InternalStudyId,BodyPart,SeriesInstanceUID,Modality,SeriesType,FrameOfReferenceUID,ImageType,SeriesNumber,SeriesDescription,WindowType,WindowWidth,WindowLevel,ReconStartDate,ReconEndDate,IsDeleted,PatientPosition,ProtocolName,ImageCount,ScanId,ReconId,ReportPath,ArchiveStatus,PrintStatus,IsProtected,CorrectStatus,SeriesPath) ");
            sqlBuilder.Append(" VALUE(@Id,@InternalStudyId,@BodyPart,@SeriesInstanceUID,@Modality,@SeriesType,@FrameOfReferenceUID,@ImageType,@SeriesNumber,@SeriesDescription,@WindowType,@WindowWidth,@WindowLevel,@ReconStartDate,@ReconEndDate,@IsDeleted,@PatientPosition,@ProtocolName,@ImageCount,@ScanId,@ReconId,@ReportPath,@ArchiveStatus,@PrintStatus,@IsProtected,@CorrectStatus,@SeriesPath);");
                        
            var result = _context.Connection.Execute(sqlBuilder.ToString(), entityList);
            if (result != entityList.Count)
            {
                _logger?.LogError($"Failed to save series.Affected count is {result}, does not equal seriesEntities'count {entityList.Count} ");
                return false;
            }

            return true;

        }

        public List<SeriesEntity> GetSeriesByStudyId(string studyId)
        {
            //_logger.LogInformation("Get series with study id");
            string sql = "select * from t_series s where s.InternalStudyId=@InternalStudyId and IsDeleted=0 order by s.ReconEndDate asc";
            var entities = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Query<SeriesEntity>(sql, new { InternalStudyId = studyId });
            }, _logger);
            return entities.ToList();
        }
        public List<string> GetSeriesIdsByStudyId(string studyId)
        {
            //_logger.LogInformation("GetSeriesIdsByStudyId");
            string sql = "select Id from t_series where InternalStudyId=@InternalStudyId order by ReconEndDate asc";
            var strings = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Query<string>(sql, new { InternalStudyId = studyId });
            }, _logger);
            return strings.ToList();
        }
        public string GetSeriesIdByStudyId(string studyId)
        {
            string sql = "SELECT a.Id from t_series a INNER JOIN t_recon_task b ON a.ReconId=b.ReconId  WHERE a.InternalStudyId=@InternalStudyId and a.ImageType='Tomo' and b.IsRTD=1 and a.IsDeleted=0;";
            var seriesId = _context.Connection.ContextExecute((connection) =>
            {
                return connection.QueryFirstOrDefault<string>(sql, new { InternalStudyId = studyId });
            }, _logger);
            return seriesId;
        }
        public List<SeriesEntity> GetTopoTomoSeriesByStudyId(string studyId)
        {
            string sql = "SELECT a.* from t_series a INNER JOIN t_recon_task b ON a.ReconId=b.ReconId  WHERE a.InternalStudyId=@InternalStudyId and (a.ImageType='Tomo' or a.ImageType='Topo') and b.IsRTD=1 and a.IsDeleted=0;";
            var entities = _context.Connection.ContextExecute((connection) =>
            {
                return connection.Query<SeriesEntity>(sql, new { InternalStudyId = studyId });
            }, _logger);
            return entities.ToList();
        }
        public string GetSeriesReportPathByStudyId(string studyId)
        {
            var reportPath = _context.Connection.ContextExecute((connection) =>
            {
                return connection.QueryFirstOrDefault<string>("SELECT ReportPath FROM t_series where InternalStudyId = @StudyId", new { StudyId = studyId });
            });
            return reportPath;

        }
        public bool UpdateArchiveStatus(List<SeriesEntity> seriesEntities)
        {
            return _context.Connection.ContextExecute((connection, transaction) =>
            {
                string sql = "Update t_series set ArchiveStatus=@ArchiveStatus Where Id=@Id";
                connection.Execute(sql, seriesEntities, transaction);
                return true;
            }, _logger);
        }

        public bool UpdatePrintStatus(List<SeriesEntity> seriesEntities)
        {
            return _context.Connection.ContextExecute((connection, transaction) =>
            {
                string sql = "Update t_series set PrintStatus=@PrintStatus Where Id=@Id";
                connection.Execute(sql, seriesEntities, transaction);
                return true;
            }, _logger);
        }

        public SeriesEntity GetSeriesById(string Id)
        {
            var entity = _context.Connection.ContextExecute((connection) =>
            {
                return connection.QueryFirstOrDefault<SeriesEntity>("select * from t_series where Id=@Id", new { Id = Id });
            });
            return entity;
        }
        public SeriesEntity GetSeriesBySeriesInstanceUID(string SeriesInstanceUID)
        {
            var entity = _context.Connection.ContextExecute((connection) =>
            {
                return connection.QueryFirstOrDefault<SeriesEntity>("select * from t_series where SeriesInstanceUID=@SeriesInstanceUID", new { SeriesInstanceUID = SeriesInstanceUID });
            });
            return entity;
        }
        public int GetSeriesCountByStudyId(string studyId)
        {
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.ExecuteScalar<int>("select COUNT(*) FROM t_series WHERE InternalStudyId=@InternalStudyId ", new { InternalStudyId = studyId });
            });
            return result;
        }
        public int GetArchiveStatusHasCompletedSeriesCountByStudyId(string studyId, string seriesId)
        {
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.ExecuteScalar<int>("select COUNT(*) FROM t_series WHERE InternalStudyId=@InternalStudyId AND Id!=@Id AND ArchiveStatus=@ArchiveStatus", new { InternalStudyId = studyId, Id = seriesId, ArchiveStatus = (int)JobTaskStatus.Completed }); //ArchiveStatus.Completed
            });
            return result;
        }

        public int GetPrintStatusHasCompletedSeriesCountByStudyId(string studyId, string seriesId)
        {
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.ExecuteScalar<int>("select COUNT(*) FROM t_series WHERE InternalStudyId=@InternalStudyId AND Id!=@Id AND PrintStatus=@PrintStatus", new { InternalStudyId = studyId, Id = seriesId, PrintStatus = (int)JobTaskStatus.Completed });
            });
            return result;
        }

        public int GetArchiveingSeriesCountByStudyId(string studyId, string seriesId)
        {
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.ExecuteScalar<int>("select COUNT(*) FROM t_series WHERE InternalStudyId=@InternalStudyId AND Id!=@Id AND ArchiveStatus=@ArchiveStatus", new { InternalStudyId = studyId, Id = seriesId, ArchiveStatus = (int)JobTaskStatus.Queued });
            });
            return result;
        }

        public int GetPrintingSeriesCountByStudyId(string studyId, string seriesId)
        {
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.ExecuteScalar<int>("select COUNT(*) FROM t_series WHERE InternalStudyId=@InternalStudyId AND Id!=@Id AND PrintStatus=@PrintStatus", new { InternalStudyId = studyId, Id = seriesId, PrintStatus = (int)JobTaskStatus.Processing });
            });
            return result;
        }

        public bool SetSeriesArchiveFail()
        {
            var result = _context.Connection.ContextExecute((connection) =>
            {
                return connection.ExecuteScalar<bool>("UPDATE t_series SET ArchiveStatus=5 WHERE ArchiveStatus=1 OR ArchiveStatus=2");
            });
            return result;
        }

        public SeriesEntity[] GetSeriesByIds(string[] ids)
        {
            var entities = _context.Connection.ContextExecute((connection) =>
            {
                string sql = "select * from t_series where Id in @Ids";
                return connection.Query<SeriesEntity>(sql, new { Ids = ids });
            });
            return entities.ToArray();
        }

        public SeriesEntity? GetSeriesByReconId(string reconId)
        {
            var entity = _context.Connection.ContextExecute((connection) =>
            {
                string sql = "select * from t_series where ReconId=@ReconId";
                return connection.QueryFirstOrDefault<SeriesEntity>(sql, new { ReconId = reconId });
            });
            return entity;
		}
        public SeriesEntity? GetScreenshotSeriesByImageType(string studyID,string imageType)
        {
            var entity = _context.Connection.ContextExecute((connection) =>
            {
                string sql = "select * from t_series where InternalStudyId=@InternalStudyId and ImageType=@ImageType and IsDeleted=0";
                return connection.QueryFirstOrDefault<SeriesEntity>(sql, new { InternalStudyId = studyID,ImageType=imageType });
            });
            return entity;
        }
        public bool UpdateScreenshotSeriesByImageType(SeriesEntity seriesEntity)
        {
            return  _context.Connection.ContextExecute((connection, transaction) =>
            {
                string sql = "update t_series set ImageCount=@ImageCount where InternalStudyId=@InternalStudyId and ImageType=@ImageType";
                connection.Execute(sql, seriesEntity, transaction);
                return true;
            },_logger);
        }


    }
}
