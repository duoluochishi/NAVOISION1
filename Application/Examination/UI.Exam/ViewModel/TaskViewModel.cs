//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Enums;

namespace NV.CT.UI.Exam.ViewModel;

public class TaskViewModel : BaseViewModel
{
    /// <summary>
    /// 扫描ID
    /// </summary>
    public string ScanId { get; set; } = string.Empty;

    /// <summary>
    /// 检查ID
    /// </summary>
    public string StudyId { get; set; } = string.Empty;

    private string _name = string.Empty;
    /// <summary>
    /// 扫描任务名称
    /// </summary>
    public string TaskName
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    /// <summary>
    /// 扫描协议ID
    /// </summary>
    public string ProtocolId { get; set; } = string.Empty;

    /// <summary>
    /// MeasurementID
    /// </summary>
    public string MeasurementId { get; set; } = string.Empty;

    /// <summary>
    /// FOR id
    /// </summary>
    public string ForId { get; set; } = string.Empty;

    public FailureReasonType failureReason = FailureReasonType.None;

    public FailureReasonType FailureReason
    {
        get => failureReason; set
        {
            SetProperty(ref failureReason, value);
            SetImageScanType();
        }
    }

    /// <summary>
    /// 扫描类型，定位片、轴扫、螺旋
    /// </summary>
    public ScanOption ScanOption { get; set; } = ScanOption.Axial;

    public ScanImageType ScanImageType
    {
        get
        {
            if (!(ScanOption == ScanOption.DualScout
                || ScanOption == ScanOption.Surview))
            {
                return ScanImageType.Tomo;
            }
            else
            {
                return ScanImageType.Topo;
            }
        }
    }

    /// <summary>
    /// 病人摆位
    /// </summary>
    public FacadeProxy.Common.Enums.PatientPosition PatientPosition { get; set; } = FacadeProxy.Common.Enums.PatientPosition.FFS;

    /// <summary>
    /// 扫描任务状态
    /// </summary>
    private PerformStatus _taskStatus = PerformStatus.Unperform;
    public PerformStatus ScanTaskStatus
    {
        get => _taskStatus;
        set
        {
            SetProperty(ref _taskStatus, value);
            SetImageScanType();
        }
    }

    private void SetImageScanType()
    {
        switch (ScanTaskStatus)
        {
            case PerformStatus.Waiting:
                ImageScanType = TaskViewImages[ProtocolParameterNames.TASKVIEWMODEL_NOTSCANED];
                break;
            case PerformStatus.Performing:
                ImageScanType = TaskViewImages[ProtocolParameterNames.TASKVIEWMODEL_STATUSSCANING];
                break;
            case PerformStatus.Performed:
                if (FailureReason == FailureReasonType.None)
                {
                    ImageScanType = TaskViewImages[ProtocolParameterNames.TASKVIEWMODEL_SCANED];
                }
                else
                {
                    ImageScanType = TaskViewImages[ProtocolParameterNames.TASKVIEWMODEL_SCANFAILED];
                }
                break;
            default:
                ImageScanType = TaskViewImages[ProtocolParameterNames.TASKVIEWMODEL_NOTSCANED];
                break;
        }
    }

    public WriteableBitmap? _imageScanType;
    public WriteableBitmap? ImageScanType
    {
        get => _imageScanType;
        set => SetProperty(ref _imageScanType, value);
    }

    /// <summary>
    /// 是否增强
    /// </summary>
    private bool _isEnhance;
    public bool IsEnhance
    {
        get => _isEnhance;
        set
        {
            if (SetProperty(ref _isEnhance, value) && value)
            {
                SEnhanceShow = true;
            }
            else
            {
                SEnhanceShow = false;
            }
        }
    }

    /// <summary>
    /// 是否紧急
    /// </summary>
    public bool IsEmergency { get; set; } = false;

    private bool _sEnhanceShow;
    public bool SEnhanceShow
    {
        get => _sEnhanceShow;
        set => SetProperty(ref _sEnhanceShow, value);
    }

    private bool _iconLineShow;
    public bool IconLineShow
    {
        get
        {
            if (IsFirst && IsLast)
            {
                _iconLineShow = false;
            }
            else
            {
                _iconLineShow = true;
            }
            return _iconLineShow;
        }
        set => SetProperty(ref _iconLineShow, value);
    }

    private bool _isFirst = true;
    public bool IsFirst
    {
        get => _isFirst;
        set => _isFirst = value;
    }

    private bool _isLast = true;
    public bool IsLast
    {
        get => _isLast;
        set => _isLast = value;
    }

    public string IconLineData
    {
        get
        {
            string str;
            if (IsFirst && IsLast)
            {
                str = string.Empty;
            }
            else if (IsFirst && (!IsLast))
            {
                str = "M3.917,17.5V36 M3.917,18H0";
            }
            else if ((!IsFirst) && (!IsLast))
            {
                str = "M3.917,0v36 M3.917,19H0";
            }
            else
            {
                str = "M3.917,18V0 M0,17.5h3.917";
            }
            return str;
        }
    }

    /// <summary>
    /// 扫描任务的参数
    /// </summary>
    private ScanParameterViewModel? _scanParameter;
    public ScanParameterViewModel? ScanParameter
    {
        get => _scanParameter;
        set => SetProperty(ref _scanParameter, value);
    }

    public TaskViewModel()
    {
        InitTaskViewImageDic();
    }

    private Dictionary<string, WriteableBitmap> _taskViewImages = new Dictionary<string, WriteableBitmap>();
    public Dictionary<string, WriteableBitmap> TaskViewImages
    {
        get => _taskViewImages;
        set => SetProperty(ref _taskViewImages, value);
    }

    private void InitTaskViewImageDic()
    {
        TaskViewImages.Add(ProtocolParameterNames.TASKVIEWMODEL_NOTSCANED, new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/NotScaned.png", UriKind.RelativeOrAbsolute))));
        TaskViewImages.Add(ProtocolParameterNames.TASKVIEWMODEL_STATUSSCANING, new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/StatusScaning.png", UriKind.RelativeOrAbsolute))));
        TaskViewImages.Add(ProtocolParameterNames.TASKVIEWMODEL_SCANED, new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/Scaned.png", UriKind.RelativeOrAbsolute))));
        TaskViewImages.Add(ProtocolParameterNames.TASKVIEWMODEL_SCANFAILED, new WriteableBitmap(new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/ScanedFailed.png", UriKind.RelativeOrAbsolute))));
    }
}