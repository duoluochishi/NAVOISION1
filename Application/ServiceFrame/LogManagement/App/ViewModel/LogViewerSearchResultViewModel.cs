using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Extensions;
using NV.CT.LogManagement.Events;
using NV.CT.LogManagement.Helpers;
using NV.CT.LogManagement.Models;
using NV.CT.UI.ViewModel;
using NV.MPS.Environment;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.LogManagement.ViewModel
{
    public class LogViewerSearchResultViewModel : BaseViewModel
    {
        private readonly ILogger<LogViewerSearchResultViewModel> _logger;

        private ObservableCollection<LogLineModel> _logLinesSource = new ObservableCollection<LogLineModel>();
        public ObservableCollection<LogLineModel> LogLinesSource
        { 
            get => _logLinesSource;
            set => SetProperty(ref _logLinesSource, value);       
        }
        
        public LogViewerSearchResultViewModel(ILogger<LogViewerSearchResultViewModel> logger)
        {
            _logger = logger;

            Commands.Add(LogConstants.COMMAND_JUMP_TO_SEARCH_RESULT, new DelegateCommand<LogLineModel>(OnJumpToSearchResult));

            EventAggregator.Instance.GetEvent<SearchRaisedEvent>().Subscribe(OnSearchResult);
            this.OnSearchResult();
        }

        private void OnSearchResult()
        {
            LogLinesSource.Clear();

            //目前每次都查询都重新读取Log文件夹下的文件，性能上可以再优化和完善。
            //后续会再补充文件监控机制，监控到查询时间范围内的文件有变化了再重新读取，否则直接查询上次的缓存结果，以提高查询性能。
            var searchCriteriaViewModel = ServiceFramework.Global.Instance.ServiceProvider.GetRequiredService<LogViewerSearchCriteriaViewModel>();

            //直接通过时间范围去筛选log文件，避免扫描无关的文件以缩短响应时间
            var beginDateTime = new DateTime(searchCriteriaViewModel.BeginDate.Year, searchCriteriaViewModel.BeginDate.Month, searchCriteriaViewModel.BeginDate.Day);
            var endDateTime = (new DateTime(searchCriteriaViewModel.EndDate.Year, searchCriteriaViewModel.EndDate.Month, searchCriteriaViewModel.EndDate.Day)).AddDays(1);

            var source = new List<LogLineModel>();
            var foundLogFiles = LoggerHelper.IdentifyLogFiles(RuntimeConfig.Console.MCSLog.Path, beginDateTime, endDateTime);

            foreach (var logFile in foundLogFiles)
            {
                try
                {
                    var logLineList = LoggerHelper.GetLogLinesByFile(logFile);
                    source.AddRange(logLineList);
                }
                catch (Exception ex)
                {
                    this._logger.LogWarning($"Failed to parse log file:{logFile.FileFullPath}, the exception is:{ex.Message}");
                    continue;
                }

            }

            var linqSource = from loglineModel in source 
                             select loglineModel;
            //apply criteria conditions
            //filtered by log level
            var logLevelCriteria = searchCriteriaViewModel.LogLevelList.Where(l => l.DisplayText != LogConstants.COMBOX_OPTION_ALL && l.IsSelected).Select(l => l.ValueText).ToArray();
            if (logLevelCriteria is not null && logLevelCriteria.Length > 0)
            {
                linqSource = linqSource.Where(l => logLevelCriteria.Contains(l.LogLevel));
            }

            //filtered by module name
            var moduleCriteria = searchCriteriaViewModel.ModuleList.Where(l => l.DisplayText != LogConstants.COMBOX_OPTION_ALL && l.IsSelected).Select(l => l.ValueText).ToArray();
            if (moduleCriteria is not null && moduleCriteria.Length > 0)
            {
                linqSource = linqSource.Where(l => moduleCriteria.Contains(l.ModuleName));
            }

            //filtered by Including words. Example  keyA:keyB means keyA || keyB
            if (!string.IsNullOrWhiteSpace(searchCriteriaViewModel.IncludingWords))
            {
                var includingWords = searchCriteriaViewModel.IncludingWords.Split(";").Where(w => !string.IsNullOrWhiteSpace(w));
                linqSource = linqSource.Where(l => includingWords.Any(w => l.Content.IndexOf(w, StringComparison.OrdinalIgnoreCase)>= 0));        
            }

            //filtered by Excluding words
            if (!string.IsNullOrWhiteSpace(searchCriteriaViewModel.ExcludingWords))
            {
                var excludingWords = searchCriteriaViewModel.ExcludingWords.Split(";").Where(w => !string.IsNullOrWhiteSpace(w));
                foreach (var excludingWord in excludingWords)
                {
                    linqSource = linqSource.Where(l => l.Content.IndexOf(excludingWord, StringComparison.OrdinalIgnoreCase)<0);
                }
            }
            
            LogLinesSource = linqSource.ToList().ToObservableCollection();
            EventAggregator.Instance.GetEvent<SearchFinishedEvent>().Publish(source);
        }

        private void OnJumpToSearchResult(LogLineModel item)
        {
            EventAggregator.Instance.GetEvent<LocationJumpedEvent>().Publish(item);        
        }

    }
}
