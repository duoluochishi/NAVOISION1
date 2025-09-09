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

using Dapper;
using Microsoft.Extensions.Logging;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.Models;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using static Dapper.SqlMapper;

namespace NV.CT.DatabaseService.Impl.Repository;

public class UserRepository
{
	private readonly DatabaseContext _context;
	private readonly ILogger<UserRepository> _logger;

	public UserRepository(DatabaseContext context, ILogger<UserRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public bool Insert(UserEntity entity)
	{
		string sql = "INSERT INTO t_users (Id,Account,Password,FirstName,LastName,Sex,Comments,IsLocked,IsDeleted,Creator,CreateTime) " +
						 "VALUE(@Id,@Account,@Password,@FirstName,@LastName,@Sex,@Comments,@IsLocked,@IsDeleted,@Creator,@CreateTime);";
		var result = _context.Connection.ContextExecute((connection) =>
		{
			return connection.Execute(sql, new
			{
				Id = entity.Id,
				Account = entity.Account,
				Password = entity.Password,
				FirstName = entity.FirstName,
				LastName = entity.LastName,
				Sex = entity.Sex,
				Comments = entity.Comments,
				IsLocked = entity.IsLocked,
				IsDeleted = entity.IsDeleted,
				Creator = entity.Creator,
				CreateTime = entity.CreateTime
			});
		}, _logger);
		return result == 1;
	}

	public bool Update(UserEntity userEntity)
	{
		string sql = "UPDATE t_users p SET  p.Account=@Account,p.FirstName=@FirstName,p.LastName=@LastName,p.Password=@Password,p.Sex=@Sex,p.Comments=@Comments,p.IsLocked=@IsLocked,p.IsDeleted=@IsDeleted,p.WrongPassLoginTimes=0 WHERE p.Id=@Id;";
		var result = _context.Connection.ContextExecute((connection) =>
		{
			return connection.Execute(sql, new
			{
				Account = userEntity.Account,
				FirstName = userEntity.FirstName,
				LastName = userEntity.LastName,
				Sex = userEntity.Sex.ToString(),
				Password = userEntity.Password,
				Comments = userEntity.Comments,
				IsLocked = userEntity.IsLocked,
				IsDeleted = userEntity.IsDeleted,
				Id = userEntity.Id,
			});
		}, _logger);
		return result == 1;
	}

	public bool InsertWithRoleList(UserEntity entity, List<RoleEntity> roleEntities)
	{
		return _context.Connection.ContextExecute((connection, transaction) =>
		{
			string sql = "INSERT INTO t_users (Id,Account,Password,FirstName,LastName,Sex,Comments,IsLocked,IsDeleted,Creator,CreateTime) " +
						 "VALUE(@Id,@Account,@Password,@FirstName,@LastName,@Sex,@Comments,@IsLocked,@IsDeleted,@Creator,@CreateTime);";
			connection.Execute(sql, new
			{
				Id = entity.Id,
				Account = entity.Account,
				Password = entity.Password,
				FirstName = entity.FirstName,
				LastName = entity.LastName,
				Sex = entity.Sex,
				Comments = entity.Comments,
				IsLocked = entity.IsLocked,
				IsDeleted = entity.IsDeleted,
				Creator = entity.Creator,
				CreateTime = entity.CreateTime

			}, transaction);

			sql = "insert into t_user_role(Id,UserId,UserAccount,RoleId,Creator,CreateTime) " +
				  "VALUE(@Id,@UserId,@UserAccount,@RoleId,@Creator,@CreateTime);";
			connection.Execute(sql, from role in roleEntities
									select new
									{
										Id = Guid.NewGuid().ToString(),
										UserId = entity.Id,
										UserAccount = entity.Account,
										RoleId = role.Id,
										Creator = entity.Creator,
										CreateTime = entity.CreateTime
									}, transaction);

			return true;
		}, _logger);

	}

	public bool UpdateWithRoleList(UserEntity userEntity, List<RoleEntity> roleEntities)
	{
		return _context.Connection.ContextExecute((connection, transaction) =>
		{
			string sql = "UPDATE t_users p SET p.Account=@Account,p.FirstName=@FirstName,p.LastName=@LastName,p.Password=@Password,p.Sex=@Sex,p.Comments=@Comments,p.IsLocked=@IsLocked,p.IsDeleted=@IsDeleted WHERE p.Id=@Id;";
			connection.Execute(sql, new
			{
				Account = userEntity.Account,
				FirstName = userEntity.FirstName,
				LastName = userEntity.LastName,
				Sex = userEntity.Sex.ToString(),
				Password = userEntity.Password,
				Comments = userEntity.Comments,
				IsLocked = userEntity.IsLocked,
				IsDeleted = userEntity.IsDeleted,
				Id = userEntity.Id,
			}, transaction);

			sql = "delete from t_user_role where UserId=@UserId;";
			connection.Execute(sql, new
			{
				UserId = userEntity.Id,
			}, transaction);

			sql = "insert into t_user_role(Id,UserId,UserAccount,RoleId,Creator,CreateTime) VALUE(@Id,@UserId,@UserAccount,@RoleId,@Creator,@CreateTime);";
			connection.Execute(sql, from role in roleEntities
									select new
									{
										Id = Guid.NewGuid().ToString(),
										UserId = userEntity.Id,
										UserAccount = userEntity.Account,
										RoleId = role.Id,
										Creator = userEntity.Creator,
										CreateTime = userEntity.CreateTime
									}, transaction);

			return true;
		}, _logger);
	}

	public bool Delete(UserEntity entity)
	{
		return _context.Connection.ContextExecute((connection, transaction) =>
		{
			string sql = "DELETE FROM t_user_role WHERE UserId=@UserId;";
			connection.Execute(sql, new
			{
				UserId = entity.Id
			}, transaction);
			sql = "UPDATE t_users SET IsDeleted=True WHERE Id =@Id";
			connection.Execute(sql, new
			{
				Id = entity.Id
			}, transaction);
			return true;
		}, _logger);
	}

	public List<UserEntity> GetAll()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT u.Id,u.Account,u.FirstName,u.LastName,u.Password,u.Sex,u.Comments,u.IsLocked,u.IsDeleted,u.IsFactory,u.Creator,u.CreateTime,ur.UserRoleName FROM t_users u LEFT JOIN (SELECT ur.UserId,GROUP_CONCAT(r.Name, '') AS UserRoleName from t_user_role ur LEFT JOIN t_roles r on ur.RoleId=r.Id GROUP BY ur.UserId) ur ON u.Id=ur.UserId WHERE u.IsDeleted = False; ");
		return _context.Connection.ContextExecute((connection) =>
		{
			var entities = connection.Query<UserEntity>(sb.ToString());
			return entities.ToList();
		}, _logger);
	}

