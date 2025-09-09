//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.UI.Exam.ViewModel;
public class ScanRangeViewModel : BaseViewModel
{
    private string _scanRangeId = string.Empty;
    public string ScanRangeId
    {
        get => _scanRangeId;
        set => SetProperty(ref _scanRangeId, value);
    }

    private bool _scanRangeIsChecked;
    public bool ScanRangeIsChecked
    {
        get => _scanRangeIsChecked;
        set => SetProperty(ref _scanRangeIsChecked, value);
    }

    private bool _isShowScanCheckBox = true;
    public bool IsShowScanCheckBox
    {
        get => _isShowScanCheckBox;
        set => SetProperty(ref _isShowScanCheckBox, value);
    }

    private string _scanRange = string.Empty;
    public string ScanRange
    {
        get => _scanRange;
        set => SetProperty(ref _scanRange, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private string _reconIdList = string.Empty;
    public string ReconIdList
    {
        get => _reconIdList;
        set => SetProperty(ref _reconIdList, value);
    }

    private string _reconNameList = string.Empty;
    public string ReconNameList
    {
        get => _reconNameList;
        set => SetProperty(ref _reconNameList, value);
    }
}