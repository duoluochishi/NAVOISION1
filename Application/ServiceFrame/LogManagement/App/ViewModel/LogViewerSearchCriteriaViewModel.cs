using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NV.CT.AppService.Contract;
using NV.CT.LogManagement.Events;
using NV.CT.LogManagement.Models;
using NV.CT.UI.ViewModel;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EventAggregator = NV.CT.LogManagement.Events.EventAggregator;

namespace NV.CT.LogManagement.ViewModel
{
    public class LogViewerSearchCriteriaViewModel : BaseViewModel
    {
        private const string SELECTED_PROPERTY = "IsSelected";

        // TODO: 把可多选下拉框封装成可复用的公共控件  --an.hu
        private readonly ILogger<LogViewerSearchCriteriaViewModel> _logger;
        private readonly List<LogLevelSettings> _logLevelConfigs;
        private readonly List<ModuleNameConfig> _moduleNameConfig;        
        private volatile bool _isLogLdevelSelectedByAll = false;
        private volatile bool _isLogLdevelSelectedByOption = false;
        private volatile bool _isModuleSelectedByAll = false;
        private volatile bool _isModuleSelectedByOption = false;

        private DateTime _beginDate;
        public DateTime BeginDate
        {
            get => _beginDate;
            set => SetProperty(ref _beginDate, value);
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private bool _isSearchCommandEnabled = true;
        public bool IsSearchCommandEnabled
        {
            get => _isSearchCommandEnabled;
            set => SetProperty(ref _isSearchCommandEnabled, value);
        }     

        private string _selectedLogLevelText = string.Empty;
        public string SelectedLogLevelText
        {
            get => _selectedLogLevelText;
            set => SetProperty(ref _selectedLogLevelText, value);
        }

        private ObservableCollection<LogLevelModel> _logLevelList = new ObservableCollection<LogLevelModel>();
        public ObservableCollection<LogLevelModel> LogLevelList
        {
            get => _logLevelList;
            set => SetProperty(ref _logLevelList, value);
        }

        private string _selectedModuleText = string.Empty;
        public string SelectedModuleText
        {
            get => _selectedModuleText;
            set => SetProperty(ref _selectedModuleText, value);
        }
        
        private ObservableCollection<ModuleModel> _moduleList = new ObservableCollection<ModuleModel>();
        public ObservableCollection<ModuleModel> ModuleList
        {
            get => _moduleList;
            set => SetProperty(ref _moduleList, value);
        }

        private string _includingWords = string.Empty;
        public string IncludingWords
        {
            get => _includingWords;
            set => SetProperty(ref _includingWords, value);
        }

        private string _excludingWords = string.Empty;
        public string ExcludingWords
        {
            get => _excludingWords;
            set => SetProperty(ref _excludingWords, value);
        }

        public LogViewerSearchCriteriaViewModel(ILogger<LogViewerSearchCriteriaViewModel> logger,
                                                IOptions<List<LogLevelSettings>> logLevelConfigs,
                                                IOptions<List<ModuleNameConfig>> moduleNameConfig)
        {

            _logger = logger;
            _logLevelConfigs = logLevelConfigs.Value;
            _moduleNameConfig = moduleNameConfig.Value;

            Commands.Add(LogConstants.COMMAND_SEARCH, new DelegateCommand(OnSearch));
            EventAggregator.Instance.GetEvent<SearchFinishedEvent>().Subscribe(OnSearchFinished);

            BeginDate = DateTime.Now;
            EndDate = DateTime.Now;

            InitLogLevelList();
            InitModuleList();

        }

        private void InitLogLevelList()
        {
            var logLevelAll = new LogLevelModel() { DisplayText = LogConstants.COMBOX_OPTION_ALL, ValueText = LogConstants.COMBOX_OPTION_ALL, IsSelected = true };
            logLevelAll.PropertyChanged += OnAllLogLevelOptionPropertyChanged;
            LogLevelList.Add(logLevelAll);

            foreach (var level in _logLevelConfigs)
            {
                var levelModel = new LogLevelModel() { DisplayText = level.Name, ValueText = level.ShortName, IsSelected = true };
                levelModel.PropertyChanged += OnLogLevelPropertyChanged;
                LogLevelList.Add(levelModel);
            }

            this.ShowSelectedLogLevelText();
        }

        private void InitModuleList()
        {
            var moduleNamesAll = new ModuleModel() { DisplayText = LogConstants.COMBOX_OPTION_ALL, ValueText = LogConstants.COMBOX_OPTION_ALL, IsSelected = true };
            moduleNamesAll.PropertyChanged += OnAllModuleOptionPropertyChanged;
            ModuleList.Add(moduleNamesAll);

            foreach (var module in _moduleNameConfig)
            {
                var moduleModel = new ModuleModel() { DisplayText = module.Name, ValueText = module.Name, IsSelected = true };
                moduleModel.PropertyChanged += OnModulePropertyChanged;
                ModuleList.Add(moduleModel);
            }

            this.ShowSelectedModuleText();
        }

        private void OnAllLogLevelOptionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != SELECTED_PROPERTY)
            {
                return;
            }

