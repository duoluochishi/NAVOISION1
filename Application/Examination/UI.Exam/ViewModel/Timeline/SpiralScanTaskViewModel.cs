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

namespace NV.CT.UI.Exam.ViewModel.Timeline;

public class SpiralScanTaskViewModel : BaseViewModel
{
    private string _scanID = string.Empty;
    public string ScanID
    {
        get => _scanID;
        set => SetProperty(ref _scanID, value);
    }

    private int _spiralIndex = 0;
    public int SpiralIndex
    {
        get => _spiralIndex;
        set => SetProperty(ref _spiralIndex, value);
    }

    /// <summary>
    /// 等待开始时长
    /// </summary>
    private double _startTime = 0;
    public double StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
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

    #region 进度条相关数据    
    /// <summary>
    /// 时间跟像素值的转换系数
    /// </summary>
    private int _convertToScale = 10;
    public int ConvertToScale
    {
        get => _convertToScale;
        set => SetProperty(ref _convertToScale, value);
    }

    public Rect ExposureTimeRect
    {
        get
        {
            return new Rect(StartTime * ConvertToScale, -3, ExposureTime * ConvertToScale, 10);
        }
    }
    #endregion
}