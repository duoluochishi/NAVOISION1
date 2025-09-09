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
using NV.CT.Language;
using System.Collections.Generic;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.Models;
using System.Data;
using NV.CT.CommonAttributeUI.AOPAttribute;

namespace NV.CT.ConfigManagement.ViewModel;

public class RoleViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IUserApplicationService _userApplicationService;

    private BaseRoleViewModel _currentRole = new BaseRoleViewModel();
    public BaseRoleViewModel CurrentRole
    {
        get => _currentRole;
        set => SetProperty(ref _currentRole, value);
    }

    public OperationType OperationType { get; set; } = OperationType.Add;

    private List<PermissionViewModel> rolePermissions = new List<PermissionViewModel>();
    public List<PermissionViewModel> RolePermissions
    {
        get => rolePermissions;
        set => SetProperty(ref rolePermissions, value);
    }

    private List<BasePermissionViewModel> allPermissions = new List<BasePermissionViewModel>();
    public List<BasePermissionViewModel> AllPermissions
    {
        get => allPermissions;
        set => SetProperty(ref allPermissions, value);
    }

    public RoleViewModel(IUserApplicationService userApplicationService,
        IDialogService dialogService)
    {
        _dialogService = dialogService;
        _userApplicationService = userApplicationService;

        Commands.Add("SaveCommand", new DelegateCommand<object>(Saved, _ => true));
        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => true));
        _userApplicationService.RoleInfoChanged += UserApplicationService_RoleInfoChanged;
        GetRoleList();
    }

    public void GetRoleList()
    {
        AllPermissions.Clear();
        List<PermissionModel> allPermissions = _userApplicationService.GetAllPermission();
        var groupPermissions = allPermissions.GroupBy(p => p.Category).Select(pGroup => new BasePermissionViewModel()
        {
            GroupName = pGroup.Key,
            PermissionViewModels = pGroup.Where(p => p.Category == pGroup.Key).Select(t => new PermissionViewModel()
            {
                Id = t.Id,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                IsDeleted = t.IsDeleted
            }).OrderBy(t => t.Code).ToList(),
        });
        AllPermissions = groupPermissions.ToList();
    }

    [UIRoute]
    private void UserApplicationService_RoleInfoChanged(object? sender, EventArgs<(OperationType operation, CT.Models.RoleModel roleModel)> e)
    {
        if (e is null)
        {
            return;
        }
        OperationType = e.Data.operation;
        SetRoleInfo(e.Data.roleModel);
    }

    private void SetRoleInfo(RoleModel roleModel)
    {
        CurrentRole = new BaseRoleViewModel();
        CurrentRole.Id = roleModel.Id;
        CurrentRole.Name = roleModel.Name;
        CurrentRole.Description = roleModel.Description;
        CurrentRole.UserCount = roleModel.UserCount;
        CurrentRole.IsFactory = roleModel.IsFactory;
        CurrentRole.IsDeleted = roleModel.IsDeleted;
        CurrentRole.Level = roleModel.Level;

        RolePermissions = new List<PermissionViewModel>();
        foreach (var permission in roleModel.PermissionList)
        {
            PermissionViewModel permissionModel = new PermissionViewModel();
            permissionModel.Id = permission.Id;
            permissionModel.IsDeleted = permission.IsDeleted;
            permissionModel.Description = permission.Description;
            permissionModel.Name = permission.Name;
            permissionModel.Code = permission.Code;
            permissionModel.Level = permission.Level;

            RolePermissions.Add(permissionModel);
        }
        SetRolePermission();
    }

    private void SetRolePermission()
    {
        AllPermissions.ForEach(role => { role.PermissionViewModels.ForEach(per => { per.IsChecked = false; }); });
        foreach (var rp in rolePermissions)
        {
            foreach (var perv in AllPermissions)
            {
                var roleP = perv.PermissionViewModels.FirstOrDefault(t => t.Id.Equals(rp.Id));
                if (roleP is not null)
                {
                    roleP.IsChecked = true;
                }
            }
        }
    }

    public void Saved(object parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }
        if (!CheckFormEmpty() || CheckNameRepeat() || !CheckNumAndEnChForm())
        {
            return;
        }

        GetRolePermission();
        RoleModel roleModel = new RoleModel();
        roleModel.Id = CurrentRole.Id;
        roleModel.Name = CurrentRole.Name;
        roleModel.Description = CurrentRole.Description;
        roleModel.Level = CurrentRole.Level;
        roleModel.IsFactory = CurrentRole.IsFactory;
        roleModel.IsDeleted = CurrentRole.IsDeleted;

        foreach (var perssion in RolePermissions)
        {
            PermissionModel perssionModel = new PermissionModel();
            perssionModel.Id = perssion.Id;
            perssionModel.Code = perssion.Code;
            perssionModel.IsDeleted = perssion.IsDeleted;
            perssionModel.Description = perssion.Description;
            perssionModel.Name = perssion.Name;
            perssionModel.Level = perssion.Level;
            roleModel.PermissionList.Add(perssionModel);
        }
        bool saveFlag = false;
        switch (OperationType)
        {
            case OperationType.Add:
                roleModel.CreateTime = DateTime.Now;
                saveFlag = _userApplicationService.AddRole(roleModel);
                break;
            case OperationType.Edit:
            default:

                saveFlag = _userApplicationService.UpdateRole(roleModel);
                break;
        }
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg =>
          {
              if (saveFlag)
              {
                  _userApplicationService.ReloadRoleList();
                  window.Hide();
              }
          }, ConsoleSystemHelper.WindowHwnd);
    }

    private void GetRolePermission()
    {
        RolePermissions = new List<PermissionViewModel>();
        foreach (var basePermissionView in AllPermissions)
        {
            RolePermissions.AddRange(basePermissionView.PermissionViewModels.Where(t => t.IsChecked).ToList());
        }
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        string message = "{0} can't be empty!";

        if (string.IsNullOrEmpty(CurrentRole.Name))
        {
            message = string.Format(message, "First name");
            flag = false;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool CheckNameRepeat()
    {
        bool flag = false;
        var users = _userApplicationService.GetAllRole();
        switch (OperationType)
        {
            case OperationType.Add:
                flag = users.Any(t => t.Name == CurrentRole.Name);
                break;
            case OperationType.Edit:
                flag = users.Any(t => t.Id != CurrentRole.Id && t.Name == CurrentRole.Name);
                break;
            default: break;
        }
        if (flag)
        {
            var message = "The name is duplicated!";
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool CheckNumAndEnChForm()
    {
        bool flag = true;
        string message = "";
        if (VerificationExtension.IsSpecialCharacters(CurrentRole.Name))
        {
            flag = false;
            message += $"Name:Special characters are not allowed!";
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    public void Closed(object parameter)
    {
        if (parameter is Window window)
        {
            _userApplicationService.ReloadRoleList();
            window.Hide();
        }
    }
}