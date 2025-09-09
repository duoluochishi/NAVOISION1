using NV.CT.DmsTest.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot.Wpf;
using OxyPlot;

namespace NV.CT.DmsTest.View
{
    /// <summary>
    /// ChartControl.xaml 的交互逻辑
    /// </summary>
    public partial class TestItemChartPlotView : Window
    {
        public TestItemChartPlotView(IEnumerable<PlotModel> plotModels)
        {
            InitializeComponent();
            TestItemChartPlotVM testItemChartPlotVM = new TestItemChartPlotVM(plotModels);
            this.DataContext = testItemChartPlotVM;

        }
    }
}
