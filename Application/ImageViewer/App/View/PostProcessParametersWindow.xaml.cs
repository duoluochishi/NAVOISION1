//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2025,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.ImageViewer.ViewModel;

namespace NV.CT.ImageViewer.View
{
    /// <summary>
    /// PostProcessParametersWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PostProcessParametersWindow : Window
    {
        public PostProcessParametersWindow()
        {
            InitializeComponent();
            DataContext = CTS.Global.ServiceProvider.GetRequiredService<SeriesViewModel>();
        }

        //private void DataGridRow_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    var row = sender as DataGridRow;
        //    row.IsSelected = true; // 强制选中该行
        //}
    }
}
