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

public class BasePermissionViewModel : BaseViewModel
{
    private string _groupName = string.Empty;
    public string GroupName
    {
        get => _groupName;
        set => SetProperty(ref _groupName, value);
    }

    private List<PermissionViewModel> _permissionViewModels = new List<PermissionViewModel>();
    public List<PermissionViewModel> PermissionViewModels
    {
        get => _permissionViewModels;
        set => SetProperty(ref _permissionViewModels, value);
    }
}

public class PermissionViewModel : BaseViewModel
{
    private string id = string.Empty;
    public string Id
    {
        get => id;
        set => SetProperty(ref id, value);
    }

    private string code = string.Empty;
    public string Code
    {
        get => code;
        set => SetProperty(ref code, value);
    }

    private string name = string.Empty;
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    private string description = string.Empty;
    public string Description
    {
        get => description;
        set => SetProperty(ref description, value);
    }

    private string category = string.Empty;
    public string Category
    {
        get => category;
        set => SetProperty(ref category, value);
    }

    private PermissionLevel level = PermissionLevel.Normal;
    public PermissionLevel Level
    {
        get => level;
        set => SetProperty(ref level, value);
    }

    private bool isDeleted = false;
    public bool IsDeleted
    {
        get => isDeleted;
        set => SetProperty(ref isDeleted, value);
    }

    private bool isChencked= false;
    public bool IsChecked
    {
        get => isChencked;
        set => SetProperty(ref isChencked, value);
    }
}