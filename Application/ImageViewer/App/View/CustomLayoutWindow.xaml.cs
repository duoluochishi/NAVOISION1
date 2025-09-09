//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.ViewModel;

namespace NV.CT.ImageViewer.View;

public partial class CustomLayoutWindow
{
    private readonly Image2DViewModel? _image2dViewModel;
    private readonly Image3DViewModel? _image3dViewModel;
    private ViewScene _viewScene;
    public CustomLayoutWindow()
    {
        InitializeComponent();

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        MouseDown += (_, _) =>
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        };

        _image2dViewModel = CTS.Global.ServiceProvider.GetService<Image2DViewModel>();
        _image3dViewModel = CTS.Global.ServiceProvider.GetService<Image3DViewModel>();
    }

    public void SetScene(ViewScene scene)
    {
        _viewScene = scene;
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// 自定义 WW/WL 
    /// </summary>
    private void BtnOk_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(txtRow.Text.Trim()) || string.IsNullOrEmpty(txtColumn.Text.Trim()))
            return;

        if (int.TryParse(txtRow.Text, out var rows) && int.TryParse(txtColumn.Text, out var columns) && rows>0 && columns > 0)
        {
            if (_viewScene == ViewScene.View2D)
            {
                _image2dViewModel?.CurrentImageViewer.SetCustomLayout2D(rows,columns) ;
            }
            else if (_viewScene == ViewScene.View3D)
            {
                _image3dViewModel?.CurrentImageViewer.SetCustomLayout3D(rows, columns);
            }
        }

        Hide();
    }
}