using NV.CT.FacadeProxy.Models.AutoCalibration;
using NV.CT.Service.AutoCali.Model;
using NV.CT.Service.Common;
using NV.CT.Service.UI.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace NV.CT.Service.AutoCali.UI.Logic
{
    public class CaliTaskViewModel<T> : BindableBase, ICaliTaskViewModel where T : IName
    {
        public string Name
        {
            get => _Name ?? (_Name = ($"{DateTime.Now.ToString("HHmmssffff")}"));
            set => _Name = value;
        }

        public T Inner { get; set; }
        public bool IsCompleted
        {
            get => CaliTaskState.Success == CaliTaskState;
            set
            {
                mIsCompleted = value;
                OnPropertyChanged("IsCompleted");
            }
        }

        public CaliTaskState CaliTaskState
        {
            get => mCaliTaskState;
            set
            {
                mCaliTaskState = value;
                OnPropertyChanged("CaliTaskState");
            }
        }

        /// <summary>
        /// 任务的状态信息队列
        /// </summary>
        public List<AlgorithmCalculateStatusInfo> TaskStatusInfos
        {
            get
            {
                if (_TaskStatusInfos == null)
                {
                    _TaskStatusInfos = new();
                }
                return _TaskStatusInfos;
            }
        }

        /// <summary>
        /// 子任务
        /// </summary>
        public ObservableCollection<ICaliTaskViewModel>? SubTaskViewModels
        {
            get => mSubTaskViewModels;
            set
            {
                if (value == mSubTaskViewModels) return;

                mSubTaskViewModels = value;
                OnPropertyChanged("SubTaskViewModels");

                SubTaskViewModels.CollectionChanged -= SubTaskViewModels_CollectionChanged;
                SubTaskViewModels.CollectionChanged += SubTaskViewModels_CollectionChanged;
            }
        }

        public CaliTaskViewModel(T inner)
        {
            Inner = inner;
            Name = inner?.Name;//设置为内置Dto对象的Name属性
        }

        /// <summary>
        /// 开始执行的命令
        /// </summary>
        public ICommand StartCmd { get; set; }

        /// <summary>
        /// 停止执行的命令
        /// </summary>
        public ICommand StopCmd { get; set; }

        /// <summary>
        /// 重新开始执行的命令
        /// </summary>
        public ICommand RepeatCmd { get; set; }

        #region 联动自身IsChecked切换 和 子项的IsChecked切换

        public bool IsChecked
        {
            get => _IsChecked;
            set
            {
                if (_IsChecked == value) return;

                _IsChecked = value;
                OnPropertyChanged("IsChecked");

                // 重置子项的IsChecked
                ResetChildren_IsChecked(value);
            }
        }

        private void SubTaskViewModels_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LogService.Instance.Debug(ServiceCategory.AutoCali, $"[{Name}][CollectionChanged] Changed");
            foreach (var child in e.NewItems)
            {
                if (child is INotifyPropertyChanged childAsPropertyChanged)
                {
                    childAsPropertyChanged.PropertyChanged -= OnPropertyChanged;
                    childAsPropertyChanged.PropertyChanged += OnPropertyChanged;
                }
            }
        }

        private static readonly string PROPERTY_NAME_IS_CHECKED = nameof(IsChecked);
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (PROPERTY_NAME_IS_CHECKED != e.PropertyName) return;

            //bool allChecked = SubTaskViewModels.All(child => child.IsChecked == true);
            //IsCheckedAllSub = allChecked;

            int allCount = SubTaskViewModels.Count();
            ChildCheckedCount = SubTaskViewModels.Count(child => child.IsChecked == true);
            ChildUnCheckedCount = allCount - ChildCheckedCount;
            IsCheckedAllSub = (allCount == ChildCheckedCount);
        }

        /// <summary>
        /// 重置子项的IsChecked
        /// </summary>
        /// <param name="value"></param>
        private void ResetChildren_IsChecked(bool value)
        {
            if (mSubTaskViewModels == null) return;

            foreach (var child in mSubTaskViewModels)
            {
                child.IsChecked = value;
            }
        }

        #endregion 联动自身IsChecked切换 和 子项的IsChecked切换

        /// <summary>
        /// 任务参数
        /// </summary>
        public Dictionary<string, object> TaskParams { get; set; }

        /// <summary>
        /// 父级节点
        /// </summary>
        public ICaliTaskViewModel Parent { get; set; }

        public bool IsCheckedAllSub
        {
            get => _IsCheckedAllSub;
            set
            {
                if (_IsCheckedAllSub == value) return;

                _IsCheckedAllSub = value;
                OnPropertyChanged("IsCheckedAllSub");
            }
        }

        /// <summary>
        /// 子项勾选计数
        /// </summary>
        public int ChildCheckedCount
        {
            get => _ChildCheckedCount;
            set
            {
                if (_ChildCheckedCount == value) return;

                _ChildCheckedCount = value;
                OnPropertyChanged("ChildCheckedCount");
            }
        }

        /// <summary>
        /// 子项未勾选计数
        /// </summary>
        public int ChildUnCheckedCount
        {
            get => _ChildUnCheckedCount;
            set
            {
                if (_ChildUnCheckedCount == value) return;

                _ChildUnCheckedCount = value;
                OnPropertyChanged("ChildUnCheckedCount");
            }
        }

        #region private

        private bool mIsCompleted;
        private CaliTaskState mCaliTaskState;
        private List<AlgorithmCalculateStatusInfo> _TaskStatusInfos;

        private string _Name;

        private bool _IsChecked;
        private bool _IsCheckedAllSub;
        private int _ChildCheckedCount;
        private int _ChildUnCheckedCount;
        private ObservableCollection<ICaliTaskViewModel>? mSubTaskViewModels;
        #endregion
    }
}
