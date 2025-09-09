using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.TubeHistory.Enums;
using NV.CT.Service.TubeHistory.Models;
using NV.CT.Service.TubeHistory.Service.Interfaces;
using NV.MPS.Configuration;
using NV.MPS.Environment;

namespace NV.CT.Service.TubeHistory.ViewModels
{
    public partial class TubeHistoryViewModel : ObservableObject
    {
        #region Field

        private readonly ILogService _logService;
        private readonly ITubeLog _tubeLogService;

        #endregion

        public TubeHistoryViewModel(ILogService logService, ITubeLog tubeLogService)
        {
            _logService = logService;
            _tubeLogService = tubeLogService;
            TubeOverviewItems = [];
            DetailItems = [];
            TubeNoArray = Enumerable.Range(1, (int)SystemConfig.SourceComponentConfig.SourceComponent.SourceCount).ToArray();
            CompareDic = Enum.GetValues<CompareType>()
                             .ToFrozenDictionary(i => i,
                                                 i => typeof(CompareType).GetField(i.ToString())?.GetCustomAttribute<DescriptionAttribute>()?.Description ?? i.ToString());
            Filter = new()
            {
                StartTime = DateTime.Now.Date,
                EndTime = DateTime.Now,
                MicroArcingCompareType = CompareType.GreaterThan,
                BigArcingCompareType = CompareType.GreaterThan,
                TubeNo = TubeNoArray.First(),
            };
        }

        #region Property

        public int[] TubeNoArray { get; }
        public FrozenDictionary<CompareType, string> CompareDic { get; }
        public ObservableCollection<TubeOverviewModel> TubeOverviewItems { get; }
        public ObservableCollection<TubeDetailModel> DetailItems { get; }
        public FilterModel Filter { get; }

        #endregion

        [RelayCommand]
        private async Task Search()
        {
            DetailItems.Clear();
            DateTime? startTime = Filter.IsTimeChecked ? Filter.StartTime : null;
            DateTime? endTime = Filter.IsTimeChecked ? Filter.EndTime : null;
            List<int> tubeId = Filter.IsTubeNoChecked ? [Filter.TubeNo - 1] : [];
            List<string> tubeSN = Filter.IsTubeSNChecked ? [..Filter.TubeSN.Split([',', '，', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)] : [];
            var microArcingCount = Filter.IsMicroArcingCountChecked ? Filter.MicroArcingCount : -1;
            var bigArcingCount = Filter.IsBigArcingCountChecked ? Filter.BigArcingCount : -1;
            var timeStr = Filter.IsTimeChecked ? $" [Time] {startTime:yyyy-MM-dd HH:mm:ss} - {endTime:yyyy-MM-dd HH:mm:ss}" : "";
            var tubeIdStr = Filter.IsTubeNoChecked ? $" [TubeID] {string.Join(',', tubeId)}" : "";
            var tubeSNStr = Filter.IsTubeSNChecked ? $" [TubeSN] {string.Join(',', tubeSN)}" : "";
            var microArcingStr = Filter.IsMicroArcingCountChecked ? $" [MicroArcing] {CompareDic[Filter.MicroArcingCompareType]}{microArcingCount}" : "";
            var bigArcingStr = Filter.IsBigArcingCountChecked ? $" [BigArcing] {CompareDic[Filter.BigArcingCompareType]}{bigArcingCount}" : "";
            _logService.Info(ServiceCategory.TubeHistory, $"Search Filter:{timeStr}{tubeIdStr}{tubeSNStr}{microArcingStr}{bigArcingStr}");
            var logItems = await Task.Run(() => _tubeLogService.GetTubeLog(startTime, endTime, tubeId, tubeSN, microArcingCount, Filter.MicroArcingCompareType, bigArcingCount, Filter.BigArcingCompareType));

            foreach (var items in logItems)
            {
                foreach (var item in items)
                {
                    var detailItem = new TubeDetailModel()
                    {
                        ScanTime = item.dateTime,
                        ScanUID = item.scanParam.ScanUID,
                        TubeNo = item.tubeID,
                        TubeSN = item.tubeSN,
                        KV = item.scanParam.KV,
                        MA = item.scanParam.MA.Select(i => ((double)i).Microsecond2Millisecond()).ToArray(),
                        ExposureTime = ((double)item.scanParam.ExposureTime).Microsecond2Millisecond(),
                        FrameTime = ((double)item.scanParam.FrameTime).Microsecond2Millisecond(),
                        ScanOption = Enum.TryParse<ScanOption>(item.scanParam.ScanOption, out var scanOption) ? scanOption : default,
                        ExposureMode = Enum.TryParse<ExposureMode>(item.scanParam.ExposureMode, out var exposureMode) ? exposureMode : default,
                        Focal = (FocalType)item.scanParam.XRayFocus,
                        TotalFrames = item.scanParam.TotalFrames,
                        KVDose = item.doseInfo.kV,
                        MADose = item.doseInfo.mA,
                        ExposureTimeDose = item.doseInfo.mS,
                        Views = item.viewCount,
                        MicroArcingCountStr = $"{item.tubeArcing.TubeSmallArcing1}{Environment.NewLine}{item.tubeArcing.TubeSmallArcing2}{Environment.NewLine}{item.tubeArcing.TubeSmallArcing3}",
                        BigArcingCountStr = $"{item.tubeArcing.TubeLargeArcing1}{Environment.NewLine}{item.tubeArcing.TubeLargeArcing2}{Environment.NewLine}{item.tubeArcing.TubeLargeArcing3}",
                        HeatCapBeforeScan = item.tubeStatusBefore.HeatCap,
                        OilTempBeforeScan = item.tubeStatusBefore.OilTemp,
                        HeatCapAfterScan = item.tubeStatusAfter.HeatCap,
                        OilTempAfterScan = item.tubeStatusAfter.OilTemp,
                    };
                    var scanParamStrList = new List<string>();

                    for (var i = 0; i < detailItem.KV.Length; i++)
                    {
                        if (detailItem.KV[i] != 0 || detailItem.MA[i] != 0)
                        {
                            scanParamStrList.Add($"{detailItem.KV[i]}kV、{detailItem.MA[i]:0.###}mA、{detailItem.ExposureTime:0.###}ms");
                        }
                    }

                    detailItem.ScanParamStr = string.Join(Environment.NewLine, scanParamStrList);
                    DetailItems.Add(detailItem);
                }
            }
        }

        public async Task OnLoaded()
        {
            TubeOverviewItems.Clear();
            var tubeLifeItems = await Task.Run(_tubeLogService.GetTubeLife);

            foreach (var item in tubeLifeItems)
            {
                TubeOverviewItems.Add(new()
                {
                    TubeNo = item.tubeID,
                    TubeSN = item.tubeSN,
                    TotalExposureTime = item.totalMs,
                });
            }
        }
    }
}