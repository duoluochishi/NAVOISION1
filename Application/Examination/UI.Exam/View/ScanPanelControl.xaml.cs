//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Windows.Input;

namespace NV.CT.UI.Exam.View;

public partial class ScanPanelControl
{
    public ScanPanelControl()
    {
        InitializeComponent();
        DataContext = Global.ServiceProvider.GetRequiredService<ScanReconViewModel>();
    }

    private void UIElement_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var scanReconViewModel = DataContext as ScanReconViewModel;
        if (scanReconViewModel is null)
            return;
        if (scanReconViewModel.SelectScanReconModel is not null && scanReconViewModel.SelectScanReconModel.IsTomo)
        {
            WapContextMenu.Visibility = Visibility.Visible;
        }
        else
        {
            WapContextMenu.Visibility = Visibility.Hidden;
        }
    }
}