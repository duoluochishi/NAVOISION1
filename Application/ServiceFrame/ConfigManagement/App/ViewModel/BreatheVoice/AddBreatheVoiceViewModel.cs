using NAudio.Wave;
using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.ConfigManagement.Extensions;
using NV.MPS.Environment;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NV.CT.ConfigManagement.ViewModel
{
    public class AddBreatheVoiceViewModel : BaseViewModel
    {
        private readonly NAudioRecorder _recorder;
        private IBreatheVoiceApplicationService _breatheVoiceApplicationService;
        private IDialogService _dialogService;
        private ILogger<BreatheVoiceListViewModel> _logger;
        private const int MAX_VOICE_LENGTH = 10;

        private ObservableCollection<KeyValuePair<string, string>> _languagelist = new ObservableCollection<KeyValuePair<string, string>>();
        public ObservableCollection<KeyValuePair<string, string>> LanguageList
        {
            get => _languagelist;
            set => SetProperty(ref _languagelist, value);
        }

        private string _recordFilePath = string.Empty;
        public string RecordFilePath
        {
            get => _recordFilePath;
            set
            {
                _recordFilePath = value;
            }
        }

        private RecordingStatus _recordStatus;
        public RecordingStatus RecordStatus
        {
            get => _recordStatus;
            set
            {
                SetProperty(ref _recordStatus, value);
            }
        }

        private BaseBreatheVoiceViewModel _currentVoice = new BaseBreatheVoiceViewModel();
        public BaseBreatheVoiceViewModel CurrentVoice
        {
            get => _currentVoice;
            set => SetProperty(ref _currentVoice, value);
        }

        public OperationType OperationType { get; set; } = OperationType.Add;

        public int GroupId { get; set; }
        public bool ImportWavSuccessful { get; private set; }

        public AddBreatheVoiceViewModel(IBreatheVoiceApplicationService breatheVoiceApplicationService, IDialogService dialogService,
            ILogger<BreatheVoiceListViewModel> logger)
        {
            _breatheVoiceApplicationService = breatheVoiceApplicationService;
            _dialogService = dialogService;
            _logger = logger;
            InitLanguageList();
            _breatheVoiceApplicationService.BreatheVoiceChanged += OnBreatheVoiceChanged;
            Commands.Add("RecordCommand", new DelegateCommand(RecordCommand));
            Commands.Add("ImportWavCommand", new DelegateCommand(ImportWavCommand, () => RecordStatus != RecordingStatus.Recording));
            Commands.Add("SaveCommand", new DelegateCommand<object>(Save, _ => RecordStatus != RecordingStatus.Recording));
            Commands.Add("CloseCommand", new DelegateCommand<object>(Close, _ => RecordStatus != RecordingStatus.Recording));

            _recorder = new NAudioRecorder();
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

        private void OnBreatheVoiceChanged(object? sender, EventArgs<(OperationType operation, int groupId, BreatheVoiceInfo breatheVoice)> e)
        {
            if (e?.Data is null)
                return;
            OperationType = e.Data.operation;
            GroupId = e.Data.groupId;
            CurrentVoice = new BaseBreatheVoiceViewModel();
            CurrentVoice.Id = e.Data.breatheVoice.Id;
            CurrentVoice.Name = e.Data.breatheVoice.Name;
            CurrentVoice.FilePath = e.Data.breatheVoice.FilePath;
            CurrentVoice.Time = e.Data.breatheVoice.Time;
            CurrentVoice.Language = e.Data.breatheVoice.Language;
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
                        var breatheVoiceRoot = RuntimeConfig.Console.MCSBreatheVoices.Path;
                        if (!Directory.Exists(Path.Combine(breatheVoiceRoot, "Custom")))
                        {
                            Directory.CreateDirectory(Path.Combine(breatheVoiceRoot, "Custom"));
                        }

                        RecordFilePath = Path.Combine(breatheVoiceRoot, "Custom", $"{CurrentVoice.Id}.wav");
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
                    CurrentVoice.FilePath = GetRelativePath(RecordFilePath);
                    break;
                default:
                    break;
            }
        }

        private string GetRelativePath(string filePath)
        {
            var breatheVoiceRoot = RuntimeConfig.Console.MCSBreatheVoices.Path;

            if (RecordFilePath.StartsWith(breatheVoiceRoot))
            {
                return RecordFilePath.Substring(breatheVoiceRoot.Length + 1);
            }
            else
            {
                _logger.LogWarning("Record file path is not under breathe voice root.");
                return RecordFilePath;
            }
        }

        private void Recorder_DataAvailable(double recordTime)
        {
            CurrentVoice.Time = recordTime;
            if (recordTime >= MAX_VOICE_LENGTH)
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
                    var voiceRoot = RuntimeConfig.Console.MCSBreatheVoices.Path;
                    if (!Directory.Exists(Path.Combine(voiceRoot, "Custom")))
                    {
                        Directory.CreateDirectory(Path.Combine(voiceRoot, "Custom"));
                    }
                    var newWavPath = Path.Combine(voiceRoot, "Custom", $"{CurrentVoice.Id}.wav");
                    //如果已存在同名语音文件，则覆盖
                    File.Copy(pathName, newWavPath, true);

                    //语音时间长度获取
                    RecordFilePath = newWavPath;
                    using (var inputStream = new AudioFileReader(newWavPath))
                    {
                        CurrentVoice.Time = Math.Round(inputStream.TotalTime.TotalSeconds, 0);
                    }

                    ImportWavSuccessful = CheckVoiceFileLength();
                    if (ImportWavSuccessful)
                    {
                        CurrentVoice.FilePath = GetRelativePath(RecordFilePath);
                        message = $"Import successful: {RecordFilePath}";
                    }
                    else
                    {
                        try
                        {
                            File.Delete(RecordFilePath);
                        }
                        catch (Exception ex)
                        {
                            message = $"Import failed: {ex.Message}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = $"Import failed: {ex.Message}";
            }
            if (!string.IsNullOrEmpty(message))
            {
                _dialogService.ShowDialog(false, MessageLeveles.Info, "Tip", message, callback => { }, ConsoleSystemHelper.WindowHwnd);
            }
        }

        public void Save(object parameter)
        {
            if (parameter is not Window window)
            {
                return;
            }

            if (CheckIsRecording()) return;

            if (OperationType == OperationType.Add && !ImportWavSuccessful && RecordStatus != RecordingStatus.None)
            {
                string msg = "Please import the voice file first!";
                _logger.LogWarning(msg);
                _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", msg,
                    arg => { }, ConsoleSystemHelper.WindowHwnd);
                return;
            }

            if (!CheckFormEmpty() || CheckNameRepeat() || !CheckNumAndEnChForm() || !CheckVoiceFile() || !CheckVoiceFileLength())
            {
                return;
            }

            var breatheVoice = new BreatheVoiceInfo()
            {
                Id = CurrentVoice.Id,
                Name = CurrentVoice.Name,
                FilePath = CurrentVoice.FilePath,
                Time = (int)CurrentVoice.Time,
                Language = CurrentVoice.Language
            };

            _breatheVoiceApplicationService.SetAddEditResult(breatheVoice);
            window.Hide();
        }

        public void Close(object parameter)
        {
            if (parameter is not Window window)
            {
                return;
            }

            if (CheckIsRecording()) return;

            // 清理没有保存的录音文件
            // case 1：语音录制或者导入成功，用户退出界面
            // case 2：语音录制或者导入失败，存在语音文件，用户退出界面
            // case 3：用户点击保存按钮，但是没有保存成功，用户退出界面
            if (OperationType == OperationType.Add && !string.IsNullOrEmpty(RecordFilePath))
            {
                _breatheVoiceApplicationService.DeleteVoiceFile(RecordFilePath);
            }

            window.Hide();
        }

        #region Conditions
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
            var group = _breatheVoiceApplicationService.GetBreatheVoiceGroupById(GroupId);
            if (group != null)
            {
                switch (OperationType)
                {
                    case OperationType.Edit:
                        flag = group.BreatheVoices.Any(t => t.Id != CurrentVoice.Id && t.Name == CurrentVoice.Name);
                        break;
                    default: break;
                }
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
            bool isExist = string.IsNullOrEmpty(CurrentVoice.FilePath) ? false : File.Exists(Path.Combine(RuntimeConfig.Console.MCSBreatheVoices.Path, CurrentVoice.FilePath));
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
            if (CurrentVoice.Time > MAX_VOICE_LENGTH)
            {
                message = $"The length of voice cannot exceed {MAX_VOICE_LENGTH} seconds!";
                _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                        arg => { }, ConsoleSystemHelper.WindowHwnd);
                return false;
            }

            return result;
        } 
        #endregion
    }
}
