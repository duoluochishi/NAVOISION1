//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using NV.CT.CTS;
using NV.CT.CTS.Encryptions;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Impl.Repository;
using NV.CT.Models;
using NV.CT.Models.User;

namespace NV.CT.DatabaseService.Impl;

public class UserService : IUserService
{
	private readonly IMapper _mapper;
	private readonly UserRepository _userRepository;
	public event EventHandler? UserLogoutSuccess;
	public event EventHandler<UserModel>? CurrentUserChanged;
	private readonly IRoleService _roleService;
	public UserService(IMapper maper, UserRepository userRepository, IRoleService roleService)
	{
		_mapper = maper;
		_userRepository = userRepository;
		_roleService = roleService;
	}

	public bool Delete(UserModel model)
	{
		UserEntity userEntity = _mapper.Map<UserEntity>(model);
		return _userRepository.Delete(userEntity);
	}

	public List<UserModel> GetAll()
	{
		List<UserModel> users = new List<UserModel>();
		foreach (var userEntity in _userRepository.GetAll())
		{
			UserModel userModel = _mapper.Map<UserModel>(userEntity);
			userModel.Behavior = UserBehavior.Logout;
			users.Add(userModel);
		}
		return users;
	}

	public UserModel GetUserById(string userEntityId)
	{
		var userEntity = _userRepository.GetUserById(userEntityId);
		UserModel userModel = _mapper.Map<UserModel>(userEntity);
		return userModel;
	}

	public UserModel? GetUserByUserName(string userName)
	{
		var userEntity = _userRepository.GetUserByUserName(userName);
		if (userEntity is null)
			return null;

		var userModel = _mapper.Map<UserModel>(userEntity);
		return userModel;
	}

	public UserModel GetUserRolePermissionList(string userID)
	{
		var entity = _userRepository.GetUserRolePermissionList(userID);
		UserEntity userEntity = entity.Item1;
		UserModel userModel = _mapper.Map<UserModel>(userEntity);
		userModel.Behavior = UserBehavior.Logout;
		if (entity.Item2.Count > 0)
		{
			foreach (var roleEntity in entity.Item2)
			{
				RoleModel roleModel = _mapper.Map<RoleModel>(roleEntity);
				userModel.RoleList.Add(roleModel);
			}
		}
		if (entity.Item3.Count > 0)
		{
			foreach (var permissionEntity in entity.Item3)
			{
				PermissionModel permissionModel = _mapper.Map<PermissionModel>(permissionEntity);
				userModel.AllUserPermissionList.Add(permissionModel);
			}
		}
		return userModel;
	}

	public bool Insert(UserModel model)
	{
		var entity = GetUserRoleEntity(model);
		if (entity.Item2.Count > 0)
		{
			return _userRepository.InsertWithRoleList(entity.Item1, entity.Item2);
		}
		else
		{
			return _userRepository.Insert(entity.Item1);
		}
	}

	public bool Update(UserModel model)
	{
		var entity = GetUserRoleEntity(model);
		if (entity.Item2.Count > 0)
		{
			return _userRepository.UpdateWithRoleList(entity.Item1, entity.Item2);
		}
		else
		{
			return _userRepository.Update(entity.Item1);
		}
	}

	private (UserEntity, List<RoleEntity>) GetUserRoleEntity(UserModel model)
	{
		UserEntity userEntity = _mapper.Map<UserEntity>(model);
		List<RoleEntity> roles = new List<RoleEntity>();
		if (model.RoleList.Count > 0)
		{
			foreach (var role in model.RoleList)
			{
				RoleEntity roleEntity = _mapper.Map<RoleEntity>(role);
				roles.Add(roleEntity);
			}
		}
		return (userEntity, roles);
	}

	public AuthorizationResult Login(AuthorizationRequest req)
	{
		var user = _userRepository.GetUserEntityByAccountAndPassword(req.Username, req.Password);
		if (user is not null)
		{
			if (user.IsDeleted)
			{
				return new(false, "user has been deleted!");
			}
			if (user.IsLocked)
			{
				return new(false, "user has been locked!");
			}
			NotifyLoginSuccess(user, UserBehavior.Login);
			return new(true, "user login success!");
		}
		return new(false, "username or password wrong!");
	}

