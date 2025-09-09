using NV.CT.Service.AutoCali.UI.Logic;
using NV.CT.Service.UI.Util;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NV.CT.Service.AutoCali.UI
{
    /// <summary>
    /// AutoCaliUC.xaml 的交互逻辑
    /// </summary>
    public partial class AutoCaliUC : UserControl
    {
        public AutoCaliUC()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //预先注册 校准场景的编辑窗口
            WindowHelper.Register<CaliScenarioEditWin>(WindowName_ScenarioEdit);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //预先注册 校准场景的编辑窗口
            WindowHelper.Unregister(WindowName_ScenarioEdit);
        }

        private void LogToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var logToggleButton = sender as ToggleButton;
            if (logToggleButton.IsChecked == true)
            {
                this.logTextBox.Height *= 2;
            }
            else
            {
                this.logTextBox.Height /= 2;
            }
        }

        private static readonly string ClassName = nameof(AutoCaliUC);
        private static readonly string WindowName_ScenarioEdit = nameof(CaliScenarioEditWin);

        private void AllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            var caliTaskViewModel = checkBox.DataContext as ICaliTaskViewModel;
            if (caliTaskViewModel != null)
            {
                caliTaskViewModel.IsChecked = (checkBox.IsChecked == true);
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedItem is ListBoxItem selectedItem)
            {
                // 获取UserControl的类型
                var userControlType = (Type)selectedItem.Tag;
                // 根据类型动态创建UserControl实例
                var userControl = (UserControl)Activator.CreateInstance(userControlType);
                // 将UserControl设置为ContentControl的内容
                contentControl.Content = userControl;
            }
        }
    }
}
