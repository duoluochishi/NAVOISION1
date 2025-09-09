//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.MPS.UI.Dialog.Service;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

using Color = System.Windows.Media.Color;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace NV.MPS.UI.Dialog;

public class DialogWindow : Window, IDialogWindow
{   
    public IDialogResult Result { get; set; } = new DialogResult();
    private double posX;
    private double posY;

    public DialogWindow()
    {
        Init();
        this.Activate();

        Activated += Window_Activated;
        MouseDown += DialogWindow_MouseDown;
        MouseMove += DialogWindow_MouseMove;
    }

    private void Init()
    {
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        BorderThickness = new Thickness(0);       
        Background = new SolidColorBrush(new Color() { A = 0 });
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Topmost = true;
        Height = Double.NaN;
        Width = Double.NaN;
        ShowInTaskbar = false;
        AllowsTransparency = true;
    }

    private void DialogWindow_MouseMove(object sender, MouseEventArgs e)
    {
        Point p = e.GetPosition(this);

        posX = p.X; // private double posX is a class member
        posY = p.Y; // private double posY is a class member
    }

    private void DialogWindow_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //屏蔽弹窗之外的区域的点击事件
        if (posX < 0 || posX > this.Width || posY < 0 || posY > this.Height)
        {
            e.Handled = true;
        }       
    }

    /// <summary>
    /// 在主窗口中居中显示
    /// </summary>
    private static void SetWindowInCenterOwner(Window subWindow, Window parentWindow)
    {
        //最大化窗口，固定的弹出到主屏幕，因此需额外处理
        if (subWindow.WindowState == WindowState.Maximized)
        {
            //子窗口最大化时，需要根据屏幕设置位置；
            var screen = Screen.FromHandle(new WindowInteropHelper(parentWindow).Handle);

            Graphics currentGraphics = Graphics.FromHwnd(new WindowInteropHelper(parentWindow).Handle);
            double dpiXRatio = currentGraphics.DpiX / 96;
            double dpiYRatio = currentGraphics.DpiY / 96;

            subWindow.Left = screen.Bounds.Left / dpiXRatio;
            subWindow.Top = screen.Bounds.Top / dpiYRatio;
        }
        if (parentWindow.WindowState == WindowState.Maximized)
        {
            //父窗口最大化时，父窗口的location,因窗口设置margin，有可能不准确，故取屏幕位置
            var screen = Screen.FromHandle(new WindowInteropHelper(parentWindow).Handle);

            Graphics currentGraphics = Graphics.FromHwnd(new WindowInteropHelper(parentWindow).Handle);
            double dpiXRatio = currentGraphics.DpiX / 96;
            double dpiYRatio = currentGraphics.DpiY / 96;

            //窗口居中显示
            subWindow.Left = screen.Bounds.Left / dpiXRatio +
                             (screen.Bounds.Width / dpiXRatio - subWindow.ActualWidth) / 2;
            subWindow.Top = screen.Bounds.Top / dpiYRatio +
                            (screen.Bounds.Height / dpiYRatio - subWindow.ActualHeight) / 2;
        }
        else
        {
            //窗口居中显示
            subWindow.Left = parentWindow.Left + (parentWindow.ActualWidth - subWindow.ActualWidth) / 2;
            subWindow.Top = parentWindow.Top + (parentWindow.ActualHeight - subWindow.ActualHeight) / 2;
        }
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        System.Windows.Input.Mouse.Capture(this, System.Windows.Input.CaptureMode.SubTree);
    }
}