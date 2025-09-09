using NV.CT.Service.AutoCali.DAL;
using NV.CT.Service.Common;
using NV.CT.Service.UI.Util;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace NV.CT.Service.AutoCali.Logic
{
    public class AutoCaliViewModel : AbstractCaliViewModel
    {
        private static readonly string ClassName = nameof(AutoCaliViewModel);

        protected override void InitDtoService()
        {
            ScenarioDtoService = AutoCaliScenarioServiceImpl.Instance;
            ScenarioConfigDtoService = AutoCaliScenarioServiceImpl.Instance;
            HistoryDtoService = AutoCaliHistoryServiceImpl.Instance;
        }

        #region 校准场景管理功能
        public ICommand AddCommand { get; private set; }

        public ICommand DeleteCommand { get; private set; }

        public ICommand EditCommand { get; private set; }
        public ICommand RunCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        private void InitManagerCommand()
        {
            AddCommand = new DelegateCommand(
                (commandParam) => AddAction(commandParam)
                );
            EditCommand = new DelegateCommand(
                (commandParam) => EditAction(commandParam),
                () => CaliScenarioList.SelectedItem != null
                );
            DeleteCommand = new DelegateCommand(
                () => DeleteAction(),
                () => CaliScenarioList.SelectedItem != null
                );
        }

        private void EditAction(object commandParam)
        {
            CaliScenarioEditViewModel editViewModel;
            var model = CaliScenarioList.SelectedItem;
            editViewModel = new CaliScenarioEditViewModel(model);
            editViewModel.SaveEventHandler += (sender, eventArgs) =>
            {
                var tempCaliScenario = eventArgs;
                //更新校准场景对象后，需要同步更新其所在的校准列表。
                int index = CaliScenarioList.SelectedIndex;
                CaliScenarioList[index] = tempCaliScenario;
                CaliScenarioList.SelectedItem = tempCaliScenario;

                SaveAction();
            };
            WindowHelper.ShowDialog("CaliScenarioEditWin", editViewModel, commandParam);
        }

        public Dispatcher Dispatcher { get; set; }
        private void AddAction(object commandParam)
        {
            CaliScenarioEditViewModel editViewModel;
            editViewModel = new CaliScenarioEditViewModel();
            editViewModel.SaveEventHandler += (sender, eventArgs) =>
            {
                var tempCaliScenario = eventArgs;
                //更新校准场景对象后，需要同步更新其所在的校准列表。

                CaliScenarioList.Add(tempCaliScenario);
                CaliScenarioList.SelectedItem = tempCaliScenario;

                SaveAction();
            };

            WindowHelper.ShowDialog("CaliScenarioEditWin", editViewModel, commandParam);
        }

        public static readonly string Msg_Confirm_DeleteScenario = "Are you sure you want to delete this scenario:{0}?";
        private void DeleteAction()
        {
            int index = CaliScenarioList.SelectedIndex;
            if (index < 0)
            {
                return;
            }

            string msg = string.Format(Msg_Confirm_DeleteScenario, CaliScenarioList.SelectedItem?.Name);
            if (!DialogService.Instance.ShowConfirm(msg))
            {
                return;
            }

            CaliScenarioList.RemoveAt(index);
            SaveAction();

            if (CaliScenarioList.Count > 0)
            {
                var newIndex = (index < CaliScenarioList.Count) ? index : (index - 1);
                CaliScenarioList.SelectedItem = CaliScenarioList[newIndex];
            }
        }

        private void SaveAction()
        {
            AutoCaliScenarioServiceImpl.Instance.Save(CaliScenarioList.ToList());
            CaliItemServiceImpl.Instance.Save(CaliItemServiceImpl.Instance.CacheCaliItems);
        }

        #endregion 校准场景管理功能

        protected override void InitCommand()
        {
            base.InitCommand();

            InitManagerCommand();
        }
    }
}
