using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Extensions;
using NV.CT.LogManagement.Events;
using NV.CT.LogManagement.Helpers;
using NV.CT.LogManagement.Models;
using NV.CT.LogManagement.View.English;
using NV.CT.UI.Controls;
using NV.CT.UI.ViewModel;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NV.CT.LogManagement.ViewModel
{
    public class LogViewerOriginalLocatorViewModel : BaseViewModel
    {
        private readonly ILogger<LogViewerOriginalLocatorViewModel> _logger;
        private LogDetailsWindow? _logDetailsWindow;

        private ObservableCollection<LogLineModel> _logLinesSource = new ObservableCollection<LogLineModel>();
        public ObservableCollection<LogLineModel> LogLinesSource
        {
            get => _logLinesSource;
            set => SetProperty(ref _logLinesSource, value);
        }

        private LogLineModel? _selectedLogLine;
        public LogLineModel? SelectedLogLine
        { 
            get => _selectedLogLine;
            set => SetProperty(ref _selectedLogLine, value);            
        }

        public LogViewerOriginalLocatorViewModel(ILogger<LogViewerOriginalLocatorViewModel> logger)
        {
            _logger = logger;

            Commands.Add(LogConstants.COMMAND_SHOW_DETAILS, new DelegateCommand<LogLineModel>(OnShowDetails));
            Commands.Add(LogConstants.COMMAND_SELECTION_CHANGED, new DelegateCommand<object>(OnSelectionChanged));

            EventAggregator.Instance.GetEvent<SearchRaisedEvent>().Subscribe(OnSearchResult);
            EventAggregator.Instance.GetEvent<LocationJumpedEvent>().Subscribe(OnJumpToSearchResult);
        }

        private void OnJumpToSearchResult(LogLineModel logLineModel)
        {
            LogLinesSource.Clear();

            var logLineList = LoggerHelper.GetLogLinesByFile(logLineModel.LogFile);

            LogLinesSource = logLineList.ToObservableCollection();
            SelectedLogLine = LogLinesSource.FirstOrDefault(i => i.LineNumber == logLineModel.LineNumber && i.OccurredDateTime == logLineModel.OccurredDateTime);
        }

        private void OnShowDetails(LogLineModel logLineModel)
        {
            if (_logDetailsWindow is null)
            {
                _logDetailsWindow = new();
            }

            var logDetailsWindowViewModel = CTS.Global.ServiceProvider.GetRequiredService<LogDetailsWindowViewModel>();
            logDetailsWindowViewModel.SetContentDetails(logLineModel);
            _logDetailsWindow.ShowWindowDialog(false);
        }

        private void OnSearchResult()
        {
            this.LogLinesSource.Clear();
        }

        private void OnSelectionChanged(object element)
        {
            var sourceElement = element as DataGrid;
            if (sourceElement is null || sourceElement.SelectedItem is null) 
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                sourceElement.ScrollIntoView(sourceElement.SelectedItem);
            });

        }

    }
}
