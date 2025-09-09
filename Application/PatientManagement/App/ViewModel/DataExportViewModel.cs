//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 11:01:27    V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Newtonsoft.Json;
using NV.CT.DicomUtility.Transfer;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.Language;
using NV.CT.MessageService.Contract;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using NV.CT.PatientManagement.Models;
using NV.CT.SystemInterface.MCSRuntime.Contract;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;

namespace NV.CT.PatientManagement.ViewModel;

public class DataExportViewModel : BaseViewModel
{
    #region Members
    private readonly ISeriesApplicationService _seriesApplicationService;
    private readonly IMessageService _messageService;
    private readonly IUSBService _usbService;
    private readonly ICDROMService _cdROMService;
    private readonly IDialogService _dialogService;
    private readonly ISpecialDiskService _specialDiskService;
    private readonly IAuthorization _authorizationService;
    private readonly IJobRequestService _jobRequestService;
    private readonly ILogger<DataExportViewModel> _logger;   
    private TreeItemBase? _previousSelectedItem = null;
    private TreeItemBase? _locationRefreshItem = null;  //在对树节点进行增、改、删操作后，重新刷新树结构时进行定位查找原来的操作节点
    private VStudyModel[] _selectedStudies;      
    private ApplicationService.Contract.Models.SeriesModel[] _selectedSeries;

    private string _localExportRootPath = string.Empty;
    private string _labelCDROM = LanguageResource.Content_CDROM;
    private string _labelCDROM_NotReady = LanguageResource.Content_CDROM_NotReady;
    private string _labelLocalPath = LanguageResource.Content_LocalPath;
    private string _labelUSB = LanguageResource.Content_USB;
    private string _labelUSB_NotReady = LanguageResource.Content_USB_NotReady;
    private bool? _isShowImageOfLocalPath = null;

    #endregion

    #region Properties
    public ObservableCollection<DriverType> DriverTypes { get; set; } = new ObservableCollection<DriverType>();

    private ObservableCollection<ComboItem> _imageFormats;

    public ObservableCollection<ComboItem> ImageFormats
    {
        get => _imageFormats;
        set => SetProperty(ref _imageFormats, value);
    }

    private ComboItem _selectedImageFormat;
    public ComboItem SelectedImageFormat
    {
        get => _selectedImageFormat;
        set => SetProperty(ref _selectedImageFormat, value);
    }

    private string? _messageContent;
    public string? MessageContent
    {
        get { return this._messageContent; }
        set
        {
            SetProperty(ref this._messageContent, value);
        }
    }

    private string? _messageTip;
    public string? MessageTip
    {
        get { return this._messageTip; }
        set
        {
            SetProperty(ref this._messageTip, value);
        }
    }

    private TreeItemBase? _selectedTreeItem;
    public TreeItemBase? SelectedTreeItem
    {
        get => _selectedTreeItem;
        set
        {
            SetProperty(ref _selectedTreeItem, value);
        }
    }

    private bool _isExportButtonEnabled = false;
    public bool IsExportButtonEnabled
    {
        get { return this._isExportButtonEnabled; }
        set
        {
            SetProperty(ref this._isExportButtonEnabled, value);
        }
    }

    private string? _omittedChosenPath;
    public string? OmittedChosenPath
    {
        get { return this._omittedChosenPath; }
        set
        {
            SetProperty(ref this._omittedChosenPath, value);
        }
    }

    private bool _isDicomChecked;
    public bool IsDicomChecked
    {
        get { return this._isDicomChecked; }
        set
        {
            SetProperty(ref this._isDicomChecked, value);
        }
    }

    private bool _isAddViewerChecked;
    public bool IsAddViewerChecked
    {
        get { return this._isAddViewerChecked; }
        set
        {
            SetProperty(ref this._isAddViewerChecked, value);
        }
    }

    private bool _showAddWiewer;
    public bool ShowAddWiewer
    {
        get { return this._showAddWiewer; }
        set
        {
            SetProperty(ref this._showAddWiewer, value);
        }
    }

    private bool _isImageChecked;
    public bool IsImageChecked
    {
        get { return this._isImageChecked; }
        set
        {
            SetProperty(ref this._isImageChecked, value);
            this.ShowTransferSyntax = !value;
        }
    }

    private bool _showImage;
    public bool ShowImage
    {
        get { return this._showImage; }
        set
        {
            SetProperty(ref this._showImage, value);
        }
    }

