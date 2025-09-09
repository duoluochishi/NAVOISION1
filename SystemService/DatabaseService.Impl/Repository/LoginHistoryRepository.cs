//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
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

public class LoginHistoryRepository
{
	private readonly DatabaseContext _context;
	private readonly ILogger<LoginHistoryRepository> _logger;

	public LoginHistoryRepository(DatabaseContext context, ILogger<LoginHistoryRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public bool Insert(LoginHistoryEntity entity)
	{
		string sql = "INSERT INTO t_login_history (Id,Account,EncryptPassword,Behavior,Comments,FailReason,IsSuccess,Creator,CreateTime) " +
						 "VALUE(@Id,@Account,@EncryptPassword,@Behavior,@Comments,@FailReason,@IsSuccess,@Creator,@CreateTime);";
		var result = _context.Connection.ContextExecute((connection) =>
		{
			return connection.Execute(sql, new
			{
				Id = entity.Id,
				Account = entity.Account,
				EncryptPassword=entity.EncryptPassword,
				Behavior = entity.Behavior,
				Comments = entity.Comments,
				FailReason = entity.FailReason,
				IsSuccess = entity.IsSuccess,
				Creator = entity.Creator,
				CreateTime = entity.CreateTime
			});
		}, _logger);
		return result == 1;
	}

	/// <summary>
	/// 取最近半年当前用户的登录历史记录
	/// </summary>
	public List<LoginHistoryEntity> GetLoginHistoryListByAccount(string account)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT * from t_login_history t WHERE t.CreateTime>=DATE_SUB(NOW(),INTERVAL 6 MONTH)  and t.Account=@Account;");
		return _context.Connection.ContextExecute((connection) =>
		{
			var entities = connection.Query<LoginHistoryEntity>(sb.ToString(), new { Account = account });
			return entities.ToList();
		});
	}

	public LoginHistoryEntity GetLastLogin()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT * from t_login_history where Behavior='Login' order by CreateTime DESC LIMIT 1;");
		return _context.Connection.ContextExecute((connection) =>
		{
			var entity = connection.QueryFirst<LoginHistoryEntity>(sb.ToString());
			return entity;
		});
	}

}