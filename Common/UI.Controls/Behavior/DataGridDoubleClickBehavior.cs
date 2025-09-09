using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace NV.CT.UI.Controls.Behavior
{

    public static class DataGridDoubleClickBehavior
    {
        // 定义附加属性：绑定 ViewModel 中的命令
        public static readonly DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.RegisterAttached(
                "DoubleClickCommand",
                typeof(ICommand),
                typeof(DataGridDoubleClickBehavior),
                new PropertyMetadata(null, OnDoubleClickCommandChanged)
            );

        public static ICommand GetDoubleClickCommand(DependencyObject obj) =>
            (ICommand)obj.GetValue(DoubleClickCommandProperty);

        public static void SetDoubleClickCommand(DependencyObject obj, ICommand value) =>
            obj.SetValue(DoubleClickCommandProperty, value);

        // 属性变化时绑定事件
        private static void OnDoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                dataGrid.MouseDoubleClick -= HandleMouseDoubleClick;
                if (e.NewValue != null)
                {
                    dataGrid.MouseDoubleClick += HandleMouseDoubleClick;
                }
            }
        }

        // 处理双击事件
        private static void HandleMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            var command = GetDoubleClickCommand(dataGrid);

            // 遍历可视化树，判断是否点击了列头
            var element = e.OriginalSource as DependencyObject;
            while (element != null && !(element is DataGridRow))
            {
                if (element is DataGridColumnHeader)
                {
                    e.Handled = true; // 阻止列头双击
                    return;
                }
                element = VisualTreeHelper.GetParent(element);
            }

            // 触发 ViewModel 中的命令，并传递行数据
            if (element is DataGridRow row && command?.CanExecute(row.Item) == true)
            {
                command.Execute(row.Item);
            }
        }
    }

}
