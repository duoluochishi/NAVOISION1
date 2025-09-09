using NV.CT.Service.AutoCali.Logic;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.AutoCali.UI.Logic;
using NV.CT.Service.UI.Util;
using System;
using System.Windows;
using System.Windows.Controls;

namespace NV.CT.Service.AutoCali.UI
{
    /// <summary>
    /// 新增或者修改 校准场景 的窗口
    /// CaliScenarioEditUC.xaml 的交互逻辑
    /// </summary>
    public partial class CaliScenarioEditUC : UserControl
    {
        private CaliScenarioEditViewModel viewModel;
        public CaliScenarioEditUC()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = this.DataContext as CaliScenarioEditViewModel;
            viewModel.CloseHostWindowHandler += OnClose;

            WindowHelper.Register<CaliItemArgProtocolEditWin>(typeof(CaliItemArgProtocolEditWin).Name);
        }

        private void OnClose(object? sender, CalibrationScenario e)
        {
            WindowHelper.Close(this);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            WindowHelper.Unregister(typeof(CaliItemArgProtocolEditWin).Name);
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            WindowHelper.Close(this);
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CaliScenarioUC.TryLoadArgProtocolControl(this.argProtocolControl, (this.DataContext as CaliScenarioViewModel));
        }
    }
}
