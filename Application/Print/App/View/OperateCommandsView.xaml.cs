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
using NV.CT.Print.ViewModel;
using NV.MPS.Configuration;
using System.Windows.Controls;
using System.Windows.Input;

namespace NV.CT.Print.View
{
    /// <summary>
    /// OperateCommandsView.xaml 的交互逻辑
    /// </summary>
    public partial class OperateCommandsView : UserControl
    {
        private const string OPERATION_TYPE_WWWL = "wwwl";
        private const string OPERATION_TYPE_FILTER = "filter";
        public OperateCommandsView()
        {
            InitializeComponent();
            DataContext = CTS.Global.ServiceProvider.GetRequiredService<OperateCommandsViewModel>();
        }

        private void OnCheckBoxWwwlPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menuWwWl.PlacementTarget = labelWwWl;
            menuWwWl.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menuWwWl.IsOpen = true;
            checkboxWwwl.IsChecked = true;
            ((OperateCommandsViewModel)DataContext).OnOperateImage(OPERATION_TYPE_WWWL);            
        }

        private void OnLabelWwWlPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menuWwWl.PlacementTarget = labelWwWl;
            menuWwWl.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menuWwWl.IsOpen = true;
            ((OperateCommandsViewModel)DataContext).OnOperateImage(OPERATION_TYPE_WWWL);
            checkboxWwwl.IsChecked = true;
        }

        /// <summary>
        /// WWWL设置事件
        /// </summary>
        private void OnContextMenuItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var tag = ((TextBlock)sender).Tag;
            var windowType = (WindowingInfo)tag;
            ((OperateCommandsViewModel)DataContext).OnClickWWWLMenuItem(windowType);
        }

        private void OnFilterMenuItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var tag = ((TextBlock)sender).Tag;
            var filterType = (KernelType)tag;
            ((OperateCommandsViewModel)DataContext).OnClickFilterMenuItem(filterType);
        }
        private void OnCheckBoxFilterPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menuFilter.PlacementTarget = labelFilter;
            menuFilter.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menuFilter.IsOpen = true;
            ((OperateCommandsViewModel)DataContext).OnOperateImage(OPERATION_TYPE_FILTER);
            checkboxFilter.IsChecked = true;
        }

        private void OnLabelFilterPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            menuFilter.PlacementTarget = labelFilter;
            menuFilter.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            menuFilter.IsOpen = true;
            ((OperateCommandsViewModel)DataContext).OnOperateImage(OPERATION_TYPE_FILTER);
            checkboxFilter.IsChecked = true;
        }

    }
}
