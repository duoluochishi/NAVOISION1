using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.ConfigManagement.View;
using NV.CT.UI.Controls;
using NV.MPS.UI.Dialog.Enum;
using NV.MPS.UI.Dialog.Service;
using System.Collections.Generic;

namespace NV.CT.ConfigManagement.ViewModel
{
    public class BreatheVoiceViewModel : BaseViewModel
    {
        private IBreatheVoiceApplicationService _breatheVoiceApplicationService;
        private IDialogService _dialogService;
        private ILogger<BreatheVoiceListViewModel> _logger;

        private const int MAX_VOICE_COUNT = 5;

        private BaseBreatheVoiceViewModel _currentGroup = new BaseBreatheVoiceViewModel();
        public BaseBreatheVoiceViewModel CurrentGroup
        {
            get => _currentGroup;
            set => SetProperty(ref _currentGroup, value);
        }

        private BaseBreatheVoiceViewModel _selectedVoice;
        public BaseBreatheVoiceViewModel SelectedVoice
        {
            get => _selectedVoice;
            set => SetProperty(ref _selectedVoice, value);
        }

        public OperationType OperationType { get; set; } = OperationType.Add;

        public BreatheVoiceViewModel(IBreatheVoiceApplicationService breatheVoiceApplicationService, 
            IDialogService dialogService,
            ILogger<BreatheVoiceListViewModel> logger)
        {
            _breatheVoiceApplicationService = breatheVoiceApplicationService;
            _dialogService = dialogService;
            _logger = logger;
            breatheVoiceApplicationService.BreatheVoiceGroupChanged += OnBreatheVoiceGroupChanged;
            breatheVoiceApplicationService.AddEditResultChanged += OnAddEditResultChanged;
            Commands.Add("VoiceAddCommand", new DelegateCommand(AddCommand, () => CanAddMoreVoices()));
            Commands.Add("VoiceDeleteCommand", new DelegateCommand(DeleteCommand, () => CurrentGroup?.BreatheVoices.Count > 0));
            Commands.Add("VoiceSaveCommand", new DelegateCommand<object>(SaveCommand));
            Commands.Add("VoiceCancelCommand", new DelegateCommand<object>(CloseCommand));
        }

        private void OnBreatheVoiceGroupChanged(object? sender, EventArgs<(OperationType operation, BreatheVoiceGroup breatheVoiceGroup)> e)
        {
            if (e is null)
            {
                return;
            }
            OperationType = e.Data.operation;
            SetBreatheVoiceGroup(e.Data.breatheVoiceGroup);
        }

        private void SetBreatheVoiceGroup(BreatheVoiceGroup breatheVoiceGroup)
        {
            CurrentGroup = new BaseBreatheVoiceViewModel();
            CurrentGroup.Id = breatheVoiceGroup.Id;
            CurrentGroup.Name = breatheVoiceGroup.Name;
            CurrentGroup.Description = breatheVoiceGroup.Description;
            CurrentGroup.IsFactory = breatheVoiceGroup.IsFactory;
            CurrentGroup.IsDefault = breatheVoiceGroup.IsDefault;
            if (breatheVoiceGroup.BreatheVoices != null && breatheVoiceGroup.BreatheVoices.Count > 0)
            {
                var temp = new ObservableCollection<BaseBreatheVoiceViewModel>();
                breatheVoiceGroup.BreatheVoices.ForEach(x =>
                {
                    var item = new BaseBreatheVoiceViewModel();
                    item.Id = x.Id;
                    item.Name = x.Name;
                    item.FilePath = x.FilePath;
                    item.Time = x.Time;
                    item.Language = x.Language;
                    temp.Add(item);
                });
                CurrentGroup.BreatheVoices = temp;
            }
        }

