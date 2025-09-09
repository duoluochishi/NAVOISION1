//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/7 10:58:04           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.Models;
using NV.CT.Models.User;

namespace NV.CT.DatabaseService.Contract;

public interface IUserService
{
	bool Insert(UserModel model);

	bool Update(UserModel model);

	bool Delete(UserModel model);

	List<UserModel> GetAll();

	UserModel GetUserById(string userEntityId);

	UserModel? GetUserByUserName(string userName);

	UserModel GetUserRolePermissionList(string userId);

	AuthorizationResult Login(AuthorizationRequest req);

	AuthorizationResult ServiceUserLogin(string encryptedContent);

	event EventHandler? UserLogoutSuccess;
	AuthorizationResult LogOut(AuthorizationRequest req);

	event EventHandler<UserModel>? CurrentUserChanged;

	UserModel GetUserModel(AuthorizationRequest req);

	bool EmergencyLogin();

	/// <summary>
	/// 判断输入密码是否正确
	/// </summary>
	bool IsPasswordCorrect(IsPasswordCorrectRequest req);

	/// <summary>
	/// 更新当前用户的密码
	/// </summary>
	bool UpdatePassword(UpdatePasswordRequest req);

	bool ResetLoginTimes(string userName);

	bool IncreWrongPassLoginTimes(string userName);

	bool LockUserByName(string userName);

	bool ToggleLockStatus(UserModel userModel);
}