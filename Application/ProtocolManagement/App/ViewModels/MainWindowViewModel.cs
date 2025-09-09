using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.UI.ViewModel;

namespace NV.CT.ProtocolManagement.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly IProtocolApplicationService _protocolApplicationService;


        public MainWindowViewModel(IProtocolApplicationService protocolApplicationService)
        {
            _protocolApplicationService = protocolApplicationService;
        }
    }
}