//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.Examination.ViewModel;

public class ScanMainViewModel : BaseViewModel
{
    private readonly ISelectionManager _selectionManager;

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
        set
        {
            if (SetProperty(ref _isScanMainShow, value))
            {               
                var vm = CTS.Global.ServiceProvider?.GetRequiredService<ScanControlsViewModel>();

                if (value)
                {
                    //扫描主界面
                    ScanMainShow = true;
                    SelectProtocolMainShow = false;
                }
                else
                {
                    // 跳转到协议选择页面
                    ScanMainShow = false;
                    SelectProtocolMainShow = true;                    
                }
            }
        }
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

    public ScanMainViewModel(ISelectionManager selectionManager)
    {
        Commands.Add("ShowParameterDetail", new DelegateCommand<object>(ShowParameterDetail, _ => true));
        _selectionManager = selectionManager;
        _selectionManager.SelectionReconChanged += SelectionManager_SelectionReconChanged;
    }

    private void SelectionManager_SelectionReconChanged(object? sender, CTS.EventArgs<ReconModel> e)
    {
        IsShowScanPara = false;
    }

    public void ShowParameterDetail(object parameter)
    {
        if (CTS.Global.ServiceProvider?.GetRequiredService<ParameterDetailViewModel>() is ParameterDetailViewModel parameterDetailViewModel)
        {
            parameterDetailViewModel.IsShowScan = IsShowScanPara;
        }
    }

    private ObservableCollection<int> _scanTaskList = new ObservableCollection<int>() { 1, 2, };
    public ObservableCollection<int> ScanTaskList
    {
        get => _scanTaskList;
        set => SetProperty(ref _scanTaskList, value);
    }
}