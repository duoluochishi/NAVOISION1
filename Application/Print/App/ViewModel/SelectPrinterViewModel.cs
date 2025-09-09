//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 10:43:11    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.ClientProxy.Workflow;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.DatabaseService.Contract;
using NV.CT.Print.Events;
using NV.CT.Print.Models;
using NV.CT.Print.View;
using NV.MPS.Configuration;
using NVCTImageViewerInterop;

namespace NV.CT.Print.ViewModel
{
    public class SelectPrinterViewModel : BaseViewModel
    {
        private readonly ILogger<SelectPrinterViewModel>? _logger;
        private readonly IPrintProtocolConfigService _printProtocolConfigService;
        private readonly IPrintConfigManager _printConfigManager;
        private ProtocolSettingsWindow? _protocolSettingsWindow;
        private string _currentStudyId;

        private bool _isShowHeader = false;
        public bool IsShowHeader
        {
            get => _isShowHeader;
            set
            {
                if (SetProperty(ref _isShowHeader, value))
                {
                    ShowHeader(value);
                    EventAggregator.Instance.GetEvent<HeaderFooterVisibilityChangedEvent>().Publish((value, this.IsShowFooter));
                }
            }
        }

        private bool _isShowFooter = false;
        public bool IsShowFooter
        {
            get => _isShowFooter;
            set
            {
                if (SetProperty(ref _isShowFooter, value))
                {
                    ShowFooter(value);
                    EventAggregator.Instance.GetEvent<HeaderFooterVisibilityChangedEvent>().Publish((this.IsShowHeader, value));
                }
            }
        }

        private int _copies = 1;
        public int Copies
        {
            get => _copies;
            set => SetProperty(ref _copies, value);
        }

        private ObservableCollection<NV.MPS.Configuration.PrinterInfo> _serverAETitles = new ObservableCollection<NV.MPS.Configuration.PrinterInfo>();
        public ObservableCollection<NV.MPS.Configuration.PrinterInfo> ServerAETitles
        {
            get => _serverAETitles;
            set => SetProperty(ref _serverAETitles, value);
        }

        private NV.MPS.Configuration.PrinterInfo _selectedServerAETitle;
        public NV.MPS.Configuration.PrinterInfo SelectedServerAETitle
        {
            get => _selectedServerAETitle;
            set => SetProperty(ref _selectedServerAETitle, value);
        }

        private ObservableCollection<OptionsBindableModel> _pageSizes = new ObservableCollection<OptionsBindableModel>();
        public ObservableCollection<OptionsBindableModel> PageSizes
        {
            get => _pageSizes;
            private set => SetProperty(ref _pageSizes, value);
        }

        private OptionsBindableModel? _selectedPageSize;
        public OptionsBindableModel? SelectedPageSize
        {
            get => _selectedPageSize;
            set 
            {
                SetProperty(ref _selectedPageSize, value);
                EventAggregator.Instance.GetEvent<PageSizeChangedEvent>().Publish(value is null ? PrintConstants.PAGE_SIZE_VALUE_14X17 : value.ValueText);
            } 
        }

        private ObservableCollection<PrintProtocol> _layouts = new ObservableCollection<PrintProtocol>();
        public ObservableCollection<PrintProtocol> Layouts
        {
            get => _layouts;
            set => SetProperty(ref _layouts, value);
        }

        private PrintProtocol? _selectedLayout;
        public PrintProtocol? SelectedLayout
        {
            get => _selectedLayout;
            set
            {
                if (SetProperty(ref _selectedLayout, value))
                {
                    ChangeLayout(value);
                }
            }
        }

        private ObservableCollection<OptionsBindableModel> _orientationList = new ObservableCollection<OptionsBindableModel>();
        public ObservableCollection<OptionsBindableModel> OrientationList
        {
            get => _orientationList;
            private set => SetProperty(ref _orientationList, value);
        }

        private OptionsBindableModel? _selectedOrientation;
        public OptionsBindableModel? SelectedOrientation
        {
            get => _selectedOrientation;
            set
            {
                SetProperty(ref _selectedOrientation, value);
                EventAggregator.Instance.GetEvent<OrientationChangedEvent>().Publish(value is null ? Constants.DICOM_ORIENTATION_PORTRAIT : value.ValueText);
            }
        }

