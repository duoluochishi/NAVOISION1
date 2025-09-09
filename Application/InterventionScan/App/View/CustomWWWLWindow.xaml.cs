//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.InterventionalScan.Models;
using NV.CT.InterventionScan.ApplicationService.Contract;
using System.Windows.Input;

namespace NV.CT.InterventionScan.View;

public partial class CustomWWWLWindow
{
    private readonly IInterventionService _interventionService;
    public CustomWWWLWindow(IInterventionService interventionService)
    {
        InitializeComponent();
        _interventionService = interventionService;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        MouseDown += (_, _) =>
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        };
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

        if (double.TryParse(txtWW.Text, out var ww) && double.TryParse(txtWL.Text, out var wl) && ww >= 0 && wl >= 0)
        {
            _interventionService.ImageOperationCommand(CommandParameters.IMAGE_OPERATE_WL, $"{ww}*{wl}");
        }
        Hide();
    }
}