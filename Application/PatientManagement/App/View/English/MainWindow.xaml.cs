//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
// <copyright file="VStudy.cs" company="纳米维景">
// 版权所有 (C)2020,纳米维景(上海)医疗科技有限公司
// </copyright>
// ---------------------------------------------------------------------

using System.Windows.Input;

namespace NV.CT.PatientManagement.View.English;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private bool IsWindowCaptionBar()
    {
        Point pp = Mouse.GetPosition(this);
        if (pp.Y is >= 0 and <= 40)
        {
            return true;
        }
        return false;
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && WindowState == WindowState.Normal && IsWindowCaptionBar())
        {
            DragMove();
        }
    }
}