        public SelectPrinterViewModel(ILogger<SelectPrinterViewModel> logger,
                                      IPrintProtocolConfigService printProtocolConfigService,
                                      IPrintConfigManager printConfigManager)
        {
            _logger = logger;
            _printProtocolConfigService = printProtocolConfigService;
            _printConfigManager = printConfigManager;

            Commands.Add(PrintConstants.COMMAND_SET_PRINT_PROTOCOL, new DelegateCommand<object>(OnSetPrintProtocol));
            EventAggregator.Instance.GetEvent<SelectedStudyChangedEvent>().Subscribe(OnCurrentStudyChanged);
            EventAggregator.Instance.GetEvent<ProtocolsChangedEvent>().Subscribe(OnProtocolsChanged);
            Initialize();
        }

        private void Initialize()
        {
            LoadServerAETitles();
            LoadOrientationList();
            LoadPageSizes();            
        }

        private void OnCurrentStudyChanged(PrintingStudyModel printingStudyModel)
        {
            if (printingStudyModel is null || printingStudyModel.PrintingSeriesModelList is null)
            {
                _logger?.LogDebug("printingStudyModel in OnCurrentStudyChanged is null.");
                return;
            }

            _currentStudyId = printingStudyModel.Id;
            LoadLayouts();

            var config = _printConfigManager.LoadConfig(printingStudyModel.Id);
            if (config.ImageList.Count == 0)
            {
                this.SetDefaultLayout();
            }
            else
            {
                this.SetDefaultLayout(config.SelectedLayoutItem);
            }
        }

        private void SetDefaultLayout(NV.CT.CTS.Models.ItemRect rect = default)
        {
            if (rect.Width == 0 && rect.Height == 0)
                SelectedLayout = Layouts.Any(l => l.IsDefault) ? Layouts.FirstOrDefault(l => l.IsDefault) : Layouts.FirstOrDefault();
            else
            {
                // 打开Print模块后，imageviwer已经设置过一次布局，为了避免重复设置，这里仅要求UI刷新
                _selectedLayout = Layouts.FirstOrDefault(l => l.Row == rect.Height && l.Column == rect.Width);
                RaisePropertyChanged(nameof(SelectedLayout));
            }
        }

        private void OnProtocolsChanged(BodyPart modifiedBodyPart) 
        {
            var currentStudy = NV.CT.Print.Global.Instance.PrintingStudy;
            if (!Enum.TryParse<BodyPart>(currentStudy?.BodyPart, true, out BodyPart currentBodyPart))
            {
                _logger?.LogWarning($"{currentStudy?.BodyPart} not found in BodyParts");
                return;
            }

            string previousLayoutId = SelectedLayout is null ? string.Empty : SelectedLayout.Id;
            //仅当修改了与当前患者相同检查的部位打印协议时，才更新布局下拉框选项, 否则不继续执行
            if (currentBodyPart != modifiedBodyPart)
            {
                return;
            }

            //如果当前所选布局未被删除，则仍显示该布局，否则默认选择第1个布局
            LoadLayouts();

            if (string.IsNullOrEmpty(previousLayoutId))
            {
                SelectedLayout = Layouts.Count > 0 ? Layouts.First() : null;
            }
            else if (Layouts.Any(l => l.Id == previousLayoutId))
            {
                SelectedLayout = Layouts.First(l => l.Id == previousLayoutId);
            }
            else
            {
                SelectedLayout = Layouts.Count > 0 ? Layouts.First() : null;
            }
        }

        private void LoadServerAETitles()
        {
            foreach (var printInfo in UserConfig.PrinterConfig.Printers)
            {
                ServerAETitles.Add(printInfo);
            }
            SelectedServerAETitle = ServerAETitles.Any(p => p.IsDefault) ? ServerAETitles.First(p => p.IsDefault) : ServerAETitles.FirstOrDefault();
        }

