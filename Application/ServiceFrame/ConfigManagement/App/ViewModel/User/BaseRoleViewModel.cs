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
namespace NV.CT.ConfigManagement.ViewModel;

public class BaseRoleViewModel : BaseViewModel
{
    private string id = string.Empty;
    public string Id
    {
        get => id;
        set => SetProperty(ref id, value);
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

    public string DescriptionDisplay
    {
        get => Description.Length > 40 ? (Description.Substring(0, 40) + "...") : Description;
    }

    private int userCount = 0;
    public int UserCount
    {
        get => userCount;
        set => SetProperty(ref userCount, value);
    }

    private bool isFactory = false;
    public bool IsFactory
    {
        get => isFactory;
        set => SetProperty(ref isFactory, value);
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

    private bool isChecked = false;
    public bool IsChecked
    {
        get => isChecked;
        set => SetProperty(ref isChecked, value);
    }
}