	/// <summary>
	/// 服务用户登录
	/// </summary>
	/// <param name="encryptedContent">加密文件，从service key 文件读进来</param>
	/// <returns></returns>
	public AuthorizationResult ServiceUserLogin(string encryptedContent)
	{
		UserEntity userEntity = new UserEntity() { Id = Guid.NewGuid().ToString() };
		var role = _roleService.GetAll().First(t => t.Name.Contains("Device"));
		if (role is RoleModel rm)
		{
			var user = _userRepository.GetUserListByRole(rm.Id).First();
			if (user is not null)
			{
				userEntity = user;
			}
		}
		NotifyLoginSuccess(userEntity, UserBehavior.Login);
		return new(true, "user validate success!");
	}

	/// <summary>
	/// 注销
	/// </summary>
	public AuthorizationResult LogOut(AuthorizationRequest req)
	{
		var user = _userRepository.GetUserEntityByAccountAndPassword(req.Username, req.Password);

		UserLogoutSuccess?.Invoke(this,EventArgs.Empty);

		if (user is not null)
		{
			if (user.IsDeleted)
			{
				return new(false, "user has been deleted!");
			}
			if (user.IsLocked)
			{
				return new(false, "user has been locked!");
			}
			NotifyLoginSuccess(user, UserBehavior.Logout);
			return new(true, "user logout success!");
		}
		return new(false, "username or password wrong!");
	}

	private void NotifyLoginSuccess(UserEntity user, UserBehavior userBehavior)
	{
		UserModel userModel = GetUserRolePermissionList(user.Id);
		userModel.Behavior = userBehavior;
		CurrentUserChanged?.Invoke(this, userModel);
	}

	public UserModel GetUserModel(AuthorizationRequest req)
	{
		UserModel userModel = new UserModel() { IsDeleted = true };
		var user = _userRepository.GetUserEntityByAccountAndPassword(req.Username, req.Password);
		if (user is not null)
		{
			userModel = GetUserRolePermissionList(user.Id);
		}
		return userModel;
	}

	public bool EmergencyLogin()
	{
		UserModel userModel = new UserModel();        //构建一个紧急登录用户
		userModel.Behavior = UserBehavior.Login;
		userModel.IsDeleted = false;
		userModel.IsLocked = false;
		userModel.FirstName = "Emergency user";
		userModel.IsFactory = true;
		userModel.Comments = "Emergency user";
		userModel.CreateTime = DateTime.Now;
		userModel.Sex = CTS.Enums.Gender.Other;

		RoleModel roleModel = _roleService.GetRoleByName("Emergency");      //获取紧急登录用户的权限,参数为紧急角色Name	
		userModel.AllUserPermissionList.AddRange(roleModel.PermissionList);
		userModel.RoleList.Add(roleModel);

		CurrentUserChanged?.Invoke(this, userModel);
		return true;
	}

	/// <summary>
	/// 判断输入密码是否正确
	/// </summary>
	public bool IsPasswordCorrect(IsPasswordCorrectRequest req)
	{
		return MD5Helper.Encrypt(req.InputPassword) == req.OldPassword;
	}

	/// <summary>
	/// 更新当前用户的密码
	/// </summary>
	public bool UpdatePassword(UpdatePasswordRequest req)
	{
		return _userRepository.UpdatePassword(req.UserEntityId, req.HashedNewPassword);
	}

	public bool IncreWrongPassLoginTimes(string userName)
	{
		return _userRepository.IncreWrongPassLoginTimes(userName);
	}

	public bool LockUserByName(string userName)
	{
		return _userRepository.LockUserByName(userName);
	}

	public bool ToggleLockStatus(UserModel userModel)
	{
		return _userRepository.ToggleLockStatus(userModel);
	}

	public bool ResetLoginTimes(string userName)
	{
		return _userRepository.ResetLoginTimes(userName);
	}
} 