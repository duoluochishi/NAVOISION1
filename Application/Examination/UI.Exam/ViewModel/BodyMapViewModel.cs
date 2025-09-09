//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.UI.Exam.ViewModel;

public class BodyMapViewModel : BaseViewModel
{
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _toolTip = string.Empty;
    public string ToolTip
    {
        get => _toolTip;
        set => SetProperty(ref _toolTip, value);
    }

    private BitmapImage _mapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFS.png", UriKind.RelativeOrAbsolute));
    public BitmapImage MapImage
    {
        get => _mapImage;
        set => SetProperty(ref _mapImage, value);
    }
}