//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Enums;
using PatientPosition = NV.CT.FacadeProxy.Common.Enums.PatientPosition;
namespace NV.CT.UI.Exam.ViewModel;

public class BodyPositionViewModel : BaseViewModel
{
    private readonly ISelectionManager _scanSelectManager;
    private readonly IProtocolHostService _protocolHostService;
    private readonly IDialogService _dialogService;

    private ObservableCollection<BodyMapViewModel> _bodyMapViewModels = new();

    private Dictionary<string, BitmapImage> _axialImages = new Dictionary<string, BitmapImage>();
    public Dictionary<string, BitmapImage> AxialImages
    {
        get => _axialImages;
        set => SetProperty(ref _axialImages, value);
    }

    private Dictionary<string, BitmapImage> _helicalImages = new Dictionary<string, BitmapImage>();
    public Dictionary<string, BitmapImage> HelicalImages
    {
        get => _helicalImages;
        set => SetProperty(ref _helicalImages, value);
    }

    private Dictionary<string, BitmapImage> _topoImages = new Dictionary<string, BitmapImage>();
    public Dictionary<string, BitmapImage> TopoImages
    {
        get => _topoImages;
        set => SetProperty(ref _topoImages, value);
    }

    public ObservableCollection<BodyMapViewModel> BodyMapViewModels
    {
        get => _bodyMapViewModels;
        set => SetProperty(ref _bodyMapViewModels, value);
    }

    private BitmapImage _imageScanType = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFS_Top.png", UriKind.RelativeOrAbsolute));
    public BitmapImage ImageScanType
    {
        get => _imageScanType;
        set => SetProperty(ref _imageScanType, value);
    }

    private bool _isPositionChanged;
    public bool IsPositionChanged
    {
        get => _isPositionChanged;
        set => SetProperty(ref _isPositionChanged, value);
    }

    private bool _isPositionChangedEnabled = true;
    public bool IsPositionChangedEnabled
    {
        get => _isPositionChangedEnabled;
        set => SetProperty(ref _isPositionChangedEnabled, value);
    }

    private ScanModel? CurrentScanModel { get; set; }

    private bool IsUIChange = true;

