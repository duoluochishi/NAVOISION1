//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.MPS.UI.Dialog.Service;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.Models;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using System.Collections.Generic;
using NV.CT.ConfigManagement.Extensions;
using NV.CT.ConfigManagement.View;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.UI.Controls;

namespace NV.CT.ConfigManagement.ViewModel;

public class RoleListViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IUserApplicationService _userApplicationService;
    private ILogger<RoleListViewModel> _logger;
    private RoleWindow? _roleWindow;

    private ObservableCollection<BaseRoleViewModel> _roleList = new ObservableCollection<BaseRoleViewModel>();
    public ObservableCollection<BaseRoleViewModel> RoleList
    {
        get => _roleList;
        set => SetProperty(ref _roleList, value);
    }

    private bool isFactory = false;
    public bool IsFactory
    {
        get => isFactory;
        set => SetProperty(ref isFactory, value);
    }

    private BaseRoleViewModel _selectedRole = new BaseRoleViewModel();
    public BaseRoleViewModel SelectedRole
    {
        get => _selectedRole;
        set
        {
            if (SetProperty(ref _selectedRole, value) && value is not null)
            {
                IsFactory = !value.IsFactory;
            }
        }
    }

    public RoleListViewModel(IUserApplicationService userApplicationService,
        IDialogService dialogService,
        ILogger<RoleListViewModel> logger)
    {
        _dialogService = dialogService;
        _userApplicationService = userApplicationService;
        _logger = logger;

        Commands.Add("RoleEditCommand", new DelegateCommand(RoleEditCommand));
        Commands.Add("RoleAddCommand", new DelegateCommand(RoleAddCommand));
        Commands.Add("RoleDeleteCommand", new DelegateCommand(RoleDeleteCommand));

        SearchRoleList(string.Empty);
        _userApplicationService.RoleListReload += UserApplicationService_RoleListReload;
    }

    [UIRoute]
    private void UserApplicationService_RoleListReload(object? sender, EventArgs e)
    {
        SearchRoleList(string.Empty);
    }

    public void SearchRoleList(string searchText)
    {
        RoleList.Clear();
        foreach (var role in _userApplicationService.GetAllRole())
        {
            BaseRoleViewModel roleViewModel = new BaseRoleViewModel()
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                UserCount = role.UserCount,
                IsFactory = role.IsFactory,
                IsDeleted = role.IsDeleted,
                Level = role.Level,
            };
            RoleList.Add(roleViewModel);
        }
        if (RoleList.Count > 0)
        {
            SelectedRole = RoleList[0];
        }
    }

    private void RoleAddCommand()
    {
        var role = new RoleModel();
        role.Id = Guid.NewGuid().ToString();
        role.IsDeleted = false;
        role.IsFactory = false;
        role.Level = PermissionLevel.Normal;
        role.PermissionList = new List<PermissionModel>();

        _userApplicationService.SetRoleInfo(OperationType.Add, role);
        ShowRoleWindow();
    }

    private void RoleEditCommand()
    {
        //出厂角色不可编辑
        if (SelectedRole is null || string.IsNullOrEmpty(SelectedRole.Id))
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
               , "Please select a role from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        var role = _userApplicationService.GetRoleById(SelectedRole.Id);
        if (role is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
             , "Please select a role from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        _userApplicationService.SetRoleInfo(OperationType.Edit, role);
        ShowRoleWindow();
    }

    public void ShowRoleWindow()
    {
        if (_roleWindow is null)
        {
            _roleWindow = CTS.Global.ServiceProvider?.GetRequiredService<RoleWindow>();
        }
        if (_roleWindow != null)
        {
            //_roleWindow.ShowWindowDialog();
            _roleWindow.ShowPopWindowDialog();
        }
    }

    private void RoleDeleteCommand()
    {
        if (SelectedRole == null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
              , "Please select a role from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        //存在属于当前角色的用户，不能删除当前角色! 
        if (SelectedRole.UserCount > 0)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                , "You can't delete the current role because there are users with the current role!! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        var entity = new RoleModel()
        {
            Id = SelectedRole.Id,
            Name = SelectedRole.Name,
            Description = SelectedRole.Description,
            UserCount = SelectedRole.UserCount,
            IsFactory = SelectedRole.IsFactory,
            IsDeleted = SelectedRole.IsDeleted,
            Level = SelectedRole.Level,
        };

        _dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm"
            , "Are you sure to delete the role? ", arg =>
            {
                if (arg.Result == ButtonResult.OK && _userApplicationService.DeleteRole(entity))
                {
                    _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                        , $"Delete role({SelectedRole.Name})! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                    SearchRoleList(string.Empty);
                }
            }, ConsoleSystemHelper.WindowHwnd);
    }
}