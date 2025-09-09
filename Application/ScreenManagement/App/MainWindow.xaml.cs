using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.AppService.Contract;
using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Forms;

using Application = System.Windows.Application;

namespace NV.CT.ScreenManagement;

public partial class MainWindow : Window
{
    //private readonly Hook hook = new Hook();
    //private readonly Timer timer = new Timer();
    private IScreenManagement? _screenManagement;

    //private readonly SecondaryWindow secondaryWindow = new();
    public MainWindow()
    {
        InitializeComponent();
        //timer.Interval = 100;
        //timer.Elapsed += Timer_Elapsed;
        //timer.Start();

        InitEvents();
    }

    private void InitEvents()
    {
        _screenManagement = CTS.Global.ServiceProvider?.GetRequiredService<IScreenManagement>();
        if (_screenManagement != null)
        {
            _screenManagement.LockScreenStatusChanged += _screenManagement_LockScreenStatusChanged;
            _screenManagement.UnlockScreenStatusChanged += _screenManagement_UnlockScreenStatusChanged;
        }
    }

    private void _screenManagement_UnlockScreenStatusChanged(object? sender, string e)
    {
        Application.Current?.Dispatcher?.Invoke(Hide);
    }

    private void _screenManagement_LockScreenStatusChanged(object? sender, string e)
    {
        Application.Current?.Dispatcher?.Invoke(() =>
        {
            //var lockReason = Enum.Parse<LockReason>(e);
            //if (lockReason == LockReason.EmergencyStop)
            //{
            //    InfoMessage.Text = "The system enters the emergency stop state！";
            //}
            //else if (lockReason == LockReason.DoorOpened)
            //{
            //    InfoMessage.Text = "The examination room door is open!";
            //}
            var logger = CTS.Global.ServiceProvider?.GetRequiredService<ILogger<MainWindow>>();
            logger?.LogDebug($"ScreenManagement.EmergencyStopped");
            Application.Current?.Dispatcher?.Invoke(Show);
        });
    }

    #region original code

    private void btn_Yes_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void EstopResumeFromGccEvent()
    {
        Dispatcher.Invoke(() =>
        {
            //  CT.SW.MainFrame.Common.DataCommon.OlogProxy.sendLog(LogCs.LogLevel.INFO, 0, "HideBtn_EstopResumeFromGccEstopView");
            InfoMessage.Text = "系统正在急停恢复中，请稍等...";
            btn_Continue.Visibility = Visibility.Collapsed;
            btn_No.Visibility = Visibility.Collapsed;

        });

    }

    private void btn_No_Click(object sender, RoutedEventArgs e)
    {

    }


    //protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    //{
    //    base.OnMouseLeftButtonDown(e);
    //    // 获取鼠标相对标题栏位置   
    //    Point position = e.GetPosition(TitleBar);
    //    // 如果鼠标位置在标题栏内，允许拖动     
    //    if (e.LeftButton == MouseButtonState.Pressed)
    //    {
    //        if (position.X >= 0 && position.X < TitleBar.ActualWidth && position.Y >= 0 && position.Y < TitleBar.ActualHeight)
    //        { DragMove(); }
    //    }
    //}

    private void btn_Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
    #endregion

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        //hook.Hook_Start();

        var screen1 = Screen.AllScreens.FirstOrDefault(s => s == Screen.PrimaryScreen);
        if (screen1 != null)
        {
            ScreenHelper.ShowOnMonitor(screen1, this);
            //secondaryWindow.Show();
        }
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        //KillTaskmgr();
    }

    private void KillTaskmgr()
    {
        //kill taskmgr process when user use Ctrl+Alt+Del to show 
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        //string user = "admin";
        //string pwd = "123456";

        //if (txtContent.Text == user)
        //{
        //    hook.Hook_Clear();
        //    timer.Stop();
        //    //关闭窗体
        //    Close();
        //}
        //else
        //{
        //    MessageBox.Show("User and Password is wrong", "error", MessageBoxButton.OK, MessageBoxImage.Error);
        //}
    }

    private void BtnOk_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
        //secondaryWindow.Close();
    }


}