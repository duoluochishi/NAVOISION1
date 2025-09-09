using Autofac.Features.OwnedInstances;
using NV.CT.Service.AutoCali.DAL;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.Common;
using NV.CT.Service.UI.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace NV.CT.Service.AutoCali.Logic
{
    public class CaliScenarioMgrViewModel : BindableBase
    {
        public ObservableCollection<CalibrationScenario> CaliScenarioList { get; set; }
        public CalibrationScenario SelectedCaliScenario
        {
            get => _SelectedCaliScenario;
            set
            {
                if (value == _SelectedCaliScenario)
                {
                    return;
                }

                _SelectedCaliScenario = value;
                OnPropertyChanged("SelectedCaliScenario");
            }
        }

        public ICommand AddCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }

        public ICommand EditCommand { get; private set; }
        public ICommand RunCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        public CaliScenarioMgrViewModel()
        {
            InitData();
            InitCommand();
        }

        private void InitData()
        {
            CaliScenarioList = new ObservableCollection<CalibrationScenario>();
            foreach (var scenario in AutoCaliScenarioServiceImpl.Instance.Get())
            {
                CaliScenarioList.Add(scenario);
            }

            if (CaliScenarioList.Count > 0)
            {
                SelectedCaliScenario = CaliScenarioList[0];
            }
        }
        private void InitCommand()
        {
            AddCommand = new DelegateCommand(
                (commandParam) => AddAction(commandParam)
                );
            EditCommand = new DelegateCommand(
                () => EditAction(),
                () => _SelectedCaliScenario != null
                );
            DeleteCommand = new DelegateCommand(
                () =>
                {
                    CaliScenarioList.Remove(_SelectedCaliScenario);
                    SaveAction();
                },
                () => _SelectedCaliScenario != null
                );

            RunCommand = new DelegateCommand(
                () => RunScenarioByTask(SelectedCaliScenario),
                () => _SelectedCaliScenario != null
                );
            CancelCommand = new DelegateCommand(
                () => CancelScenarioTask(SelectedCaliScenario),
                () => _SelectedCaliScenario != null
                );
        }

        private void EditAction()
        {
            CaliScenarioEditViewModel editViewModel;
            var model = _SelectedCaliScenario;
            editViewModel = new CaliScenarioEditViewModel(model);
            editViewModel.SaveEventHandler += (sender, eventArgs) =>
            {
                var tempCaliScenario = eventArgs;
                //更新校准场景对象后，需要同步更新其所在的校准列表。
                int index = CaliScenarioList.IndexOf(_SelectedCaliScenario);
                CaliScenarioList[index] = tempCaliScenario;
                SelectedCaliScenario = tempCaliScenario;

                SaveAction();
            };
            WindowHelper.ShowDialog("CaliScenarioEditWin", editViewModel);
        }

        private void AddAction(object commandParam)
        {
            CaliScenarioEditViewModel editViewModel;
            editViewModel = new CaliScenarioEditViewModel();
            editViewModel.SaveEventHandler += (sender, eventArgs) =>
            {
                CaliScenarioList.Add(eventArgs);
                SelectedCaliScenario = eventArgs;
                SaveAction();
            };
            WindowHelper.ShowDialog("CaliScenarioEditWin", editViewModel, commandParam);
        }

        private void SaveAction()
        {
            AutoCaliScenarioServiceImpl.Instance.Save(CaliScenarioList.ToList());
            CaliItemServiceImpl.Instance.Save(CaliItemServiceImpl.Instance.CacheCaliItems);
        }

        private void RunScenarioByTask(CalibrationScenario caliScenario)
        {
            EventMediator.Raise("RunScenarioTask", new TEventArgs<CalibrationScenario>());
        }

        private void CancelScenarioTask(CalibrationScenario caliScenario)
        {
            var msg = string.Format(Msg_Confrim_Cancel_Run_Scenario,caliScenario.Name);
            var dlgResult = DialogService.Instance.ShowConfirm(msg);
            if (!dlgResult) return;

            EventMediator.Raise("CancelScenarioTask", new TEventArgs<CalibrationScenario>());
        }

        private CalibrationScenario _SelectedCaliScenario;

        /// <summary>
        /// [ToDo]放入资源文件中
        /// </summary>
        private static readonly string Msg_Confrim_Cancel_Run_Scenario = "Are you sure you want to cancel the run (#Scenario:{0})?";
    }
}
