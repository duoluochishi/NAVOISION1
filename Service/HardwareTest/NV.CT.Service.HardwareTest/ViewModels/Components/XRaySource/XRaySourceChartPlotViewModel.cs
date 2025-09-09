using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Attachments.Configurations;
using NV.CT.Service.HardwareTest.Attachments.Repository;
using NV.CT.Service.HardwareTest.Models.Components.XRaySource;
using NV.CT.Service.HardwareTest.Share.Defaults;
using NV.CT.Service.HardwareTest.Share.Enums.Components;
using NV.CT.Service.HardwareTest.Share.Utils;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.Service.HardwareTest.ViewModels.Components.XRaySource
{
    public partial class XRaySourceChartPlotViewModel : ObservableObject
    {
        private readonly ILogService logService;
        private readonly XRaySourceConfigOptions xRaySourceConfigService;
        private readonly IRepository<XRaySourceHistoryData> xRaySourceHistoryDataRepository;
        
        public XRaySourceChartPlotViewModel(
            ILogService logService,
            IOptions<XRaySourceConfigOptions> xRaySourceConfigOptions,
            IRepository<XRaySourceHistoryData> xRaySourceHistoryDataRepository)
        {
            /** get services from DI **/
            this.logService = logService;
            this.xRaySourceConfigService = xRaySourceConfigOptions.Value;
            this.xRaySourceHistoryDataRepository = xRaySourceHistoryDataRepository;
            /** Initialize **/
            this.InitializeProperties();
        }

        #region Initialize

        private void InitializeProperties() 
        {
            /** 实例化 **/
            this.PlotModels = new ObservableCollection<PlotModel>();
            /** 绘制空图 **/
            this.PerformChartPlot(new());
        }

        #endregion

        #region Properties

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Start))]
        private DateTime startDate = DateTime.Now;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Start))]
        private DateTime startTime = DateTime.Now.AddHours(-1);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(End))]
        private DateTime endDate = DateTime.Now;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(End))]
        private DateTime endTime = DateTime.Now;

        [ObservableProperty]
        private XRaySourceHistoryDataType historyDataType = XRaySourceHistoryDataType.HeatCap;

        [ObservableProperty]
        private string logFilePath = string.Empty;

        public DateTime Start => DateTimeUtils.Combine(StartDate, StartTime);
        public DateTime End => DateTimeUtils.Combine(EndDate, EndTime);
        public ObservableCollection<PlotModel> PlotModels { get; private set; } = null!;

        public double AxisYStart { get; set; } = 0;
        public double AxisYEnd { get; set; } = 150;

        #endregion

        #region PropertiesChanged

        partial void OnHistoryDataTypeChanged(XRaySourceHistoryDataType oldValue, XRaySourceHistoryDataType newValue)
        {
            (this.AxisYStart, this.AxisYEnd) = newValue switch
            {
                XRaySourceHistoryDataType.kV => (60, 160),
                XRaySourceHistoryDataType.mA => (20, 220),
                XRaySourceHistoryDataType.HeatCap => (0, 150),
                XRaySourceHistoryDataType.OilTemp => (0, 120),
                _ => (0, 100)
            };
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void SelectLogFilePath() 
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Text documents (.txt)|*.txt|All files (*.*)|*.*"
            };

            var result = openFileDialog.ShowDialog();

            if (result.HasValue && result == true) 
            {
                this.LogFilePath = openFileDialog.FileName;
            }
        }

        [RelayCommand]
        private void PlotChart() 
        {
            /** 时间校验 **/
            if (Start >= End)
            {
                logService.Warn(
                    ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.XRaySourceComponent}] Invalid Parameters - StartTime: {Start.ToLongTimeString} is later than EndTime: {End.ToLongTimeString}");
                return;
            }

            /** 获取时间范围内的有效数据 **/
            var dataCollection = xRaySourceHistoryDataRepository.GetBetweenTimeSpan(Start, End);
            /** 限定数据类型 **/
            var specificHistoryData = dataCollection.Where(data => data.DataType == HistoryDataType).ToList();
            /** 绘制 **/
            if (specificHistoryData.Count > 0)
            {
                /** 绘图 **/
                this.PerformChartPlot(specificHistoryData);
                /** 记录 **/
                logService.Info(
                    ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.XRaySourceComponent}] Totally [{specificHistoryData.Count}] data in type [{Enum.GetName(HistoryDataType)}] has been plotted.");
            }
            else
            {
                logService.Info(
                    ServiceCategory.HardwareTest,
                    $"[{ComponentDefaults.XRaySourceComponent}] No history data found by Start: {Start}, End: {End}, DataType:{Enum.GetName(HistoryDataType)}");
            }
        }

        [RelayCommand]
        private void ResetChart() 
        {
            /** 绘制空图 **/
            this.PerformChartPlot(new());
        }

        #endregion

        #region Plot Related

        private void PerformChartPlot(List<XRaySourceHistoryData> historyDataList)
        {
            this.PlotModels.Clear();

            for ( int i = 0; i < xRaySourceConfigService.TubeInterfaceCount; i++ ) 
            {
                var plotModel = this.CreatePlotModel(i+1, Start, End);

                for (int j = 0; j < xRaySourceConfigService.XRaySourceCountPerTubeInterface; j++) 
                {
                    /** 获取Index **/
                    uint index = (uint)(i * xRaySourceConfigService.XRaySourceCountPerTubeInterface + j + 1);
                    /** 取与Index对应数据集合并生成点集 **/
                    var pointList = historyDataList
                        .Where(item => item.Index == index)
                        .OrderBy(item => item.TimeStamp)
                        .Select(item => new DataPoint(DateTimeAxis.ToDouble(item.TimeStamp), item.Value))
                        .ToList();
                    /** 生成series **/
                    LineSeries lineSeries = new LineSeries
                    {
                        Title = $"XRaySource-{index.ToString("00")}",
                        InterpolationAlgorithm = InterpolationAlgorithms.CatmullRomSpline
                    };
                    /** 添加点集 **/
                    if (pointList.Count > 0) 
                    {
                        lineSeries.Points.AddRange(pointList);
                    }                       
                    plotModel.Series.Add(lineSeries);
                }
                this.PlotModels.Add(plotModel);
            }
        }

        private PlotModel CreatePlotModel(int index, DateTime start, DateTime end)
        {
            /** 实例 **/
            var plotModel = new PlotModel
            {
                Title = $"Tube Interface {index.ToString("00")}"
            };

            /** X轴-日期 **/
            var XAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = DateTimeAxis.ToDouble(start),
                Maximum = DateTimeAxis.ToDouble(end),
                StringFormat = "HH:mm:ss"          
            };
            plotModel.Axes.Add(XAxis);

            /** Y轴-值 **/
            var YAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                IsZoomEnabled = true,
                IsPanEnabled = true,
                Minimum = AxisYStart,
                Maximum = AxisYEnd
            };
            plotModel.Axes.Add(YAxis);

            /** 图例 **/
            var legend = new Legend 
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendBorderThickness = 2,
                LegendPosition = LegendPosition.TopCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendFont = "微软雅黑",
                LegendFontSize = 12,
                LegendFontWeight = 1
               
            };
            plotModel.Legends.Add(legend);

            return plotModel;
        }

        #endregion

    }
}
