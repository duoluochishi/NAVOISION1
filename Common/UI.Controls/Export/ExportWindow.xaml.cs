//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/9/10 11:01:27    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
using Microsoft.Extensions.DependencyInjection;
using NV.CT.CTS;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NV.CT.UI.Controls.Export
{
    /// <summary>
    /// ExportWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ExportWindow : Window
    {
        private readonly ExportWindowViewModel _viewModel;

        public ExportWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            MouseDown += (_, _) =>
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            };

            _viewModel = Global.ServiceProvider.GetRequiredService<ExportWindowViewModel>();
            DataContext = _viewModel;
            this.treeDriver.PreviewMouseRightButtonUp += TreeDriver_PreviewMouseRightButtonUp;
        }
        private void TreeDriver_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = GetTemplatedAncestor<TreeViewItem>(e.OriginalSource as FrameworkElement);
            item.IsSelected = true;
        }

        private static T GetTemplatedAncestor<T>(FrameworkElement element) where T : FrameworkElement
        {
            if (element is null)
            {
                return null;
            }

            if (element is T)
            {
                return element as T;
            }

            FrameworkElement templatedParent = element.TemplatedParent as FrameworkElement;
            if (templatedParent is not null)
            {
                return GetTemplatedAncestor<T>(templatedParent);
            }

            return null;
        }
    }
}
