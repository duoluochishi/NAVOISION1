//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Windows;

namespace NV.MPS.UI.Dialog;

public class Dialog
{
    public static readonly DependencyProperty WindowStyleProperty =
        DependencyProperty.RegisterAttached("WindowStyle", typeof(Style), typeof(Dialog), new PropertyMetadata(null));

    public static Style GetWindowStyle(DependencyObject obj)
    {
        return (Style)obj.GetValue(WindowStyleProperty);
    }

    public static void SetWindowStyle(DependencyObject obj, Style value)
    {
        obj.SetValue(WindowStyleProperty, value);
    }

    public static readonly DependencyProperty WindowStartupLocationProperty =
        DependencyProperty.RegisterAttached("WindowStartupLocation", typeof(WindowStartupLocation), typeof(Dialog), new UIPropertyMetadata(OnWindowStartupLocationChanged));


    public static WindowStartupLocation GetWindowStartupLocation(DependencyObject obj)
    {
        return (WindowStartupLocation)obj.GetValue(WindowStartupLocationProperty);
    }

    public static void SetWindowStartupLocation(DependencyObject obj, WindowStartupLocation value)
    {
        obj.SetValue(WindowStartupLocationProperty, value);
    }

    private static void OnWindowStartupLocationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is Window window)
        {
            window.WindowStartupLocation = (WindowStartupLocation)e.NewValue;
        }
    }
}