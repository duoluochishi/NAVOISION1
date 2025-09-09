//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/03/22 13:16:10     V1.0.0       胡安
// </summary>
//-----------------------------------------------------------------------

using NV.CT.CTS.Extensions;
using NV.CT.DatabaseService.Contract;
using NV.CT.Language;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using NV.CT.PatientManagement.ApplicationService.Impl;
using NV.CT.PatientManagement.Models;
using NV.CT.SystemInterface.MCSRuntime.Contract;
using NV.MPS.Exception;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;

namespace NV.CT.PatientManagement.ViewModel
{
    public class RawDataManagementViewModel : BaseViewModel
    {
        #region Members
        private readonly IUSBService _usbService;
        private readonly ISpecialDiskService _specialDiskService;
        private readonly IRawDataService _rawDataService;
        private readonly ISeriesApplicationService _seriesApplicationService;
        private readonly ILogger<RawDataManagementViewModel> _logger;
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;
        private TreeItemBase? _locationRefreshItem = null;  //在对树节点进行增、改、删操作后，重新刷新树结构时进行定位查找原来的操作节点

        private string _localExportRootPath = string.Empty;
        private string _labelLocalPath = LanguageResource.Content_LocalPath;
        private string _labelUSB = LanguageResource.Content_USB;
        private string _labelUSB_NotReady = LanguageResource.Content_USB_NotReady;

        #endregion

        #region Properties

        private ObservableCollection<RawDataViewModel> _rawDataList = new ObservableCollection<RawDataViewModel>();
        public ObservableCollection<RawDataViewModel> RawDataList
        {
            get
            {
                return _rawDataList;
            }
            set
            {
                SetProperty(ref _rawDataList, value);
            }

        }

        private RawDataViewModel _selectedRawData;
        public RawDataViewModel SelectedRawData
        {
            get
            {
                return _selectedRawData;
            }
            set
            {
                SetProperty(ref _selectedRawData, value);
            }
        }
        private List<NV.CT.DatabaseService.Contract.Models.RawDataModel> _rawDataModelList = new List<NV.CT.DatabaseService.Contract.Models.RawDataModel>();
        public List<NV.CT.DatabaseService.Contract.Models.RawDataModel> RawDataModelList
        {
            get
            {
                return _rawDataModelList;
            }
            set
            {
                SetProperty(ref _rawDataModelList, value);
            }

        }
        private string _studyId;

        public string StudyId
        {
            get { return _studyId; }
            set { SetProperty(ref _studyId, value); }
        }
        private List<string> _rtdSeriesPathList = new List<string>();

        public List<string> RTDSeriesPathList
        {
            get { return _rtdSeriesPathList; }
            set { SetProperty(ref _rtdSeriesPathList, value); }
        }

        public ObservableCollection<DriverType> DriverTypes { get; } = new ObservableCollection<DriverType>();
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

