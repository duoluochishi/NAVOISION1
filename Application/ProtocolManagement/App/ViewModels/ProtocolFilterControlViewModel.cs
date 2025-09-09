using NV.CT.CTS;
using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.UI.ViewModel;
using Prism.Commands;

namespace NV.CT.ProtocolManagement.ViewModels
{
    public class ProtocolFilterControlViewModel : BaseViewModel
    {
        private readonly IProtocolApplicationService _protocolApplicationService;

        private string _currentBodyPart = string.Empty;
        public string CurrentBodyPart
        {
            get => _currentBodyPart;
            set
            {
                SetProperty(ref _currentBodyPart, value);
            }
        }
        private bool _isAdult = true;
        public bool IsAdult
        {
            get => _isAdult;
            set
            {
                SetProperty(ref _isAdult, value);
                FilterProtocol();
            }
        }
        private bool _isChild = true;
        public bool IsChild
        {
            get => _isChild;
            set
            {
                SetProperty(ref _isChild, value);
                FilterProtocol();
            }
        }
        private bool _isAxial = true;
        public bool IsAxial
        {
            get => _isAxial;
            set
            {
                SetProperty(ref _isAxial, value);
                FilterProtocol();
            }
        }
        private bool _isSpiral = true;
        public bool IsSpiral
        {
            get => _isSpiral;
            set
            {
                SetProperty(ref _isSpiral, value);
                FilterProtocol();
            }
        }
        private bool _isFactory = true;
        public bool IsFactory
        {
            get => _isFactory;
            set
            {
                SetProperty(ref _isFactory, value);
                FilterProtocol();
            }
        }
        private bool _isCustom = true;
        public bool IsCustom
        {
            get => _isCustom;
            set
            {
                SetProperty(ref _isCustom, value);
                FilterProtocol();
            }
        }

        public ProtocolFilterControlViewModel(IProtocolApplicationService protocolApplicationService)
        {
            _protocolApplicationService = protocolApplicationService;
            _currentBodyPart = CTS.Enums.BodyPart.Head.ToString();

            //_protocolApplicationService.SelectBodyPartForProtocolChanged += OnSelectBodyPartForProtocolChanged;
            Commands.Add(Constants.COMMAND_SWITCH_PROTOCOL_FILTER, new DelegateCommand<string>(SwitchProtocol));
            Commands.Add(Constants.COMMAND_SWITCH_EMERGENCY_PROTOCOL_FILTER, new DelegateCommand(SwitchEmergencyProtocol));
        }

        private void SwitchEmergencyProtocol()
        {
            _protocolApplicationService.SwitchEmergencyProtocol();
        }

        private void SwitchProtocol(string bodyPart)
        {
            CurrentBodyPart = bodyPart;
            //_protocolApplicationService.ChangeBodyPartForProtocol(bodyPart);
            _protocolApplicationService.FilterProtocolByCondition((bodyPart,IsAdult,IsChild,IsAxial,IsSpiral,IsFactory,IsCustom));
        }

        public void FilterProtocol()
        {
            _protocolApplicationService.FilterProtocolByCondition((CurrentBodyPart, IsAdult, IsChild, IsAxial, IsSpiral, IsFactory, IsCustom));
        }
    }
}
