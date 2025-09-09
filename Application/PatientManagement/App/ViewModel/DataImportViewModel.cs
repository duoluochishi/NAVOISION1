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

using FellowOakDicom;
using Newtonsoft.Json;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.CT.Language;
using NV.CT.MessageService.Contract;
using NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;
using NV.CT.PatientManagement.ApplicationService.Impl;
using NV.CT.PatientManagement.Models;
using NV.CT.SystemInterface.MCSRuntime.Contract;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Collections.Concurrent;

namespace NV.CT.PatientManagement.ViewModel;

public class DataImportViewModel : BaseViewModel
{
    #region Members 
    private readonly IMessageService _messageService;
    private readonly IUSBService _usbService;
    private readonly ICDROMService _cdROMService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<DataExportViewModel> _logger;
    private readonly IAuthorization _authorizationService;
    private readonly IJobRequestService _jobRequestService;
    private readonly IStudyApplicationService _studyApplicationService;
    private readonly ISeriesApplicationService _seriesApplicationService;

    private string _localImportRootPath = string.Empty;
    private string _labelCDROM = LanguageResource.Content_CDROM;
    private string _labelCDROM_NotReady = LanguageResource.Content_CDROM_NotReady;
    private string _labelLocalPath = LanguageResource.Content_LocalPath;
    private string _labelUSB = LanguageResource.Content_USB;
    private string _labelUSB_NotReady = LanguageResource.Content_USB_NotReady;

    #endregion

    #region Properties

    public ObservableCollection<DriverType> DriverTypes { get; } = new ObservableCollection<DriverType>();

    private string? _messageContent = string.Empty;
    public string? MessageContent
    {
        get { return this._messageContent; }
        set
        {
            SetProperty(ref this._messageContent, value);
        }
    }

    private string? _messageTip = string.Empty;
    public string? MessageTip
    {
        get { return this._messageTip; }
        set
        {
            SetProperty(ref this._messageTip, value);
        }
    }

    private TreeItemBase? _selectedItem;
    public TreeItemBase? SelectedItem
    {
        get => _selectedItem;
        set
        {
            SetProperty(ref _selectedItem, value);
        }
    }

    private bool _isImportButtonEnabled;
    public bool IsImportButtonEnabled
    {
        get { return this._isImportButtonEnabled; }
        set
        {
            SetProperty(ref this._isImportButtonEnabled, value);
        }
    }

    private string? _omittedChosenPath = string.Empty;
    public string? OmittedChosenPath
    {
        get { return this._omittedChosenPath; }
        set
        {
            SetProperty(ref this._omittedChosenPath, value);
        }
    }
    private List<string> _fileList = new();
    #endregion

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public DataImportViewModel(IStudyApplicationService studyApplicationService,
                               ISeriesApplicationService seriesApplicationService,
                               IMessageService messageService,
                               IUSBService usbService,
                               ICDROMService cdROMService,
                               IDialogService dialogService,
                               ILogger<DataExportViewModel> logger,
                               IAuthorization authorizationService,
                               IJobRequestService jobRequestService)
    {
        _messageService = messageService;
        _usbService = usbService;
        _cdROMService = cdROMService;
        _dialogService = dialogService;
        _authorizationService = authorizationService;
        _jobRequestService = jobRequestService;
        _logger = logger;
        _studyApplicationService= studyApplicationService;
        _seriesApplicationService=seriesApplicationService;

        Commands.Add(PatientManagementConstants.COMMAND_SELECTED_TREE_ITEM_CHANGED, new DelegateCommand<TreeItemBase?>(OnSelectedTreeItemChanged));
        Commands.Add(PatientManagementConstants.COMMAND_IMPORT, new DelegateCommand(OnImport));
        Commands.Add(PatientManagementConstants.COMMAND_CLOSE, new DelegateCommand<object>(OnClose, _ => true));

        this.Initialize();

        _messageService.MessageNotify += OnNotifyMessage;
    }

