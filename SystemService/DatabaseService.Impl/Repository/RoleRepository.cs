//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/2 16:14:52           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.DatabaseService.Contract.Entities;
using System.Text;
using static Dapper.SqlMapper;

namespace NV.CT.DatabaseService.Impl.Repository;

public class RoleRepository
{
	private readonly DatabaseContext _context;
	private readonly ILogger<RoleRepository> _logger;

	public RoleRepository(DatabaseContext context, ILogger<RoleRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public bool Insert(RoleEntity entity)
	{
		string sql = "INSERT INTO t_roles (Id,Name,Description,Level,IsFactory,IsDeleted,Creator,CreateTime) " +
					 "VALUE(@Id,@Name,@Description,@Level,@IsFactory,@IsDeleted,@Creator,@CreateTime);";
		var result = _context.Connection.ContextExecute((connection) =>
		{
			return connection.Execute(sql, new
			{
				Id = entity.Id,
				Name = entity.Name,
				Description = entity.Description,
				Level = entity.Level,
				IsFactory = entity.IsFactory,
				IsDeleted = entity.IsDeleted,
				Creator = entity.Creator,
				CreateTime = entity.CreateTime
			});
		}, _logger);
		return result == 1;
	}

	public bool Update(RoleEntity entity)
	{
		string sql = "UPDATE t_roles p SET  p.Name=@Name,p.Description=@Description,p.Level=@Level,p.IsFactory=@IsFactory,p.IsDeleted=@IsDeleted WHERE p.Id=@Id";
		var result = _context.Connection.ContextExecute((connection) =>
		{
			return connection.Execute(sql, new
			{
				Name = entity.Name,
				Description = entity.Description,
				Level = entity.Level,
				IsFactory = entity.IsFactory,
				IsDeleted = entity.IsDeleted,
				Id = entity.Id,
			});
		}, _logger);
		return result == 1;
	}

	public bool Delete(RoleEntity entity)
	{
		return _context.Connection.ContextExecute((connection, transaction) =>
		{
			string sql = "DELETE FROM t_role_permission WHERE RoleId=@RoleId;";
			connection.Execute(sql, new
			{
				RoleId = entity.Id
			}, transaction);
			sql = "DELETE FROM t_user_role WHERE RoleId=@RoleId;";
			connection.Execute(sql, new
			{
				RoleId = entity.Id
			}, transaction);
			sql = "UPDATE t_roles p SET p.IsDeleted=TRUE WHERE p.Id=@Id";
			connection.Execute(sql, new
			{
				Id = entity.Id
			}, transaction);
			return true;
		}, _logger);
	}

	public List<RoleEntity> GetAll()
	{
		string sql = "SELECT r.Id,r.Name,r.Description,r.IsFactory,r.Creator,r.CreateTime,ur.UserCount FROM t_roles r left join (SELECT count(UserId) UserCount,RoleId FROM t_user_role group by RoleId) ur on r.Id=ur.RoleId WHERE r.Level<2 and r.IsDeleted = FALSE;";
		return _context.Connection.ContextExecute((connection) =>
		{
			var entities = connection.Query<RoleEntity>(sql);
			return entities.ToList();
		}, _logger);
	}

	public (RoleEntity, List<PermissionEntity>) GetRoleById(string roleId)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT r.Id,r.Name,r.Description,r.IsFactory,r.Creator,r.CreateTime,ur.UserCount FROM t_roles r left join (SELECT count(UserId) UserCount,RoleId FROM t_user_role group by RoleId) ur on r.Id=ur.RoleId WHERE r.IsDeleted = FALSE AND r.Id = @RoleID;");
		sb.AppendLine("SELECT p.Id,p.Code,p.Name,p.Description,p.Category,p.Level,p.IsDeleted,p.Creator,p.CreateTime FROM t_role_permission r LEFT JOIN t_permissions p ON r.PermissionCode=p.Code WHERE p.IsDeleted = FALSE AND r.RoleId=@RoleID;");
		return _context.Connection.ContextExecute<(RoleEntity, List<PermissionEntity>)>((connection) =>
		{
			var data = connection.QueryMultiple(sb.ToString(), new { RoleID = roleId });
			var roleEntity = data.ReadFirstOrDefault<RoleEntity>()??new RoleEntity();
			var permissions = data.Read<PermissionEntity>().Distinct().ToList();
			return (roleEntity, permissions);
		});
	}

