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
using Newtonsoft.Json;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.ConfigManagement.Extensions;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.Language;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Collections.Generic;
using System.Windows.Forms;
using Enum = System.Enum;

namespace NV.CT.ConfigManagement.ViewModel;

public class VoiceViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IVoiceApplicationService _voiceApplicationService;
    private readonly NAudioRecorder _recorder;
    private readonly ILogger<VoiceViewModel> _logger;
    private BaseVoiceViewModel _currentVoice = new BaseVoiceViewModel();

    private const int MAX_PRE_VOICE_LENGTH = 10;
    private const int MAX_POST_VOICE_LENGTH = 6;

    public BaseVoiceViewModel CurrentVoice
    {
        get => _currentVoice;
        set => SetProperty(ref _currentVoice, value);
    }

    private ObservableCollection<KeyValuePair<string, string>> _languagelist = new ObservableCollection<KeyValuePair<string, string>>();
    public ObservableCollection<KeyValuePair<string, string>> LanguageList
    {
        get => _languagelist;
        set => SetProperty(ref _languagelist, value);
    }

    private double _recordTime = 0.0;
    public double RecordTime
    {
        get => _recordTime;
        set
        {
            if (SetProperty(ref _recordTime, value))
            {
                CurrentVoice.VoiceLength = (int)value;
            }
        }
    }

    private string _recordFilePath = string.Empty;
    public string RecordFilePath
    {
        get => _recordFilePath;
        set
        {
            if (SetProperty(ref _recordFilePath, value))
            {
                string directory = Directory.GetParent(value)?.Name;
                string fileName = Path.GetFileName(value);
                if (!string.IsNullOrEmpty(directory))
                    CurrentVoice.FilePath = Path.Combine(directory, fileName);
            }
        }
    }

    private RecordingStatus _recordStatus = RecordingStatus.None;
    public RecordingStatus RecordStatus
    {
        get => _recordStatus;
        set => SetProperty(ref _recordStatus, value);
    }

    private bool _importWavSuccessful = false;
    public bool ImportWavSuccessful
    {
        get => _importWavSuccessful;
        set
        {
            SetProperty(ref _importWavSuccessful, value);
        }
    }

    private bool isFrontChecked = true;
    public bool IsFrontChecked
    {
        get => isFrontChecked;
        set
        {
            if (SetProperty(ref isFrontChecked, value))
            {
                InitAvailablePairVoices();
            }
        }
    }

    private bool _isBackChecked = false;
    public bool IsBackChecked
    {
        get => _isBackChecked;
        set
        {
            SetProperty(ref _isBackChecked, value);
        }
    }

    public OperationType OperationType { get; set; } = OperationType.Add;

    private ObservableCollection<VoiceModel> _availablePairVoices = new ObservableCollection<VoiceModel>();
    public ObservableCollection<VoiceModel> AvailablePairVoices
    {
        get => _availablePairVoices;
        set => SetProperty(ref _availablePairVoices, value);
    }

    private VoiceModel _selectedPairedVoice;
    public VoiceModel SelectedPairedVoice
    {
        get => _selectedPairedVoice;
        set => SetProperty(ref _selectedPairedVoice, value);
    }

    public VoiceViewModel(IVoiceApplicationService voiceApplicationService,
        IDialogService dialogService,
        ILogger<VoiceViewModel> logger)
    {
        _dialogService = dialogService;
        _voiceApplicationService = voiceApplicationService;
        _logger = logger;
        InitLanguageList();
        Commands.Add("RecordCommand", new DelegateCommand(RecordCommand));
        Commands.Add("ImportWavCommand", new DelegateCommand(ImportWavCommand, () => RecordStatus != RecordingStatus.Recording));
        Commands.Add("SaveCommand", new DelegateCommand<object>(Saved, _ => RecordStatus != RecordingStatus.Recording));
        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => RecordStatus != RecordingStatus.Recording));
        _voiceApplicationService.VoiceInfoChanged += VoiceApplicationService_VoiceInfoChanged;

        _recorder = new NAudioRecorder();
    }

    [UIRoute]
    private void VoiceApplicationService_VoiceInfoChanged(object? sender, EventArgs<(OperationType operation, VoiceModel voiceModel)> e)
    {
        if (e is null)
        {
            return;
        }
        OperationType = e.Data.operation;
        SetRoleInfo(e.Data.voiceModel);
    }

    private void SetRoleInfo(VoiceModel voiceModel)
    {
        CurrentVoice = new BaseVoiceViewModel();
        CurrentVoice.ID = voiceModel.Id;
        CurrentVoice.PairId = voiceModel.PairId;
        CurrentVoice.Name = voiceModel.Name;
        CurrentVoice.InternalId = voiceModel.InternalId;
        CurrentVoice.Description = voiceModel.Description;
        CurrentVoice.VoiceLength = voiceModel.VoiceLength;
        CurrentVoice.RealVoiceLength = voiceModel.RealVoiceLength;
        CurrentVoice.Language = voiceModel.Language;
        CurrentVoice.IsDefault = voiceModel.IsDefault;
        CurrentVoice.IsFactory = voiceModel.IsFactory;
        CurrentVoice.IsValid = voiceModel.IsValid;
        CurrentVoice.BodyPart = voiceModel.BodyPart;
        CurrentVoice.FilePath = voiceModel.FilePath;

        if (voiceModel.IsFront)
        {
            IsFrontChecked = true;
        }
        else
        {
            IsBackChecked = true;
        }

        InitAvailablePairVoices();
    }

    private void InitLanguageList()
    {
        foreach (var enumItem in Enum.GetValues(typeof(LanguageType)))
        {
            if (enumItem is not null)
            {
                LanguageList.Add(new KeyValuePair<string, string>(enumItem.ToString(), enumItem.ToString()));
            }
        }
    }

    private void InitAvailablePairVoices()
    {
        var voiceModels = _voiceApplicationService.GetVoiceModels();

        AvailablePairVoices = new ObservableCollection<VoiceModel>(voiceModels.Where(v =>
          {
              if (CurrentVoice.InternalId == v.InternalId)
                  return false;

              if (v.IsFactory)
                  return false;

              if (IsFrontChecked)
                  return !v.IsFront;
              else
                  return v.IsFront;
          }));

        if (CurrentVoice != null && CurrentVoice.PairId > 0)
        {
            SelectedPairedVoice = AvailablePairVoices.FirstOrDefault(v => v.InternalId == CurrentVoice.PairId);
        }
    }

    private void RecordCommand()
    {
        switch (RecordStatus)
        {
            //开始录音
            case RecordingStatus.None:
                {
                    if (!_recorder.HasAvailableRecordingDevice())
                    {
                        _dialogService.ShowDialog(false, MessageLeveles.Info, "Tip", "Please plug in the microphone!",
                            callback => { }, ConsoleSystemHelper.WindowHwnd);
                        return;
                    }
                    var voiceRoot = RuntimeConfig.Console.MCSVoices.Path;
                    if (!Directory.Exists(Path.Combine(voiceRoot, "Custom")))
                    {
                        Directory.CreateDirectory(Path.Combine(voiceRoot, @"Custom"));
                    }
                    if (OperationType == OperationType.Add)
                    {
                        CurrentVoice.InternalId = _voiceApplicationService.GetMaxInternalId() + 1;
                    }
                    RecordFilePath = Path.Combine(voiceRoot, "Custom", $"{_voiceApplicationService.GetMaxInternalId() + 1}.wav");
                    _recorder.DataAvailable += Recorder_DataAvailable;
                    _recorder.SetFileName(RecordFilePath);
                    _recorder.StartRec();
                    RecordStatus = RecordingStatus.Recording;
                    break;
                }
            case RecordingStatus.Recording:
                _recorder.DataAvailable -= Recorder_DataAvailable;
                _recorder.StopRec();
                RecordStatus = RecordingStatus.None;
                break;
            default:
                break;
        }
    }

    private void Recorder_DataAvailable(double recordTime)
    {
        RecordTime = recordTime;
        if (IsFrontChecked && recordTime >= MAX_PRE_VOICE_LENGTH)
        {
            RecordCommand();
        }

        if (IsBackChecked && recordTime >= MAX_POST_VOICE_LENGTH)
        {
            RecordCommand();
        }
    }

    private void ImportWavCommand()
    {
        if (CheckIsRecording()) return;

        string message = string.Empty;
        try
        {
            ImportWavSuccessful = false;
            OpenFileDialog dialog = new()
            {
                DefaultExt = ".wav",
                Filter = "wav file|*.wav"
            };
            DialogResult result = dialog.ShowDialog();
            if (result != DialogResult.Cancel)
            {
                var pathName = dialog.FileName;
                //复制到指定目录
                var voiceRoot = RuntimeConfig.Console.MCSVoices.Path;
                if (!Directory.Exists(Path.Combine(voiceRoot, @"Custom")))
                {
                    Directory.CreateDirectory(Path.Combine(voiceRoot, @"Custom"));
                }
                var newWavPath = string.Empty;
                if (OperationType == OperationType.Add)
                {
                    CurrentVoice.InternalId = _voiceApplicationService.GetMaxInternalId() + 1;
                    newWavPath = Path.Combine(voiceRoot, @"Custom", $"{CurrentVoice.InternalId}.wav");
                }
                else if (OperationType == OperationType.Edit)
                {
                    newWavPath = Path.Combine(voiceRoot, @"Custom", $"{CurrentVoice.InternalId}.wav");
                }
                //如果已存在同名语音文件，则覆盖
                File.Copy(pathName, newWavPath, true);
                //Todo:语音文件大小是否做限制，时间长度是否要做限制，语音长度参数放到系统配置里
                CurrentVoice.FilePath = newWavPath.Replace(voiceRoot + @"\", "");
                //语音时间长度获取
                RecordFilePath = newWavPath;
                using (var inputStream = new AudioFileReader(newWavPath))
                {
                    RecordTime = Math.Round(inputStream.TotalTime.TotalSeconds, 0);
                    CurrentVoice.RealVoiceLength = (decimal)Math.Round(inputStream.TotalTime.TotalSeconds, 2, MidpointRounding.AwayFromZero);
                }
                CurrentVoice.VoiceLength = (int)RecordTime;
                var isVoiceFileValid = CheckVoiceFileLength();
                if (isVoiceFileValid)
                {
                    ImportWavSuccessful = isVoiceFileValid;
                    message = $"Import successful:{RecordFilePath}";
                }
                else
                {
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            message = ex.Message + ";Import failed!";
        }
        if (!string.IsNullOrEmpty(message))
        {
            _dialogService.ShowDialog(false, MessageLeveles.Info, "Tip", message, callback => { }, ConsoleSystemHelper.WindowHwnd);
        }
    }

    public void Saved(object parameter)
    {
        if (CheckIsRecording()) return;

        if (OperationType == OperationType.Add && !ImportWavSuccessful && RecordStatus != RecordingStatus.None)
        {
            string msg = " Please import the voice file first!";
            _logger.LogWarning(msg);
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", msg,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
            return;
        }
        if (parameter is not Window window)
        {
            return;
        }
        if (!CheckFormEmpty() || CheckNameRepeat() || !CheckNumAndEnChForm() || !CheckVoiceFile() || !CheckVoiceFileLength())
        {
            return;
        }
        VoiceModel voiceInfo = new VoiceModel();
        voiceInfo.Id = CurrentVoice.ID;
        voiceInfo.Name = CurrentVoice.Name;
        voiceInfo.InternalId = (ushort)CurrentVoice.InternalId;
        voiceInfo.PairId = (ushort)CurrentVoice.PairId;
        voiceInfo.Description = CurrentVoice.Description;
        voiceInfo.VoiceLength = (ushort)CurrentVoice.VoiceLength;
        voiceInfo.RealVoiceLength = CurrentVoice.RealVoiceLength;
        voiceInfo.Language = CurrentVoice.Language;
        voiceInfo.IsDefault = CurrentVoice.IsDefault;
        voiceInfo.IsFactory = CurrentVoice.IsFactory;
        voiceInfo.IsValid = CurrentVoice.IsValid;
        voiceInfo.BodyPart = CurrentVoice.BodyPart;
        voiceInfo.FilePath = CurrentVoice.FilePath;
        if (IsFrontChecked & !IsBackChecked)
        {
            voiceInfo.IsFront = true;
        }
        if (!IsFrontChecked & IsBackChecked)
        {
            voiceInfo.IsFront = false;
        }
        if (OperationType == OperationType.Add)
        {
            voiceInfo.IsValid = true;
            voiceInfo.Id = Guid.NewGuid().ToString();
            voiceInfo.IsFactory = false;
            voiceInfo.CreateTime = DateTime.Now;
        }
        if (voiceInfo.RealVoiceLength == 0)
        {
            using (var inputStream = new AudioFileReader(Path.Combine(RuntimeConfig.Console.MCSVoices.Path, voiceInfo.FilePath)))
            {
                voiceInfo.RealVoiceLength = (decimal)Math.Round(inputStream.TotalTime.TotalSeconds, 2, MidpointRounding.AwayFromZero);
            }
        }

        List<int> pairsPendingClear=new List<int>();
        if (SelectedPairedVoice != null && (voiceInfo.PairId != SelectedPairedVoice.InternalId || SelectedPairedVoice.PairId != voiceInfo.InternalId))
        {
            // 若已有配对，则加入缓存，等待语音设置成功后清除
            if (voiceInfo.PairId > 0)
                pairsPendingClear.Add(voiceInfo.PairId);
            if (SelectedPairedVoice.PairId > 0)
                pairsPendingClear.Add(SelectedPairedVoice.PairId);

            voiceInfo.PairId = SelectedPairedVoice.InternalId;
            SelectedPairedVoice.PairId = voiceInfo.InternalId;
        }
        bool saveFlag = false;
        try
        {
            if (!_voiceApplicationService.AddOrUpdate(voiceInfo))
            {
                string msg = "Adding or updating voice file failed!";
                _logger.LogWarning(msg);
                _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", msg,
                    arg => { }, ConsoleSystemHelper.WindowHwnd);
                return;
            }
            _logger.LogWarning($"Adding or updating voice file success to board: {voiceInfo.FilePath}");

            // 清除原配对语音的PairId
            _logger.LogInformation($"Begin to clear {pairsPendingClear.Count} original pairid(s)");
            foreach (int pairId in pairsPendingClear)
            {
                saveFlag = ClearPairedVoice(pairId);
                if (!saveFlag)
                {
                    _logger.LogWarning($"Clearing paired voice error: {pairId}");
                    break;
                }
                _logger.LogInformation($"Clearing paired voice: {pairId} success.");
            }

            // 添加或者更新当前语音
            switch (OperationType)
            {
                case OperationType.Add:
                    saveFlag = _voiceApplicationService.Add(voiceInfo);
                    break;
                case OperationType.Edit:
                default:
                    saveFlag = _voiceApplicationService.Update(voiceInfo);
                    break;
            }
            string result = saveFlag ? "successfully" : "failed";
            _logger.LogWarning($"Adding or updating voice file to DB: {voiceInfo.FilePath} {result}");
            _logger.LogWarning($"Adding or updating voice ViewModel is: {JsonConvert.SerializeObject(voiceInfo)}");

            // 更新现配对语音的PairId
            if (SelectedPairedVoice != null && saveFlag)
            {
                saveFlag = _voiceApplicationService.Update(SelectedPairedVoice);
                result = saveFlag ? "successfully" : "failed";
                _logger.LogWarning($"Updating pairid of the selected pair voice to DB: {SelectedPairedVoice.FilePath} {result}");
            }
            if (voiceInfo.IsDefault)
            {
                _voiceApplicationService.SetDefault(voiceInfo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Adding or updating voice file error from exception : {ex.Message}");
        }
        _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", saveFlag ? LanguageResource.Message_Info_SaveSuccessfullyPara : LanguageResource.Message_Info_FailedToSavePara,
          arg =>
          {
              if (saveFlag)
              {
                  _voiceApplicationService.ReloadVoiceList();
                  window.Hide();
              }
          }, ConsoleSystemHelper.WindowHwnd);
    }

    private bool CheckFormEmpty()
    {
        bool flag = true;
        StringBuilder sb = new StringBuilder();
        string message = "{0} can't be empty!";

        if (string.IsNullOrEmpty(CurrentVoice.Name))
        {
            sb.Append(string.Format(message, "Name"));
            flag = false;
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", sb.ToString(),
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool CheckNameRepeat()
    {
        bool flag = false;
        List<VoiceModel> voices = _voiceApplicationService.GetVoiceModels();
        switch (OperationType)
        {
            case OperationType.Edit:
                flag = voices.Any(t => t.InternalId != CurrentVoice.InternalId && t.Name == CurrentVoice.Name);
                break;
            default: break;
        }
        if (flag)
        {
            var message = "The name is duplicated!";
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool CheckNumAndEnChForm()
    {
        bool flag = true;
        string message = "";
        if (VerificationExtension.IsSpecialCharacters(CurrentVoice.Name))
        {
            flag = false;
            message += $"Name:Special characters are not allowed!";
        }
        if (!flag)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return flag;
    }

    private bool CheckVoiceFile()
    {
        bool isExist = File.Exists(Path.Combine(RuntimeConfig.Console.MCSVoices.Path, CurrentVoice.FilePath));
        if (!isExist)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", "Voice file cannot be empty!",
                arg => { }, ConsoleSystemHelper.WindowHwnd);
        }
        return isExist;
    }

    private bool CheckIsRecording()
    {
        if (RecordStatus == RecordingStatus.Recording)
        {
            string msg = "Please stop recording first!";
            _logger.LogWarning(msg);
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", msg,
                arg => { }, ConsoleSystemHelper.WindowHwnd);
            return true;
        }
        return false;
    }

    private bool CheckVoiceFileLength()
    {
        bool result = true;
        string message = string.Empty;
        if (IsFrontChecked && RecordTime > MAX_PRE_VOICE_LENGTH)
        {
            message = $"The length of pre-voice cannot exceed {MAX_PRE_VOICE_LENGTH} seconds!";
            result = false;
        }
        else if (IsBackChecked && RecordTime > MAX_POST_VOICE_LENGTH)
        {
            message = $"The length of post-voice cannot exceed {MAX_POST_VOICE_LENGTH} seconds!";
            result = false;
        }

        if (!result)
        {
            _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                    arg => { }, ConsoleSystemHelper.WindowHwnd);
        }

        return result;
    }

    private bool ClearPairedVoice(int internalId)
    {
        var voiceInfo = _voiceApplicationService.GetVoiceInfoByID(internalId.ToString());
        if (voiceInfo != null)
        {
            voiceInfo.PairId = 0;
            return _voiceApplicationService.Update(voiceInfo);
        }

        return false;
    }

    public void Closed(object parameter)
    {
        if (CheckIsRecording()) return;

        if (parameter is Window window)
        {
            _voiceApplicationService.ReloadVoiceList();
            window.Hide();
        }
    }
}