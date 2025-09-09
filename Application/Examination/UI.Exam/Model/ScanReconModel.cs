//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.UI.Exam.Model;

public class ScanReconModel : BaseViewModel
{
    public int ReconNum
    {
        get => this.ReconModel.SeriesNumber;
    }

    private string _scanID = string.Empty;
    public string ScanID
    {
        get => _scanID;
        set => SetProperty(ref _scanID, value);
    }

    private string _id = string.Empty;
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private string _reconTaskID = string.Empty;
    public string ReconTaskID
    {
        get => _reconTaskID;
        set => SetProperty(ref _reconTaskID, value);
    }

    private string _seriesInStanceUID = string.Empty;
    public string SeriesInStanceUID
    {
        get => _seriesInStanceUID;
        set => SetProperty(ref _seriesInStanceUID, value);
    }

    private string _content = string.Empty;
    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    private WriteableBitmap? _image;
    public WriteableBitmap? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public bool IsRTD { get; set; }

    private string _imagePath = string.Empty;
    public string ImagePath
    {
        get => _imagePath;
        set => SetProperty(ref _imagePath, value);
    }

    private bool _isTomo;
    /// <summary>
    /// 是否Tomo，用于界面绑定非Tomo的时候右键菜单是否可用
    /// </summary>
    public bool IsTomo
    {
        get => _isTomo;
        set => SetProperty(ref _isTomo, value);
    }

    /// <summary>
    /// 表明当前Recon是在进行中还是 （已经完成，或者未开始阶段）
    /// </summary>
    public bool IsReconProcessing
    {
        get
        {
            if (ReconModel.Status == PerformStatus.Waiting || ReconModel.Status == PerformStatus.Performing)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private ReconModel _reconModel = new ReconModel();
    public ReconModel ReconModel
    {
        get => _reconModel;
        set => SetProperty(ref _reconModel, value);
    }

    private string? _toolTipMessage = null;
    public string? ToolTipMessage
    {
        get => _toolTipMessage;
        set => SetProperty(ref _toolTipMessage, value);
    }
}