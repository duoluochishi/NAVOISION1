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

public class BaseWindowingViewModel : BaseViewModel
{
    private string _id = string.Empty;
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private string _bodyPart = string.Empty;
    public string BodyPart
    {
        get => _bodyPart;
        set => SetProperty(ref _bodyPart, value);
    }

    private int _windowWidth = 0;
    public int WindowWidth
    {
        get => _windowWidth;
        set => SetProperty(ref _windowWidth, value);
    }

    private int _windowLevel = 0;
    public int WindowLevel
    {
        get => _windowLevel;
        set => SetProperty(ref _windowLevel, value);
    }

    private string _shortcut = string.Empty;
    public string Shortcut
    {
        get => _shortcut;
        set => SetProperty(ref _shortcut, value);
    }

    private bool _isFactory = false;
    public bool IsFactory
    {
        get => _isFactory;
        set => SetProperty(ref _isFactory, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private bool _isDefault = false;
    public bool IsDefault
    {
        get => _isDefault;
        set => SetProperty(ref _isDefault, value);
    }
}