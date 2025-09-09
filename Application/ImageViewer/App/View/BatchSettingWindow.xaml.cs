//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.Extensions;
using NV.CT.ImageViewer.ViewModel;

namespace NV.CT.ImageViewer.View;

public partial class BatchSettingWindow
{
    private readonly BatchReconViewModel? _batchReconViewModel;
    public BatchSettingWindow()
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
        _batchReconViewModel = CTS.Global.ServiceProvider.GetService<BatchReconViewModel>();
        DataContext = _batchReconViewModel;
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}