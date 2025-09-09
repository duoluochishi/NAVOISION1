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
using NV.CT.DatabaseService.Contract;
using NV.CT.Models;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.WorkflowService.Impl;

public class AuthorizationService : IAuthorization
{
    public event EventHandler<UserModel?>? CurrentUserChanged;

    private readonly IUserService _userService;

    private UserModel? _currentUser;
    private UserModel? CurrentUser 
    {
        get => _currentUser;
        set
        { 
            _currentUser = value;
            CurrentUserChanged?.Invoke(this, value);
        }
    }
    public AuthorizationService(IUserService userService)
    {
        _userService = userService;
        _userService.CurrentUserChanged += UserService_CurrentUserChanged;
    }

    private void UserService_CurrentUserChanged(object? sender, UserModel e)
    {
        if (e.Behavior == UserBehavior.Login)
        {
            CurrentUser = e;
        }
        else if (e.Behavior == UserBehavior.Logout)
        {
            CurrentUser = null;
        }        
    }

    public UserModel? GetCurrentUser()
    {
        return CurrentUser;
    }

    public AuthorizationResult AuthenticationCurrentUser(string permissionCode)
    {
        var user = CurrentUser;
        if (user is not null)
        {
            if (user.IsDeleted)
            {
                return new(false, "user has been deleted!", false);
            }
            if (user.IsLocked)
            {
                return new(false, "user has been locked!", false);
            }
            var per = user.AllUserPermissionList.FirstOrDefault(t => t.Code.Equals(permissionCode));
            if (per is not null)
            {
                return new(true, "Authentication passed!", true);
            }
            else
            {
                return new(false, "The user does not have permissions!", false);
            }
        }
        return new(false, "username or password wrong!", false);
    }

    public AuthorizationResult AuthenticationOtherUser(AuthorizationRequest request)
    {
        var user = _userService.GetUserModel(request);
        if (user is not null)
        {
            if (user.IsDeleted)
            {
                return new(false, "user has been deleted!", false);
            }
            if (user.IsLocked)
            {
                return new(false, "user has been locked!", false);
            }
            var per = user.AllUserPermissionList.FirstOrDefault(t => t.Code.Equals(request.PermissionCode));
            if (per is not null)
            {
                return new(true, "Authentication passed!", true);
            }
            else
            {
                return new(false, "The user does not have permissions!", false);
            }
        }
        return new(false, "username or password wrong!", false);
    }

    public bool IsAuthorized(string permissionCode)
    {
        var permission = AuthenticationCurrentUser(permissionCode);
        if (permission is not null && permission.IsSuccess && permission.HasPermissions)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}