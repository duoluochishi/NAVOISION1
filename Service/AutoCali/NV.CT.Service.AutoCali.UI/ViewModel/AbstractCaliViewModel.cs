using NV.CT.FacadeProxy.Models.DataAcquisition;
using NV.CT.Service.AutoCali.DAL;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Resources;
using NV.CT.Service.UI.Util;
using NV.CT.ServiceFramework.Contract;
using System;
using System.Linq;
using System.Windows.Input;

namespace NV.CT.Service.AutoCali.Logic
{
    public abstract class AbstractCaliViewModel : BindableBase
    {
        public AbstractCaliViewModel()
        {
            string block = $"{ClassName}.ctor";
            LogService.Instance.Info(ServiceCategory.AutoCali, $"Beginning {block}");

            InitData();
            InitCommand();

            InitCaliScenarioSelection();

            LogService.Instance.Info(ServiceCategory.AutoCali, $"Ended {block}");
        }

        public ObservableCollectionWrapper<CalibrationScenario> CaliScenarioList { get; set; }

        public ObservableCollectionWrapper<CaliHistoryItem> CaliHistoryList { get; set; }

        public CaliScenarioTaskViewModel CaliTask
        {
            get { return _CaliTask; }
            set
            {
                if (value == _CaliTask)
                {
                    return;
                }

                RegisterCaliTaskEvent(value);

                _CaliTask = value;
                if (_CaliTask != null)
                {
                    _CaliTask.IsAutoConfirmResult = IsAutoConfirmResult;
                    _CaliTask.XScatteringDetectorGain = XScatteringDetectorGain;
                }

                OnPropertyChanged("CaliTask");
            }
        }

        public ICommand SelectionChangedCommand { get; set; }

        private void InitData()
        {
            string block = $"{ClassName}.InitData";
            LogService.Instance.Info(ServiceCategory.AutoCali, $"Beginning {block}");

            InitDtoService();

            CaliScenarioList = new ObservableCollectionWrapper<CalibrationScenario>();
            foreach (var scenario in ScenarioDtoService.Get())
            {
                CaliScenarioList.Add(scenario);
            }

            //校准历史
            CaliHistoryList = new ObservableCollectionWrapper<CaliHistoryItem>();
            foreach (var historyItem in HistoryDtoService.Get())
            {
                CaliHistoryList.Add(historyItem);
            }

            IsAutoConfirmResult = ScenarioConfigDtoService?.GetConfig()?.AutoConfirmResult == true;

            string strScatteringDetectorGain = ScenarioConfigDtoService?.GetConfig()?.ScatteringDetectorGain;
            LogService.Instance.Info(ServiceCategory.AutoCali, $"Got ScatteringDetectorGain('{strScatteringDetectorGain}') from ScenarioConfig");

            ScatteringDetectorGain valueScatteringDetectorGain = ScatteringDetectorGainDefault;
            if (Enum.TryParse<ScatteringDetectorGain>(strScatteringDetectorGain, true, out valueScatteringDetectorGain))
            {
                XScatteringDetectorGain = valueScatteringDetectorGain;
                LogService.Instance.Info(ServiceCategory.AutoCali, $"Parsed ScatteringDetectorGain from ScenarioConfig, [Out] {XScatteringDetectorGain}({(int)XScatteringDetectorGain})");
            }
            else
            {
                XScatteringDetectorGain = ScatteringDetectorGainDefault;
                LogService.Instance.Info(ServiceCategory.AutoCali, $"Failed to Parse ScatteringDetectorGain from ScenarioConfig, use the default, [Out] {XScatteringDetectorGain}({(int)XScatteringDetectorGain})");
            }

            LogService.Instance.Info(ServiceCategory.AutoCali, $"Ended {block}");
        }
        protected abstract void InitDtoService();

        protected ICaliScenarioDAO ScenarioDtoService { get; set; }
        protected ICaliScenarioConfigDAO ScenarioConfigDtoService { get; set; }
        protected IDtoService<CaliHistoryItem> HistoryDtoService { get; set; }

