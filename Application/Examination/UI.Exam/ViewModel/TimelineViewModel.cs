//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/1/23 9:28:29           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.DatabaseService.Contract;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.UI.Exam.ViewModel.Timeline;
using NV.MPS.Configuration;
using NV.MPS.Environment;
using System.Windows.Media;

namespace NV.CT.UI.Exam.ViewModel;
public class TimelineViewModel : BaseViewModel
{
    private readonly ILogger<TimelineViewModel> _logger;
    private readonly IProtocolHostService _protocolHostService;
    private readonly IUIRelatedStatusService _uiRelatedStatusService;
    private readonly ISelectionManager _scanSelectManager;
    private readonly IVoiceService _voiceService;

    private ObservableCollection<MeasurementViewModel> _scanList = new();
    public ObservableCollection<MeasurementViewModel> ScanList
    {
        get => _scanList;
        set => SetProperty(ref _scanList, value);
    }

    private MeasurementViewModel _selectScan = new MeasurementViewModel();
    public MeasurementViewModel SelectScan
    {
        get => _selectScan;
        set => SetProperty(ref _selectScan, value);
    }

    private bool _isCompleted = false;
    public bool IsArrowEnable
    {
        get => _isCompleted;
        set => SetProperty(ref _isCompleted, value);
    }

    public ListView _listView;

    delegate void SetSelectItem(ListViewItem listViewItem);

    public TimelineViewModel(ILogger<TimelineViewModel> logger,
        IProtocolHostService protocolHostService,
        IUIRelatedStatusService uiRelatedStatusService,
        ISelectionManager scanSelectionManager,
        IVoiceService voiceService)
    {
        _logger = logger;
        _protocolHostService = protocolHostService;
        _uiRelatedStatusService = uiRelatedStatusService;
        _scanSelectManager = scanSelectionManager;
        _voiceService = voiceService;

        Commands.Add(CommandParameters.COMMAND_TIMELINENEXT, new DelegateCommand<object>(Next, (object obj) => true));
        Commands.Add(CommandParameters.COMMAND_TIMELINEBACK, new DelegateCommand<object>(Back, (object obj) => true));

        _protocolHostService.StructureChanged -= ProtocolHostService_StructureChanged;
        _protocolHostService.StructureChanged += ProtocolHostService_StructureChanged;

        _protocolHostService.ParameterChanged -= ProtocolHostService_ParameterChanged;
        _protocolHostService.ParameterChanged += ProtocolHostService_ParameterChanged;

        _uiRelatedStatusService.RealtimeStatusChanged -= RealtimeStatusChanged;
        _uiRelatedStatusService.RealtimeStatusChanged += RealtimeStatusChanged;
        _scanSelectManager.SelectionScanChanged -= SelectionScanChanged;
        _scanSelectManager.SelectionScanChanged += SelectionScanChanged;
        _protocolHostService.PerformStatusChanged -= OnPerformStatusChanged;
        _protocolHostService.PerformStatusChanged += OnPerformStatusChanged;
    }

