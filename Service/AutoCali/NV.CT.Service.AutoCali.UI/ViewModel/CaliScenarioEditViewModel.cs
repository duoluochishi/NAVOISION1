using NV.CT.FacadeProxy.Common.Helpers;
using NV.CT.FacadeProxy.Helpers;
using NV.CT.Service.AutoCali.DAL;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.AutoCali.UI;
using NV.CT.Service.AutoCali.UI.Logic;
using NV.CT.Service.AutoCali.Util;
using NV.CT.Service.Common;
using NV.CT.Service.UI.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace NV.CT.Service.AutoCali.Logic
{
    /// <summary>
    /// 校准场景的编辑功能的视图模型，供View绑定使用
    /// </summary>
    internal class CaliScenarioEditViewModel : CaliScenarioViewModel
    {
        public EventHandler<CalibrationScenario> SaveEventHandler;
        public EventHandler<CalibrationScenario> CloseHostWindowHandler;

        public CaliScenarioEditViewModel(CalibrationScenario? caliScenario = null)
        {
            try
            {
                if (null == caliScenario)
                {
                    //新增模式下
                    WrappedCaliScenario = new CalibrationScenario();
                }
                else
                {
                    //编辑模式下，用户确认“保存”才更新数据，所以UI使用克隆对象避免界面绑定原对象被直接修改
                    string json = JsonSerializeHelper.ToJson(caliScenario);
                    WrappedCaliScenario = JsonSerializeHelper.Deserialize<CalibrationScenario>(json);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            InitData();
            InitCommand();
        }

        /// <summary>
        /// 本校准场景未关联的校准项目
        /// </summary>
        public ObservableCollection<CalibrationItem> UnAssociatedCaliItems { get; private set; }

        /// <summary>
        /// 确认保存的命令
        /// </summary>
        public ICommand ConfirmCommand { get; private set; }

        #region 关联,取消关联, 上移，下移的命令

        /// <summary>
        /// 校准场景关联某个校准项目的命令
        /// </summary>
        public ICommand AssociateCommand { get; private set; }

        /// <summary>
        /// 校准场景取消关联某个校准项目的命令
        /// </summary>
        public ICommand UnAssociateCommand { get; private set; }

        /// <summary>
        /// 校准场景关联某个校准项目向上调整位置的命令
        /// </summary>
        public ICommand UpCommand { get; private set; }

        /// <summary>
        /// 校准场景关联某个校准项目向下调整位置的命令
        /// </summary>
        public ICommand DownCommand { get; private set; }

        private bool CanAction(ICollection<CalibrationItem> targetSourceItems)
        {
            return targetSourceItems == null
                ? false
                : targetSourceItems.Contains(SelectedCaliItem);
        }

        /// <summary>
        /// 将选中项从源列表 到 目标列表 来回移动
        /// </summary>
        /// <param name="sourceItems"></param>
        /// <param name="destItems"></param>
        private void MoveAction(ICollection<CalibrationItem> sourceItems, ICollection<CalibrationItem> destItems)
        {
            var temp = SelectedCaliItem;
            sourceItems.Remove(temp);
            destItems.Add(temp);
            SelectedCaliItem = temp;
        }

        private void UpAction(IList<CalibrationItem> sourceItems)
        {
            var index = sourceItems.IndexOf(SelectedCaliItem);
            if (index == 0)
            {
                return;
            }

            //移动上一个（不移动本尊），可以避免本尊的状态（选中）被影响
            var temp = sourceItems[index - 1];
            sourceItems.Remove(temp);
            sourceItems.Insert(index, temp);
        }

        private void DownAction(IList<CalibrationItem> sourceItems)
        {
            var index = sourceItems.IndexOf(SelectedCaliItem);
            if (index + 1 == sourceItems.Count)
            {
                return;
            }

            //移动下一个（不移动本尊），可以避免本尊的状态（选中）被影响
            var temp = sourceItems[index + 1];
            sourceItems.Remove(temp);
            sourceItems.Insert(index, temp);
        }
        #endregion 关联,取消关联, 上移，下移的命令

        #region 校准项目的参数协议的相关（新增，创建副本，删除，修改，上移，下移）的命令

        /// <summary>
        /// 校准场景关联的某个校准项目，添加参数协议的命令
        /// </summary>
        public ICommand AddArgProtocolCommand { get; private set; }

        /// <summary>
        /// 创建副本参数协议的命令
        /// </summary>
        public ICommand CopyAsArgProtocolCommand { get; private set; }

        /// <summary>
        /// 删除参数协议的命令
        /// </summary>
        public ICommand DeleteArgProtocolCommand { get; private set; }

        /// <summary>
        /// 修改参数协议的命令
        /// </summary>
        public ICommand EditArgProtocolCommand { get; private set; }

        #endregion

        internal override void InitData()
        {
            base.InitData();

            UnAssociatedCaliItems = new ObservableCollection<CalibrationItem>();
            var associatedCaliItemNameList = this.WrappedCaliScenario?.CalibrationItemReferenceGroup;
            foreach (var caliItem in systemCaliItems)
            {
                if (false == associatedCaliItemNameList?.Contains(caliItem.Name))
                {
                    UnAssociatedCaliItems.Add(caliItem);
                }
            }
        }

        /// <summary>
        /// 编辑校准场景时，会联动编辑其关联的校准项目，以及校准项目所包含的参数协议
        /// 由于UI动态双向绑定，为避免UI上发生修改，但最终取消保存，影响了源数据，所以克隆一份源数据
        /// </summary>
        /// <returns></returns>
        internal override IList<CalibrationItem> GetSystemCaliItems()
        {
            var source = CaliItemServiceImpl.Instance.CacheCaliItems;
            string json = JsonSerializeHelper.ToJson(source);
            var cloned = JsonSerializeHelper.Deserialize<IList<CalibrationItem>>(json);
            return cloned!;
        }

        private void InitCommand()
        {
            AssociateCommand = new DelegateCommand(
                () => { MoveAction(UnAssociatedCaliItems, AssociatedCaliItems); },
                () => { return CanAction(UnAssociatedCaliItems); }
                );

            UnAssociateCommand = new DelegateCommand(
                () => { MoveAction(AssociatedCaliItems, UnAssociatedCaliItems); },
                () => { return CanAction(AssociatedCaliItems); }
                );

            UpCommand = new DelegateCommand(
                () => { UpAction(AssociatedCaliItems); },
                () => { return CanAction(AssociatedCaliItems); }
                );

            DownCommand = new DelegateCommand(
                () => { DownAction(AssociatedCaliItems); },
                () => { return CanAction(AssociatedCaliItems); }
                );

            ConfirmCommand = new DelegateCommand(
                () =>
                {
                    if (null == SaveEventHandler) return;


                    string scenarioName = WrappedCaliScenario.Name;
                    if (string.IsNullOrEmpty(scenarioName))
                    {
                        DialogService.Instance.ShowWarning(Res_Cali_Warning_Save_Failed_Because_Of_Empty_Scenario_Name);
                        return;
                    }
                    //else if (null != CaliScenarioList.FirstOrDefault(p => 0 == string.Compare(p.Name, scenarioName, System.StringComparison.OrdinalIgnoreCase)))
                    //{
                    //    DialogService.Instance.ShowWarningDialog(Res_Cali_Warning_Save_Failed_Because_Calibration_Already_Exists);
                    //    return;

                    //}

                    WrappedCaliScenario.CalibrationItemReferenceGroup.Clear();
                    var names = from caliItem in this.AssociatedCaliItems
                                select caliItem.Name;
                    WrappedCaliScenario.CalibrationItemReferenceGroup.AddRange(names);

                    CaliItemServiceImpl.Instance.CacheCaliItems = this.systemCaliItems;
                    SaveEventHandler(this, WrappedCaliScenario);
                    this.CloseHostWindowHandler?.Invoke(this, null);
                });

            this.AddArgProtocolCommand = new DelegateCommand(
                () => ExecAddArgProtocolCmd()
                );

            this.CopyAsArgProtocolCommand = new DelegateCommand(
                () => ExecCopuAsArgProtocolCmd(),
                () => null != this.SelectedArgProtocolViewModel
                );

            this.DeleteArgProtocolCommand = new DelegateCommand(
                () => ExecDeleteArgProtocolCmd(),
                () => null != this.SelectedArgProtocolViewModel
                ); ;

            this.EditArgProtocolCommand = new DelegateCommand(
                () => ExecEditArgProtocolCmd(),
                () => null != this.SelectedArgProtocolViewModel
                );
        }

        public static readonly string Res_Cali_Warning_Save_Failed_Because_Of_Empty_Scenario_Name =
            "Save failed because of empty scenario name!";
        public static readonly string Res_Cali_Warning_Save_Failed_Because_Calibration_Already_Exists =
            "Save failed because a calibration scenario named \"{0}\" already exists!";

        private void ExecAddArgProtocolCmd()
        {
            var argProtocolViewModel = GetArgProtocolViewModelForCommand(isNew: true);
            argProtocolViewModel.SaveEventHandler += OnAddArgProtocol;
            WindowHelper.ShowDialog(typeof(CaliItemArgProtocolEditWin).Name, argProtocolViewModel);
        }
        private void ExecCopuAsArgProtocolCmd()
        {
            var argProtocolViewModel = CloneArgProtocolViewModel();
            argProtocolViewModel.SaveEventHandler += OnAddArgProtocol;
            WindowHelper.ShowDialog(typeof(CaliItemArgProtocolEditWin).Name, argProtocolViewModel);
        }
        private void ExecEditArgProtocolCmd()
        {
            var argProtocolViewModel = GetArgProtocolViewModelForCommand(isNew: false);
            argProtocolViewModel.SaveEventHandler += OnEditArgProtocol;
            WindowHelper.ShowDialog(typeof(CaliItemArgProtocolEditWin).Name, argProtocolViewModel);
        }
        private void ExecDeleteArgProtocolCmd()
        {
            var messageBoxResult = DialogService.Instance.ShowConfirm("你确定删除这个参数协议吗?");
            if (!messageBoxResult) return;

            bool isRemoved = false;
            for (int i = this.SelectedCaliItem.CalibrationProtocolGroup.Count - 1; i >= 0; i--)
            {
                var group = this.SelectedCaliItem.CalibrationProtocolGroup[i];
                for (int j = group.HandlerGroup.Count - 1; j >= 0; j--)
                {
                    var cur = group.HandlerGroup[j];
                    if (cur == this.SelectedArgProtocolViewModel.XHandler)
                    {
                        group.HandlerGroup.RemoveAt(j);
                        isRemoved = true;

                        Console.WriteLine($"Removed the selected item [{cur}]");
                        break;
                    }
                }

                if (isRemoved)
                {
                    if (group.HandlerGroup.Count == 0)
                    {
                        this.SelectedCaliItem.CalibrationProtocolGroup.RemoveAt(i);
                        Console.WriteLine($"The protocol group [{group}] hasn't contian any protocol, so removed");
                    }
                    break;
                }
            }

            this.CurrentArgProtocolViewModels.Remove(this.SelectedArgProtocolViewModel);
        }

        private ProtocolVM GetArgProtocolViewModelForCommand(bool isNew = false)
        {
            var argProtocolViewModel = ProtocolVM_Factory.GetProtocolViewModel(SelectedCaliItem);
            argProtocolViewModel.EnableEditMode();
            if (!isNew)
            {
                argProtocolViewModel.SetValueFrom(this.SelectedArgProtocolViewModel.XHandler);
            }
            return argProtocolViewModel;
        }

        private ProtocolVM CloneArgProtocolViewModel()
        {
            var argProtocolViewModel = ProtocolVM_Factory.GetProtocolViewModel(SelectedCaliItem);
            argProtocolViewModel.EnableEditMode();
            string json = JsonSerializeHelper.ToJson(this.SelectedArgProtocolViewModel.XHandler);
            var copyArgProtocol = JsonSerializeHelper.Deserialize<Handler>(json)!;
            copyArgProtocol.ID = ScanUIDHelper.Generate18UID();
            argProtocolViewModel.SetValueFrom(copyArgProtocol);
            return argProtocolViewModel;
        }

        private void OnAddArgProtocol(object? sender, Handler eventArgs)
        {
            var protocolGroup = this.SelectedCaliItem.CalibrationProtocolGroup;
            if (protocolGroup.Count < 1)
            {
                protocolGroup.Add(new CalibrationProtocol()
                {
                    Name = $"Protocol_{DateTime.Now.ToString("yyyyMMddHHmmss")}"
                });
            }
            this.SelectedCaliItem.CalibrationProtocolGroup[0].HandlerGroup.Add(eventArgs);
            if (sender is ProtocolVM)
            {
                var argProtocolViewModel = (ProtocolVM)sender;
                this.CurrentArgProtocolViewModels.Add(argProtocolViewModel);
                this.SelectedArgProtocolViewModel = argProtocolViewModel;
            }
        }
        private void OnEditArgProtocol(object? sender, Handler eventArgs)
        {
            var index = this.SelectedCaliItem.CalibrationProtocolGroup[0].HandlerGroup.FindIndex(argProtocol => argProtocol.ID == eventArgs.ID);
            if (index < 0)
            {
                return;
            }

            this.SelectedCaliItem.CalibrationProtocolGroup[0].HandlerGroup[index] = eventArgs;

            var newSelectedArgProtocolViewModel = sender as ProtocolVM;
            this.CurrentArgProtocolViewModels[index] = newSelectedArgProtocolViewModel;
            this.SelectedArgProtocolViewModel = newSelectedArgProtocolViewModel;
        }
    }
}
