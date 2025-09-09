//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:59    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.MPS.UI.Dialog.Service;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.Models;
using System.Collections.Generic;
using NV.CT.ConfigManagement.View;
using NV.CT.ConfigManagement.Extensions;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.UI.Controls;

namespace NV.CT.ConfigManagement.ViewModel;

public class UserListViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IUserApplicationService _userApplicationService;
    private ILogger<UserListViewModel> _logger;
    private ObservableCollection<BaseUserViewModel> _userList = new ObservableCollection<BaseUserViewModel>();
    public ObservableCollection<BaseUserViewModel> UserList
    {
        get => _userList;
        set => SetProperty(ref _userList, value);
    }

    private bool isFactory = false;
    public bool IsFactory
    {
        get => isFactory;
        set => SetProperty(ref isFactory, value);
    }

    private BaseUserViewModel _selectUser = new BaseUserViewModel();
    public BaseUserViewModel SelectedUser
    {
        get => _selectUser;
        set
        {
            if (SetProperty(ref _selectUser, value) && value is not null)
            {
                IsFactory = !value.IsFactory;
            }
        }
    }

    private UserWindow? _userWindow;

    private string lastSearchText = string.Empty;
    public string LastSearchText
    {
        get => lastSearchText;
        set => SetProperty(ref lastSearchText, value);
    }

    public UserListViewModel(IUserApplicationService userApplicationService,
        IDialogService dialogService,
        ILogger<UserListViewModel> logger)
    {
        _dialogService = dialogService;
        _userApplicationService = userApplicationService;
        _logger = logger;

        Commands.Add("SearchCommand", new DelegateCommand<string>(SearchUserList));
        Commands.Add("UserEditCommand", new DelegateCommand(UserEditCommand));
        Commands.Add("UserAddCommand", new DelegateCommand(UserAddCommand));
        Commands.Add("UserDeleteCommand", new DelegateCommand(UserDeleteCommand));
        Commands.Add("UserLockUnlockCommand", new DelegateCommand(UserLockUnlockCommand));
        _userApplicationService.UserListReload += UserApplicationService_UserListReload;
        SearchUserList(string.Empty);
    }

    private void UserLockUnlockCommand()
    {
        if (SelectedUser is null || string.IsNullOrEmpty(SelectedUser.Id))
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                , "Please select a user from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        if (SelectedUser.IsFactory)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                , "Can not change factory user! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        _dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm"
            , "Are you sure to toggle lock/unlock? ", arg =>
            {
                if (arg.Result == ButtonResult.OK && _userApplicationService.ToggleLockStatus(new UserModel
                {
                    Id = SelectedUser.Id,
                    FirstName = SelectedUser.FirstName,
                    LastName = SelectedUser.LastName,
                    Account = SelectedUser.Account
                }))
                {
                    SearchUserList(LastSearchText);
                }
            }, ConsoleSystemHelper.WindowHwnd);
    }

    [UIRoute]
    private void UserApplicationService_UserListReload(object? sender, EventArgs e)
    {
        SearchUserList(string.Empty);
    }

    public void SearchUserList(string searchText)
    {
        LastSearchText = searchText;
        _userList.Clear();
        List<UserModel> li = _userApplicationService.GetAllUser();
        var list = li.ToObservableCollection();
        if (!string.IsNullOrEmpty(searchText))
        {
            list = li.FindAll(t => t.Account.ToLower().Contains(searchText.ToLower()) || (t.FirstName + t.LastName).ToLower().Contains(searchText.ToLower())).ToObservableCollection();
        }
        foreach (var item in list)
        {
            BaseUserViewModel userViewModel = new BaseUserViewModel();
            userViewModel.Id = item.Id;
            userViewModel.Account = item.Account;
            userViewModel.FirstName = item.FirstName;
            userViewModel.LastName = item.LastName;
            userViewModel.Comments = item.Comments;
            userViewModel.Password = item.Password;
            userViewModel.Sex = item.Sex;
            userViewModel.IsLocked = item.IsLocked;
            userViewModel.IsDeleted = item.IsDeleted;
            userViewModel.RoleNames = item.UserRoleName;
            userViewModel.IsFactory = item.IsFactory;

            UserList.Add(userViewModel);
        }
        if (UserList.Count > 0)
        {
            SelectedUser = UserList[0];
        }
    }

    private void UserAddCommand()
    {
        var user = new UserModel();
        user.Id = Guid.NewGuid().ToString();
        user.IsDeleted = false;
        user.IsLocked = false;
        user.RoleList = new List<RoleModel>();
        user.Account = Guid.NewGuid().ToString().Substring(24, 8);
        _userApplicationService.SetUserInfo(OperationType.Add, user);
        ShowUserWindow();
    }

    private void UserEditCommand()
    {
        if (SelectedUser is null || string.IsNullOrEmpty(SelectedUser.Id))
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
             , "Please select a user from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        var user = _userApplicationService.GetUserRolePermissionList(SelectedUser.Id);
        if (user is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
             , "Please select a user from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        _userApplicationService.SetUserInfo(OperationType.Edit, user);
        ShowUserWindow();
    }

    public void ShowUserWindow()
    {
        if (_userWindow is null)
        {
            _userWindow = CTS.Global.ServiceProvider?.GetRequiredService<UserWindow>();
        }
        if (_userWindow != null)
        {
            // _userWindow.ShowWindowDialog();
            _userWindow.ShowPopWindowDialog();
        }
    }

    private void UserDeleteCommand()
    {
        if (SelectedUser is null || string.IsNullOrEmpty(SelectedUser.Id))
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
             , "Please select a user from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        if (SelectedUser.IsLocked)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                , "Locked user can't be deleted! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        _dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm"
            , "Are you sure to delete? ", arg =>
            {
                if (arg.Result == ButtonResult.OK && _userApplicationService.DeleteUser(new UserModel
                {
                    Id = SelectedUser.Id,
                    FirstName = SelectedUser.FirstName,
                    LastName = SelectedUser.LastName,
                    Account = SelectedUser.Account
                }))
                {
                    SearchUserList(LastSearchText);
                }
            }, ConsoleSystemHelper.WindowHwnd);
    }
}