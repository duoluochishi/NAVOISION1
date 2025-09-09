using NV.CT.Service.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NV.CT.Service.UI.Util
{
    public class WindowHelper
    {
        private static Hashtable registerWindows = new Hashtable();
        private static Dictionary<string, Window> cachedWindows = new Dictionary<string, Window>();

        public static void Register<T>(string key)
        {
            Register(key, typeof(T));
        }

        public static void Register(string key, Type t)
        {
            if (!registerWindows.ContainsKey(key))
            {
                registerWindows.Add(key, t);
            }
            else
            {
                Console.WriteLine($"已注册：{key}，请勿重复注册！");//TODO:LOG
            }
        }

        public static void Unregister(string key)
        {
            if (registerWindows.ContainsKey(key))
            {
                registerWindows.Remove(key);
            }
            else
            {
                Console.WriteLine($"取消注册失败，未注册：{key}！");//TODO:LOG
            }
        }

        public static void ShowDialog(string key, object viewModel, object sender = null)
        {
            var poppuWindow = CreateWindow(key, viewModel);
            if (poppuWindow == null)
            {
                LogService.Instance.Error(ServiceCategory.AutoCali, $"ShowDialog for {key}, but created window is NUll");
                return;
            }

            DialogService.Instance.SetOwner(sender, poppuWindow);
            poppuWindow.ShowDialog();
            LogService.Instance.Debug(ServiceCategory.AutoCali, $"Closed the window({poppuWindow.GetType()})");
        }

        /// <summary>
        /// 由窗口的主题内容提供关闭功能
        /// </summary>
        /// <param name="windowContent"></param>
        public static async void Close(UserControl windowContent)
        {
            Window parentWindow = null;
            try
            {
                parentWindow = Window.GetWindow(windowContent);
                //parentWindow.Close();

                //由于MCS服务框架弹窗关闭后，第二次弹窗异常，修改为只是隐藏，由外层保证单例调用
                parentWindow.Visibility = Visibility.Collapsed;

                //ToDo:异步等待1秒后关闭，等ViewModel完成后清空ViewModel，否则ViewModel没有机会执行Close之前的保存动作
                await Task.Delay(1000);
                parentWindow.DataContext = null;
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(ServiceCategory.AutoCali, $"Close window({parentWindow})", ex);
            }
        }

        private static Window? CreateWindow(string key, object viewModel)
        {
            Window window = null;
            try
            {
                if (!registerWindows.ContainsKey(key))
                {
                    throw new Exception($"未注册：{key}对应的界面窗口");
                }

                //window = (Window)Activator.CreateInstance((Type)registerWindows[key]);

                if (cachedWindows.TryGetValue(key, out window))
                {
                    window.Visibility = Visibility.Visible;
                }
                else
                {
                    window = (Window)Activator.CreateInstance((Type)registerWindows[key]);
                }
                window.DataContext = viewModel;
            }
            catch (Exception ex)
            {
                LogService.Instance.Error(ServiceCategory.AutoCali, $"{nameof(WindowHelper)}", ex);
            }
            return window;
        }
    }
}
