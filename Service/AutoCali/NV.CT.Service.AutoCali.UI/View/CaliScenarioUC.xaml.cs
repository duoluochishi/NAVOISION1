using NV.CT.Service.AutoCali.Logic;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.AutoCali.UI.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace NV.CT.Service.AutoCali.UI
{
    /// <summary>
    /// 校准场景配置UI查看
    /// </summary>
    public partial class CaliScenarioUC : UserControl
    {
        public CaliScenarioUC()
        {
            InitializeComponent();
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void CaliItemListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TryLoadArgProtocolControl(this.argProtocolControl, (this.DataContext as CaliScenarioViewModel));
        }

        internal static void TryLoadArgProtocolControl(UserControl userControl, CaliScenarioViewModel scenarioViewModel)
        {
            var caliItem = scenarioViewModel?.SelectedCaliItem;
            if (caliItem == null)
            {
                return;
            }
            //切换当前校准项目对应的参数协议Control
            Control dataGrid = GetArgProtocolControl(caliItem);
            if (dataGrid == null)
            {
                return;
            }

            dataGrid.DataContext = scenarioViewModel;
            //this.argProtocolControl.Content = dataGrid;
            userControl.Content = dataGrid;
        }

        private static Control GetArgProtocolControl(CalibrationItem caliItem)
        {
            var caliItemVM = new CaliItemVM(caliItem);
            currentArgProtocolUIControlType = caliItemVM.UIControlType;
            Control uiControl = null;
            if (!(argProtocolControlDic.TryGetValue(currentArgProtocolUIControlType, out uiControl)))
            {
                uiControl = (Control)System.Activator.CreateInstance(currentArgProtocolUIControlType);
                argProtocolControlDic.TryAdd(currentArgProtocolUIControlType, uiControl);
            }

            return uiControl;
        }

        private static Dictionary<Type, Control> argProtocolControlDic = new Dictionary<Type, Control>();
        private static Type currentArgProtocolUIControlType;
    }
}