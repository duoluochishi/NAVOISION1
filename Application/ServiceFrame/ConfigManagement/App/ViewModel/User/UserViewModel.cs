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
using System.Collections.Generic;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.Models;
using NV.CT.CTS.Encryptions;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.Language;

namespace NV.CT.ConfigManagement.ViewModel;

public class UserViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IUserApplicationService _userApplicationService;
    private ILogger<UserViewModel> _logger;

    private string password = string.Empty;
    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }

    private string confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => confirmPassword;
        set => SetProperty(ref confirmPassword, value);
    }

    private bool isMale = false;
    public bool IsMale
    {
        get => isMale;
        set => SetProperty(ref isMale, value);
    }

    private bool isFemale = false;
    public bool IsFemale
    {
        get => isFemale;
        set => SetProperty(ref isFemale, value);
    }

    private bool isModifyPassword = false;
    public bool IsModifyPassword
    {
        get => isModifyPassword;
        set => SetProperty(ref isModifyPassword, value);
    }

    private bool isModifyPasswordEnable = false;
    public bool IsModifyPasswordEnable
    {
        get => isModifyPasswordEnable;
        set => SetProperty(ref isModifyPasswordEnable, value);
    }

    private BaseUserViewModel _currentUser = new BaseUserViewModel();
    public BaseUserViewModel CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    private List<BaseRoleViewModel> userRoles = new List<BaseRoleViewModel>();
    public List<BaseRoleViewModel> UserRoles
    {
        get => userRoles;
        set => SetProperty(ref userRoles, value);
    }

    public OperationType OperationType { get; set; } = OperationType.Add;

    private ObservableCollection<BaseRoleViewModel> _roleList = new ObservableCollection<BaseRoleViewModel>();
    public ObservableCollection<BaseRoleViewModel> RoleList
    {
        get => _roleList;
        set => SetProperty(ref _roleList, value);
    }

    public UserViewModel(IUserApplicationService userApplicationService,
        IDialogService dialogService,
        ILogger<UserViewModel> logger)
    {
        _dialogService = dialogService;
        _userApplicationService = userApplicationService;
        _logger = logger;
        Commands.Add("SaveCommand", new DelegateCommand<object>(Saved, _ => true));
        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => true));
        GetRoleList();
        _userApplicationService.UserInfoChanged += UserApplicationService_UserInfoChanged;
    }

    [UIRoute]
    private void UserApplicationService_UserInfoChanged(object? sender, EventArgs<(OperationType operation, CT.Models.UserModel userModel)> e)
    {
        if (e is null)
        {
            return;
        }
        GetRoleList();
        OperationType = e.Data.operation;
        switch (e.Data.operation)
        {
            case OperationType.Add:
                IsModifyPassword = true;
                IsModifyPasswordEnable = false;
                break;
            default:
                IsModifyPassword = false;
                IsModifyPasswordEnable = true;
                break;
        }
        Password = string.Empty;
        ConfirmPassword = string.Empty;

        SetUserInfo(e.Data.userModel);
    }

    private void SetUserInfo(UserModel userModel)
    {
        CurrentUser = new BaseUserViewModel();
        CurrentUser.Id = userModel.Id;
        CurrentUser.Account = userModel.Account;
        CurrentUser.FirstName = userModel.FirstName;
        CurrentUser.LastName = userModel.LastName;
        CurrentUser.Comments = userModel.Comments;
        CurrentUser.Sex = userModel.Sex;
        if (userModel.Sex == Gender.Female)
        {
            IsFemale = true;
        }
        else
        {
            IsMale = true;
        }
        CurrentUser.IsLocked = userModel.IsLocked;
        CurrentUser.IsDeleted = userModel.IsDeleted;
        CurrentUser.RoleNames = userModel.UserRoleName;
        CurrentUser.Password = userModel.Password;
        UserRoles = new List<BaseRoleViewModel>();
        foreach (var role in userModel.RoleList)
        {
            BaseRoleViewModel roleViewModel = new BaseRoleViewModel();
            roleViewModel.Id = role.Id;
            roleViewModel.IsFactory = role.IsFactory;
            roleViewModel.IsDeleted = role.IsDeleted;
            roleViewModel.Description = role.Description;
            roleViewModel.Name = role.Name;
            roleViewModel.Level = role.Level;
            roleViewModel.UserCount = role.UserCount;
            UserRoles.Add(roleViewModel);
        }
        SetUserRoles();
    }

    public void Saved(object parameter)
    {
        if (parameter is not Window window)
        {
            return;
        }
        if (!CheckFormEmpty()
            || !CheckPasswordLength()
            || (IsModifyPassword && !CheckPasswordForm())
            || CheckAccountRepeat()
            || (!CheckPasswordUpperLower())
            || !CheckNumAndEnChForm())
        {
            return;
        }
        if (IsModifyPassword)
        {
            if (!CheckPasswordForm())
            {

            }
        }

        GetUserSex();
        GetUserRoles();
        UserModel userModel = new UserModel();
        userModel.Id = CurrentUser.Id;
        userModel.Account = CurrentUser.Account;
        userModel.FirstName = CurrentUser.FirstName;
        userModel.LastName = CurrentUser.LastName;
        userModel.Comments = CurrentUser.Comments;
        userModel.Password = IsModifyPassword ? MD5Helper.Encrypt(Password) : CurrentUser.Password;
        userModel.Sex = CurrentUser.Sex;
        userModel.IsLocked = CurrentUser.IsLocked;
        userModel.IsDeleted = CurrentUser.IsDeleted;
        foreach (var role in UserRoles)
        {
            RoleModel roleModel = new RoleModel();
            roleModel.Id = role.Id;
            roleModel.IsFactory = role.IsFactory;
            roleModel.IsDeleted = role.IsDeleted;
            roleModel.Description = role.Description;
            roleModel.Name = role.Name;
            roleModel.Level = role.Level;
            roleModel.UserCount = role.UserCount;
            userModel.RoleList.Add(roleModel);
        }
        bool saveFlag = false;
        switch (OperationType)
        {
            case OperationType.Add:
                saveFlag = _userApplicationService.AddUser(userModel);
                break;
            case OperationType.Edit:
            default:
                saveFlag = _userApplicationService.UpdateUser(userModel);
                break;
        }
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg =>
          {
              if (saveFlag)
              {
                  _userApplicationService.ReloadUserList();
                  window.Hide();
              }
          }, ConsoleSystemHelper.WindowHwnd);
    }

    private void GetUserSex()
    {
        if (IsFemale & !IsMale)
        {
            CurrentUser.Sex = Gender.Female;
        }
        if (IsMale & !IsFemale)
        {
            CurrentUser.Sex = Gender.Male;
        }
    }

    private void SetUserRoles()
    {
        RoleList.ForEach(role => { role.IsChecked = false; });
        foreach (var role in UserRoles)
        {
            var ur = RoleList.FirstOrDefault(t => t.Id.Equals(role.Id));
            if (ur is not null)
            {
                ur.IsChecked = true;
            }
        }
    }

    private void GetUserRoles()
    {
        UserRoles = new List<BaseRoleViewModel>();
        UserRoles = RoleList.Where(t => t.IsChecked).ToList();
    }

    public void Closed(object parameter)
    {
        if (parameter is Window window)
        {
            _userApplicationService.ReloadUserList();
            window.Hide();
        }
    }

    public void GetRoleList()
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
    }

    private bool CheckAccountRepeat()
    {
        bool flag = false;
        var users = _userApplicationService.GetAllUser();
        switch (OperationType)
        {
            case OperationType.Add:
                flag = users.Any(t => t.Account == CurrentUser.Account);
                break;
            case OperationType.Edit:
                flag = users.Any(t => t.Id != CurrentUser.Id && t.Account == CurrentUser.Account);
                break;
            default: break;
        }
        if (flag)
        {
            var message = "The account is duplicated!";
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        string message = "{0} can't be empty!";
        if (IsModifyPassword)
        {
            if (string.IsNullOrEmpty(Password))
            {
                message = string.Format(message, "Password");
                flag = false;
            }
            if (string.IsNullOrEmpty(ConfirmPassword))
            {
                message = string.Format(message, "Confirm password");
                flag = false;
            }
        }
        if (string.IsNullOrEmpty(CurrentUser.FirstName))
        {
            message = string.Format(message, "First name");
            flag = false;
        }
        if (string.IsNullOrEmpty(CurrentUser.Account))
        {
            message = string.Format(message, "Account");
            flag = false;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool CheckPasswordLength()
    {
        bool flag = true;
        if (!IsModifyPassword)
        {
            return flag;
        }
        string message = string.Empty;
        if (Password.Length < 6 || ConfirmPassword.Length < 6)
        {
            flag = false;
            message = $"Password Minimum length greater than or equal to 6!";
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool CheckPasswordUpperLower()
    {
        bool flag = true;
        string message = string.Empty;
        if (!string.IsNullOrEmpty(Password) && !(VerificationExtension.HasUpperCase(Password) && VerificationExtension.HasIsLowerCase(Password)))
        {
            flag = false;
            message = $"Passwords must contain both upper and lower case letters!";
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool CheckPasswordForm()
    {
        bool flag = true;
        string message = "Password inconsistency!";
        if (IsModifyPassword && Password != ConfirmPassword)
        {
            flag = false;
        }

        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool CheckNumAndEnChForm()
    {
        bool flag = true;
        string message = "";

        if (VerificationExtension.IsSpecialCharacters(CurrentUser.Account))
        {
            flag = false;
            message += $"Account:Special characters are not allowed!";
        }
        if (VerificationExtension.IsSpecialCharacters(CurrentUser.FirstName))
        {
            flag = false;
            message += $"FirstName:Special characters are not allowed!";
        }
        if (VerificationExtension.IsSpecialCharacters(CurrentUser.LastName))
        {
            flag = false;
            message += $"LastName:Special characters are not allowed!";
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }
}