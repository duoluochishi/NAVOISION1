using Microsoft.Extensions.Logging;
using NV.CT.UI.ViewModel;

namespace NV.CT.LogManagement.ViewModel
{
    public class LogViewerViewModel : BaseViewModel
    {

        private readonly ILogger<LogViewerViewModel> _logger;

        public LogViewerViewModel(ILogger<LogViewerViewModel> logger)
        {
            _logger = logger;  
        }
    }

}
