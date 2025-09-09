namespace NV.CT.RGT.ViewModel;

public class ScanMainViewModel : BaseViewModel
{
    private bool _scanMainShow;
    public bool ScanMainShow
    {
        get => _scanMainShow;
        set => SetProperty(ref _scanMainShow, value);
    }

    private bool _reconMainShow = true;
    public bool SelectProtocolMainShow
    {
        get => _reconMainShow;
        set => SetProperty(ref _reconMainShow, value);
    }

    private bool _isScanMainShow;
    public bool IsScanMainShow
    {
        get => _isScanMainShow;
        set => SetProperty(ref _isScanMainShow, value);
    }

    private bool _scanParameterShow = true;
    public bool ScanParameterShow
    {
        get => _scanParameterShow;
        set => SetProperty(ref _scanParameterShow, value);
    }

    private bool _reconParameterShow;

    public bool ReconParameterShow
    {
        get => _reconParameterShow;
        set => SetProperty(ref _reconParameterShow, value);
    }

    private bool _isShowScanPara = true;
    public bool IsShowScanPara
    {
        get => _isShowScanPara;
        set
        {
            if (SetProperty(ref _isShowScanPara, value))
            {
                if (value)
                {
                    ScanParameterShow = true;
                    ReconParameterShow = false;
                }
                else
                {
                    ScanParameterShow = false;
                    ReconParameterShow = true;
                }
            }
        }
    }

    public ScanMainViewModel()
    {
    }

    private void SelectionManager_SelectionReconChanged(object? sender, CTS.EventArgs<ReconModel> e)
    {
        IsShowScanPara = false;
    }

    private ObservableCollection<int> _scanTaskList = new ObservableCollection<int>() { 1, 2, };
    public ObservableCollection<int> ScanTaskList
    {
        get => _scanTaskList;
        set => SetProperty(ref _scanTaskList, value);
    }
}