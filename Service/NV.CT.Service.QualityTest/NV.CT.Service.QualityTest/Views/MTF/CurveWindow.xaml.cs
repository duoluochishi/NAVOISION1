using System.Linq;
using System.Windows.Media;
using NV.CT.Service.QualityTest.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace NV.CT.Service.QualityTest.Views.MTF
{
    /// <summary>
    /// CurveWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CurveWindow
    {
        private MTFCurveModel Data { get; }

        public CurveWindow(MTFCurveModel data)
        {
            InitializeComponent();
            Data = data;
            DataContext = Data;
            var controller = new PlotController();
            controller.BindMouseEnter(PlotCommands.HoverPointsOnlyTrack);
            Plot.Controller = controller;
        }

        private void BaseCustomWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var foregroundColor = (SolidColorBrush)FindResource("BaseForegroundColor");
            var oxyForegroundColor = OxyColor.FromArgb(foregroundColor.Color.A, foregroundColor.Color.R, foregroundColor.Color.G, foregroundColor.Color.B);

            // PlotModel
            var model = new PlotModel
            {
                Padding = new OxyThickness(0, 5, 5, 0),
                TextColor = oxyForegroundColor,
                PlotAreaBorderColor = oxyForegroundColor,
            };

            // X坐标轴和Y坐标轴
            var xAxis = new LinearAxis()
            {
                Title = "lp/cm",
                Position = AxisPosition.Bottom,
                AxisTitleDistance = 10,
                TitlePosition = 0.97,
                AbsoluteMinimum = 0,
                Minimum = 0,
                TitleColor = oxyForegroundColor,
                TextColor = oxyForegroundColor,
                AxislineColor = oxyForegroundColor,
                TicklineColor = oxyForegroundColor,
            };
            var yAxis = new LinearAxis()
            {
                Title = "MTF",
                Position = AxisPosition.Left,
                AxisTitleDistance = 10,
                AbsoluteMinimum = 0,
                Minimum = 0,
                TitleColor = oxyForegroundColor,
                TextColor = oxyForegroundColor,
                AxislineColor = oxyForegroundColor,
                TicklineColor = oxyForegroundColor,
            };

            // 数据曲线
            var lineSeries = new LineSeries()
            {
                TrackerFormatString = "MTF {4:0.##%}\r{2:F2} lp/cm",
                CanTrackerInterpolatePoints = false,
                MarkerType = MarkerType.Circle,
                InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline,
            };
            lineSeries.Points.AddRange(Data.LastMTFArray.Select(i => new DataPoint(i.X, i.Y)));

            // Apply
            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);
            model.Series.Add(lineSeries);
            Plot.Model = model;
            model.InvalidatePlot(true);
        }
    }
}