            if (this._isLogLdevelSelectedByOption)
            {
                return;
            }

            this._isLogLdevelSelectedByAll = true;

            var source = (LogLevelModel)sender;
            foreach (var logLevel in LogLevelList)
            {
                logLevel.IsSelected = source.IsSelected;
            }

            //show display text of comboBox
            this.ShowSelectedLogLevelText();
            this._isLogLdevelSelectedByAll = false;
        }

        private void OnLogLevelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != SELECTED_PROPERTY)
            {
                return;
            }

            if (this._isLogLdevelSelectedByAll)
            {
                return;
            }

            this._isLogLdevelSelectedByOption = true;

            var countOfUnselected = LogLevelList.Count(l => l.DisplayText != LogConstants.COMBOX_OPTION_ALL && !l.IsSelected);
            var allOption = LogLevelList.First(l => l.DisplayText == LogConstants.COMBOX_OPTION_ALL);
            allOption.IsSelected = countOfUnselected == 0 ? true : false;

            //show display text of comboBox
            this.ShowSelectedLogLevelText();

            this._isLogLdevelSelectedByOption = false;

        }

        private void OnAllModuleOptionPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != SELECTED_PROPERTY)
            {
                return;
            }

            if (this._isModuleSelectedByOption)
            {
                return;
            }

            this._isModuleSelectedByAll = true;

            var source = (ModuleModel)sender;
            foreach (var module in ModuleList)
            {
                module.IsSelected = source.IsSelected;
            }

            //show display text of comboBox
            this.ShowSelectedModuleText();
            this._isModuleSelectedByAll = false;

        }

        private void OnModulePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != SELECTED_PROPERTY)
            {
                return;
            }

            if (this._isModuleSelectedByAll)
            {
                return;
            }

            this._isModuleSelectedByOption = true;

            var countOfUnselected = ModuleList.Count(l => l.DisplayText != LogConstants.COMBOX_OPTION_ALL && !l.IsSelected);
            var allOption = ModuleList.First(l => l.DisplayText == LogConstants.COMBOX_OPTION_ALL);
            allOption.IsSelected = countOfUnselected == 0 ? true : false;

            //show display text of comboBox
            this.ShowSelectedModuleText();

            this._isModuleSelectedByOption = false;

        }

        private void ShowSelectedLogLevelText()
        {
            //figure out the check status of LOG_LEVEL_ALL in real time
            var countOfUnselected = LogLevelList.Count(l => l.DisplayText != LogConstants.COMBOX_OPTION_ALL && !l.IsSelected);
            if (countOfUnselected == 0)
            {
                SelectedLogLevelText = LogConstants.COMBOX_OPTION_ALL;
            }
            else
            {
                var selectedLogLevels = LogLevelList.Where(l => l.DisplayText != LogConstants.COMBOX_OPTION_ALL && l.IsSelected).Select(l => l.DisplayText).ToArray();
                SelectedLogLevelText = selectedLogLevels.Length == 0 ? string.Empty : string.Join(';', selectedLogLevels);
            }
        }

        private void ShowSelectedModuleText()
        {
            //figure out the check status of LOG_MODULE_ALL in real time
            var countOfUnselected = ModuleList.Count(l => l.DisplayText != LogConstants.COMBOX_OPTION_ALL && !l.IsSelected);
            if (countOfUnselected == 0)
            {
                SelectedModuleText = LogConstants.COMBOX_OPTION_ALL;
            }
            else
            {
                var selectedModules = ModuleList.Where(l => l.DisplayText != LogConstants.COMBOX_OPTION_ALL && l.IsSelected).Select(l => l.DisplayText).ToArray();
                SelectedModuleText = selectedModules.Length == 0 ? string.Empty : string.Join(';', selectedModules);
            }
        }

        private void OnSearch()
        {
            IsSearchCommandEnabled = false;
            EventAggregator.Instance.GetEvent<SearchRaisedEvent>().Publish();
        }

        private void OnSearchFinished(List<LogLineModel> logLineModels)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                IsSearchCommandEnabled = true;
            });
        }
    }
}
