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
using System.Windows.Input;

namespace NV.CT.PatientManagement.View.English
{
    /// <summary>
    /// SeriesViewControl.xaml 的交互逻辑
    /// </summary>
    public partial class SeriesViewControl : UserControl
    {
        public SeriesViewControl()
        {
            InitializeComponent();
            DataContext = Global.Instance.ServiceProvider.GetRequiredService<SeriesViewModel>();

            this.dgSeries.PreviewMouseLeftButtonUp += OnDgSeriesMouseLeftButtonUp;
        }

        private void OnDgSeriesMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((SeriesViewModel)DataContext).IsClickedOnDataRowOfDatagrid = IsDataGridRowClick(sender as DataGrid, e);
        }

        /// <summary>
        /// 根据可视化层级关系准确判断是否点击了datagrid的数据行 
        /// </summary>
        /// <param name="dg"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static bool IsDataGridRowClick(DataGrid dg, MouseButtonEventArgs e)
        {
            var element = dg.InputHitTest(e.GetPosition(dg));
            var target = element as DependencyObject;
            int count = 0;
            while (target != null && count < 100)
            {
                if (target is DataGridRow)
                {
                    return true;
                }
                target = System.Windows.Media.VisualTreeHelper.GetParent(target);
                count++;
            }
            return false;
        }
    }
       
}