	public List<UserEntity> GetUserListByRole(string roleID)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT u.Id,u.Account,u.FirstName,u.LastName,u.Password,u.Sex,u.Comments,u.IsLocked,u.IsDeleted,u.IsFactory,u.Creator,u.CreateTime,ur.UserRoleName FROM t_users u LEFT JOIN (SELECT tur.UserId,GROUP_CONCAT(tr.Name, '') AS UserRoleName from t_user_role tur LEFT JOIN t_roles tr on tur.RoleId=tr.Id GROUP BY tur.UserId) ur ON u.Id=ur.UserId LEFT JOIN t_user_role r ON u.Id=r.UserId WHERE u.IsDeleted = False and r.RoleId=@RoleId;");
		return _context.Connection.ContextExecute((connection) =>
		{
			var entities = connection.Query<UserEntity>(sb.ToString(), new { RoleId = roleID });
			return entities.ToList();
		});
	}

	public UserEntity GetUserById(string userEntityId)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT * FROM t_users u WHERE u.Id=@UserId;");
		return _context.Connection.ContextExecute((connection) =>
		{
			var entity = connection.QueryFirst<UserEntity>(sb.ToString(), new { UserId = userEntityId });
			return entity;
		});
	}

	public (UserEntity, List<RoleEntity>, List<PermissionEntity>) GetUserRolePermissionList(string userID)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT u.Id,u.Account,u.FirstName,u.LastName,u.Password,u.Sex,u.Comments,u.IsLocked,u.IsDeleted,u.IsFactory,u.Creator,u.CreateTime,ur.UserRoleName FROM t_users u LEFT JOIN (SELECT tur.UserId,GROUP_CONCAT(tr.Name, '') AS UserRoleName from t_user_role tur LEFT JOIN t_roles tr on tur.RoleId=tr.Id GROUP BY tur.UserId) ur ON u.Id=ur.UserId WHERE u.IsDeleted = False and u.Id=@UserUID;");
		sb.AppendLine("SELECT r.Id,r.Name,r.Description,r.IsFactory,r.Creator,r.CreateTime,ur.UserCount FROM t_user_role u LEFT JOIN t_roles r on u.RoleId=r.Id LEFT JOIN (SELECT count(UserId) UserCount,RoleId FROM t_user_role group by RoleId) ur on r.Id=ur.RoleId WHERE r.IsDeleted = FALSE AND u.UserId=@UserUID;");
		sb.AppendLine("SELECT per.Id,per.Code,per.Name,per.Description,per.Category,per.Level,per.IsDeleted,per.Creator,per.CreateTime FROM t_user_role u LEFT JOIN t_role_permission rp ON rp.RoleId=u.RoleId LEFT JOIN t_permissions per ON rp.PermissionId=per.Id WHERE per.IsDeleted = FALSE and u.UserId=@UserUID;");
		return _context.Connection.ContextExecute((connection) =>
		{
			var data = connection.QueryMultiple(sb.ToString(), new { UserUID = userID });
			var user = data.ReadFirstOrDefault<UserEntity>() ?? new UserEntity();
			var role = data.Read<RoleEntity>().ToList();
			var permissions = data.Read<PermissionEntity>().Distinct().ToList();
			return (user, role, permissions);
		});
	}

	public UserEntity GetUserEntityByAccountAndPassword(string account, string password)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT u.Id,u.Account,u.FirstName,u.LastName,u.Password,u.Sex,u.Comments,u.IsLocked,u.IsDeleted,u.IsFactory,u.Creator,u.CreateTime,ur.UserRoleName FROM t_users u LEFT JOIN (SELECT tur.UserId,GROUP_CONCAT(tr.Name, '') AS UserRoleName from t_user_role tur LEFT JOIN t_roles tr on tur.RoleId=tr.Id GROUP BY tur.UserId) ur ON u.Id=ur.UserId WHERE u.IsDeleted = False AND u.Account=@Account and u.Password=@Password; ");
		return _context.Connection.ContextExecute((connection) =>
		{
			return connection.QueryFirstOrDefault<UserEntity>(sb.ToString(), new { Account = account, Password = password });
		}, _logger);
	}

	public bool UpdatePassword(string userEntityId, string hashedNewPassword)
	{
		string sql = "UPDATE t_users p SET p.Password=@Password,WrongPassLoginTimes=0 WHERE p.Id=@Id;";
		var result = _context.Connection.ContextExecute((connection) =>
		{
			return connection.Execute(sql, new
			{
				Password = hashedNewPassword,
				Id = userEntityId,
			});
		}, _logger);
		return result == 1;
	}

	public bool IncreWrongPassLoginTimes(string userName)
	{
		return _context.Connection.ContextExecute((connection, transaction) =>
		{
			string sql = "UPDATE t_users p SET p.WrongPassLoginTimes=p.WrongPassLoginTimes+1 WHERE p.Account=@Account;";
			connection.Execute(sql, new
			{
				Account = userName
			}, transaction);

			return true;
		}, _logger);
	}

	public UserEntity? GetUserByUserName(string userName)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT * FROM t_users u WHERE u.Account=@Account;");
		return _context.Connection.ContextExecute((connection) =>
		{
			var entity = connection.QueryFirst<UserEntity?>(sb.ToString(), new { Account = userName });
			return entity;
		});
	}

	public bool LockUserByName(string userName)
	{
		return _context.Connection.ContextExecute((connection, transaction) =>
		{
			string sql = "UPDATE t_users p SET p.IsLocked=1 WHERE p.Account=@Account;";
			connection.Execute(sql, new
			{
				Account = userName
			}, transaction);

			return true;
		}, _logger);
	}

	public bool ToggleLockStatus(UserModel userModel)
	{
		return _context.Connection.ContextExecute((connection, transaction) =>
		{
			string sql = "UPDATE t_users SET IsLocked = 1 - IsLocked , WrongPassLoginTimes=0 WHERE Account=@Account";
			connection.Execute(sql, new
			{
				Account = userModel.Account
			}, transaction);

			return true;
		}, _logger);
	}

	public bool ResetLoginTimes(string userName)
	{
		return _context.Connection.ContextExecute((connection, transaction) =>
		{
			string sql = "UPDATE t_users SET WrongPassLoginTimes=0 WHERE Account=@Account";
			connection.Execute(sql, new
			{
				Account = userName
			}, transaction);

			return true;
		}, _logger);
	}
}