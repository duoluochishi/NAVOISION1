using NV.CT.Service.AutoCali.DAL;
using NV.CT.Service.AutoCali.Logic;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.Common;
using NV.CT.Service.UI.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NV.CT.Service.AutoCali.UI.Logic
{
    /// <summary>
    /// 校准场景的查看功能的视图模型，供View绑定使用
    /// </summary>
    internal class CaliScenarioViewModel : BindableBase
    {
        public CaliScenarioViewModel() : this(null)
        { }

        public CaliScenarioViewModel(CalibrationScenario? caliScenario)
        {
            try
            {
                WrappedCaliScenario = caliScenario ?? new CalibrationScenario();
                InitData();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal virtual void InitData()
        {
            systemCaliItems = GetSystemCaliItems();

            //根据校准名称关联系统提供的校准项目对象
            AssociatedCaliItems = new ObservableCollection<CalibrationItem>();
            foreach (var caliItemName in WrappedCaliScenario?.CalibrationItemReferenceGroup)
            {
                var pickedCaliItem = systemCaliItems.FirstOrDefault(p => p.Name == caliItemName);
                if (null == pickedCaliItem)
                {
                    logWrapper.Warn($"未找到校准场景【{WrappedCaliScenario.Name}】的关联的校准项目【{caliItemName}】");
                    continue;
                }

                AssociatedCaliItems.Add(pickedCaliItem);
            }

            //默认选中第1个
            if (AssociatedCaliItems.Count > 0)
            {
                SelectedCaliItem = AssociatedCaliItems[0];
            }
        }

        internal virtual IList<CalibrationItem> GetSystemCaliItems()
        {
            return CaliItemServiceImpl.Instance.CacheCaliItems;
        }

        public CalibrationScenario? WrappedCaliScenario { get; set; }

        public ObservableCollection<CalibrationItem> AssociatedCaliItems { get; private set; }

        public CalibrationItem SelectedCaliItem
        {
            get => _SelectedCaliItem;
            set
            {
                if (value == _SelectedCaliItem)
                {
                    return;
                }

                _SelectedCaliItem = value;
                if (null == _SelectedCaliItem)
                {
                    return;
                }

                OnPropertyChanged("SelectedCaliItem");
                //UpdateArgProtocolDataView();
                UpdateCurrentArgProtocolViewModels();
            }
        }

        public CaliItemVM SelectedCaliItemVM { get; set; }

        ///// <summary>
        ///// 参数协议的数据表格视图，切换校准项目会导致表格列名和行数据发生变化
        ///// 比如，校准项目A，数据【名称：a】，【扫描速度】
        ///// </summary>
        //public DataView ArgProtocolDataView { get; private set; }

        public ObservableCollection<ProtocolVM> CurrentArgProtocolViewModels { get; private set; } = new ObservableCollection<ProtocolVM>();

        private ProtocolVM mSelectedArgProtocolViewModel;
        public ProtocolVM SelectedArgProtocolViewModel
        {
            get => mSelectedArgProtocolViewModel;
            set
            {
                if (value == mSelectedArgProtocolViewModel)
                {
                    return;
                }

                mSelectedArgProtocolViewModel = value;
                if (null == mSelectedArgProtocolViewModel)
                {
                    return;
                }

                OnPropertyChanged("SelectedArgProtocolViewModel");
            }
        }

        //protected void UpdateArgProtocolDataView()
        //{
        //    CreateArgProtocolDataTable();
        //    ArgProtocolDataView = _ArgProtocolDataTable.AsDataView();
        //    OnPropertyChanged("ArgProtocolDataView");
        //}

        protected void UpdateCurrentArgProtocolViewModels()
        {
            CurrentArgProtocolViewModels.Clear();
            if (_SelectedCaliItem == null)
            {
                return;
            }

            //foreach (var argProtocol in _SelectedCaliItem.ArgProtocolGroup)
            foreach (var caliProtocol in _SelectedCaliItem.CalibrationProtocolGroup)
            {
                foreach (var argProtocol in caliProtocol?.HandlerGroup)
                {
                    var argProtocolViewModel = ProtocolVM_Factory.GetProtocolViewModel(_SelectedCaliItem, argProtocol);
                    //argProtocolViewModel.SetValueFrom(argProtocol);
                    CurrentArgProtocolViewModels.Add(argProtocolViewModel);
                }
            }
        }

        private CalibrationItem _SelectedCaliItem;
        //private DataTable _ArgProtocolDataTable;

        private static readonly LogWrapper logWrapper = new LogWrapper(ServiceCategory.AutoCali);

        internal IList<CalibrationItem> systemCaliItems;
    }
}
