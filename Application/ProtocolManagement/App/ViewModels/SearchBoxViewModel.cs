using NV.CT.ProtocolManagement.ApplicationService.Contract;
using NV.CT.UI.ViewModel;
using Prism.Commands;

namespace NV.CT.ProtocolManagement.ViewModels
{
    public class SearchBoxViewModel : BaseViewModel
    {
        private readonly IProtocolApplicationService _protocolApplicationService;


        public SearchBoxViewModel(IProtocolApplicationService protocolApplicationService)
        {
            _protocolApplicationService = protocolApplicationService;
            ColletCommand();
        }


        private void ColletCommand()
        {
            Commands.Add("SearchCommand", new DelegateCommand<string>(Search));
        }

        private void Search(string searchTXT)
        {
            _protocolApplicationService.Search(searchTXT);
        }
    }
}
