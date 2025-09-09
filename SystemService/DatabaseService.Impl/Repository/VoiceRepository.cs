//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/9/5 14:02:11           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Dapper;
using Microsoft.Extensions.Logging;
using NV.CT.DatabaseService.Contract.Models;
using static Dapper.SqlMapper;

namespace NV.CT.DatabaseService.Impl.Repository;

public class VoiceRepository
{
    private readonly DatabaseContext _context;
    private readonly ILogger<VoiceRepository> _logger;

    public VoiceRepository(DatabaseContext context, ILogger<VoiceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 获取全部语音信息
    /// </summary>
    /// <returns></returns>
    public List<VoiceModel> GetAll()
    {
        var sql = "select * from t_voices where IsValid = TRUE";
        return _context.Connection.ContextExecute((connection) =>
        {
            var entities = connection.Query<VoiceModel>(sql);
            return entities.ToList();
        }, _logger);
    }

    /// <summary>
    /// 根据条件获取语音信息
    /// </summary>
    /// <returns></returns>
    public List<VoiceModel> Get(bool isValid)
    {
        var sql = $"select * from t_voices where IsValid = @IsValid";

        return _context.Connection.ContextExecute((connection) =>
        {
            var entities = connection.Query<VoiceModel>(sql, new { IsValid = isValid });
            return entities.ToList();
        }, _logger);
    }

    /// <summary>
    /// 获取默认语音信息列表
    /// </summary>
    /// <returns></returns>
    public List<VoiceModel> GetDefaultList()
    {
        string sql = "SELECT * FROM t_voices WHERE IsValid = TRUE AND IsDefault=TRUE";
        return _context.Connection.ContextExecute((connection) =>
        {
            var entities = connection.Query<VoiceModel>(sql);
            return entities.ToList();
        }, _logger);
    }

    /// <summary>
    /// 设置默认语音列表新信息到数据库
    /// </summary>
    /// <param name="list">默认语音信息列表</param>
    /// <returns></returns>
    public bool SetDefaultList(List<VoiceModel> list)
    {
        return _context.Connection.ContextExecute((connection, transaction) =>
        {
            var result = connection.Execute("UPDATE t_voices SET IsDefault = FALSE;UPDATE t_voices SET IsDefault=TRUE WHERE InternalId=@InternalId", list, transaction);
            return result > 0;
        }, _logger);
    }

    public bool SetDefault(VoiceModel model)
    {
        if (model.InternalId <= 0) return false;

        string sql = $"UPDATE t_voices p SET p.IsDefault=FALSE WHERE p.IsFront=@IsFront and p.IsDefault=True;";
        sql += $"UPDATE t_voices p SET p.IsDefault=TRUE WHERE p.InternalId=@InternalId;";

        return _context.Connection.ContextExecute((connection, transaction) =>
        {
            var res = connection.Execute(sql, new
            {
                IsFront = model.IsFront,
                InternalId = model.InternalId
            }, transaction);
            return res > 0;
        }, _logger);
    }

    public bool Insert(VoiceModel entity)
    {
        string sql = "INSERT INTO t_voices (Id,InternalId,Name,Description,BodyPart,FilePath,IsFront,VoiceLength,Language,IsFactory,IsDefault,IsValid,Creator,CreateTime) " +
            "VALUE(@Id,@InternalId,@Name,@Description,@BodyPart,@FilePath,@IsFront,@VoiceLength,@Language,@IsFactory,@IsDefault,@IsValid,@Creator,@CreateTime);";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                Id = entity.Id,
                InternalId = entity.InternalId,
                Name = entity.Name,
                Description = entity.Description,
                BodyPart = entity.BodyPart,
                FilePath = entity.FilePath,
                IsFront = entity.IsFront,
                VoiceLength = entity.VoiceLength,
                Language = entity.Language,
                IsFactory = entity.IsFactory,
                IsDefault = entity.IsDefault,
                IsValid = entity.IsValid,
                Creator = entity.Creator,
                CreateTime = entity.CreateTime
            });
        }, _logger);
        return result == 1;
    }

    public bool Update(VoiceModel entity)
    {
        string sql = "UPDATE t_voices p SET  p.InternalId=@InternalId,p.Name=@Name,p.Description=@Description,p.BodyPart=@BodyPart,p.FilePath=@FilePath,p.IsFront=@IsFront,p.VoiceLength=@VoiceLength,p.Language=@Language,p.IsFactory=@IsFactory,p.IsDefault=@IsDefault,p.IsValid=@IsValid WHERE p.Id=@Id";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                InternalId = entity.InternalId,
                Name = entity.Name,
                Description = entity.Description,
                BodyPart = entity.BodyPart,
                FilePath = entity.FilePath,
                IsFront = entity.IsFront,
                VoiceLength = entity.VoiceLength,
                Language = entity.Language,
                IsFactory = entity.IsFactory,
                IsDefault = entity.IsDefault,
                IsValid = entity.IsValid,
                Id = entity.Id,
            });
        }, _logger);
        return result == 1;
    }

    public bool Delete(VoiceModel entity)
    {
        string sql = "DELETE FROM t_voices WHERE Id =@Id";
        var result = _context.Connection.ContextExecute((connection) =>
        {
            return connection.Execute(sql, new
            {
                Id = entity.Id
            });
        }, _logger);
        return result == 1;
    }

    public VoiceModel GetVoiceInfo(string id)
    {
        var sql = $"select * from t_voices where IsValid = TRUE and InternalId = @Id";
        return _context.Connection.ContextExecute((connection) =>
        {
            return connection.QueryFirst<VoiceModel>(sql, new { Id = id });
        }, _logger);
    }

    public List<VoiceModel> GetAllByFrontType(string front)
    {
        var sql = "select * from t_voices where IsValid = TRUE";
        var sqlFrount = "select * from t_voices where IsValid = TRUE and IsFront = @IsFront";

        return _context.Connection.ContextExecute((connection) =>
        {
            if (!string.IsNullOrEmpty(front))
            {
                var entities = connection.Query<VoiceModel>(sqlFrount, new { IsFront = bool.Parse(front) });
                return entities.ToList();
            }
            else
            {
                var entities = connection.Query<VoiceModel>(sql);
                return entities.ToList();
            }
        }, _logger);
    }
}