    #endregion

    #region Public methods

    public void LoadAllDirectories()
    {
        DriverTypes.Clear();

        var cdromFolders = this.FetchCdRomFolders();
        string cdromName = cdromFolders.Length == 0 ? _labelCDROM_NotReady : _labelCDROM;
        var cdDriverType = new DriverType(TargetDiskType.CDROM, cdromName, string.Empty, string.Format($"[{_labelCDROM}]"), cdromFolders);
        DriverTypes.Add(cdDriverType);

        var localSystemDriverType = new DriverType(TargetDiskType.LocalPath, _labelLocalPath,
                                                   _localImportRootPath, string.Format($"[{_labelLocalPath}]"),
                                                   this.FetchSystemLocalFolders(_localImportRootPath),
                                                   true, true);
        DriverTypes.Add(localSystemDriverType);

        var usbFolders = this.FetchUsbFolders();
        string usbName = usbFolders.Length == 0 ? _labelUSB_NotReady : _labelUSB;
        var usbDriverType = new DriverType(TargetDiskType.USB, usbName, string.Empty, string.Format($"[{_labelUSB}]"), usbFolders);
        DriverTypes.Add(usbDriverType);

        this.OnSelectedTreeItemChanged(localSystemDriverType);

    }

    #endregion

    #region Private methods   

