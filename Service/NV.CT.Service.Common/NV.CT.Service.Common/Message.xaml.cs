using NV.CT.Service.Common.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NV.CT.Service.Common
{
    /// <summary>
    /// Message.xaml 的交互逻辑
    /// </summary>
    public partial class Message : Window
    {
        public Message()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public void SetTitle(string title)
        {
            this.Title = title;
        }

        public void SetMessage(string msg)
        {
            this.txt.Text = msg;
        }

        public static bool ShowDiaolog(string message)
        {
            try
            {
                bool res = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var msg = new Message();
                    var handle = ServiceFramework.Global.Instance.MainWindowHwnd;
                    //无法通过handle=0 来区别是主窗口通过MCS启动的，还是通过单独的Demo启动的。
                    //之前MCS服务框架启动的，handle不为0，需要设定handl为其owner
                    if (handle == IntPtr.Zero)
                    {
                        //需要避免msg和MainWindow是同一个对象，否则异常“不能设定owner为自身”
                        //msg.Owner = Application.Current.MainWindow;
                    }
                    else
                    {
                        WindowOwnerHelper.SetWindowOwner(msg, handle);
                    }
                    msg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    msg.SetMessage(message);
                    msg.ShowDialog();
                    res = (msg.DialogResult == true);
                });
                return res;
            }
            catch (Exception e)
            {
                LogService.Instance.Error(ServiceCategory.Common, $"show message error", e);
                return false;
            }
        }

        public static bool ShowDiaolog(string title, string message)
        {
            try
            {
                bool res = false;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var msg = new Message();
                    var handle = ServiceFramework.Global.Instance.MainWindowHwnd;
                    if (handle == IntPtr.Zero)
                    {
                        //msg.Owner = Application.Current.MainWindow;
                    }
                    else
                    {
                        WindowOwnerHelper.SetWindowOwner(msg, handle);
                    }
                    msg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    msg.SetTitle(title);
                    msg.SetMessage(message);
                    msg.ShowDialog();
                    res = (msg.DialogResult == true);
                });
                return res;
            }
            catch (Exception e)
            {
                LogService.Instance.Error(ServiceCategory.Common, $"show message error", e);
                return false;
            }
        }
    }
}