        private void OnAddEditResultChanged(object? sender, EventArgs<BreatheVoiceInfo> e)
        {
            if (e?.Data != null)
            {
                var voice = CurrentGroup.BreatheVoices.FirstOrDefault(x => x.Id == e.Data.Id)
                            ?? new BaseBreatheVoiceViewModel();

                OperationType operation = (voice.Id != 0 && voice.Id == e.Data.Id) ? OperationType.Edit : OperationType.Add;

                voice.Id = e.Data.Id;
                voice.Name = e.Data.Name;
                voice.FilePath = e.Data.FilePath;
                voice.Time = e.Data.Time;
                voice.Language = e.Data.Language;

                if (operation == OperationType.Add)
                {
                    CurrentGroup.BreatheVoices.Add(voice);
                }
            }
        }

        private void AddCommand()
        {
            if (!CanAddMoreVoices())
            {
                _dialogService.ShowDialog(false, MessageLeveles.Info, "Info", "Maximum number of voices reached!",
                    arg => { }, ConsoleSystemHelper.WindowHwnd);
                return;
            }

            if (CurrentGroup.Id < 2)
            {
                _dialogService.ShowDialog(false, MessageLeveles.Info, "Info", "Factory voice group cannot be edited!",
                    arg => { }, ConsoleSystemHelper.WindowHwnd);
                return;
            }

            var voice = new BreatheVoiceInfo();
            // id与voice filename一致， 其中1 - 20 预留给factory语音
            // voice id = Groupid * 10 + index eg. groupid = 2, index=1 => voiceId = 21
            // Range eg:21 - 30 共10个
            int begin = CurrentGroup.Id * 10 + 1;
            int end = CurrentGroup.Id * 10 + 10;
            for (int i = begin; i <= end; i++)
            {
                if (!CurrentGroup.BreatheVoices.Any(x => x.Id == i))
                {
                    voice.Id = i;
                    break;
                }
            }

            if (voice.Id > 0)
                ShowWindow(OperationType.Add, voice);
        }

        private void ShowWindow(OperationType operation, BreatheVoiceInfo voice)
        {
            var window = CTS.Global.ServiceProvider?.GetRequiredService<AddBreatheVoiceWindow>();
            _breatheVoiceApplicationService.SetBreatheVoice(operation, CurrentGroup.Id, voice);
            window?.ShowPopWindowDialog();
        }

        private void DeleteCommand()
        {
            if (SelectedVoice == null)
            {
                _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info"
                    , "Please select a voice from the list! ", arg => { }, ConsoleSystemHelper.WindowHwnd);
                return;
            }

            _dialogService.ShowDialog(true, MessageLeveles.Info, "Info", $"Do you want to delete the voice {SelectedVoice.Name}?", res =>
            {
                if (res.Result == ButtonResult.OK)
                {
                    CurrentGroup.BreatheVoices.Remove(SelectedVoice);
                }
            }, ConsoleSystemHelper.WindowHwnd);
        }

        /// <summary>
        /// 因为呼吸语音的添加总是一组进行的，所以这里仅做保存到xml的和上传语音文件的逻辑，
        /// 往auxboard中发送列表的逻辑根据isdefault标识来处理。
        /// </summary>
        /// <param name="parameter"></param>
        private void SaveCommand(object parameter)
        {
            // Conditions check and then save into xml
            if (parameter is not Window window)
            {
                return;
            }

            if (!CheckFormEmpty() || CheckNameRepeat() || !CheckNumAndEnChForm() || !CheckVoices())
            {
                return;
            }

            if (CurrentGroup == null || CurrentGroup.IsFactory)
            {
                return;
            }

            var group = new BreatheVoiceGroup()
            {
                Id = CurrentGroup.Id,
                Name = CurrentGroup.Name,
                Description = CurrentGroup.Description,
                IsDefault = CurrentGroup.IsDefault,
                IsFactory = CurrentGroup.IsFactory
            };

            if (CurrentGroup.BreatheVoices != null && CurrentGroup.BreatheVoices.Count > 0)
            {
                CurrentGroup.BreatheVoices.ForEach(x =>
                {
                    var item = new BreatheVoiceInfo();
                    item.Id = x.Id;
                    item.Name = x.Name;
                    item.FilePath = x.FilePath;
                    item.Time = (int)x.Time;
                    item.Language = x.Language;
                    group.BreatheVoices.Add(item);
                });
            }
            // 上传音频文件到Auxboard
            if (_breatheVoiceApplicationService.AddOrUpdateAudioFile(group.BreatheVoices))
            {
                // 更新本地配置
                if (OperationType == OperationType.Add)
                {
                    _breatheVoiceApplicationService.AddBreatheVoiceGroup(group);
                }
                else
                {
                    _breatheVoiceApplicationService.UpdateBreatheVoiceGroup(group);
                }
            }
            else
            {
                _dialogService.ShowDialog(false, MPS.UI.Dialog.Enum.MessageLeveles.Info, "Info", "Updating voice file to the auxboard failed!", null, ConsoleSystemHelper.WindowHwnd);
                return;
            }

            window.Hide();
        }

