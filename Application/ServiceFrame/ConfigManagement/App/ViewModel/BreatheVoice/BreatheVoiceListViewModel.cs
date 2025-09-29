using NV.CT.ConfigManagement.ApplicationService.Contract;
using NV.CT.ConfigManagement.View;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.CT.UI.Controls;
using NV.MPS.UI.Dialog.Service;

namespace NV.CT.ConfigManagement.ViewModel
{
    public class BreatheVoiceListViewModel : BaseViewModel
    {
        private IDialogService _dialogService;
        private IBreatheVoiceApplicationService _breatheVoiceApplicationService;
        private ILogger<BreatheVoiceListViewModel> _logger;

        private ObservableCollection<BaseBreatheVoiceViewModel> _breatheVoicesGroup;
        public ObservableCollection<BaseBreatheVoiceViewModel> BreatheVoicesGroup
        {
            get => _breatheVoicesGroup;
            set => SetProperty(ref _breatheVoicesGroup, value);
        }

        private BaseBreatheVoiceViewModel _selectedVoiceGroup;
        public BaseBreatheVoiceViewModel SelectedVoiceGroup
        {
            get => _selectedVoiceGroup;
            set
            {
                if (SetProperty(ref _selectedVoiceGroup, value) && value != null)
                {
                    IsFactory = !value.IsFactory;
                }
            }
        }

        private bool _isFactory;
        public bool IsFactory
        {
            get => _isFactory;
            set => SetProperty(ref _isFactory, value);
        }

        public BreatheVoiceListViewModel(IBreatheVoiceApplicationService breatheVoiceApplicationService,
            IDialogService dialogService,
            ILogger<BreatheVoiceListViewModel> logger)
        {
            _dialogService = dialogService;
            _breatheVoiceApplicationService = breatheVoiceApplicationService;
            _logger = logger;
            Commands.Add("VoiceAddCommand", new DelegateCommand(AddCommand));
            Commands.Add("VoiceEditCommand", new DelegateCommand(EditCommand));
            Commands.Add("VoiceDeleteCommand", new DelegateCommand(DeleteCommand));
            Commands.Add("VoicePlayCommand", new DelegateCommand<object>(SetPlaylistCommand));
            LoadBreatheVoices();
        }

        void LoadBreatheVoices()
        {
            var temp = new ObservableCollection<BaseBreatheVoiceViewModel>();
            _breatheVoiceApplicationService.GetAll().ForEach(x =>
            {
                var item = new BaseBreatheVoiceViewModel();
                item.Id = x.Id;
                item.Name = x.Name;
                item.Description = x.Description;
                item.IsDefault = x.IsDefault;
                item.IsFactory = x.IsFactory;
                item.Count = x.BreatheVoices.Count;
                temp.Add(item);
            });
            BreatheVoicesGroup = temp;
        }

        private void AddCommand()
        {
            var group = new BreatheVoiceGroup();
            group.Id = _breatheVoiceApplicationService.GetLatestGroupId() + 1;
            ShowWindow(OperationType.Add, group);
        }

        private void EditCommand()
        {
            if (SelectedVoiceGroup is null)
            {
                _dialogService.ShowDialog(false, MPS.UI.Dialog.Enum.MessageLeveles.Info, "Info", "Please select a voice group!", null, ConsoleSystemHelper.WindowHwnd);
                return;
            }

            var group = _breatheVoiceApplicationService.GetBreatheVoiceGroupById(SelectedVoiceGroup.Id);
            if (group == null)
            {
                _dialogService.ShowDialog(false, MPS.UI.Dialog.Enum.MessageLeveles.Info, "Info", "Please select a voice group!", null, ConsoleSystemHelper.WindowHwnd);
                return;
            }

            ShowWindow(OperationType.Edit, group);
        }

        void ShowWindow(OperationType operation, BreatheVoiceGroup breatheVoiceGroup)
        {
            var window = CTS.Global.ServiceProvider?.GetRequiredService<BreatheVoiceWindow>();
            _breatheVoiceApplicationService.SetBreatheVoiceGroup(operation, breatheVoiceGroup);
            window?.ShowPopWindowDialog();
            LoadBreatheVoices();
            // 若是default语音，则更新playlist
            if (breatheVoiceGroup.IsDefault)
            {
                var defaultGroup = BreatheVoicesGroup.Where(x => x.Id == breatheVoiceGroup.Id).First();
                if (defaultGroup != null)
                {
                    SetPlaylistCommand(defaultGroup);
                }
            }
        }

        /// <summary>
        /// 删除默认语音，需要清除auxboard语音列表
        /// </summary>
        private void DeleteCommand()
        {
            if (SelectedVoiceGroup != null && !SelectedVoiceGroup.IsFactory)
            {
                string msg = string.Empty;
                if (SelectedVoiceGroup.IsDefault)
                    msg = "Are you sure you want to delete the default voice group? If so, you'll need to set a new default voice later!";
                else
                    msg = "Are you sure you want to delete the voice group?";
                _dialogService.ShowDialog(true, MPS.UI.Dialog.Enum.MessageLeveles.Info, "Info", msg, res =>
                {
                    if (res.Result == MPS.UI.Dialog.Enum.ButtonResult.OK)
                    {
                        // 1，default情况下清除呼吸语音列表                       
                        // 2，删除远端音频文件
                        // 3，清除保存的数据和语音文件
                        if (!_breatheVoiceApplicationService.DeleteBreatheVoiceGroup(SelectedVoiceGroup.Id))
                        {
                            _dialogService.ShowDialog(true, MPS.UI.Dialog.Enum.MessageLeveles.Info, "Info",
                                $"Failed to deleted the voice group {SelectedVoiceGroup.Name}! Please restart the system and try again!",
                                null, ConsoleSystemHelper.WindowHwnd);
                            return;
                        }
                        LoadBreatheVoices();
                    }
                }, ConsoleSystemHelper.WindowHwnd);
            }
        }

        /// <summary>
        /// ToDo: Send the selected list to the auxboard
        /// </summary>
        private void SetPlaylistCommand(object parameter)
        {
            // Condition check
            // Set playlist
            // Compare the local voice files with the files in the auxboard
            // Compare the playlist with the auxboard
            // Update xml
            // If failed, show error message
            if (parameter is BaseBreatheVoiceViewModel groupViewModel)
            {
                SelectedVoiceGroup = groupViewModel;
            }
            else if (parameter is not DataGridRow)
            {
                return;
            }

            if (SelectedVoiceGroup is null)
            {
                _dialogService.ShowDialog(false, MPS.UI.Dialog.Enum.MessageLeveles.Info, "Info", "Please select a voice group!", null, ConsoleSystemHelper.WindowHwnd);
                return;
            }

            if (_breatheVoiceApplicationService.SetDefaultVoiceGroup(SelectedVoiceGroup.Id))
            {
                SelectedVoiceGroup.IsDefault = true;
            }
            else
            {
                _dialogService.ShowDialog(false, MPS.UI.Dialog.Enum.MessageLeveles.Info, "Info", "Failed to set play list, Please try again!", null, ConsoleSystemHelper.WindowHwnd);
                return;
            }
            LoadBreatheVoices();
        }
    }
}