        /// <summary>
        /// 是否自动确认结果
        /// </summary>
        protected bool IsAutoConfirmResult
        {
            get => _IsAutoConfirmResult;
            set
            {
                _IsAutoConfirmResult = value;

                OnPropertyChanged("IsAutoConfirmResult");
            }
        }
        protected bool _IsAutoConfirmResult;

        private static ScatteringDetectorGain ScatteringDetectorGainDefault = ScatteringDetectorGain.Fix16Pc;
        /// <summary>
        /// 散射探测器的增益模式，默认16pc
        /// </summary>
        protected ScatteringDetectorGain XScatteringDetectorGain { get; private set; } = ScatteringDetectorGainDefault;

        /// <summary>
        /// 当用户退出时，告知外界提示信息
        /// </summary>
        public string? TipOnExiting { get; protected set; }

        public string ServiceAppName { get; set; }

        protected virtual void InitCommand()
        {
            SelectionChangedCommand = new DelegateCommand(() =>
            {
                string block = $"{ClassName}.SelectionChangedCommand";
                LogService.Instance.Info(ServiceCategory.AutoCali, $"Beginning {block}");

                OnCaliTaskChanged(CaliScenarioList.SelectedItem);

                LogService.Instance.Info(ServiceCategory.AutoCali, $"Ended {block}");
            });
        }

        private void OnCaliTaskStarted(object? sender, EventArgs e)
        {
            TipOnExiting = Calibration_Lang.Calibration_TipOnExiting;
            ServiceToken.Take(ServiceAppName);

            CaliHistoryItem item = new CaliHistoryItem()
            {
                CreatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                OperationObject = this.CaliTask.Inner.Name
            };
            this.CaliHistoryList.Add(item);

            HistoryDtoService.Save(CaliHistoryList.ToList());
        }

        private void OnCaliTaskEnded(object? sender, EventArgs e)
        {
            TipOnExiting = null;
            ServiceToken.Release(ServiceAppName);
        }

        private void RegisterCaliTaskEvent(CaliScenarioTaskViewModel newCaliTask)
        {
            UnregisterCaliTaskEvent();

            if (newCaliTask != null)
            {
                newCaliTask.CaliTaskStarted += OnCaliTaskStarted;
                newCaliTask.CaliTaskEnded += OnCaliTaskEnded;
            }
        }

        public void UnregisterCaliTaskEvent()
        {
            if (_CaliTask != null)
            {
                _CaliTask.CaliTaskStarted -= OnCaliTaskStarted;
                _CaliTask.CaliTaskEnded -= OnCaliTaskEnded;
            }
        }

        private void InitCaliScenarioSelection()
        {
            var scenario = (CaliScenarioList.Count > 0) ? CaliScenarioList[0] : null;
            OnCaliScenarioSelectionChanged(scenario);
        }

        private void OnCaliScenarioSelectionChanged(CalibrationScenario selectedScenario)
        {
            string block = $"{ClassName}.OnCaliScenarioSelectionChanged";
            LogService.Instance.Info(ServiceCategory.AutoCali, $"Beginning {block}");

            if (selectedScenario == CaliScenarioList.SelectedItem)
            {
                LogService.Instance.Info(ServiceCategory.AutoCali, $"Cali Scenario Selection hasn't Changed");
                return;
            }

            CaliScenarioList.SelectedItem = selectedScenario;
            OnCaliTaskChanged(selectedScenario);
            LogService.Instance.Info(ServiceCategory.AutoCali, $"Ended {block}");
        }

        private void OnCaliTaskChanged(CalibrationScenario caliScenario)
        {
            string block = $"{ClassName}.OnCaliTaskChanged";
            LogService.Instance.Info(ServiceCategory.AutoCali, $"Beginning {block}");

            CaliService.Instance.UnRegisterMrsEvent(this.CaliTask);//先卸载旧的注册事件

            this.CaliTask = new CaliScenarioTaskViewModel(caliScenario);

            CaliService.Instance.InitEvent(this.CaliTask);//后注册新的事件

            LogService.Instance.Info(ServiceCategory.AutoCali, $"Ended {block}");
        }

        private CaliScenarioTaskViewModel _CaliTask;

        private static readonly string ClassName = nameof(AbstractCaliViewModel);
    }
}
