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
using NV.MPS.UI.Dialog.Service;
using NV.MPS.UI.Dialog.Enum;
using System.Collections.Generic;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NAudio.Wave;
using NV.CT.CommonAttributeUI.AOPAttribute;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.ConfigManagement.Extensions;
using Enum = System.Enum;
using NV.MPS.Environment;
using System.Windows.Forms;
using NV.CT.Language;
using System.Windows.Interop;
using Newtonsoft.Json;

namespace NV.CT.ConfigManagement.ViewModel;

public class VoiceViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IVoiceApplicationService _voiceApplicationService;
    private readonly NAudioRecorder _recorder;
    private readonly ILogger<VoiceViewModel> _logger;
    private BaseVoiceViewModel _currentVoice = new BaseVoiceViewModel();
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
        set => SetProperty(ref _recordFilePath, value);
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

    private bool isFrontChecked = false;
    public bool IsFrontChecked
    {
        get => isFrontChecked;
        set
        {
            SetProperty(ref isFrontChecked, value);
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

    public VoiceViewModel(IVoiceApplicationService voiceApplicationService,
        IDialogService dialogService,
        ILogger<VoiceViewModel> logger)
    {
        _dialogService = dialogService;
        _voiceApplicationService = voiceApplicationService;
        _logger = logger;
        InitLanguageList();
        Commands.Add("RecordCommand", new DelegateCommand(RecordCommand));
        Commands.Add("ImportWavCommand", new DelegateCommand(ImportWavCommand));
        Commands.Add("SaveCommand", new DelegateCommand<object>(Saved, _ => true));
        Commands.Add("CloseCommand", new DelegateCommand<object>(Closed, _ => true));
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
        CurrentVoice.Name = voiceModel.Name;
        CurrentVoice.InternalId = voiceModel.InternalId;
        CurrentVoice.Description = voiceModel.Description;
        CurrentVoice.VoiceLength = voiceModel.VoiceLength;
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

    private void RecordCommand()
    {
        switch (RecordStatus)
        {
            //开始录音
            case RecordingStatus.None:
                {
                    var voiceRoot = RuntimeConfig.Console.MCSVoices.Path;
                    if (!Directory.Exists(Path.Combine(voiceRoot, @"Custom\")))
                    {
                        Directory.CreateDirectory(Path.Combine(voiceRoot, @"Custom"));
                    }
                    RecordFilePath = Path.Combine(voiceRoot, $"Custom/{(_voiceApplicationService.GetMaxInternalId() + 1).ToString()}.wav");
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
                CurrentVoice.FilePath = RecordFilePath;
				break;
            default:
                break;
        }
    }

    private void Recorder_DataAvailable(double recordTime)
    {
        RecordTime = recordTime;
    }

    private void ImportWavCommand()
    {
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
                if (!Directory.Exists(Path.Combine(voiceRoot, @"Custom\")))
                {
                    Directory.CreateDirectory(Path.Combine(voiceRoot, @"Custom"));
                }
                var newWavPath = string.Empty;
                if (OperationType == OperationType.Add)
                {
                    CurrentVoice.InternalId = _voiceApplicationService.GetMaxInternalId() + 1;
                    newWavPath = Path.Combine(voiceRoot, @"Custom\", $"{(CurrentVoice.InternalId).ToString()}.wav");
                }
                else if (OperationType == OperationType.Edit)
                {
                    newWavPath = Path.Combine(voiceRoot, @"Custom\", $"{CurrentVoice.InternalId.ToString()}.wav");
                }
                //如果已存在同名语音文件，则覆盖
                File.Copy(pathName, newWavPath, true);
                //Todo:语音文件大小是否做限制，时间长度是否要做限制，语音长度参数放到系统配置里
                CurrentVoice.FilePath = newWavPath.Replace(voiceRoot + @"\", "");
                //语音时间长度获取
                RecordFilePath = newWavPath;
                var inputStream = new AudioFileReader(newWavPath);
                RecordTime = Math.Round(inputStream.TotalTime.TotalSeconds, 0);
                CurrentVoice.VoiceLength = (int)RecordTime;
                message = $"Import successful:{RecordFilePath}";

                ImportWavSuccessful = true;
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
        if (!CheckFormEmpty() || CheckNameRepeat() || !CheckNumAndEnChForm())
        {
            return;
        }
        VoiceModel voiceInfo = new VoiceModel();
        voiceInfo.Id = CurrentVoice.ID;
        voiceInfo.Name = CurrentVoice.Name;
        voiceInfo.InternalId = (ushort)CurrentVoice.InternalId;
        voiceInfo.Description = CurrentVoice.Description;
        voiceInfo.VoiceLength = (ushort)CurrentVoice.VoiceLength;
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
            _logger.LogWarning($"Adding or updating voice file success to DB: {voiceInfo.FilePath}");
            _logger.LogWarning($"Adding or updating voice ViewModel is: {JsonConvert.SerializeObject(voiceInfo)}");
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

    public void Closed(object parameter)
    {
        if (parameter is Window window)
        {
            _voiceApplicationService.ReloadVoiceList();
            window.Hide();
        }
    }
}