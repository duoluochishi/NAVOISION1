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
using System.Windows.Controls;

namespace NV.CT.Print.View
{
    /// <summary>
    /// SeriesListView.xaml 的交互逻辑
    /// </summary>
    public partial class SeriesListView : UserControl
    {
        public SeriesListView()
        {
            InitializeComponent();
            DataContext = CTS.Global.ServiceProvider.GetRequiredService<SeriesListViewModel>();
        }
    }
}
