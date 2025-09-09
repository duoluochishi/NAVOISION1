//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:43:11    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.Print.Events;
using NV.CT.Print.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NV.CT.Print.View
{
    /// <summary>
    /// ProtocolSettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProtocolSettingsWindow : Window
    {
        public ProtocolSettingsWindow()
        {
            InitializeComponent();
            //MouseDown += (_, _) =>
            //{
            //    if (Mouse.LeftButton == MouseButtonState.Pressed)
            //    {
            //        DragMove();
            //    }
            //};

            using (var scope = CTS.Global.ServiceProvider.CreateScope())
            {
                DataContext = scope.ServiceProvider.GetRequiredService<ProtocolSettingsViewModel>();
            }
        }

        private void OnCellMouseMove(object sender, MouseEventArgs e)
        {
            var source = sender as TextBlock;
            var cell = source.Tag as CellViewModel;
            EventAggregator.Instance.GetEvent<CellMouseMovedEvent>().Publish(cell);
        }

        private void OnCellMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var source = sender as TextBlock;
            var cell = source.Tag as CellViewModel;
            EventAggregator.Instance.GetEvent<CellLeftMouseUpEvent>().Publish(cell);
        }

        private void OnCellMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var source = sender as TextBlock;
            var cell = source.Tag as CellViewModel;
            EventAggregator.Instance.GetEvent<CellLeftMouseDownEvent>().Publish(cell);
        }
    }
}
