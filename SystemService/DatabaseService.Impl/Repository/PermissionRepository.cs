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
using System.Text;
using static Dapper.SqlMapper;

namespace NV.CT.DatabaseService.Impl.Repository;

public class PermissionRepository
{
	private readonly DatabaseContext _context;
	private readonly ILogger<PermissionRepository> _logger;

	public PermissionRepository(DatabaseContext context, ILogger<PermissionRepository> logger)
	{
		_context = context;
		_logger = logger;
	}

	public PermissionEntity GetRightByID(string permissionID)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT Id,Code,Name,Description,Category,Level,IsDeleted,Creator,CreateTime FROM t_permissions WHERE IsDeleted = FALSE AND Id=@permissionID;");
		return _context.Connection.ContextExecute<PermissionEntity>((connection) =>
		{
			var entities = connection.Query<PermissionEntity>(sb.ToString(), new { permissionID = permissionID });
			return entities.FirstOrDefault();
		});
	}

	public PermissionEntity GetRightByCode(string code)
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendLine("SELECT Id,Code,Name,Description,Category,Level,IsDeleted,Creator,CreateTime FROM t_permissions WHERE IsDeleted = FALSE AND Code=@code;");
		return _context.Connection.ContextExecute<PermissionEntity>((connection) =>
		{
			var entities = connection.Query<PermissionEntity>(sb.ToString(), new { code = code });
			return entities.FirstOrDefault();
		});
	}

	public List<PermissionEntity> GetAll()
	{
		string sql = "SELECT Id,Code,Name,Description,Category,Level,IsDeleted,Creator,CreateTime FROM t_permissions WHERE IsDeleted = FALSE AND Level<1;";
		return _context.Connection.ContextExecute((connection) =>
		{
			var entities = connection.Query<PermissionEntity>(sql);
			return entities.ToList();
		}, _logger);
	}
}