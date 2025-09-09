using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NV.CT.Service.Common.Framework;
using NV.CT.Service.TubeWarmUp.Interfaces;
using NV.CT.Service.TubeWarmUp.Models;
using NV.CT.Service.TubeWarmUp.Services;

namespace NV.CT.Service.TubeWarmUp.ViewModels
{
    public class WarmUpTaskViewModel : ViewModelBase
    {
        public event EventHandler<EventArgs> WarmUpTaskChanged;

        private readonly IDataService _dataService;

        public WarmUpTaskViewModel(IDataService dataService)
        {
            this._dataService = dataService;
            this.Tasks = new ObservableCollection<WarmUpTask>();
            this.Tasks.CollectionChanged += Tasks_CollectionChanged;
        }

        private void RaiseWarmUpTaskChanged()
        {
            try
            {
                this.WarmUpTaskChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception)
            {
            }
        }

        //订阅取消订阅保存事件
        private void Tasks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is WarmUpTask task)
                    {
                        task.TaskSave += OnTaskSave;
                    }
                }
                RaiseWarmUpTaskChanged();
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is WarmUpTask task)
                    {
                        task.TaskSave -= OnTaskSave;
                    }
                }
                RaiseWarmUpTaskChanged();
            }
        }

        private void OnTaskSave(object obj, EventArgs e)
        {
            if (obj is WarmUpTask task)
            {
                RaiseWarmUpTaskChanged();
                this._dataService.UpdateWarmUpTask(task);
            }
        }

        private ObservableCollection<WarmUpTask> tasks;

        public ObservableCollection<WarmUpTask> Tasks
        {
            get { return tasks; }
            set { SetProperty(ref tasks, value); }
        }

        private bool loadedTask;

        /// <summary>
        /// 加载预热任务
        /// </summary>
        public void Load()
        {
            if (loadedTask)
            {
                return;
            }
            loadedTask = true;

            this.Tasks!.Clear();
            foreach (var item in this._dataService.GetWarmUpTasks())
            {
                this.Tasks!.Add(item);
            }
        }

        private DelegateCommand? addCommand;

        public DelegateCommand? AddCommand => addCommand == null ?
            addCommand = new DelegateCommand(OnAdd) : addCommand;

        private void OnAdd()
        {
            var task = this._dataService.AddWarmUpTask(new WarmUpTask());
            this.Tasks.Add(task);
        }

        private DelegateCommand<WarmUpTask>? deleteCommand;

        public DelegateCommand<WarmUpTask>? DeleteCommand => deleteCommand == null ?
            deleteCommand = new DelegateCommand<WarmUpTask>(OnDelete) : deleteCommand;

        private void OnDelete(WarmUpTask task)
        {
            this._dataService.DeleteWarmUpTask(task);
            this.Tasks.Remove(task);
        }
    }
}