        private void LoadPageSizes()
        {
            PageSizes.Add(new OptionsBindableModel() { DisplayText = PrintConstants.PAGE_SIZE_DISPLAY_8X10, ValueText= PrintConstants.PAGE_SIZE_VALUE_8X10 });
            PageSizes.Add(new OptionsBindableModel() { DisplayText = PrintConstants.PAGE_SIZE_DISPLAY_10X12, ValueText= PrintConstants.PAGE_SIZE_VALUE_10X12 });
            PageSizes.Add(new OptionsBindableModel() { DisplayText = PrintConstants.PAGE_SIZE_DISPLAY_11X14, ValueText = PrintConstants.PAGE_SIZE_VALUE_11X14 });
            PageSizes.Add(new OptionsBindableModel() { DisplayText = PrintConstants.PAGE_SIZE_DISPLAY_14X17, ValueText = PrintConstants.PAGE_SIZE_VALUE_14X17 });

            SelectedPageSize = SelectedServerAETitle is not null ? PageSizes.FirstOrDefault(p => p.ValueText == SelectedServerAETitle.PaperSize) :
                                                                   PageSizes.First(p => p.ValueText == PrintConstants.PAGE_SIZE_VALUE_14X17);

        }

        private void LoadOrientationList()
        {
            OrientationList.Add(new OptionsBindableModel() { DisplayText = Constants.DICOM_ORIENTATION_PORTRAIT, ValueText = Constants.DICOM_ORIENTATION_PORTRAIT });
            OrientationList.Add(new OptionsBindableModel() { DisplayText = Constants.DICOM_ORIENTATION_LANDSCAPE, ValueText = Constants.DICOM_ORIENTATION_LANDSCAPE });
            SelectedOrientation = OrientationList.Count > 0 ? OrientationList.First() : null;
        }

        private void LoadLayouts()
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                var currentStudy = NV.CT.Print.Global.Instance.PrintingStudy;
                if (!Enum.TryParse<BodyPart>(currentStudy?.BodyPart, true, out BodyPart defaultBodyPart))
                {
                    _logger?.LogWarning($"{currentStudy?.BodyPart} not found in BodyParts");
                    return;
                }

                Layouts.Clear();

                var printProtocolConfig = _printProtocolConfigService.GetConfigs();
                var protocolsOfCurrentBodyPart = printProtocolConfig?.PrintProtocols.Where(p => p.BodyPart == defaultBodyPart.ToString()).ToList();
                foreach (var protocol in protocolsOfCurrentBodyPart)
                {
                    Layouts.Add(protocol);
                }                
            });
        }

        private void OnSetPrintProtocol(object element)
        {
            if (_protocolSettingsWindow is null)
            {
                _protocolSettingsWindow = new();
                //var button = element as Button;
                //if (button != null)
                //{
                //    var positionOfButton = button.PointFromScreen(new Point(0, 0));
                //    _protocolSettingsWindow.Left = Math.Abs(positionOfButton.X);
                //    _protocolSettingsWindow.Top = Math.Abs(positionOfButton.Y);
                //}
            }

            var protocolSettingsViewModel = CTS.Global.ServiceProvider.GetRequiredService<ProtocolSettingsViewModel>();
            protocolSettingsViewModel.Initialize();

            _protocolSettingsWindow.ShowWindowDialog();

        }

        private void ShowHeader(bool isShowHeader)
        {
            var imageViewer = Global.Instance.ImageViewer;
            imageViewer.SetVisibilityOfPageHeader(isShowHeader);
        }
        private void ShowFooter(bool isShowFooter)
        {
            var imageViewer = Global.Instance.ImageViewer;
            imageViewer.SetVisibilityOfPageFooter(isShowFooter);

        }

        private void ChangeLayout(PrintProtocol? printProtocol)
        {
            if (printProtocol == null)
            {
                return;
            }
            CallLayoutMethod(printProtocol);
            if (!string.IsNullOrEmpty(_currentStudyId))
            {
                _printConfigManager.UpdateSelectedLayOutInformation(_currentStudyId, new NV.CT.CTS.Models.ItemRect() { Height = printProtocol.Row, Width = printProtocol.Column });
            }
        }

        private void CallLayoutMethod(PrintProtocol? printProtocol)
        {
            if (printProtocol is null)
            {
                return;
            }

            Global.Instance.ImageViewer.SetLayout(printProtocol.Column, printProtocol.Row);
        }

    }
}
