//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.ViewModel;

namespace NV.CT.ImageViewer.View;

public partial class DicomTagWindow
{
    private readonly Image2DViewModel? _2dViewModel;
    public DicomTagWindow()
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

        _2dViewModel = CTS.Global.ServiceProvider.GetService<Image2DViewModel>();

        DataContext = _2dViewModel;
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}