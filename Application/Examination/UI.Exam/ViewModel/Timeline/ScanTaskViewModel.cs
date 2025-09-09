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
using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.UI.Exam.ViewModel.Timeline;

public class ScanTaskViewModel : BaseViewModel
{
    private string _scanID = string.Empty;
    public string ScanID
    {
        get => _scanID;
        set => SetProperty(ref _scanID, value);
    }

    private int _scanIndex = 0;
    public int ScanIndex
    {
        get => _scanIndex;
        set => SetProperty(ref _scanIndex, value);
    }

    private string _scanName = string.Empty;
    public string ScanName
    {
        get => _scanName;
        set => SetProperty(ref _scanName, value);
    }

    private string _measurementID = string.Empty;
    public string MeasurementID
    {
        get => _measurementID;
        set => SetProperty(ref _measurementID, value);
    }

    private ScanOption _scanOption = ScanOption.None;
    public ScanOption ScanOption
    {
        get => _scanOption;
        set => SetProperty(ref _scanOption, value);
    }

    private PerformStatus _scanTaskStatus = PerformStatus.Unperform;
    public PerformStatus ScanTaskStatus
    {
        get => _scanTaskStatus;
        set
        {
            if (SetProperty(ref _scanTaskStatus, value) && value == PerformStatus.Performed)
            {
                IsCompleted = true;
            }
        }
    }

    /// <summary>
    /// 扫描是否已经完成
    /// </summary>
    private bool _isCompleted = false;
    public bool IsCompleted
    {
        get => _isCompleted;
        set => SetProperty(ref _isCompleted, value);
    }

    private bool _isEnhance = false;
    public bool IsEnhance
    {
        get => _isEnhance;
        set => SetProperty(ref _isEnhance, value);
    }

    private bool _isAutoScan = false;
    public bool IsAutoScan
    {
        get => _isAutoScan;
        set => SetProperty(ref _isAutoScan, value);
    }

    private int _autoScanIndex = 0;
    public int AutoScanIndex
    {
        get => _autoScanIndex;
        set => SetProperty(ref _autoScanIndex, value);
    }

    private bool _isVoiceEnable = false;
    public bool IsVoiceEnable
    {
        get => _isVoiceEnable;
        set => SetProperty(ref _isVoiceEnable, value);
    }

    /// <summary>
    /// 一个扫描的总时长，包含等待时长+延迟时长+扫描时长
    /// </summary>
    private double _scanTime = 0;
    public double ScanTime
    {
        get => _scanTime;
        set => SetProperty(ref _scanTime, value);
    }

    /// <summary>
    /// 等待开始时长
    /// </summary>
    private double _awaitTime = 0;
    public double AwaitTime
    {
        get => _awaitTime;
        set => SetProperty(ref _awaitTime, value);
    }

    /// <summary>
    /// 曝光时长
    /// </summary>
    private double _exposureTime = 0;
    public double ExposureTime
    {
        get => _exposureTime;
        set => SetProperty(ref _exposureTime, value);
    }

    /// <summary>
    /// 扫描延迟时长
    /// </summary>
    private double _exposureDelayTime = 0;
    public double ExposureDelayTime
    {
        get => _exposureDelayTime;
        set => SetProperty(ref _exposureDelayTime, value);
    }

    private double _tableAccelerationTime = 0;
    public double TableAccelerationTime
    {
        get => _tableAccelerationTime;
        set => SetProperty(ref _tableAccelerationTime, value);
    }

    #region 进度条相关数据
    /// <summary>
    /// 进度条的高度
    /// </summary>
    private double _strokeThickness = 10;
    public double StrokeThickness
    {
        get => _strokeThickness;
        set => SetProperty(ref _strokeThickness, value);
    }

    /// <summary>
    /// 进度条的高度坐标
    /// </summary>
    private int _yAxis = 2;
    public int YAxis
    {
        get => _yAxis;
        set => SetProperty(ref _yAxis, value);
    }

    /// <summary>
    /// 时间跟像素值的转换系数
    /// </summary>
    private int _convertToScale = 10;
    public int ConvertToScale
    {
        get => _convertToScale;
        set => SetProperty(ref _convertToScale, value);
    }

    /// <summary>
    /// 延迟时间进度条起点位置
    /// </summary>    
    public double DelayTimeX1Axis
    {
        get
        {
            return AwaitTime * ConvertToScale + StrokeThickness / 2;
        }
    }

    /// <summary>
    /// 延迟时间进度条终点位置
    /// </summary>
    public double DelayTimeX2Axis
    {
        get
        {
            return (AwaitTime + ExposureDelayTime) * ConvertToScale - StrokeThickness / 2;
        }
    }

    public Rect DelayTimeRect
    {
        get
        {
            return new Rect(AwaitTime * ConvertToScale, -3, ExposureDelayTime * ConvertToScale, 10);
        }
    }

    public double TableAccelerationTimeX1Axis
    {
        get
        {
            return (AwaitTime + ExposureDelayTime + ExposureTime) * ConvertToScale + StrokeThickness / 2;
        }
    }

    /// <summary>
    /// 延迟时间进度条终点位置
    /// </summary>
    public double TableAccelerationTimeX2Axis
    {
        get
        {
            return (AwaitTime + ExposureDelayTime + ExposureTime + TableAccelerationTime) * ConvertToScale - StrokeThickness / 2;
        }
    }

    public Rect TableAccelerationTimeRect
    {
        get
        {
            return new Rect((AwaitTime + ExposureDelayTime + ExposureTime) * ConvertToScale, -3, TableAccelerationTime * ConvertToScale, 10);
        }
    }

    public Rect ExposureTimeRect
    {
        get
        {
            return new Rect((AwaitTime + ExposureDelayTime) * ConvertToScale, -3, ExposureTime * ConvertToScale, 10);
        }
    }
    #endregion

    private bool _isSpiralScan = false;
    public bool IsSpiralScan
    {
        get => _isSpiralScan;
        set => SetProperty(ref _isSpiralScan, value);
    }

    private ObservableCollection<SpiralScanTaskViewModel> _spiralScanTask = new();
    public ObservableCollection<SpiralScanTaskViewModel> SpiralScanTaskList
    {
        get => _spiralScanTask;
        set
        {
            SetProperty(ref _spiralScanTask, value);
            if (value.Count > 0)
            {
                IsSpiralScan = true;
            }
            else
            {
                IsSpiralScan = false;
            }
        }
    }
}