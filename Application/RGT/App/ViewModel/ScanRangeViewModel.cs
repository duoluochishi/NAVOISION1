using Prism.Mvvm;

namespace NV.CT.RGT.ViewModel;

public class ScanRangeViewModel : BindableBase
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

    private bool _scanTypeShow;
    public bool ScanTypeShow
    {
        get => _scanTypeShow;
        set => SetProperty(ref _scanTypeShow, value);
    }
}