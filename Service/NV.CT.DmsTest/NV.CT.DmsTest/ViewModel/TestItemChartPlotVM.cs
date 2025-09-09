using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot.Wpf;
using OxyPlot;

namespace NV.CT.DmsTest.ViewModel
{
    public partial class TestItemChartPlotVM : ObservableObject
    {
       public TestItemChartPlotVM(IEnumerable<PlotModel> plotModels)
        {
            testItemPlotModels = new ObservableCollection<PlotModel>(plotModels);
        }
        [ObservableProperty]
        public ObservableCollection<PlotModel>? testItemPlotModels = null;

    }
}
