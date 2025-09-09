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

using NV.CT.Models;

namespace NV.CT.DatabaseService.Contract;

public interface IRoleService
{
	bool Insert(RoleModel model);

	bool Update(RoleModel model);

	bool Delete(RoleModel model);

	List<RoleModel> GetAllWithRight();

	List<RoleModel> GetAll();

	RoleModel GetRoleById(string roleId);

    RoleModel GetRoleByName(string nabme);

    bool InsertRoleAndRight(RoleModel entity, List<PermissionModel> permissions);

	bool UpdateRoleAndRight(RoleModel entity, List<PermissionModel> permissions);
}