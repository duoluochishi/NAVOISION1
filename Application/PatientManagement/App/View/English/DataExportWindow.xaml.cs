//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
 // </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.PatientManagement.Models;
using System.Windows.Input;

namespace NV.CT.PatientManagement.View.English;

public partial class DataExportWindow : Window
{
    private readonly DataExportViewModel _viewModel;

    public DataExportWindow()
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

        _viewModel = Global.Instance.ServiceProvider.GetRequiredService<DataExportViewModel>();

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
        if (templatedParent != null)
        {
            return GetTemplatedAncestor<T>(templatedParent);
        }

        return null;
    }

}