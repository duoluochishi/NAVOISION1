//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.ViewModel;

namespace NV.CT.ImageViewer.View;

public partial class CustomRotateDegreeWindow
{
    private readonly CustomRotateViewModel? _customRotateViewModel;
    public CustomRotateDegreeWindow()
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

        _customRotateViewModel =  CTS.Global.ServiceProvider.GetRequiredService<CustomRotateViewModel>();
        DataContext = _customRotateViewModel;
    }

    private void BtnClose_OnClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// 自定义 角度
    /// </summary>
    private void BtnOk_OnClick(object sender, RoutedEventArgs e)
    {
        if (Validation.GetHasError(txtAngle)) return;
        Hide();
    }

    private void IntegerValidationRule_ValidationTriggered(bool state)
    {
        if (_customRotateViewModel is null) return;
        _customRotateViewModel.IsButtonEnabled = state;
    }
}