    private BodyMapViewModel? _selectBodyMap;
    public BodyMapViewModel? SelectBodyMap
    {
        get
        {
            switch (PatientPosition)
            {
                case PatientPosition.HFS:
                    return BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_HFS));
                case PatientPosition.HFP:
                    return BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_HFP));
                case PatientPosition.HFDL:
                    return BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_HFDL));
                case PatientPosition.HFDR:
                    return BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_HFDR));
                case PatientPosition.FFS:
                    return BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_FFS));
                case PatientPosition.FFP:
                    return BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_FFP));
                case PatientPosition.FFDL:
                    return BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_FFDL));
                case PatientPosition.FFDR:
                    return BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_FFDR));
                default:
                    return BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_HFS));
            }
        }
        set
        {
            if (value is null)
            {
                return;
            }
            if (SetProperty(ref _selectBodyMap, value))
            {
                switch (value?.Name)
                {
                    case ProtocolParameterNames.PATIENTPOSITION_HFS:
                        PatientPosition = PatientPosition.HFS;
                        break;
                    case ProtocolParameterNames.PATIENTPOSITION_HFP:
                        PatientPosition = PatientPosition.HFP;
                        break;
                    case ProtocolParameterNames.PATIENTPOSITION_HFDL:
                        PatientPosition = PatientPosition.HFDL;
                        break;
                    case ProtocolParameterNames.PATIENTPOSITION_HFDR:
                        PatientPosition = PatientPosition.HFDR;
                        break;
                    case ProtocolParameterNames.PATIENTPOSITION_FFS:
                        PatientPosition = PatientPosition.FFS;
                        break;
                    case ProtocolParameterNames.PATIENTPOSITION_FFP:
                        PatientPosition = PatientPosition.FFP;
                        break;
                    case ProtocolParameterNames.PATIENTPOSITION_FFDL:
                        PatientPosition = PatientPosition.FFDL;
                        break;
                    case ProtocolParameterNames.PATIENTPOSITION_FFDR:
                        PatientPosition = PatientPosition.FFDR;
                        break;
                    default:
                        PatientPosition = PatientPosition.HFS;
                        break;
                }
                if (IsUIChange)
                {
                    IsPositionChanged = true;
                    if (CurrentScanModel is not null
                        && _protocolHostService.Models.FirstOrDefault(item => item.Measurement is not null && item.Measurement.Descriptor.Id.Equals(CurrentScanModel.Parent.Descriptor.Id)).Measurement is MeasurementModel measurement1)
                    {
                        _protocolHostService.ModifyFOR(measurement1.Descriptor.Id, PatientPosition);
                        _dialogService.ShowDialog(false, MessageLeveles.Warning, LanguageResource.Message_Warning_GoWarningTitle, LanguageResource.Message_Warning_ConfirmPositionInfo, arg => { }, ConsoleSystemHelper.WindowHwnd);
                        IsTomoPositionChanged(CurrentScanModel);
                    }
                    else //当没有选择扫描任务时修改第一个未开始的measurement
                    {
                        var activeItem = _protocolHostService.Models.FirstOrDefault(item => item.Measurement.Status == PerformStatus.Unperform);
                        if (activeItem.Measurement is MeasurementModel measurement)
                        {
                            _protocolHostService.ModifyFOR(measurement.Descriptor.Id, PatientPosition);
                        }
                    }
                }
                GetImageScanType();
            }
        }
    }

    private PatientPosition _patientPosition = PatientPosition.HFP;
    /// <summary>
    /// 获取选择的扫描体位及设定扫描体位
    /// </summary>
    public PatientPosition PatientPosition
    {
        get => _patientPosition;
        set => SetProperty(ref _patientPosition, value);
    }

    /// <summary>
    /// 扫描模式
    /// </summary>
    private ScanOption _scanMode = ScanOption.Axial;
    public ScanOption ScanMode
    {
        get => _scanMode;
        set => SetProperty(ref _scanMode, value);
    }

    private string _patientPositionChangedMessage = LanguageResource.Message_Info_PatientPositionChangedMessage;

    public string PatientPositionChangedMessage
    {
        get => _patientPositionChangedMessage;
        set => SetProperty(ref _patientPositionChangedMessage, value);
    }

    private bool _isPositionChangedMessage = false;
    public bool IsPositionChangedMessage
    {
        get => _isPositionChangedMessage;
        set => SetProperty(ref _isPositionChangedMessage, value);
    }

    public BodyPositionViewModel(IProtocolHostService protocolHostService,
        ISelectionManager scanSelectManager,
        IDialogService dialogService)
    {
        _protocolHostService = protocolHostService;
        _scanSelectManager = scanSelectManager;
        _dialogService = dialogService;
        InitAxialImageDic();
        InitHelicalImageDic();
        InitTopoImageDic();
        InitBodyMapViewModels();

        _protocolHostService.PerformStatusChanged -= ProtocolHostService_PerformStatusChanged;
        _protocolHostService.PerformStatusChanged += ProtocolHostService_PerformStatusChanged;
        _scanSelectManager.SelectionScanChanged -= ScanSelectManager_SelectScanChanged;
        _scanSelectManager.SelectionScanChanged += ScanSelectManager_SelectScanChanged;
    }

    [UIRoute]
    private void ProtocolHostService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
    {
        if (e is null)
        {
            return;
        }
        if (e.Data.Model is ScanModel scanModel && scanModel.Descriptor.Id.Equals(CurrentScanModel?.Descriptor.Id))
        {
            IsPositionChangedEnabled = e.Data.NewStatus == PerformStatus.Unperform;
        }
    }

    [UIRoute]
    private void ScanSelectManager_SelectScanChanged(object? sender, EventArgs<ScanModel> e)
    {
        if (e is null || e.Data is null)
        {
            return;
        }
        ScanModel scanModel = e.Data;
        CurrentScanModel = scanModel;
        IsUIChange = false;
        if (scanModel is not null)
        {
            ScanMode = scanModel.ScanOption;
            if (scanModel.PatientPosition != PatientPosition)
            {
                IsPositionChanged = false;
            }
            if (scanModel.PatientPosition is not null)
            {
                PatientPosition = scanModel.PatientPosition.Value;
                SetBodyMapViewModel(PatientPosition);
            }
            GetImageScanType();
            IsPositionChangedEnabled = scanModel.Status == PerformStatus.Unperform;

            TheTomoHasTopo(scanModel);
        }
        IsUIChange = true;
    }

    private void SetBodyMapViewModel(PatientPosition patientPosition)
    {
        switch (patientPosition)
        {
            case PatientPosition.HFS:
                SelectBodyMap = BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_HFS));
                break;
            case PatientPosition.HFP:
                SelectBodyMap = BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_HFP));
                break;
            case PatientPosition.HFDL:
                SelectBodyMap = BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_HFDL));
                break;
            case PatientPosition.HFDR:
                SelectBodyMap = BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_HFDR));
                break;
            case PatientPosition.FFS:
                SelectBodyMap = BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_FFS));
                break;
            case PatientPosition.FFP:
                SelectBodyMap = BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_FFP));
                break;
            case PatientPosition.FFDL:
                SelectBodyMap = BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_FFDL));
                break;
            case PatientPosition.FFDR:
                SelectBodyMap = BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_FFDR));
                break;
            default:
                SelectBodyMap = BodyMapViewModels.FirstOrDefault(t => t.Name.Equals(ProtocolParameterNames.PATIENTPOSITION_HFS));
                break;
        }
    }

    private void GetImageScanType()
    {
        switch (ScanMode)
        {
            case ScanOption.Axial:
            case ScanOption.TestBolus:
            case ScanOption.NVTestBolus:
            case ScanOption.NVTestBolusBase:
            case ScanOption.BolusTracking:
                ImageScanType = GetAxialImage();
                break;
            case ScanOption.Helical:
                ImageScanType = GetHelicalImage();
                break;
            default:
                ImageScanType = GetTopoImage();
                break;
        }
    }

    private BitmapImage GetAxialImage()
    {
        switch (PatientPosition)
        {
            case PatientPosition.HFS:
                return AxialImages[ProtocolParameterNames.PATIENTPOSITION_HFS];
            case PatientPosition.HFP:
                return AxialImages[ProtocolParameterNames.PATIENTPOSITION_HFP];
            case PatientPosition.HFDL:
                return AxialImages[ProtocolParameterNames.PATIENTPOSITION_HFDL];
            case PatientPosition.HFDR:
                return AxialImages[ProtocolParameterNames.PATIENTPOSITION_HFDR];
            case PatientPosition.FFS:
                return AxialImages[ProtocolParameterNames.PATIENTPOSITION_FFS];
            case PatientPosition.FFP:
                return AxialImages[ProtocolParameterNames.PATIENTPOSITION_FFP];
            case PatientPosition.FFDL:
                return AxialImages[ProtocolParameterNames.PATIENTPOSITION_FFDL];
            case PatientPosition.FFDR:
                return AxialImages[ProtocolParameterNames.PATIENTPOSITION_FFDR];
            default:
                return AxialImages[ProtocolParameterNames.PATIENTPOSITION_HFS];
        }
    }

    private BitmapImage GetHelicalImage()
    {
        switch (PatientPosition)
        {
            case PatientPosition.HFS:
                return HelicalImages[ProtocolParameterNames.PATIENTPOSITION_HFS];
            case PatientPosition.HFP:
                return HelicalImages[ProtocolParameterNames.PATIENTPOSITION_HFP];
            case PatientPosition.HFDL:
                return HelicalImages[ProtocolParameterNames.PATIENTPOSITION_HFDL];
            case PatientPosition.HFDR:
                return HelicalImages[ProtocolParameterNames.PATIENTPOSITION_HFDR];
            case PatientPosition.FFS:
                return HelicalImages[ProtocolParameterNames.PATIENTPOSITION_FFS];
            case PatientPosition.FFP:
                return HelicalImages[ProtocolParameterNames.PATIENTPOSITION_FFP];
            case PatientPosition.FFDL:
                return HelicalImages[ProtocolParameterNames.PATIENTPOSITION_FFDL];
            case PatientPosition.FFDR:
                return HelicalImages[ProtocolParameterNames.PATIENTPOSITION_FFDR];
            default:
                return HelicalImages[ProtocolParameterNames.PATIENTPOSITION_HFS];
        }
    }

    private BitmapImage GetTopoImage()
    {
        switch (PatientPosition)
        {
            case PatientPosition.HFS:
                return TopoImages[ProtocolParameterNames.PATIENTPOSITION_HFS];
            case PatientPosition.HFP:
                return TopoImages[ProtocolParameterNames.PATIENTPOSITION_HFP];
            case PatientPosition.HFDL:
                return TopoImages[ProtocolParameterNames.PATIENTPOSITION_HFDL];
            case PatientPosition.HFDR:
                return TopoImages[ProtocolParameterNames.PATIENTPOSITION_HFDR];
            case PatientPosition.FFS:
                return TopoImages[ProtocolParameterNames.PATIENTPOSITION_FFS];
            case PatientPosition.FFP:
                return TopoImages[ProtocolParameterNames.PATIENTPOSITION_FFP];
            case PatientPosition.FFDL:
                return TopoImages[ProtocolParameterNames.PATIENTPOSITION_FFDL];
            case PatientPosition.FFDR:
                return TopoImages[ProtocolParameterNames.PATIENTPOSITION_FFDR];
            default:
                return TopoImages[ProtocolParameterNames.PATIENTPOSITION_HFS];
        }
    }

    private void InitBodyMapViewModels()
    {
        BodyMapViewModel bodyMapViewModel = new BodyMapViewModel();
        bodyMapViewModel.Name = ProtocolParameterNames.PATIENTPOSITION_HFS;
        bodyMapViewModel.ToolTip = ProtocolParameterNames.PATIENTPOSITION_HFS;
        bodyMapViewModel.MapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFS.png", UriKind.RelativeOrAbsolute));
        BodyMapViewModels.Add(bodyMapViewModel);

        bodyMapViewModel = new BodyMapViewModel();
        bodyMapViewModel.Name = ProtocolParameterNames.PATIENTPOSITION_HFP;
        bodyMapViewModel.ToolTip = ProtocolParameterNames.PATIENTPOSITION_HFP;
        bodyMapViewModel.MapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFP.png", UriKind.RelativeOrAbsolute));
        BodyMapViewModels.Add(bodyMapViewModel);

        bodyMapViewModel = new BodyMapViewModel();
        bodyMapViewModel.Name = ProtocolParameterNames.PATIENTPOSITION_HFDL;
        bodyMapViewModel.ToolTip = ProtocolParameterNames.PATIENTPOSITION_HFDL;
        bodyMapViewModel.MapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFL.png", UriKind.RelativeOrAbsolute));
        BodyMapViewModels.Add(bodyMapViewModel);

        bodyMapViewModel = new BodyMapViewModel();
        bodyMapViewModel.Name = ProtocolParameterNames.PATIENTPOSITION_HFDR;
        bodyMapViewModel.ToolTip = ProtocolParameterNames.PATIENTPOSITION_HFDR;
        bodyMapViewModel.MapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFR.png", UriKind.RelativeOrAbsolute));
        BodyMapViewModels.Add(bodyMapViewModel);

        bodyMapViewModel = new BodyMapViewModel();
        bodyMapViewModel.Name = ProtocolParameterNames.PATIENTPOSITION_FFS;
        bodyMapViewModel.ToolTip = ProtocolParameterNames.PATIENTPOSITION_FFS;
        bodyMapViewModel.MapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFS.png", UriKind.RelativeOrAbsolute));
        BodyMapViewModels.Add(bodyMapViewModel);

        bodyMapViewModel = new BodyMapViewModel();
        bodyMapViewModel.Name = ProtocolParameterNames.PATIENTPOSITION_FFP;
        bodyMapViewModel.ToolTip = ProtocolParameterNames.PATIENTPOSITION_FFP;
        bodyMapViewModel.MapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFP.png", UriKind.RelativeOrAbsolute));
        BodyMapViewModels.Add(bodyMapViewModel);

        bodyMapViewModel = new BodyMapViewModel();
        bodyMapViewModel.Name = ProtocolParameterNames.PATIENTPOSITION_FFDL;
        bodyMapViewModel.ToolTip = ProtocolParameterNames.PATIENTPOSITION_FFDL;
        bodyMapViewModel.MapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFL.png", UriKind.RelativeOrAbsolute));
        BodyMapViewModels.Add(bodyMapViewModel);

        bodyMapViewModel = new BodyMapViewModel();
        bodyMapViewModel.Name = ProtocolParameterNames.PATIENTPOSITION_FFDR;
        bodyMapViewModel.ToolTip = ProtocolParameterNames.PATIENTPOSITION_FFDR;
        bodyMapViewModel.MapImage = new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFR.png", UriKind.RelativeOrAbsolute));
        BodyMapViewModels.Add(bodyMapViewModel);
    }
       
    private void InitAxialImageDic()
    {
        AxialImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFS, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFS_Axial.png", UriKind.RelativeOrAbsolute)));
        AxialImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFP, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFP_Axial.png", UriKind.RelativeOrAbsolute)));
        AxialImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFDL, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFL_Axial.png", UriKind.RelativeOrAbsolute)));
        AxialImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFDR, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFR_Axial.png", UriKind.RelativeOrAbsolute)));
        AxialImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFS, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFS_Axial.png", UriKind.RelativeOrAbsolute)));
        AxialImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFP, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFP_Axial.png", UriKind.RelativeOrAbsolute)));
        AxialImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFDL, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFL_Axial.png", UriKind.RelativeOrAbsolute)));
        AxialImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFDR, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFR_Axial.png", UriKind.RelativeOrAbsolute)));
    }

    private void InitHelicalImageDic()
    {
        HelicalImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFS, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFS_Helical.png", UriKind.RelativeOrAbsolute)));
        HelicalImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFP, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFP_Helical.png", UriKind.RelativeOrAbsolute)));
        HelicalImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFDL, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFL_Helical.png", UriKind.RelativeOrAbsolute)));
        HelicalImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFDR, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFR_Helical.png", UriKind.RelativeOrAbsolute)));
        HelicalImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFS, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFS_Helical.png", UriKind.RelativeOrAbsolute)));
        HelicalImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFP, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFP_Helical.png", UriKind.RelativeOrAbsolute)));
        HelicalImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFDL, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFL_Helical.png", UriKind.RelativeOrAbsolute)));
        HelicalImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFDR, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFR_Helical.png", UriKind.RelativeOrAbsolute)));
    }

    private void InitTopoImageDic()
    {
        TopoImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFS, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFS_Top.png", UriKind.RelativeOrAbsolute)));
        TopoImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFP, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFP_Top.png", UriKind.RelativeOrAbsolute)));
        TopoImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFDL, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFL_Top.png", UriKind.RelativeOrAbsolute)));
        TopoImages.Add(ProtocolParameterNames.PATIENTPOSITION_HFDR, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/HFR_Top.png", UriKind.RelativeOrAbsolute)));
        TopoImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFS, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFS_Top.png", UriKind.RelativeOrAbsolute)));
        TopoImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFP, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFP_Top.png", UriKind.RelativeOrAbsolute)));
        TopoImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFDL, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFL_Top.png", UriKind.RelativeOrAbsolute)));
        TopoImages.Add(ProtocolParameterNames.PATIENTPOSITION_FFDR, new BitmapImage(new Uri("pack://application:,,,/NV.CT.UI.Controls;component/Icons/FFR_Top.png", UriKind.RelativeOrAbsolute)));
    }

    private void IsTomoPositionChanged(ScanModel scanModel)
    {
        if (scanModel is null)
        {
            return;
        }
        if (scanModel.Status == PerformStatus.Unperform)
        {
            var model = _protocolHostService.Models.FirstOrDefault(t => t.Scan.Descriptor.Id.Equals(scanModel.Descriptor.Id));
            int index = _protocolHostService.Models.IndexOf(model);
            if (scanModel.ScanOption != ScanOption.Surview
            && scanModel.ScanOption != ScanOption.DualScout
            && index > 0)
            {
                bool isTopoExistFlag = false;
                for (var i = index - 1; i >= 0; i--)
                {
                    if ((_protocolHostService.Models[i].Scan.ScanOption == ScanOption.Surview
                        || _protocolHostService.Models[i].Scan.ScanOption == ScanOption.DualScout)
                        && _protocolHostService.Models[i].Scan.Parent.Parent.PatientPosition != model.Scan.Parent.Parent.PatientPosition)
                    {
                        isTopoExistFlag = true;
                        break;
                    }
                }
                if (isTopoExistFlag)
                {
                    IsPositionChangedMessage = true;
                    PatientPositionChangedMessage = LanguageResource.Message_Info_PatientPositionChangedMessage;
                }
            }
        }
    }

    private void TheTomoHasTopo(ScanModel scanModel)
    {
        IsPositionChangedMessage = false;
        if (scanModel is null)
        {
            return;
        }
        if (scanModel is ScanModel scan
            && scan.ScanOption != ScanOption.Surview
            && scan.ScanOption != ScanOption.DualScout
            && scan.Status == PerformStatus.Unperform)
        {
            var model = _protocolHostService.Models.FirstOrDefault(t => t.Scan.Descriptor.Id.Equals(scan.Descriptor.Id));
            int index = _protocolHostService.Models.IndexOf(model);
            bool isTopoExistFlag = false;
            if (index >= 1)
            {
                for (var i = 0; i < index; i++)
                {
                    if (_protocolHostService.Models[i].Scan.ScanOption == ScanOption.Surview
                        || _protocolHostService.Models[i].Scan.ScanOption == ScanOption.DualScout)
                    {
                        isTopoExistFlag = true;
                        break;
                    }
                }
            }
            if (index < 1 || !isTopoExistFlag)
            {
                IsPositionChangedMessage = true;
                PatientPositionChangedMessage = LanguageResource.Message_Info_PatientPositionNoTopoMessage;
            }
        }
    }
}