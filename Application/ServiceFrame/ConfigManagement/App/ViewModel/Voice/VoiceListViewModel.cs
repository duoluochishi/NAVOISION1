//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/6/6 16:35:51    V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NAudio.Wave;
using NV.MPS.UI.Dialog.Service;
using NV.MPS.UI.Dialog.Enum;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.ConfigManagement.Extensions;
using NV.CT.ConfigManagement.View;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.DatabaseService.Contract.Models;
using NV.MPS.Environment;
using NV.CT.UI.Controls;
using NV.MPS.Configuration;
namespace NV.CT.ConfigManagement.ViewModel;

public class VoiceListViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IVoiceApplicationService _voiceApplicationService;
    private ILogger<VoiceListViewModel> _logger;
    private VoiceWindow? _editWindow;
    private WaveOut _waveOutDevice;

    private ObservableCollection<BaseVoiceViewModel> _voiceList = new ObservableCollection<BaseVoiceViewModel>();

    public ObservableCollection<BaseVoiceViewModel> VoiceList
    {
        get => _voiceList;
        set => SetProperty(ref _voiceList, value);
    }

    private BaseVoiceViewModel _selectedVoice = new BaseVoiceViewModel();
    public BaseVoiceViewModel SelectedVoice
    {
        get => _selectedVoice;
        set
        {
            if (SetProperty(ref _selectedVoice, value) && value is not null)
            {
                IsFactory = !value.IsFactory;
            }
        }
    }

    private ApiType currentIsFront = ApiType.All;
    public ApiType CurrentIsFront
    {
        get => currentIsFront;
        set => SetProperty(ref currentIsFront, value);
    }

    private bool _preVoice = false;
    public bool PreVoice
    {
        get => _preVoice;
        set
        {
            SetProperty(ref _preVoice, value);
            if (value)
            {
                CurrentIsFront = ApiType.Front;
                SearchVoiceList(CurrentIsFront);
            }
        }
    }

    private bool _postVoice = false;
    public bool PostVoice
    {
        get => _postVoice;
        set
        {
            SetProperty(ref _postVoice, value);
            if (value)
            {
                CurrentIsFront = ApiType.Back;
                SearchVoiceList(CurrentIsFront);
            }
        }
    }

    private bool isAll = true;
    public bool IsAll
    {
        get => isAll;
        set
        {
            SetProperty(ref isAll, value);
            if (value)
            {
                CurrentIsFront = ApiType.All;
                SearchVoiceList(CurrentIsFront);
            }
        }
    }

    private bool isFactory = false;
    public bool IsFactory
    {
        get => isFactory;
        set => SetProperty(ref isFactory, value);
    }

    private VoicePlayStatus currentPlayStatus = VoicePlayStatus.None;
    public VoicePlayStatus CurrentPlayStatus
    {
        get => currentPlayStatus;
        set => SetProperty(ref currentPlayStatus, value);
    }

    private string _lastVoicePath = string.Empty;
    public string LastFilePath
    {
        get => _lastVoicePath;
        set => SetProperty(ref _lastVoicePath, value);
    }

    public VoiceListViewModel(IVoiceApplicationService voiceApplicationService,
        IDialogService dialogService,
        ILogger<VoiceListViewModel> logger)
    {
        _dialogService = dialogService;
        _voiceApplicationService = voiceApplicationService;
        _logger = logger;
        _waveOutDevice = new WaveOut();
        Commands.Add("VoiceEditCommand", new DelegateCommand(EditCommand));
        Commands.Add("VoiceAddCommand", new DelegateCommand(AddCommand));
        Commands.Add("VoiceDeleteCommand", new DelegateCommand(DeleteCommand));

        Commands.Add("SetDefaultCommand", new DelegateCommand<BaseVoiceViewModel>(SetDefault));
        Commands.Add("ResumeCommand", new DelegateCommand<BaseVoiceViewModel>(ResumeCommand));
        Commands.Add("PauseCommand", new DelegateCommand<BaseVoiceViewModel>(PauseCommand));
        Commands.Add("StopCommand", new DelegateCommand<BaseVoiceViewModel>(StopCommand));

        SearchVoiceList(ApiType.All);
        _voiceApplicationService.VoiceListReload += VoiceApplicationService_VoiceListReload;
    }

    [UIRoute]
    private void VoiceApplicationService_VoiceListReload(object? sender, EventArgs e)
    {
        SearchVoiceList(CurrentIsFront);
    }

    public void SearchVoiceList(ApiType apiType)
    {
        VoiceList.Clear();
        string apiFront = string.Empty;
        switch (apiType)
        {
            case ApiType.All:
                apiFront = string.Empty;
                break;
            case ApiType.Front:
                apiFront = true.ToString();
                break;
            case ApiType.Back:
                apiFront = false.ToString();
                break;
            default:
                apiFront = string.Empty;
                break;
        }

        foreach (var voiceModel in _voiceApplicationService.GetVoiceInfo(apiFront))
        {
            BaseVoiceViewModel voiceViewModel = new BaseVoiceViewModel()
            {
                ID = voiceModel.Id,
                BodyPart = voiceModel.BodyPart,
                Description = voiceModel.Description,
                FilePath = voiceModel.FilePath,
                InternalId = voiceModel.InternalId,
                IsDefault = voiceModel.IsDefault,
                IsFactory = voiceModel.IsFactory,
                IsFront = voiceModel.IsFront,
                IsValid = voiceModel.IsValid,
                Language = voiceModel.Language,
                Name = voiceModel.Name,
                VoiceLength = voiceModel.VoiceLength,
            };
            VoiceList.Add(voiceViewModel);
        }
        if (VoiceList.Count > 0)
        {
            SelectedVoice = VoiceList[0];
        }
    }

    private void AddCommand()
    {
        var voiceModel = new VoiceModel();
        voiceModel.IsFactory = false;
        voiceModel.Id = Guid.NewGuid().ToString();

        _voiceApplicationService.SetVoiceInfo(OperationType.Add, voiceModel);
        ShowWindow();
    }

    private void EditCommand()
    {
        //出厂角色不可编辑
        if (SelectedVoice is null || string.IsNullOrEmpty(SelectedVoice.ID))
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
               , "Please select a voice from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var voice = _voiceApplicationService.GetVoiceInfoByID(SelectedVoice.ID);
        if (voice is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
             , "Please select a voice from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        _voiceApplicationService.SetVoiceInfo(OperationType.Edit, voice);
        ShowWindow();
    }

    public void ShowWindow()
    {
        if (_editWindow is null)
        {
            _editWindow = CTS.Global.ServiceProvider?.GetRequiredService<VoiceWindow>();
        }
        if (_editWindow is not null)
        {            //_editWindow.ShowWindowDialog();
            _editWindow.ShowPopWindowDialog();
        }
    }

    private void DeleteCommand()
    {
        if (SelectedVoice is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
              , "Please select a voice from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        if (SelectedVoice.IsFactory)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                , "You can't delete the current voice because there are  factory! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var voiceModel = new VoiceModel();
        voiceModel.IsFactory = SelectedVoice.IsFactory;
        voiceModel.Id = SelectedVoice.ID;
        voiceModel.Name = SelectedVoice.Name;
        voiceModel.InternalId = (ushort)SelectedVoice.InternalId;
        voiceModel.Description = SelectedVoice.Description;
        voiceModel.VoiceLength = (ushort)SelectedVoice.VoiceLength;
        voiceModel.Language = SelectedVoice.Language;
        voiceModel.IsDefault = SelectedVoice.IsDefault;
        voiceModel.IsValid = SelectedVoice.IsValid;
        voiceModel.BodyPart = SelectedVoice.BodyPart;
        voiceModel.FilePath = SelectedVoice.FilePath;

        _dialogService?.ShowDialog(true, MessageLeveles.Info, "Confirm"
            , "Are you sure to delete the voice? ", arg =>
            {
                if (arg.Result == ButtonResult.OK)
                {
                    bool flag = false;
                    string msg = string.Empty;
                    try
                    {
                        flag = _voiceApplicationService.Delete(voiceModel);
                    }
                    catch (Exception ex)
                    {
                        flag = false;
                        msg = ex.Message;
                    }
                    if (flag)
                    {
                        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                            , $"Delete voice({SelectedVoice.Name})! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                        SearchVoiceList(CurrentIsFront);
                    }
                    else
                    {
                        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                           , $"Delete voice error({msg})! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                    }
                }
            }, ConsoleSystemHelper.WindowHwnd);
    }

    private void SetDefault(BaseVoiceViewModel voiceModel)
    {
        if (voiceModel is null || (voiceModel is not null && voiceModel.IsDefault))
        {
            return;
        }
        var voice = _voiceApplicationService.GetVoiceInfoByID(voiceModel.ID);
        if (voice is not null)
        {
            voice.IsDefault = true;
            _voiceApplicationService.SetDefault(voice);
            SearchVoiceList(CurrentIsFront);
        }
    }

    private void ResumeCommand(BaseVoiceViewModel voiceModel)
    {
        var voiceRoot = RuntimeConfig.Console.MCSVoices.Path;
        var filePath = Path.Combine(voiceRoot, voiceModel.FilePath);
        if (!LastFilePath.Equals(filePath))
        {
            _waveOutDevice.Stop();
        }
        LastFilePath = filePath;
        if (string.IsNullOrEmpty(voiceModel.FilePath) || !File.Exists(LastFilePath))
        {
            return;
        }
        if (_waveOutDevice.PlaybackState == PlaybackState.Stopped)
        {
            var inputStream = new AudioFileReader(LastFilePath);
            var aggregator = new NAudioReader(inputStream);
            _waveOutDevice.Init(aggregator);
            _waveOutDevice.Volume = 1.0F;
            _waveOutDevice.PlaybackStopped += WaveOutDevice_PlaybackStopped;
            _waveOutDevice.Play();

            CurrentPlayStatus = VoicePlayStatus.Playing;
        }

        if (_waveOutDevice.PlaybackState == PlaybackState.Paused)
        {
            _waveOutDevice.Resume();
            CurrentPlayStatus = VoicePlayStatus.Playing;
        }
    }

    [UIRoute]
    private void WaveOutDevice_PlaybackStopped(object? sender, StoppedEventArgs e)
    {
        CurrentPlayStatus = VoicePlayStatus.None;
    }

    private void PauseCommand(BaseVoiceViewModel voiceModel)
    {
        if (_waveOutDevice.PlaybackState != PlaybackState.Playing)
        {
            return;
        }
        _waveOutDevice.Pause();
        CurrentPlayStatus = VoicePlayStatus.Pause;
    }

    private void StopCommand(BaseVoiceViewModel voiceModel)
    {
        if (_waveOutDevice.PlaybackState != PlaybackState.Playing)
        {
            return;
        }
        _waveOutDevice.Stop();
        CurrentPlayStatus = VoicePlayStatus.None;
    }
}