        private void CloseCommand(object parameter)
        {
            if (parameter is not Window window)
            {
                return;
            }
            // 清理没有保存的录音文件，前提条件：用户保存后界面自动关闭
            // case 1：语音添加成功，用户退出界面没有保存
            // - Add模式下，清理整个group没有保存的录音文件
            // - Edit模式下，清理某一个voice没有保存的录音文件
            // case 2：语音添加失败，用户退出界面
            // - Add模式下，清理整个group没有保存的录音文件
            // - Edit模式下，清理某一个voice没有保存的录音文件
            if (OperationType == OperationType.Add)
            {
                foreach(var voice in CurrentGroup.BreatheVoices)
                {
                    if (!string.IsNullOrEmpty(voice.FilePath))
                    {
                        _breatheVoiceApplicationService.DeleteVoiceFile(voice.FilePath);
                    }
                }
            }
            else
            {
                var realVoices = _breatheVoiceApplicationService.GetBreatheVoiceGroupById(CurrentGroup.Id)?.BreatheVoices;
                HashSet<string> realFilePaths;
                if (realVoices != null)
                {
                    realFilePaths = new HashSet<string>(realVoices.Select(x => x.FilePath));
                }
                else
                    realFilePaths = new HashSet<string>();

                HashSet<string> currentFilePaths = new HashSet<string>(CurrentGroup.BreatheVoices.Select(x => x.FilePath));

                var diffFilePaths = currentFilePaths.Except(realFilePaths).ToList();
                foreach (var filePath in diffFilePaths)
                {
                    _breatheVoiceApplicationService.DeleteVoiceFile(filePath);
                }
            }

            window.Hide();
        }

        #region Conditions
        private bool CanAddMoreVoices()
        {
            return CurrentGroup?.BreatheVoices.Count < MAX_VOICE_COUNT;
        }
        private bool CheckFormEmpty()
        {
            bool flag = true;
            StringBuilder sb = new StringBuilder();
            string message = "{0} can't be empty!";

            if (string.IsNullOrEmpty(CurrentGroup.Name))
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
            var groups = _breatheVoiceApplicationService.GetAll();
            switch (OperationType)
            {
                case OperationType.Edit:
                    flag = groups.Any(t => t.Id != CurrentGroup.Id && t.Name == CurrentGroup.Name);
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
            if (VerificationExtension.IsSpecialCharacters(CurrentGroup.Name))
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

        private bool CheckVoices()
        {
            bool flag = true;
            string message = "";
            if (CurrentGroup.BreatheVoices.Count == 0)
            {
                flag = false;
                message += $"Voice group cannot be empty!";
            }
            if (!flag)
            {
                _dialogService?.ShowDialog(false, MessageLeveles.Info, "Info", message,
                    arg => { }, ConsoleSystemHelper.WindowHwnd);
            }
            return flag;
        }

        #endregion
    }
}
