//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.ViewModel;
using EventAggregator = NV.CT.ImageViewer.Extensions.EventAggregator;
namespace NV.CT.ImageViewer.View;

public partial class CustomWWWLWindow
{
    private readonly Image2DViewModel? _image2dViewModel;
    private readonly Image3DViewModel? _image3dViewModel;
    private ViewScene _viewScene;
    public CustomWWWLWindow()
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
        double ww=0, wl=0;
        if (_viewScene == ViewScene.View2D)
        {
            _image2dViewModel?.CurrentImageViewer.GetWWWL(ref ww, ref wl);
            txtWW.Text=ww.ToString("F0");
            txtWL.Text = wl.ToString("F0");
            EventAggregator.Instance.GetEvent<Update2DHotKeyEvent>().Publish(Switch2DButtonType.txtWWWL.GetDisplayName());
        }
        else if (_viewScene == ViewScene.View3D)
        {
            _image3dViewModel?.CurrentImageViewer.GetWWWL(ref ww, ref wl);
            txtWW.Text = ww.ToString("F0");
            txtWL.Text = wl.ToString("F0");
            EventAggregator.Instance.GetEvent<Update3DHotKeyEvent>().Publish(Switch3DButtonType.txtWWWL.GetDisplayName());
        }
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
        if (string.IsNullOrEmpty(txtWW.Text.Trim()) || string.IsNullOrEmpty(txtWL.Text.Trim()))
            return;

        if (double.TryParse(txtWW.Text, out var ww) && double.TryParse(txtWL.Text, out var wl) && ww > 0)
        {
            if (_viewScene == ViewScene.View2D)
            {
                _image2dViewModel?.CurrentImageViewer.SetWWWL(ww, wl);

            }
            else if (_viewScene == ViewScene.View3D)
            {
                _image3dViewModel?.CurrentImageViewer.SetWWWL3D(ww, wl);
            }
        }

        Hide();
    }
}