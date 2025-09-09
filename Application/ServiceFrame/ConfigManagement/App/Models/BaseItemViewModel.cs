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

public class BaseItemViewModel<T> : BaseViewModel
{
    private bool _isChecked = false;
    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }

    private T _key = default;
    public T Key
    {
        get => _key;
        set => SetProperty(ref _key, value);
    }

    private string _display = string.Empty;
    public string Display
    {
        get => _display;
        set => SetProperty(ref _display, value);
    }
}