    [UIRoute]
    private void ProtocolHostService_ParameterChanged(object? sender, EventArgs<(BaseModel baseModel, List<string> list)> e)
    {
        if (e is null || e.Data.baseModel is null || e.Data.list is null)
        {
            return;
        }
        if (e.Data.baseModel is ScanModel scanModel)
        {
            var mScan = ScanList.FirstOrDefault(t => t.MeasurementID == scanModel.Parent.Descriptor.Id);
            if (mScan is null)
            {
                return;
            }
            if (e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_IS_VOICE_SUPPORTED)) is not null)
            {
                mScan.IsVoiceEnable = scanModel.IsVoiceSupported;
            }
            if (e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.IS_ENHANCED)) is not null)
            {
                mScan.IsEnhance = scanModel.IsEnhanced;
            }
            if (e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_LENGTH)) is not null
                || e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_RECON_VOLUME_START_POSITION)) is not null
                || e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_RECON_VOLUME_END_POSITION)) is not null
                || e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_LOOPS)) is not null
                || e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_LOOP_TIME)) is not null
                || e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_EXPOSURE_DELAY_TIME)) is not null
                || e.Data.list.FirstOrDefault(t => t.Equals(ProtocolParameterNames.SCAN_EXPOSURE_INTERVAL_TIME)) is not null)
            {
                ProtocolStructureChanged();
            }
        }
    }

    [UIRoute]
    private void ProtocolHostService_StructureChanged(object? sender, EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)> e)
    {
        if (e is null)
        {
            return;
        }
        if (e.Data.Current is not ReconModel
            && (e.Data.ChangeType == StructureChangeType.Delete
            || e.Data.ChangeType == StructureChangeType.Add
            || e.Data.ChangeType == StructureChangeType.Replace))
        {
            ProtocolStructureChanged();
        }
    }

    private void ProtocolStructureChanged()
    {
        if (_protocolHostService.Models is null || _protocolHostService.Models.Count == 0)
        {
            return;
        }
        string id = string.Empty;
        if (SelectScan is not null || !string.IsNullOrEmpty(SelectScan.MeasurementID))
        {
            id = SelectScan.MeasurementID;
        }
        SelectScan = new MeasurementViewModel();
        ScanList.Clear();
        int scanIndex = 1;
        foreach (var form in _protocolHostService.Instance.Children)
        {
            foreach (var measurement in form.Children)
            {
                MeasurementViewModel measurementViewModel = new MeasurementViewModel();
                measurementViewModel.MeasurementID = measurement.Descriptor.Id;
                measurementViewModel.MeasurementStatus = measurement.Status;
                measurementViewModel.IsVoiceEnable = false;
                measurementViewModel.IsEnhance = false;
                int index = 1;
                foreach (var scan in measurement.Children)
                {
                    if (scan.IsVoiceSupported)
                    {
                        measurementViewModel.IsVoiceEnable = true;
                    }
                    if (scan.IsEnhanced)
                    {
                        measurementViewModel.IsEnhance = true;
                    }
                    ScanTaskViewModel scanTaskViewModel = new ScanTaskViewModel();
                    scanTaskViewModel.IsEnhance = scan.IsEnhanced;
                    scanTaskViewModel.IsVoiceEnable = scan.IsVoiceSupported;
                    scanTaskViewModel.ScanID = scan.Descriptor.Id;
                    scanTaskViewModel.MeasurementID = measurement.Descriptor.Id;
                    scanTaskViewModel.ScanName = scan.Descriptor.Name;
                    scanTaskViewModel.ScanIndex = scanIndex;
                    scanTaskViewModel.ScanOption = scan.ScanOption;
                    scanTaskViewModel.ScanTaskStatus = scan.Status;
                    scanTaskViewModel.IsAutoScan = scan.AutoScan;

                    scanTaskViewModel.AwaitTime = 0;
                    scanTaskViewModel.ExposureDelayTime = GetExposureDelayTime(measurementViewModel, UnitConvert.Microsecond2Second((double)scan.ExposureDelayTime));
                    scanTaskViewModel.ExposureTime = Math.Round((double)ScanTimeHelper.GetScanTime(scan), 1);

                    //扫描结束的时间如果没有后语音，默认设置为2秒
                    scanTaskViewModel.TableAccelerationTime = 2;
                    if (scan.IsVoiceSupported && scan.PostVoiceId > 0)
                    {
                        var model = _voiceService.GetVoiceInfo(scan.PostVoiceId.ToString());
                        if (model is not null)
                        {
                            scanTaskViewModel.TableAccelerationTime = model.VoiceLength;
                        }
                    }
                    //TODO:时间间隔计算扫描结束后时间长度
                    if ((scan.AutoScan || measurement.Children.Count > 1) && index >= 1 && index < measurement.Children.Count)
                    {
                        int preTime = 0;
                        ScanModel nextScan = measurement.Children[index];
                        if (nextScan.IsVoiceSupported && nextScan.PreVoiceId > 0)
                        {
                            var model = _voiceService.GetVoiceInfo(nextScan.PreVoiceId.ToString());
                            if (model is not null)
                            {
                                preTime += UnitConvert.Second2Microsecond(model.VoiceLength) + SystemConfig.AcquisitionConfig.Acquisition.PostPreVoiceDelayTime.Value;
                            }
                        }
                        //preTime += (int)nextScan.PreVoiceDelayTime;
                        scanTaskViewModel.TableAccelerationTime = Math.Abs(UnitConvert.Microsecond2Second((double)nextScan.ExposureIntervalTime - preTime));
                    }

                    if (scan.Status != PerformStatus.Performed && scan.Loops > 0 && scan.LoopTime > 0)
                    {
                        scanTaskViewModel.IsSpiralScan = true;
                        scanTaskViewModel = GetSpiralScanTask(scanTaskViewModel, scan, (scanTaskViewModel.AwaitTime + scanTaskViewModel.ExposureDelayTime));
                    }
                    else
                    {
                        scanTaskViewModel.IsSpiralScan = false;
                    }
                    scanTaskViewModel.ScanTime = scanTaskViewModel.AwaitTime + scanTaskViewModel.ExposureDelayTime + scanTaskViewModel.ExposureTime + scanTaskViewModel.TableAccelerationTime;
                    measurementViewModel.TotalScanTime += scanTaskViewModel.ScanTime;

                    if (scan.AutoScan || measurement.Children.Count > 1)
                    {
                        scanTaskViewModel.AutoScanIndex = index;
                        index++;
                    }
                    measurementViewModel.ScanChildren.Add(scanTaskViewModel);
                    scanIndex++;
                }
                ScanList.Add(measurementViewModel);
            }
        }

        if (ScanList.FirstOrDefault(t => t.MeasurementID == id) is MeasurementViewModel select)
        {
            SelectScan = select;
        }
        else if (ScanList.FirstOrDefault(t => t.MeasurementStatus == PerformStatus.Unperform) is MeasurementViewModel unSelect)
        {
            SelectScan = unSelect;
        }
        else if (ScanList.FirstOrDefault(t => _scanSelectManager.CurrentSelection.Measurement != null && t.MeasurementID == _scanSelectManager.CurrentSelection.Measurement.Descriptor.Id) is MeasurementViewModel selectPerformed)
        {
            SelectScan = selectPerformed;
        }
        IsArrowEnable = GetTotalWidth();
    }

    private double GetExposureDelayTime(MeasurementViewModel measurement, double exposureDelayTime)
    {
        double dt = 0;
        if (measurement.ScanChildren is not null && measurement.ScanChildren.Count > 0)
        {
            foreach (var item in measurement.ScanChildren)
            {
                dt += item.ScanTime;
            }
        }
        return exposureDelayTime - dt;
    }

    //处理多圈扫描的曝光进度条的数据
    private ScanTaskViewModel GetSpiralScanTask(ScanTaskViewModel scanTaskViewModel, ScanModel scanModel, double startTime)
    {
        double loopTime = Math.Round((double)ScanTimeHelper.GetScanTime(scanModel), 1) / scanModel.Loops;
        for (int i = 0; i < scanModel.Loops; i++)
        {
            SpiralScanTaskViewModel spiralScanTaskViewModel = new SpiralScanTaskViewModel();
            spiralScanTaskViewModel.ScanID = scanModel.Descriptor.Id;
            spiralScanTaskViewModel.SpiralIndex = i;
            if (i == 0)
            {
                spiralScanTaskViewModel.StartTime = startTime;
            }
            else
            {
                spiralScanTaskViewModel.StartTime = 0;
            }
            spiralScanTaskViewModel.ExposureTime = loopTime;

            scanTaskViewModel.SpiralScanTaskList.Add(spiralScanTaskViewModel);
        }
        return scanTaskViewModel;
    }

    public bool GetTotalWidth()
    {
        double totalWidth = 0;
        foreach (var item in ScanList)
        {
            totalWidth += item.TotalScanTime * 10 + 22 + 16;       //20:时间转换成像素的转换率，单位秒；22是图标的宽度，16是布局宽度值双边
        }
        return totalWidth > 1500;      //1500表示最多的默认宽度
    }

    [UIRoute]
    private void SelectionScanChanged(object? sender, EventArgs<ScanModel> e)
    {
        if (e is null || e.Data is null || e.Data.Parent is null)
        {
            return;
        }
        var model = ScanList.FirstOrDefault(t => t.MeasurementID == e.Data.Parent.Descriptor.Id);
        if (model is MeasurementViewModel)
        {
            SelectScan = model;
            if (_listView is ListView sListView
                && _listView.SelectedItem is not null)
            {
                _listView.ScrollIntoView(_listView.SelectedItem);
            }
        }
    }

    [UIRoute]
    private void RealtimeStatusChanged(object? sender, EventArgs<RealtimeInfo> e)
    {
        //曝光开始
        if (ScanList.Count > 0
            && e.Data.Status == RealtimeStatus.ExposureStarted
            && !string.IsNullOrEmpty(e.Data.ScanId))
        {
            var model = ScanList.FirstOrDefault(t => t.ScanChildren.FirstOrDefault(t => t.ScanID == e.Data.ScanId) is not null);
            if (model is MeasurementViewModel measurement && model.MeasurementStatus != PerformStatus.Performed)
            {
                var scan = measurement.ScanChildren.FirstOrDefault(t => t.ScanID == e.Data.ScanId);
                if (measurement.ScanChildren.FirstOrDefault(t => t.ScanID == e.Data.ScanId) is ScanTaskViewModel scanTaskView)
                {
                    if (scanTaskView.AutoScanIndex > 1) //大于1表示非第一个的连扫
                    {
                        measurement.IsStarting = false;
                    }
                    else
                    {
                        measurement.IsStarting = true;
                    }
                }
            }
        }
        //TODO:暂先移除
        //if (ScanList.Count > 0
        //    && (e.Data.Status == RealtimeStatus.NormalScanStopped
        //    || e.Data.Status == RealtimeStatus.EmergencyScanStopped
        //    || e.Data.Status == RealtimeStatus.Error)
        //    && !string.IsNullOrEmpty(e.Data.ScanId))
        //{
        //    var model = ScanList.FirstOrDefault(t => t.ScanChildren.FirstOrDefault(t => t.ScanID == e.Data.ScanId) is not null);
        //    if (model is MeasurementViewModel measurement)
        //    {
        //        foreach (var scanModel in measurement.ScanChildren)
        //        {
        //            if (scanModel.ScanID == e.Data.ScanId)
        //            {
        //                scanModel.ScanTaskStatus = PerformStatus.Performed;
        //            }
        //        }
        //        measurement.MeasurementStatus = PerformStatus.Performed;
        //    }
        //}
    }

    [UIRoute]
    private void OnPerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
    {
        if (e.Data.Model is MeasurementModel measurementModel)
        {
            var model = ScanList.FirstOrDefault(t => t.ScanChildren.FirstOrDefault(t => t.MeasurementID == measurementModel.Descriptor.Id) is not null);
            if (model is MeasurementViewModel measurement)
            {
                foreach (var scanModel in measurement.ScanChildren)
                {
                    scanModel.ScanTaskStatus = e.Data.NewStatus;
                }
                measurement.MeasurementStatus = e.Data.NewStatus;
            }
        }
    }

    private void Next(object objListView)
    {
        if (objListView is ListView listView
            && VisualTreeHelper.GetChild(listView, 0) is Border border
            && border.Child is ScrollViewer scview)
        {
            scview.ScrollToHorizontalOffset(scview.HorizontalOffset - 1);
        }
    }

    private void Back(object objListView)
    {
        if (objListView is ListView listView
            && VisualTreeHelper.GetChild(listView, 0) is Border border
            && border.Child is ScrollViewer scview)
        {
            scview.ScrollToHorizontalOffset(scview.HorizontalOffset + 1);
        }
    }
}