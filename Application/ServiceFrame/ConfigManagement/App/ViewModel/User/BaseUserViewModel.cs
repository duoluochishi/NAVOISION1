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

using System.Collections.Generic;

namespace NV.CT.ConfigManagement.ViewModel;

public class BaseUserViewModel : BaseViewModel
{    
    private string id = string.Empty;
    public string Id
    {
        get => id;
        set => SetProperty(ref id, value);
    }

    private string account = string.Empty;
    public string Account
    {
        get => account;
        set => SetProperty(ref account, value);
    }

    private string password = string.Empty;
    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }
       
    public string Name
    {
        get => FirstName + "^" + LastName;
    }

    private string firstName = string.Empty;
    public string FirstName
    {
        get => firstName;
        set => SetProperty(ref firstName, value);
    }

    private string lastName = string.Empty;
    public string LastName
    {
        get => lastName;
        set => SetProperty(ref lastName, value);
    }

    private Gender sex = Gender.Other;
    public Gender Sex
    {
        get => sex;
        set => SetProperty(ref sex, value);
    }

    private string comments = string.Empty;
    public string Comments
    {
        get => comments;
        set => SetProperty(ref comments, value);
    }

    private bool isDeleted = false;
    public bool IsDeleted
    {
        get => isDeleted;
        set => SetProperty(ref isDeleted, value);
    }

    private bool isLocked = false;
    public bool IsLocked
    {
        get => isLocked;
        set => SetProperty(ref isLocked, value);
    }

    private bool isFactory = false;
    public bool IsFactory
    {
        get => isFactory;
        set => SetProperty(ref isFactory, value);
    }

    private string roleNames = string.Empty;
    public string RoleNames
    {
        get => roleNames;
        set => SetProperty(ref roleNames, value);
    }

    private string roleNamesDisplay = string.Empty;
    public string RoleNamesDisplay
    {
        get
        {
            return RoleNames.Length > 50 ? $"{RoleNames.Substring(0, 50)}..." : RoleNames;
        }
    }

    private List<BaseRoleViewModel> userRoles = new List<BaseRoleViewModel>();
    public List<BaseRoleViewModel> UserRoles
    {
        get => userRoles;
        set => SetProperty(ref userRoles, value);
    }
}