    private bool _isAnonymouseChecked;
    public bool IsAnonymouseChecked
    {
        get { return this._isAnonymouseChecked; }
        set
        {
            SetProperty(ref this._isAnonymouseChecked, value);
        }
    }

    private bool _isMenuAddEnabled = false;
    public bool IsMenuAddEnabled
    {
        get { return this._isMenuAddEnabled; }
        set
        {
            SetProperty(ref this._isMenuAddEnabled, value);
        }
    }

    private bool _isMenuEditEnabled = false;
    public bool IsMenuEditEnabled
    {
        get { return this._isMenuEditEnabled; }
        set
        {
            SetProperty(ref this._isMenuEditEnabled, value);
        }
    }

    private SupportedTransferSyntax[] _dicomTransferSyntaxTypes;
    public SupportedTransferSyntax[] DicomTransferSyntaxTypes
    {
        get => _dicomTransferSyntaxTypes;
        set
        {
            SetProperty(ref _dicomTransferSyntaxTypes, value);
        }
    }

    private SupportedTransferSyntax _selectedDicomTransferSyntax;
    public SupportedTransferSyntax SelectedDicomTransferSyntax
    {
        get => _selectedDicomTransferSyntax;
        set
        {
            SetProperty(ref _selectedDicomTransferSyntax, value);
        }
    }

    private bool _showTransferSyntax = true;
    public bool ShowTransferSyntax
    {
        get { return this._showTransferSyntax; }
        set
        {
            SetProperty(ref this._showTransferSyntax, value);
        }
    }

    #endregion

    #region Constructor
    public DataExportViewModel(ISeriesApplicationService seriesApplicationService,
                               IDicomFileService dicomFileService, 
                               IMessageService messageService, 
                               IUSBService usbService, 
                               ICDROMService cdROMService,
                               IDialogService dialogService, 
                               ISpecialDiskService specialDiskService,
                               IAuthorization authorizationService,
                               IJobRequestService jobRequestService,
                               ILogger<DataExportViewModel> logger)
    {
        _seriesApplicationService = seriesApplicationService;
        _messageService = messageService;
        _usbService = usbService;
        _cdROMService = cdROMService;
        _dialogService = dialogService;
        _specialDiskService = specialDiskService;
        _authorizationService = authorizationService;
        _jobRequestService = jobRequestService;
        _logger = logger;                

        Commands.Add(PatientManagementConstants.COMMAND_SELECTED_TREE_ITEM_CHANGED, new DelegateCommand<TreeItemBase?>(OnSelectedTreeItemChanged));
        Commands.Add(PatientManagementConstants.COMMAND_ADD_SUBFOLDER, new DelegateCommand(OnAddSubfolder));
        Commands.Add(PatientManagementConstants.COMMAND_EDIT_FOLDER_NAME, new DelegateCommand(OnEditFolderName));
        Commands.Add(PatientManagementConstants.COMMAND_DELETE_FOLDER, new DelegateCommand(OnDeleteFolder));
        Commands.Add(PatientManagementConstants.COMMAND_EXPORT, new DelegateCommand(OnExport));
        Commands.Add(PatientManagementConstants.COMMAND_CLOSE, new DelegateCommand<object>(OnClose, _ => true));
        
        _messageService.MessageNotify += OnNotifyMessage;        

        this.Initialize();
    }

    #endregion

    #region Public methods

    public void SetSelectedStudies(VStudyModel[] studies)
    {
        this._selectedStudies = studies;
        this._selectedSeries = null;  //设置选定为study类型时，设置选定的series为null，指示当前操作导出类型为study
    }

    public void SetSelectedSeries(ApplicationService.Contract.Models.SeriesModel[] series)
    {
        this._selectedStudies = null; //设置选定为series类型时，设置选定的study为null，指示当前操作导出类型为series
        this._selectedSeries = series;
    }

