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

using System.Windows.Threading;

namespace NV.CT.UI.Exam.ViewModel.Timeline;
public class MeasurementViewModel : BaseViewModel
{
    private int _measurementIndex = 0;
    public int MeasurementIndex
    {
        get => _measurementIndex;
        set => SetProperty(ref _measurementIndex, value);
    }

    private string _measurementID = string.Empty;
    public string MeasurementID
    {
        get => _measurementID;
        set => SetProperty(ref _measurementID, value);
    }

    private PerformStatus _measurementStatus = PerformStatus.Unperform;
    public PerformStatus MeasurementStatus
    {
        get => _measurementStatus;
        set
        {
            if (SetProperty(ref _measurementStatus, value) && value == PerformStatus.Performed)
            {
                IsCompleted = true;
            }
            if (value is PerformStatus.Performed or PerformStatus.Unperform)
            {
                ScanTimeRect = new Rect(0, 0, 0, 52);
				timer.Enabled = false;
            }
        }
    }

    private bool _isEnhance = false;
    public bool IsEnhance
    {
        get => _isEnhance;
        set => SetProperty(ref _isEnhance, value);
    }

    private bool _isVoiceEnable = false;
    public bool IsVoiceEnable
    {
        get => _isVoiceEnable;
        set => SetProperty(ref _isVoiceEnable, value);
    }

    private bool _isCompleted = false;
    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            if (SetProperty(ref _isCompleted, value) && value)
            {
                ScanTimeRect = new Rect(0, 0, 0, 52);
            }
        }
    }

    /// <summary>
    /// 一个measurement下面左右扫描任务的总时长
    /// </summary>
    private double _totalScanTime = 0;
    public double TotalScanTime
    {
        get => _totalScanTime;
        set => SetProperty(ref _totalScanTime, value);
    }

    private ObservableCollection<ScanTaskViewModel> _scanChildren = new();
    public ObservableCollection<ScanTaskViewModel> ScanChildren
    {
        get => _scanChildren;
        set => SetProperty(ref _scanChildren, value);
    }

    #region 进度条相关数据
    /// <summary>
    /// 
    /// </summary>
    private int _convertToScale = 10;
    public int ConvertToScale
    {
        get => _convertToScale;
        set => SetProperty(ref _convertToScale, value);
    }

    private Rect _scanTimeRect = new Rect(0, 0, 0, 52);
    public Rect ScanTimeRect
    {
        get => _scanTimeRect;
        set => SetProperty(ref _scanTimeRect, value);
    }
    #endregion

    #region 计时追踪 
    System.Timers.Timer timer;
    DateTime _startTime = DateTime.Now;
    private object _startTimeLock = new object();
    public bool IsStarting
    {
        set
        {
            if (value)
            {
                _startTime = DateTime.Now;
            }
            timer.Enabled = true;
        }
    }

    public MeasurementViewModel()
    {
        timer = new System.Timers.Timer(150);
        timer.Elapsed -= Timer_Elapsed;
        timer.Elapsed += Timer_Elapsed;
        timer.Enabled = false;
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        Application.Current?.Dispatcher?.Invoke(DispatcherPriority.Normal, () =>
        {
            lock (_startTimeLock)
            {
                double lastTime = (DateTime.Now - _startTime).TotalSeconds;
                if (lastTime <= TotalScanTime)
                {
                    ScanTimeRect = new Rect(0, 0, lastTime * ConvertToScale, 52);
                }
                else
                {
                    timer.Enabled = false;
                    IsCompleted = true;
                }
            }
        });
    }
    #endregion
}