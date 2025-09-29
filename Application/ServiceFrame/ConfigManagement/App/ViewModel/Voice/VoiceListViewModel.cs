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
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.ConfigManagement.View;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.UI.Controls;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;

namespace NV.CT.ConfigManagement.ViewModel;

public class VoiceListViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IVoiceApplicationService _voiceApplicationService;
    private ILogger<VoiceListViewModel> _logger;
    private VoiceWindow? _editWindow;
    private WaveOut _waveOutDevice;
    private const int MAX_VOICE_COUNT = 50;

    private ObservableCollection<BaseVoiceViewModel> _voiceList = new ObservableCollection<BaseVoiceViewModel>();

    public ObservableCollection<BaseVoiceViewModel> VoiceList
    {
        get => _voiceList;
        set => SetProperty(ref _voiceList, value);
    }

    private BaseVoiceViewModel _selectedVoice;
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
        var tempVoiceList = new ObservableCollection<BaseVoiceViewModel>();
        var voiceModels = _voiceApplicationService.GetVoiceModels();
        foreach (var voiceModel in voiceModels)
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
                RealVoiceLength = voiceModel.RealVoiceLength
            };

            if (voiceModel.PairId > 0)
            {
                voiceViewModel.PairedVoiceName = voiceModels.FirstOrDefault(v => v.InternalId == voiceModel.PairId)?.Name;
            }

            if (apiType == ApiType.Front && !voiceModel.IsFront)
                continue;
            else if (apiType == ApiType.Back && voiceModel.IsFront)
                continue;

            tempVoiceList.Add(voiceViewModel);
        }
        VoiceList = tempVoiceList;
    }

    private void AddCommand()
    {
        if (!CanAddMoreVoices())
        {
            _dialogService.ShowDialog(false, MessageLeveles.Info, "Info", "Maximum number of voices reached!",
                arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var voiceModel = new VoiceModel();
        voiceModel.IsFactory = false;
        voiceModel.Id = Guid.NewGuid().ToString();

        ShowWindow(OperationType.Add, voiceModel);
    }

    private void EditCommand()
    {
        //出厂角色不可编辑
        if (SelectedVoice is null || string.IsNullOrEmpty(SelectedVoice.InternalId.ToString()))
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
               , "Please select a voice from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        var voice = _voiceApplicationService.GetVoiceInfoByID(SelectedVoice.InternalId.ToString());
        if (voice is null)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
             , "Please select a voice from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }

        ShowWindow(OperationType.Edit, voice);
    }

    public void ShowWindow(OperationType operation, VoiceModel voice)
    {
        if (_editWindow is null)
        {
            _editWindow = CTS.Global.ServiceProvider?.GetRequiredService<VoiceWindow>();
        }

        _voiceApplicationService.SetVoiceInfo(operation, voice);

        if (_editWindow is not null)
        {
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
        voiceModel.PairId = (ushort)SelectedVoice.PairId;
        voiceModel.Description = SelectedVoice.Description;
        voiceModel.VoiceLength = (ushort)SelectedVoice.VoiceLength;
        voiceModel.RealVoiceLength = SelectedVoice.RealVoiceLength;
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
                        // 同时删除配对语音中的记录
                        if (voiceModel.PairId > 0)
                        {
                            var pairedVoice = _voiceApplicationService.GetVoiceInfoByID(voiceModel.PairId.ToString());
                            pairedVoice.PairId = 0;
                            flag = _voiceApplicationService.Update(pairedVoice);
                        }
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

    /// <summary>
    /// 设置默认语音，默认语音具有唯一性
    /// </summary>
    /// <param name="voiceModel"></param>
    private void SetDefault(BaseVoiceViewModel voiceModel)
    {
        if (voiceModel is null || voiceModel.IsDefault)
        {
            return;
        }
        var voices = _voiceApplicationService.GetVoiceModels();
        foreach (var item in voices)
        {
            // 先清除所有的默认
            if (item.IsDefault)
            {
                item.IsDefault = false;
                _voiceApplicationService.Update(item);
            }
            // 再设置当前默认
            if (item.InternalId == voiceModel.InternalId)
            {
                item.IsDefault = true;
                _voiceApplicationService.SetDefault(item);
            }
        }
        SearchVoiceList(CurrentIsFront);
    }

    private void ResumeCommand(BaseVoiceViewModel voiceModel)
    {
        var voiceRoot = RuntimeConfig.Console.MCSVoices.Path;
        var filePath = Path.Combine(voiceRoot, voiceModel.FilePath);
        LastFilePath = filePath;
        if (string.IsNullOrEmpty(voiceModel.FilePath) || !File.Exists(LastFilePath))
        {
            return;
        }
        if (voiceModel.PlayStatus != VoicePlayStatus.Playing)
        {
            if (!CheckVoiceFileConsistency(voiceModel))
            {
                return;
            }
            voiceModel.PlayStatus = VoicePlayStatus.Playing;
            _voiceApplicationService.PlayAudioFile((ushort)voiceModel.InternalId, args =>
            {
                var currentVoice = voiceModel; //记得缓存，防止闭包影响
                if (args.Reason != FacadeProxy.Common.Enums.AudioFileEnums.PlayCompletionReason.Completed)
                {
                    _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                            , $"Play voice failed, Please try again! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                }
                currentVoice.PlayStatus = VoicePlayStatus.None;
            });
            voiceModel.PlayStatus = VoicePlayStatus.None; //ToDo:facadeproxy中PlayAudioFile callback有bug，暂时直接赋值为None
        }
    }

    private bool CanAddMoreVoices()
    {
        var items = _voiceApplicationService.GetVoiceModels();
        return items.Count < MAX_VOICE_COUNT;
    }

    /// <summary>
    /// 以本地数据库为基准，检查与远程auxboard语音文件的一致性
    /// 远程缺少，从本地补足
    /// </summary>
    /// <returns></returns>
    private bool CheckVoiceFileConsistency(BaseVoiceViewModel voiceModel)
    {
        // 检查本地语音文件的InternalId是否都在远程列表中
        var remoteVoices = _voiceApplicationService.GetAll();

        if (remoteVoices is not null && !remoteVoices.Contains((ushort)voiceModel.InternalId))
        {
            var vm = new VoiceModel();
            vm.InternalId = (ushort)voiceModel.InternalId;
            vm.FilePath = voiceModel.FilePath;
            try
            {
                if(_voiceApplicationService.AddOrUpdate(vm)) // ToDo: 需要注意AddOrUpdate可能会有更新成功，但是还没有往auxboard上传完成的情况
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddOrUpdate voice failed");
            }

            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                              , "The selected voice does not exist in the remote auxboard!", arg => { }, ConsoleSystemHelper.WindowHwnd);

            _logger.LogError( $"AddOrUpdate voice failed. {voiceModel.InternalId} {voiceModel.FilePath}");

            return false;
        }

        return true;
    }
}