    private  void OnImport()
    {
        if (this.SelectedItem is null)
        {
            _dialogService.Show(false, MessageLeveles.Info,
                                LanguageResource.Message_Info_CloseInformationTitle, LanguageResource.Message_Info_MustSelectItem,
                                null, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var path = this.SelectedItem.FullPath;
        if (!Directory.Exists(path))
        {
            _dialogService.Show(false, MessageLeveles.Error,
                                LanguageResource.Message_Info_CloseErrorTitle, LanguageResource.Message_Error_SourceFolderNotExist,
                                null, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        var diretoryList= Directory.GetDirectories(path);
        foreach (var dir in diretoryList) 
        {
            SendImportJobTask(dir);
        }
        if (diretoryList.Length==0)
        {
            SendImportJobTask(path);
        }
    }
    private  void SendImportJobTask(string dir)
    {
        Task task=new Task(() => { InitFilesList(dir); });
        task.RunSynchronously();
        string studyInstanceUID = GetStudyInstanceUID(_fileList);
        string seriesInstanceUID = GetSeriesInstanceUID(_fileList);
        bool isStudyExists = _studyApplicationService.GetStudyByStudyInstanceUID(studyInstanceUID);
        bool isSeriesExists = _studyApplicationService.GetSeriesBySeriesInstanceUID(seriesInstanceUID);
        if (isSeriesExists)
        {
            MessageTip = LanguageResource.Message_Info_SeriesAlreadyExists;
            MessageContent = TrimStringLength(MessageTip, 106);
            return;
        }

        MessageTip = LanguageResource.ToolTip_Importing;
        MessageContent = TrimStringLength(MessageTip, 40);

        var importJobTask = new ImportJobRequest();
        importJobTask.Id = Guid.NewGuid().ToString();
        importJobTask.WorkflowId = Guid.NewGuid().ToString();
        importJobTask.InternalPatientID = string.Empty;
        importJobTask.InternalStudyID = string.Empty;
        importJobTask.Priority = 5;
        importJobTask.JobTaskType = JobTaskType.ImportJob;
        importJobTask.Creator = _authorizationService.GetCurrentUser() is null ? string.Empty : _authorizationService.GetCurrentUser().Account;
        importJobTask.SourcePath = dir;
        importJobTask.VirtualSourcePath = this.SelectedItem.VirtualPath;
        importJobTask.Parameter = importJobTask.ToJson();

        var result = this._jobRequestService.EnqueueJobRequest(importJobTask);
        if (result.Status == CommandExecutionStatus.Success)
        {
            MessageContent = LanguageResource.Message_Info_ImportTaskStarted;
            MessageTip = MessageContent;
        }
        else
        {
            MessageContent = LanguageResource.Message_Error_ImportTaskFailed;
            MessageTip = MessageContent;
        }
    }
    private string GetStudyInstanceUID(List<string> fileList)
    {
        string studyInstanceUID = string.Empty;
        foreach (var file in fileList)
        {
            //validate file
            if (!DicomFile.HasValidHeader(file))
            {
                _logger.LogError($"[ImportByDirExecutor][ReadDicomFile]: The file does not match Dicom3.0 standard! The file name is:{file}");
                continue;
            }
            try
            {
                var dicomFile = DicomFile.Open(file);
                if (!dicomFile.Dataset.Contains(DicomTag.StudyInstanceUID))
                {
                    _logger.LogError($"[ImportByDirExecutor][ReadDicomFile]: The file does not contain StudyID! The file name is:{file}");
                    continue;
                }else
                {
                    studyInstanceUID= dicomFile.Dataset.GetString(DicomTag.StudyInstanceUID);
                    break;                 
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"[ImportByDirExecutor][ReadDicomFile]: Not a valid dicom file! It is failed to open file:{file} with error reason:{ex.Message}");
                continue;
            }          
        }
        return studyInstanceUID;
    }
    private string GetSeriesInstanceUID(List<string> fileList)
    {
        string seriesInstanceUID = string.Empty;
        foreach (var file in fileList)
        {
            //validate file
            if (!DicomFile.HasValidHeader(file))
            {
                _logger.LogError($"[ImportByDirExecutor][ReadDicomFile]: The file does not match Dicom3.0 standard! The file name is:{file}");
                continue;
            }
            try
            {
                var dicomFile = DicomFile.Open(file);
                if (!dicomFile.Dataset.Contains(DicomTag.SeriesInstanceUID))
                {
                    _logger.LogError($"[ImportByDirExecutor][ReadDicomFile]: The file does not contain StudyID! The file name is:{file}");
                    continue;
                }
                else
                {
                    seriesInstanceUID = dicomFile.Dataset.GetString(DicomTag.SeriesInstanceUID);
                    break;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"[ImportByDirExecutor][ReadDicomFile]: Not a valid dicom file! It is failed to open file:{file} with error reason:{ex.Message}");
                continue;
            }
        }
        return seriesInstanceUID;
    }
    private void InitFilesList(string _sourceRootPath)
    {
        _fileList.Clear();
        //处理所有Export源目录中的文件内容，添加到待导出列表。
        if (Directory.Exists(_sourceRootPath))
        {
            //默认当前文件夹下所有文件需要传输，不考虑后缀名
            var dir = new DirectoryInfo(_sourceRootPath);
            _fileList.AddRange(dir.GetFiles("*.*", SearchOption.AllDirectories).Select(x => x.FullName).ToList());
        }
        else if (File.Exists(_sourceRootPath))
        {
            _fileList.Add(_sourceRootPath);
        }
        else
        {
            _logger.LogError($"[InitFilesList] The source folder does not exist:{_sourceRootPath}");
            throw new DirectoryNotFoundException(_sourceRootPath);
        }
    }

    private void OnSelectedTreeItemChanged(TreeItemBase? selectedTreeItem)
    {
        this.SelectedItem = selectedTreeItem;

        if (null == SelectedItem)
        {
            return;
        }

        var item = (TreeItemBase?)SelectedItem;
        string virtualPath = item.VirtualPath;
        if (SelectedItem is DriverType)
        {
            IsImportButtonEnabled = false;

            var driverTypeNode = (DriverType)SelectedItem;
            if (driverTypeNode.Folders.Count() == 0)
            {
                virtualPath = $"{virtualPath} ({LanguageResource.Content_NotReady})";
            }
        }
        else
        {
            IsImportButtonEnabled = true;
        }

        OmittedChosenPath = $"{LanguageResource.Content_ChosenPath} {this.TrimStringLength(virtualPath, 80)}";
    }

    private void OnClose(object parameter)
    {
        this.ClearMessage();

        if (parameter is Window window)
        {
            window.Hide();
        }
    }

    private void OnNotifyMessage(object? sender, MessageInfo e)
    {
        //这里仅处理ImportJobResponse类型
        if (e.Sender != MessageSource.ImportJob)
        {
            return;
        }
        if (null == e.Remark || string.IsNullOrEmpty(e.Remark))
        {
            return;
        }

        JobTaskMessage jobTaskMessage;
        try
        {
            jobTaskMessage = JsonConvert.DeserializeObject<JobTaskMessage>(e.Remark);
        }
        catch
        {
            return;
        }

        if (jobTaskMessage?.JobStatus == JobTaskStatus.Completed ||
            jobTaskMessage?.JobStatus == JobTaskStatus.Cancelled ||
            jobTaskMessage?.JobStatus == JobTaskStatus.Failed)
        {
            MessageTip = LanguageResource.Content_ImportingFinished;
            MessageContent = MessageTip;
        }
    }

    private void Initialize()
    {
        if (string.IsNullOrEmpty(RuntimeConfig.Console.ExportData.Path))
        {
            string message = "Source path of import is empty.";
            _logger.LogError(message);
            throw new ArgumentNullException(message);
        }

        if (!Directory.Exists(RuntimeConfig.Console.ExportData.Path))
        {
            try
            {
                Directory.CreateDirectory(RuntimeConfig.Console.ExportData.Path);
            }
            catch
            {
                string message = $"Failed to create path for import:[{RuntimeConfig.Console.ExportData.Path}].";
                _logger.LogError(message);
                throw new DirectoryNotFoundException(message);
            }
        }

        this._localImportRootPath = (new DirectoryInfo(RuntimeConfig.Console.ExportData.Path)).FullName; //让配置文件中的路径进行字符串合规化

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
        bool isExpanded = false;

        foreach (var cdRomDisk in cdRomDisks)
        {
            if (!new DirectoryInfo(cdRomDisk.Name).Exists)
            {
                continue;
            }
            var subFolder = new Folder(TargetDiskType.CDROM, cdRomDisk.Name, cdRomDisk.Name, $"[{_labelCDROM}]{cdRomDisk.Name}", isSelected, isExpanded, isLogicalDisk);
            LoadSubFolders(TargetDiskType.CDROM, subFolder);
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
        bool isExpanded = false;

        foreach (var externalDisk in externalDisks)
        {
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

            var subFolder = new Folder(TargetDiskType.LocalPath, directoryInfo.Name, directoryInfo.FullName, virtualName, false, false);

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
        foreach (var subDirectory in subDirectories)
        {
            var directoryInfo = new DirectoryInfo(subDirectory);
            if ((directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                continue;
            }

            string virtualName = GetVirtualNameByFullName(directoryTreeItemType, directoryInfo.FullName);

            var subFolder = new Folder(directoryTreeItemType, directoryInfo.Name, directoryInfo.FullName, virtualName);
            LoadSubFolders(directoryTreeItemType, subFolder);
            subFolders.Add(subFolder);
        }

        folder.SubFolders = new ObservableCollection<Folder>(subFolders);
    }

    private string GetVirtualNameByFullName(TargetDiskType directoryTreeItemType, string fullName)
    {
        string virtualName = string.Empty;

        if (directoryTreeItemType == TargetDiskType.LocalPath)
        {
            virtualName = fullName.Replace(_localImportRootPath, $"[{_labelLocalPath}]");
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

    private string TrimStringLength(string content, int length)
    {
        if (string.IsNullOrEmpty(content) || content.Length <= length)
        {
            return content;
        }

        return $"{content.Substring(0, length)}...";
    }

    private void ClearMessage()
    {
        this.MessageContent = string.Empty;
        this.MessageTip = string.Empty;
    }

    #endregion
}