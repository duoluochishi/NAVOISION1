using Microsoft.Extensions.Logging;
using NV.CT.LogManagement.Models;
using NV.CT.UI.ViewModel;
using Prism.Commands;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace NV.CT.LogManagement.ViewModel
{
    public class LogDetailsWindowViewModel : BaseViewModel
    {
        private const string TYPE_FILE = "File";
        private const string TYPE_LINE = "Line";

        private readonly ILogger<LogDetailsWindowViewModel> _logger;
        private readonly SolidColorBrush foreGround = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));

        private string _windowTitle = string.Empty;
        public string WindowTitle
        {
            get => this._windowTitle;
            set => SetProperty(ref this._windowTitle, value);
        }

        private FlowDocument _contentDetails = new FlowDocument();
        public FlowDocument ContentDetails 
        {
            get
            { 
                return _contentDetails;
            }
            set 
            { 
                SetProperty(ref _contentDetails, value);
            }        
        }

        public LogDetailsWindowViewModel(ILogger<LogDetailsWindowViewModel> logger)
        {
            _logger = logger;

            Commands.Add(LogConstants.COMMAND_CLOSE, new DelegateCommand<object>(Closed, _ => true));
        }

        public void SetContentDetails(LogLineModel logLineModel)
        {
            WindowTitle = this.ConvertLogLevel(logLineModel.LogLevel);

            this.ContentDetails.Blocks.Clear();
            var para = new Paragraph();
            para.Inlines.Add(this.GetLogDetails(logLineModel));
            this.ContentDetails.Blocks.Add(para);
        }

        private string GetLogDetails(LogLineModel logLineModel)
        {
            if (logLineModel is null)
            {
                return string.Empty;
            }

            var detailsString = new StringBuilder();
            detailsString.AppendLine($"{TYPE_FILE}  : {logLineModel.LogFile.FileFullPath}");
            detailsString.AppendLine($"{TYPE_LINE} : {logLineModel.LineNumber}");
            detailsString.AppendLine(logLineModel.Content);
            return detailsString.ToString();
        }

        private void Closed(object parameter)
        {
            if (parameter is Window window)
            {
                window.Hide();
            }
        }

        private string ConvertLogLevel(string logLevel)
        {
            string levelName = LogConstants.COMBOX_OPTION_NONE;
            switch (logLevel)
            {
                case LogConstants.LOG_LEVEL_DBG:
                    levelName = LogLevel.Debug.ToString();
                    break;
                case LogConstants.LOG_LEVEL_TRC:
                    levelName = LogLevel.Trace.ToString();
                    break;
                case LogConstants.LOG_LEVEL_INF:
                    levelName = LogLevel.Information.ToString();
                    break;
                case LogConstants.LOG_LEVEL_WRN:
                    levelName = LogLevel.Warning.ToString();
                    break;
                case LogConstants.LOG_LEVEL_ERR:
                    levelName = LogLevel.Error.ToString();
                    break;
                case LogConstants.LOG_LEVEL_FTL:
                    levelName = LogConstants.LOG_LEVEL_FATAL.ToString();
                    break;

                default: 
                    return string.Empty;

            }

            return levelName;        
        }
    }
}