    public void LoadAllDirectories()
    {
        DriverTypes.Clear();

        var cdromFolders = this.FetchCdRomFolders();
        string cdromName = cdromFolders.Length == 0 ? _labelCDROM_NotReady : _labelCDROM;
        bool isCdRomExpanded = (_locationRefreshItem != null && _locationRefreshItem.VirtualPath.StartsWith($"[{_labelCDROM}]"));

        var cdDriverType = new DriverType(TargetDiskType.CDROM, cdromName, string.Empty, $"[{_labelCDROM}]", cdromFolders, false, isCdRomExpanded);
        DriverTypes.Add(cdDriverType);

        bool isLocalSystemTypeExpanded = (_locationRefreshItem != null && _locationRefreshItem.VirtualPath.StartsWith($"[{_labelLocalPath}]"));
        var localSystemDriverType = new DriverType(TargetDiskType.LocalPath,
                                                _labelLocalPath,
                                                _localExportRootPath,
                                                $"[{_labelLocalPath}]",
                                                this.FetchSystemLocalFolders(_localExportRootPath),
                                                false, isLocalSystemTypeExpanded);

        DriverTypes.Add(localSystemDriverType);

        var usbFolders = this.FetchUsbFolders();
        string usbName = usbFolders.Length == 0 ? _labelUSB_NotReady : _labelUSB;
        bool isUsbTypeExpanded = (_locationRefreshItem != null && _locationRefreshItem.VirtualPath.StartsWith($"[{_labelUSB}]"));
        var usbDriverType = new DriverType(TargetDiskType.USB, usbName, string.Empty, $"[{_labelUSB}]", usbFolders, false, isUsbTypeExpanded);
        DriverTypes.Add(usbDriverType);

        ResetUnselectedTreeItem();
    }

    #endregion

    #region Private methods

    #region Command events

