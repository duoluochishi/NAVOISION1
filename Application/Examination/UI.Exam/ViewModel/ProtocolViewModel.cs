//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

namespace NV.CT.UI.Exam.ViewModel;

public class ProtocolViewModel : BaseViewModel
{
    //private readonly IProtocolOperation _protocolOperation;
    private string _id = string.Empty;
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    private string _protocolName = string.Empty;
    public string ProtocolName
    {
        get => _protocolName;
        set => SetProperty(ref _protocolName, value);
    }

    private CTS.Enums.BodyPart _bodyPart = CTS.Enums.BodyPart.Abdomen;
    public CTS.Enums.BodyPart BodyPart
    {
        get => _bodyPart;
        set => SetProperty(ref _bodyPart, value);
    }

    private bool _isAdult;
    public bool IsAdult
    {
        get => _isAdult;
        set => SetProperty(ref _isAdult, value);
    }

    private bool _isEnhanced;
    public bool IsEnhanced
    {
        get => _isEnhanced;
        set => SetProperty(ref _isEnhanced, value);
    }

    private bool _isEmergency;
    public bool IsEmergency
    {
        get => _isEmergency;
        set => SetProperty(ref _isEmergency, value);
    }

    private FacadeProxy.Common.Enums.PatientPosition _patientPosition = FacadeProxy.Common.Enums.PatientPosition.HFP;
    public FacadeProxy.Common.Enums.PatientPosition PatientPosition
    {
        get => _patientPosition;
        set => SetProperty(ref _patientPosition, value);
    }

    private bool _isDefaultMatch = false;
    public bool IsDefaultMatch
    {
        get => _isDefaultMatch;
        set
        {
            SetProperty(ref _isDefaultMatch, value);          
        }
    }

    private bool _isFactory = true;
    public bool IsFactory
    {
        get => _isFactory;
        set => SetProperty(ref _isFactory, value);
    }

    private bool _isOnTop = false;
    public bool IsOnTop
    {
        get => _isOnTop;
        set => SetProperty(ref _isOnTop, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private string _scanMode = string.Empty;
    public string ScanMode
    {
        get => _scanMode;
        set => SetProperty(ref _scanMode, value);
    }

    private ObservableCollection<ScanRangeViewModel> _scanRangeList = new ObservableCollection<ScanRangeViewModel>();
    public ObservableCollection<ScanRangeViewModel> ScanRangeList
    {
        get => _scanRangeList;
        set => SetProperty(ref _scanRangeList, value);
    }
}