        private bool _isAllSelected = false;
        public bool IsAllSelected
        {
            get { return this._isAllSelected; }
            set
            {
                SetProperty(ref this._isAllSelected, value);
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
        private Action<RawDataExport>? _callback;

        #endregion

        #region Constructor
        public RawDataManagementViewModel(IMapper mapper,
                                          IDialogService dialogService,
                                          IRawDataService rawDataService,
                                          IUSBService usbService,
                                          ISpecialDiskService specialDiskService,
                                          ILogger<RawDataManagementViewModel> logger, 
                                          ISeriesApplicationService seriesApplicationService)
        {
            _mapper = mapper;
            _rawDataService = rawDataService;
            _usbService = usbService;
            _specialDiskService = specialDiskService;
            _dialogService = dialogService;
            _logger = logger;
            _seriesApplicationService = seriesApplicationService;

            Commands.Add(PatientManagementConstants.COMMAND_SELECTED_TREE_ITEM_CHANGED, new DelegateCommand<TreeItemBase?>(OnSelectedTreeItemChanged));
            Commands.Add(PatientManagementConstants.COMMAND_ADD_SUBFOLDER, new DelegateCommand(OnAddSubfolder));
            Commands.Add(PatientManagementConstants.COMMAND_EDIT_FOLDER_NAME, new DelegateCommand(OnEditFolderName));
            Commands.Add(PatientManagementConstants.COMMAND_DELETE_FOLDER, new DelegateCommand(OnDeleteFolder));
            Commands.Add(PatientManagementConstants.COMMAND_EXPORT, new DelegateCommand<object>(OnExport));
            Commands.Add(PatientManagementConstants.COMMAND_CLOSE, new DelegateCommand<object>(OnClose, _ => true));
        }

        #endregion

        public void SetSelectedStudy(VStudyModel studyModel, Action<RawDataExport> callback)
        {
            ClearInfomation();
            _callback = callback;
            LoadRawDataListByStudyId(studyModel.StudyId);
            LoadSeriesListByStudyId(studyModel.StudyId);
            LoadDirectories();
        }
        private void LoadSeriesListByStudyId(string studyId)
        {
            StudyId = studyId;
           var seriesList=  _seriesApplicationService.GetSeriesByStudyId(studyId);
           var filterSeriesList= seriesList.FindAll(r=>r.ImageType == Constants.IMAGE_TYPE_TOPO|| r.ImageType == Constants.IMAGE_TYPE_TOMO);
            RTDSeriesPathList.Clear();
            foreach (var series in filterSeriesList) 
            {
                RTDSeriesPathList.Add(series.SeriesPath);
            }
        }
        private void LoadRawDataListByStudyId(string studyId)
        {
            StudyId=studyId;
            //Clear previous datasource
            foreach (var item in this.RawDataList)
            {
                item.SelectionChanged -= OnItemSelectionChanged;
            }
            this.RawDataList.Clear();
            var rawDataList = this._rawDataService.GetRawDataListByStudyId(studyId);
            RawDataModelList = rawDataList;
            var rawDataViewModelList = this._mapper.Map<List<RawDataViewModel>>(rawDataList);
            this.RawDataList = new ObservableCollection<RawDataViewModel>(rawDataViewModelList);

            foreach (var item in this.RawDataList)
            {
                item.SelectionChanged += OnItemSelectionChanged;
            }

            this.ResetExportEnable();
        }

        private void OnItemSelectionChanged(object? sender, bool e)
        {
            this.ResetExportEnable();
        }

        public void LoadDirectories()
        {
            DriverTypes.Clear();

            var usbFolders = this.FetchUsbFolders();
            string usbName = usbFolders.Length == 0 ? _labelUSB_NotReady : _labelUSB;
            bool isUsbTypeExpanded = (_locationRefreshItem != null && _locationRefreshItem.VirtualPath.StartsWith($"[{_labelUSB}]"));
            var usbDriverType = new DriverType(TargetDiskType.USB, usbName, string.Empty, $"[{_labelUSB}]", usbFolders, false, isUsbTypeExpanded);

            DriverTypes.Add(usbDriverType);
        }

        private Folder[] FetchUsbFolders()
        {
            var folders = new List<Folder>();

            var externalDisks = _usbService.GetAllDisks();
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

        private void LoadSubFolders(TargetDiskType directoryTreeItemType, Folder folder)
        {
            var folders = new List<Folder>();
            var subDirectories = Directory.GetDirectories(folder.FullPath);
            if (subDirectories.Length == 0)
            {
                return;
            }

            var subFolders = new List<Folder>();
            foreach (var subDirectory in subDirectories)
            {
                if ((new FileInfo(subDirectory).Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                var directoryInfo = new DirectoryInfo(subDirectory);

                string virtualName = GetVirtualNameByFullName(directoryTreeItemType, directoryInfo.FullName);

                var subFolder = new Folder(directoryTreeItemType, directoryInfo.Name, directoryInfo.FullName, virtualName);
                subFolder.Parent = folder;
                //LoadSubFolders(directoryTreeItemType, subFolder);

                if (_locationRefreshItem != null && _locationRefreshItem.FullPath.Contains(subFolder.FullPath))
                {
                    subFolder.IsExpanded = true;
                    subFolder.IsSelected = true;
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

        private void OnSelectedTreeItemChanged(TreeItemBase? selectedTreeItem)
        {
            this.ResetMenuItemsEnable(selectedTreeItem);

            SelectedTreeItem = selectedTreeItem;
            if (SelectedTreeItem is null)
            {
                this.ClearInfomation();
                return;
            }

            string virtualPath = SelectedTreeItem?.VirtualPath;
            OmittedChosenPath = $"{LanguageResource.Content_ChosenPath} {this.TrimStringLength(virtualPath, 100)}";
            this.ResetExportEnable();
        }

        private void ResetMenuItemsEnable(TreeItemBase? treeItem)
        {
            if (treeItem is null)
            {
                IsMenuAddEnabled = false;
                IsMenuEditEnabled = false;
            }
            else if (treeItem is DriverType)
            {
                IsMenuEditEnabled = false;
                if (Directory.Exists(treeItem.FullPath))  //如果路劲存在，显示该驱动已就绪
                {
                    IsMenuAddEnabled = true;
                }
                else
                {
                    IsMenuAddEnabled = false;

                }
            }
            else if (treeItem is Folder)
            {
                IsMenuAddEnabled = true;
                var folderNode = (Folder)treeItem;
                IsMenuEditEnabled = folderNode.IsLogicalDisk ? false : true;
            }

        }

        private void OnClose(object parameter)
        {
            this.ClearInfomation();

            if (parameter is Window window)
            {
                window.Hide();
            }
        }

        private void OnExport(object paramWindow)
        {
            //Check whether available space of target is enough.
            if (this._callback is null)
            {
                this._logger.LogWarning("callback is null in OnExportRawData.");
                return;
            }
            var rawDataExport = new RawDataExport();
            rawDataExport.StudyID = StudyId;
            rawDataExport.RawDataModels = RawDataModelList;
            var selectedTreeItem = this.SelectedTreeItem as TreeItemBase;
            rawDataExport.OutputDir = selectedTreeItem.FullPath;
            rawDataExport.RTDSeriesPathList = RTDSeriesPathList;
            var driverRoot = new DirectoryInfo(selectedTreeItem.FullPath).Root.Name;
            if (!this.CheckAvailableSpace(driverRoot))
            {
                MessageContent = "No enough available space on target path!";
                return;            
            }

            MessageContent = LanguageResource.Content_Exporting;
            //Task.Run(() => { this.ExoprtRawDataList(); });
            this.OnClose(paramWindow);
            this._callback?.Invoke(rawDataExport);
        }
        /// <summary>
        /// Check whether available space of target is enough.
        /// </summary>
        /// <returns></returns>
        private bool CheckAvailableSpace(string driverRoot)
        {            
            long totalSizeOfSource = 0;
            //var selectedRawDataList = this.RawDataList.Where(x => x.IsSelected).ToList();
            foreach (var selectedRawData in RawDataList)
            {
                if (string.IsNullOrEmpty(selectedRawData.Path))
                {
                    continue;
;                }
                totalSizeOfSource += _specialDiskService.GetDirectorySize(selectedRawData.Path);
            }
            var freeSpace = _specialDiskService.GetDriverFreeSpace(driverRoot);

            return freeSpace > totalSizeOfSource;
        }

        private void ResetExportEnable()
        {
            //if (!this.RawDataList.Any(x => x.IsSelected))
            //{
            //    IsExportButtonEnabled = false;
            //    return;
            //}

            if (SelectedTreeItem is null )
            {
                IsExportButtonEnabled = false;
                return;
            }

            if(string.IsNullOrEmpty(SelectedTreeItem.FullPath)) //最顶级节点是虚节点，不允许导出
            {
                IsExportButtonEnabled = false;
                return;
            }

            IsExportButtonEnabled = true; 
         
        }

        private void ExoprtRawDataList()
        {
            _logger.LogTrace($"Export rawdata starts.");

            var selectedTreeItem = this.SelectedTreeItem as TreeItemBase;
            string targetPath = selectedTreeItem.FullPath;
            //var selectedRawDataList = this.RawDataList.Where(x => x.IsSelected).ToList();

            Application.Current?.Dispatcher.Invoke(() =>
            {
                this.IsExportButtonEnabled = false;
            });
            
            try
            {
                foreach (var selectedRawData in RawDataList)
                {
                    string sourcePath = selectedRawData.Path;
                    if (File.Exists(sourcePath))
                    {
                        //FileOperationHelper.CopyFile(sourcePath, targetPath);
                    }
                    else if (Directory.Exists(sourcePath))
                    {
                        //FileOperationHelper.CopyFolder(sourcePath, targetPath);
                    }
                    else
                    {
                        string errorMessage = $"ExoprtRawDataList: sourcePath does not exist: {sourcePath}";
                        this._logger.LogWarning(errorMessage);
                        throw new NanoException(errorMessage);
                    }

                    selectedRawData.IsExported = true;
                    //Update ExportStatus
                    this._rawDataService.UpdateExportStatusById(selectedRawData.Id, true);
                }
            }
            catch(Exception ex)
            {
                this._logger.LogError($"Failed to export rawdata with exception:{ex.Message}.");
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    MessageContent = LanguageResource.Content_ExportFailed;
                });
                return;
            }
            finally 
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    this.IsExportButtonEnabled = true;
                });
            }

            //Reload directory
            Application.Current?.Dispatcher.Invoke(() =>
            {
                this._locationRefreshItem = SelectedTreeItem;
                this.LoadDirectories();
                MessageContent = LanguageResource.Content_ExportingFinished;
            });    

            _logger.LogTrace($"Export rawdata ends.");
        }

        private void ClearInfomation()
        {
            this.OmittedChosenPath = $"{LanguageResource.Content_ChosenPath}";
            this.MessageContent = string.Empty;
            this.MessageTip = string.Empty;
            this.IsExportButtonEnabled = false;
        }

        private void OnAddSubfolder()
        {
            if (SelectedTreeItem == null)
            {
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
                    LoadDirectories();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to create new subfolder:{newPath}. The error is:{ex.Message}");
                    this.MessageContent = LanguageResource.Message_Error_FailedNewFolder;
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
                    LoadDirectories();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to rename folder:{SelectedTreeItem.FullPath}. The error is:{ex.Message}");
                    this.MessageContent = LanguageResource.Message_Error_RenameNewFolder;
                    return;
                }
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
                this.LoadDirectories();
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

        private string TrimStringLength(string content, int length)
        {
            if (string.IsNullOrEmpty(content) || content.Length <= length)
            {
                return content;
            }

            return $"{content.Substring(0, length)}...";
        }
    }
}
