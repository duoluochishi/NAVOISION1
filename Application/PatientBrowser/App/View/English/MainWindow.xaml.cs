//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:54:59    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using NV.CT.PatientBrowser.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;

namespace NV.CT.PatientBrowser.View.English;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public PatientInfoViewModel PatientInfoViewModel { get; set; }

    public WorkListViewModel WorkListViewModel { get; set; }
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = this;
        using (var scope = Global.Instance.ServiceProvider.CreateScope())
        {
            PatientInfoViewModel = scope.ServiceProvider.GetRequiredService<PatientInfoViewModel>();
            WorkListViewModel = scope.ServiceProvider.GetRequiredService<WorkListViewModel>();
        }
    }

    private bool IsWindowCaptionBar()
    {
        Point pp = Mouse.GetPosition(this);
        if (pp.Y >= 0 && pp.Y <= 40)
        {
            return true;
        }
        return false;
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if ((e.LeftButton == MouseButtonState.Pressed) && (WindowState == WindowState.Normal) && IsWindowCaptionBar())
        {
            this.DragMove();
        }
    }
}