    private void OnAddSubfolder()
    {
        if (SelectedTreeItem == null)
        {
            return;
        }

        if (!Directory.Exists(SelectedTreeItem.FullPath))
        {
            _dialogService.Show(false, MessageLeveles.Error,LanguageResource.Message_Info_CloseErrorTitle, LanguageResource.Message_Error_PathNotExist, null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var window = Global.Instance.ServiceProvider.GetRequiredService<AddEditFolderWindow>();
        if (window is null)
            return;

        var viewModel = window.DataContext as AddEditFolderViewModel;
        if (viewModel is null)
            return;

        var isResultOK = false;
        var wih = new WindowInteropHelper(window);
        if (ConsoleSystemHelper.WindowHwnd != IntPtr.Zero)
        {
            if (wih.Owner == IntPtr.Zero)
                wih.Owner = ConsoleSystemHelper.WindowHwnd;

            if (!window.IsVisible)
            {
                viewModel.IsAddMode = true;
                viewModel.FolderName = string.Empty;
                var addEditFolderWindow = window as AddEditFolderWindow;
                addEditFolderWindow.SetDefaultInputFocus();

                //隐藏底部状态栏
                window.Topmost = true;
                window.ShowDialog();
                isResultOK = viewModel.IsResultOK;
            }
        }
        else
        {
            if (Application.Current.MainWindow is not null && wih.Owner == IntPtr.Zero)
            {
                wih.Owner = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            }

            if (!window.IsVisible)
            {
                viewModel.IsAddMode = true;
                viewModel.FolderName = string.Empty;
                var addEditFolderWindow = window as AddEditFolderWindow;
                addEditFolderWindow.SetDefaultInputFocus();

                //必须加这个，不加就会导致状态栏出来
                window.Topmost = true;
                window.Show();
                window.Activate();
                isResultOK = viewModel.IsResultOK;
            }
        }

        if (isResultOK)
        {
            var newPath = Path.Combine(SelectedTreeItem.FullPath, viewModel.FolderName.Trim());
            try
            {
                Directory.CreateDirectory(newPath);
                _locationRefreshItem = SelectedTreeItem;
                LoadAllDirectories();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create new subfolder:{newPath}. The error is:{ex.Message}");
                _dialogService.Show(false, MessageLeveles.Error, LanguageResource.Message_Info_CloseErrorTitle, LanguageResource.Message_Error_FailedNewFolder, null, ConsoleSystemHelper.WindowHwnd);
                return;
            }
        }
    }

    private void OnEditFolderName()
    {
        if (SelectedTreeItem == null)
        {
            return;
        }

        if (!Directory.Exists(SelectedTreeItem.FullPath))
        {
            _dialogService.Show(false, MessageLeveles.Error, 
                                LanguageResource.Message_Info_CloseErrorTitle, LanguageResource.Message_Error_PathNotExist, 
                                null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var window = Global.Instance.ServiceProvider.GetRequiredService<AddEditFolderWindow>();
        if (window is null)
            return;

        var viewModel = window.DataContext as AddEditFolderViewModel;
        if (viewModel is null)
            return;

        var isResultOK = false;
        var wih = new WindowInteropHelper(window);
        if (ConsoleSystemHelper.WindowHwnd != IntPtr.Zero)
        {
            if (wih.Owner == IntPtr.Zero)
                wih.Owner = ConsoleSystemHelper.WindowHwnd;

            if (!window.IsVisible)
            {
                viewModel.IsAddMode = false;
                viewModel.FolderName = Path.GetFileName(SelectedTreeItem.FullPath) ?? string.Empty;
                var addEditFolderWindow = window as AddEditFolderWindow;
                addEditFolderWindow.SetDefaultInputFocus();

                //隐藏底部状态栏
                window.Topmost = true;
                window.ShowDialog();
                isResultOK = viewModel.IsResultOK;
            }
        }
        else
        {
            if (Application.Current.MainWindow is not null && wih.Owner == IntPtr.Zero)
            {
                wih.Owner = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            }

            if (!window.IsVisible)
            {
                viewModel.IsAddMode = false;
                viewModel.FolderName = Path.GetFileName(SelectedTreeItem.FullPath) ?? string.Empty;
                var addEditFolderWindow = window as AddEditFolderWindow;
                addEditFolderWindow.SetDefaultInputFocus();

                //必须加这个，不加就会导致状态栏出来
                window.Topmost = true;
                window.Show();
                window.Activate();
                isResultOK = viewModel.IsResultOK;
            }
        }

        var originalName = Path.GetFileName(SelectedTreeItem.FullPath) ?? string.Empty;
        bool isNameChanged = viewModel.FolderName.Trim() != originalName.Trim();
        //仅当点击OK并且输入名称有变化，才进行实际的磁盘操作
        if (isResultOK && isNameChanged)
        {
            try
            {
                var currentDirectory = new DirectoryInfo(SelectedTreeItem.FullPath);
                var parentPath = currentDirectory.Parent?.FullName;
                var newPath = Path.Combine(parentPath, viewModel.FolderName.Trim());
                Directory.Move(currentDirectory.FullName, newPath);
                _locationRefreshItem = SelectedTreeItem.Parent;
                LoadAllDirectories();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to rename folder:{SelectedTreeItem.FullPath}. The error is:{ex.Message}");
                _dialogService.Show(false, MessageLeveles.Error, 
                                    LanguageResource.Message_Info_CloseErrorTitle, LanguageResource.Message_Error_RenameNewFolder, 
                                    null, ConsoleSystemHelper.WindowHwnd);
                return;
            }
        }
    }

    private void OnSelectedTreeItemChanged(TreeItemBase? selectedTreeItem)
    {
        SelectedTreeItem = selectedTreeItem;

        if (null == SelectedTreeItem)
        {
            _previousSelectedItem = null;
            IsMenuAddEnabled = false;
            IsMenuEditEnabled = false;
            ResetUnselectedTreeItem();
            return;
        }

        string virtualPath = SelectedTreeItem?.VirtualPath;
        if (SelectedTreeItem is DriverType)
        {
            var driverTypeNode = (DriverType)SelectedTreeItem;
            if (Directory.Exists(SelectedTreeItem.FullPath))  //如果路劲存在，显示该驱动已就绪
            {
                IsExportButtonEnabled = true;
                IsMenuAddEnabled = true;
            }
            else if (!Directory.Exists(SelectedTreeItem.FullPath) && driverTypeNode.Folders.Count > 0) //虽然路劲不存在，但存在子节点，显示该驱动已就绪，同时需禁用按钮和右键菜单(比如USB节点)
            {
                IsExportButtonEnabled = false;
                IsMenuAddEnabled = false;
            }
            else //其它情况视为该驱动未就绪
            {
                IsExportButtonEnabled = false;
                IsMenuAddEnabled = false;
                virtualPath = $"{virtualPath} ({LanguageResource.Content_NotReady})";
            }

            IsMenuEditEnabled = false;
        }
        else
        {
            IsMenuAddEnabled = true;
            IsExportButtonEnabled = true;

            var folderNode = (Folder)SelectedTreeItem;
            IsMenuEditEnabled = folderNode.IsLogicalDisk ? false : true;
        }

        OmittedChosenPath = $"{LanguageResource.Content_ChosenPath} {this.TrimStringLength(virtualPath, 100)}";

        if (SelectedTreeItem.DirectoryTreeItemType == TargetDiskType.LocalPath
            && _previousSelectedItem?.DirectoryTreeItemType != TargetDiskType.LocalPath)
        {
            ResetLocalPath();
        }
        else if (SelectedTreeItem.DirectoryTreeItemType == TargetDiskType.CDROM
            && _previousSelectedItem?.DirectoryTreeItemType != TargetDiskType.CDROM)
        {
            ResetCdRom();
        }
        else if (SelectedTreeItem.DirectoryTreeItemType == TargetDiskType.USB
            && _previousSelectedItem?.DirectoryTreeItemType != TargetDiskType.USB)
        {
            ResetUSB();
        }

        _previousSelectedItem = SelectedTreeItem;

    }

    private void OnClose(object parameter)
    {
        this.Clear();

        if (parameter is Window window)
        {
            window.Hide();
        }
    }

    private void OnExport()
    {
        _logger.LogTrace($"Export starts.");    

        if (this._selectedStudies is not null && this._selectedStudies.Count() > 0)
        {
            ExportStudies(this._selectedStudies);
        }
        else if (this._selectedSeries is not null && this._selectedSeries.Count() > 0)
        {
            ExportSeries(this._selectedSeries, OperationLevel.Series);
        }
        else
        {
            _dialogService.Show(false, MessageLeveles.Error, 
                                LanguageResource.Message_Info_CloseErrorTitle, LanguageResource.Message_Info_SelecteStudyOrSeries, 
                                null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        _logger.LogTrace($"Export ends.");

        _isShowImageOfLocalPath = this.IsImageChecked;
    }

    private void OnNotifyMessage(object? sender, MessageInfo e)
    {
        //这里仅处理Export类型
        if (e.Sender != MessageSource.ExportJob)
        {
            return;
        }

        if ( null == e.Remark || string.IsNullOrEmpty( e.Remark))
        {
            return;
        }

        JobTaskMessage exportJobMessage;
        try
        {
            exportJobMessage = JsonConvert.DeserializeObject<JobTaskMessage>(e.Remark);
        }
        catch
        {
            return;
        }

        if (exportJobMessage?.JobStatus == JobTaskStatus.Completed ||
            exportJobMessage?.JobStatus == JobTaskStatus.Cancelled ||
            exportJobMessage?.JobStatus == JobTaskStatus.Failed)
        {
            MessageTip = LanguageResource.Content_ExportingFinished;
            MessageContent = TrimStringLength(MessageTip, 48);
        }

        if (exportJobMessage?.JobStatus == JobTaskStatus.Completed)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                this.LoadAllDirectories();
            });
        }
    }

    private void OnDeleteFolder()
    {
        if (SelectedTreeItem == null)
        {
            return;
        }

        var callback = new Action<IDialogResult>(result => {

            if (result.Result == ButtonResult.OK)
            {
                DoDeleteFolder();
            }
        });
        _dialogService.Show(true, MessageLeveles.Warning, LanguageResource.Message_Info_CloseConfirmTitle, LanguageResource.Message_Confirm_Delete, callback, ConsoleSystemHelper.WindowHwnd);
        return;

    }

    private void DoDeleteFolder()
    {
        if (!Directory.Exists(this.SelectedTreeItem?.FullPath))
        {
            _dialogService.Show(false, MessageLeveles.Error,
                                LanguageResource.Message_Info_CloseErrorTitle, LanguageResource.Message_Error_PathNotExist, 
                                null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        try
        {
            Directory.Delete(this.SelectedTreeItem.FullPath, true);
            _locationRefreshItem = SelectedTreeItem.Parent;
            this.LoadAllDirectories();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to delete folder: {this.SelectedTreeItem.FullPath}, the error is: {ex.Message}");
            _dialogService.Show(false, MessageLeveles.Error, 
                                LanguageResource.Message_Info_CloseErrorTitle, 
                                LanguageResource.Message_Error_FailedToDeleteFolder, 
                                null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

    }

    #endregion

    private void Initialize()
    {
        if (string.IsNullOrEmpty(RuntimeConfig.Console.ExportData.Path))
        {
            throw new ArgumentNullException("Target path of export is empty.");
        }

        if (!Directory.Exists(RuntimeConfig.Console.ExportData.Path))
        {
            try
            {
                Directory.CreateDirectory(RuntimeConfig.Console.ExportData.Path);
            }
            catch
            {
                throw new DirectoryNotFoundException($"Failed to create path for export:[{RuntimeConfig.Console.ExportData.Path}].");
            }
        }

        this._localExportRootPath = (new DirectoryInfo(RuntimeConfig.Console.ExportData.Path)).FullName; //让配置文件中的路径进行字符串合规化

        var bmp = FileExtensionType.Bmp.ToString();
        var gif = FileExtensionType.Gif.ToString();
        var jpeg = FileExtensionType.Jpeg.ToString();
        var png = FileExtensionType.Png.ToString();

        ImageFormats = new ObservableCollection<ComboItem>
        {
            new ComboItem(bmp, bmp),
            new ComboItem(gif, gif),
            new ComboItem(jpeg, jpeg),
            new ComboItem(png, png)
        };
        SelectedImageFormat = ImageFormats[0];

        this._dicomTransferSyntaxTypes = EnumHelper.GetAllItems<SupportedTransferSyntax>().ToArray();
        this._selectedDicomTransferSyntax = SupportedTransferSyntax.ImplicitVRLittleEndian;
    }

    private Folder[] FetchCdRomFolders()
    {
        var cdRomDisks = _cdROMService.Currents;
        if (cdRomDisks.Count == 0)
        {
            return new Folder[] { };
        }

        var folders = new List<Folder>();

        bool isSelected = false;
        bool isLogicalDisk = true;

        foreach (var cdRomDisk in cdRomDisks)
        {
            //如果CDROM未就绪，不能烧录，则不加载
            if(!cdRomDisk.IsReady)
                continue;
            bool isExpanded = (_locationRefreshItem != null && _locationRefreshItem.FullPath.Contains(cdRomDisk.Name));

            var subFolder = new Folder(TargetDiskType.CDROM, cdRomDisk.Name, cdRomDisk.Name, $"[{_labelCDROM}]{cdRomDisk.Name}", isSelected, isExpanded, isLogicalDisk);

            if (new DirectoryInfo(subFolder.FullPath).Exists)
            {
                LoadSubFolders(TargetDiskType.CDROM, subFolder);                
            }
            folders.Add(subFolder);
        }

        return folders.ToArray();

    }

    private Folder[] FetchUsbFolders()
    {
        var folders = new List<Folder>();

        var externalDisks = _usbService.FindExternalDisks();
        bool isSelected = false;        
        bool isLogicalDisk = true;

        foreach (var externalDisk in externalDisks)
        {
            bool isExpanded = (_locationRefreshItem != null && _locationRefreshItem.FullPath.Contains(externalDisk));
            var subFolder = new Folder(TargetDiskType.USB, externalDisk, externalDisk, $"[{_labelUSB}]{externalDisk}", isSelected, isExpanded, isLogicalDisk);
            LoadSubFolders(TargetDiskType.USB, subFolder);
            folders.Add(subFolder);
        }
        
        return folders.ToArray();
    }

    private Folder[] FetchSystemLocalFolders(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || !Directory.Exists(folderPath))
        {
            return new Folder[] { };
        }

        var subDirectories = Directory.GetDirectories(folderPath);
        if (subDirectories.Length == 0)
        {
            return new Folder[] { };
        }

        var subFolders = new List<Folder>();
        foreach (var subDirectory in subDirectories)
        {
            var directoryInfo = new DirectoryInfo(subDirectory);
            if ((directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                continue;
            }

            string virtualName = GetVirtualNameByFullName(TargetDiskType.LocalPath, directoryInfo.FullName);
            bool isExpanded = (_locationRefreshItem != null && _locationRefreshItem.FullPath.Contains(directoryInfo.FullName));
            bool isSelected = isExpanded;
            var subFolder = new Folder(TargetDiskType.LocalPath, directoryInfo.Name, directoryInfo.FullName, virtualName, isSelected, isExpanded);

            LoadSubFolders(TargetDiskType.LocalPath, subFolder);
            subFolders.Add(subFolder);
        }

        return subFolders.ToArray();
    }

    private void LoadSubFolders(TargetDiskType directoryTreeItemType, Folder folder)
    {
        var folders = new List<Folder>();
        var subDirectories = Directory.GetDirectories(folder.FullPath);
        if (subDirectories.Length == 0)
        {
            return;
        }

        var subFolders = new List<Folder>();
        foreach(var subDirectory in subDirectories)
        {
            if ((new FileInfo(subDirectory).Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                continue;
            }

            var directoryInfo = new DirectoryInfo(subDirectory);

            string virtualName = GetVirtualNameByFullName(directoryTreeItemType, directoryInfo.FullName);

            var subFolder = new Folder(directoryTreeItemType, directoryInfo.Name, directoryInfo.FullName, virtualName);
            subFolder.Parent = folder;
            LoadSubFolders(directoryTreeItemType, subFolder);

            if (_locationRefreshItem != null && _locationRefreshItem.FullPath.Contains(subFolder.FullPath))
            {
                subFolder.IsExpanded = true;
            }

            subFolders.Add(subFolder);
        }

        folder.SubFolders = new ObservableCollection<Folder>(subFolders);
    }

    private string GetVirtualNameByFullName(TargetDiskType directoryTreeItemType, string fullName)
    {
        string virtualName = string.Empty;

        if (directoryTreeItemType == TargetDiskType.LocalPath)
        {
            virtualName = fullName.Replace(_localExportRootPath, $"[{_labelLocalPath}]");
        }
        else if (directoryTreeItemType == TargetDiskType.CDROM)
        {
            virtualName = $"[{_labelCDROM}]{fullName}";
        }
        else if (directoryTreeItemType == TargetDiskType.USB)
        {
            virtualName = $"[{_labelUSB}]{fullName}";
        }
        else
        {
            virtualName = fullName;
        }

        return virtualName;
    }

    private void ExportStudies(VStudyModel[] studyList)
    {    
        var seriesList = new List<ApplicationService.Contract.Models.SeriesModel>();
        foreach (var item in studyList)
        {
            var seriesModels = _seriesApplicationService.GetSeriesByStudyId(item.StudyId);
            if (seriesModels == null || seriesModels.Count() == 0)
            {
                string errorMessage = $"The study is not found in DB with StudyID:[{item.StudyId}]";
                _logger.LogError(errorMessage);

                MessageTip = LanguageResource.Message_Error_ExportNoStudyFound;
                MessageContent = MessageTip;

                //_dialogService.Show(false, MessageLeveles.Info, 
                //                    LanguageResource.Message_Info_CloseErrorTitle, LanguageResource.Message_Error_ExportNoStudyFound,
                //                    null, ConsoleSystemHelper.WindowHwnd);
                //return;
                continue;
            }

            seriesList.AddRange(seriesModels);
        }
        ExportSeries(seriesList.ToArray(), OperationLevel.Study);
    }

    private void ExportSeries(ApplicationService.Contract.Models.SeriesModel[] seriesList, OperationLevel operationLevel)
    {
        //check available size
        if (!CheckEnoughSpace(seriesList))
        {
            MessageContent = LanguageResource.Message_Error_NoEnoughSpace;
            return;
        }
        
        var exportJobRequest = new ExportJobRequest();
        var selectedTreeItem = this.SelectedTreeItem;
        foreach (var series in seriesList)
        {
            bool isDirectory = Directory.Exists(series.SeriesPath);
            bool isFile = File.Exists(series.SeriesPath);
            if (!isDirectory && !isFile)
            {
                continue;
            }
            exportJobRequest.InputFolders.Add(series.SeriesPath);
        }
        exportJobRequest.PatientNames = FetchExportedPatientNameList(seriesList);
        exportJobRequest.IsAnonymouse = this.IsAnonymouseChecked;
        exportJobRequest.IsExportedToDICOM = this.IsDicomChecked;
        exportJobRequest.IsExportedToImage = this.IsImageChecked;
        exportJobRequest.OutputVirtualPath = selectedTreeItem.VirtualPath;
        exportJobRequest.OutputFolder = selectedTreeItem.FullPath;
        exportJobRequest.IsBurnToCDROM = selectedTreeItem.DirectoryTreeItemType == TargetDiskType.CDROM;
        exportJobRequest.IsAddViewer = exportJobRequest.IsBurnToCDROM ? IsAddViewerChecked : false;
        exportJobRequest.DicomTransferSyntax = SelectedDicomTransferSyntax.ToString();
        if (this.IsImageChecked)
        {
            Enum.TryParse<FileExtensionType>(this.SelectedImageFormat?.Value, true, out var pictureType);
            exportJobRequest.PictureType = pictureType;
        }

        exportJobRequest.Id = Guid.NewGuid().ToString();
        exportJobRequest.WorkflowId = Guid.NewGuid().ToString();
        exportJobRequest.InternalPatientID = string.Empty;
        exportJobRequest.InternalStudyID = string.Empty;
        exportJobRequest.Priority = 5;
        exportJobRequest.JobTaskType = JobTaskType.ExportJob;
        exportJobRequest.Creator = _authorizationService.GetCurrentUser() is null ? string.Empty : _authorizationService.GetCurrentUser().Account;
        exportJobRequest.OperationLevel = operationLevel;
        exportJobRequest.StudyId = seriesList.Length > 0 ? seriesList[0].InternalStudyId : string.Empty;
        exportJobRequest.SeriesIdList = seriesList.Select(x => x.Id).ToList();
        exportJobRequest.Parameter = exportJobRequest.ToJson();

        var result = this._jobRequestService.EnqueueJobRequest(exportJobRequest);
        if (result.Status == CommandExecutionStatus.Success)
        {
            this._locationRefreshItem = selectedTreeItem;
            MessageContent = LanguageResource.Message_Info_ExportTaskStarted;
            MessageTip = MessageContent;
        }
        else
        {
            MessageContent = LanguageResource.Message_Error_ExportTaskFailed;
            MessageTip = MessageContent;
        }
    }

    private bool CheckEnoughSpace(ApplicationService.Contract.Models.SeriesModel[] seriesList)
    {
        long totalSizeOfSeriesList = 0;
        foreach (var series in seriesList)
        {
            bool isDirectory = Directory.Exists(series.SeriesPath);
            bool isFile = File.Exists(series.SeriesPath);
            if (!isDirectory && !isFile)
            {
                string errorMessage = $"The series path does not exist:[{series.SeriesPath}]";
                _logger.LogError(errorMessage);

                MessageTip = LanguageResource.Message_Error_SeriesPathNotExist;
                MessageContent = MessageTip;

                continue;
            }

            //calculate size of all seriess
            totalSizeOfSeriesList += _specialDiskService.GetDirectorySize(series.SeriesPath);
        }

        var selectedTreeItem = this.SelectedTreeItem as TreeItemBase;
        var driverRoot = new DirectoryInfo(selectedTreeItem.FullPath).Root.Name;
        var freeSpace = _specialDiskService.GetDriverFreeSpace(driverRoot);

        return freeSpace > totalSizeOfSeriesList;
    }

    private List<string> FetchExportedPatientNameList(ApplicationService.Contract.Models.SeriesModel[] seriesList)
    {     
        var distinctStudyIds = seriesList.Select(x => x.InternalStudyId).Distinct();

        var studyModel = Global.Instance.ServiceProvider.GetRequiredService<StudyViewModel>();
        var patientNameList = studyModel.VStudies.Where(s => distinctStudyIds.Contains(s.StudyId)).Select(s => s.PatientName).ToList();
        return patientNameList;    
    }

    private string TrimStringLength(string content, int length)
    {
        if (string.IsNullOrEmpty(content) || content.Length <= length)
        {
            return content;
        }

        return $"{content.Substring(0, length)}...";
    }

    private void ResetLocalPath()
    { 
        this.ShowImage = true;
        this.ShowAddWiewer = false;

        if (_isShowImageOfLocalPath is null)
        {
            this.IsDicomChecked = true;
            this.IsImageChecked = false;
        }
        else
        {
            this.IsDicomChecked = !_isShowImageOfLocalPath.Value;
            this.IsImageChecked = _isShowImageOfLocalPath.Value;
            _isShowImageOfLocalPath = null;
        }

    }

    private void ResetCdRom()
    {
        this.ShowImage = false;
        this.ShowAddWiewer = true;
        this.IsDicomChecked = true;
        this.IsImageChecked = false;
    }

    private void ResetUSB()
    {
        this.ShowImage = true;
        this.ShowAddWiewer = false;
        this.IsDicomChecked = true;
        this.IsImageChecked = false;
    }

    private void ResetUnselectedTreeItem()
    {
        this.ShowImage = false;
        this.ShowAddWiewer = false;
        this.IsDicomChecked = false;
        this.IsImageChecked = false;
        this.IsExportButtonEnabled = false;
    }

    private void Clear()
    {
        this.OmittedChosenPath = string.Empty;
        this.MessageContent = string.Empty;
        this.MessageTip = string.Empty;
        _locationRefreshItem = null;
    }

    #endregion
}