	public (RoleEntity, List<PermissionEntity>) GetRoleByName(string name)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT r.Id,r.Name,r.Description,r.IsFactory,r.Creator,r.CreateTime,ur.UserCount FROM t_roles r left join (SELECT count(UserId) UserCount,RoleId FROM t_user_role group by RoleId) ur on r.Id=ur.RoleId WHERE r.IsDeleted = FALSE AND r.Name=@name;");
		sb.AppendLine("SELECT p.Id,p.Code,p.Name,p.Description,p.Category,p.Level,p.IsDeleted,p.Creator,p.CreateTime FROM t_role_permission r LEFT JOIN t_roles tr on r.RoleId=tr.Id LEFT JOIN t_permissions p ON r.PermissionCode=p.Code WHERE p.IsDeleted = FALSE AND tr.Name=@name;");
		return _context.Connection.ContextExecute<(RoleEntity, List<PermissionEntity>)>((connection) =>
		{
			var data = connection.QueryMultiple(sb.ToString(), new { name = name });
			var roleEntity = data.ReadFirstOrDefault<RoleEntity>() ?? new RoleEntity();
			var permissions = data.Read<PermissionEntity>().Distinct().ToList();
			return (roleEntity, permissions);
		});
	}

    public bool InsertRoleAndRight(RoleEntity entity, List<PermissionEntity> permissionEntities)
	{
		return _context.Connection.ContextExecute((connection, transaction) =>
		{
			string sql = "INSERT INTO t_roles (Id,Name,Description,Level,IsFactory,IsDeleted,Creator,CreateTime) " +
					 "VALUE(@Id,@Name,@Description,@Level,@IsFactory,@IsDeleted,@Creator,@CreateTime);";
			connection.Execute(sql, new
			{
				Id = entity.Id,
				Name = entity.Name,
				Description = entity.Description,
				Level = entity.Level,
				IsFactory = entity.IsFactory,
				IsDeleted = entity.IsDeleted,
				Creator = entity.Creator,
				CreateTime = entity.CreateTime
			});
			sql = $"INSERT INTO t_role_permission(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime) ";
			sql += "VALUE(@Id,@RoleId,@PermissionId,@PermissionCode,@Creator,@CreateTime);";
			connection.Execute(sql, from permission in permissionEntities
									select new
									{
										Id = Guid.NewGuid(),
										RoleId = entity.Id,
										PermissionId = permission.Id,
										PermissionCode = permission.Code,
										Creator = "",
										CreateTime = DateTime.Now
									}, transaction);

			return true;
		}, _logger);
	}

	public bool UpdateRoleAndRight(RoleEntity entity, List<PermissionEntity> permissionEntities)
	{
		return _context.Connection.ContextExecute((connection, transaction) =>
		{
			string sql = "UPDATE t_roles p SET p.Name=@Name,p.Description=@Description,p.Level=@Level,p.IsFactory=@IsFactory,p.IsDeleted=@IsDeleted WHERE p.Id=@Id";
			connection.Execute(sql, new
			{
				Id = entity.Id,
				Name = entity.Name,
				Description = entity.Description,
				Level = entity.Level,
				IsFactory = entity.IsFactory,
				IsDeleted = entity.IsDeleted,
				Creator = entity.Creator,
				CreateTime = entity.CreateTime
			});
			sql = "delete from t_role_permission where RoleId=@RoleId;";
			connection.Execute(sql, new
			{
				RoleId = entity.Id
			});
			sql = $"INSERT INTO t_role_permission(Id, RoleId, PermissionId, PermissionCode, Creator, CreateTime) ";
			sql += "VALUE(@Id,@RoleId,@PermissionId,@PermissionCode,@Creator,@CreateTime);";

			connection.Execute(sql, from permission in permissionEntities
									select new
									{
										Id = Guid.NewGuid(),
										RoleId = entity.Id,
										PermissionId = permission.Id,
										PermissionCode = permission.Code,
										Creator = entity.Creator,
										CreateTime = DateTime.Now
									}, transaction);

			return true;
		}, _logger);
	}
}