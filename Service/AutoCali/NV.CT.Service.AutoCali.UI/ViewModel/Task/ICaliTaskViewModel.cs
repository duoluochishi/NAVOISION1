using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NV.CT.Service.AutoCali.UI.Logic
{
    public interface ICaliTaskViewModel
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        string Name { get; set; }

        bool IsCompleted { get; set; }

        CaliTaskState CaliTaskState { get; set; }

        /// <summary>
        /// 开始执行的命令
        /// </summary>
        ICommand StartCmd { get; set; }

        /// <summary>
        /// 停止执行的命令
        /// </summary>
        ICommand StopCmd { get; set; }

        /// <summary>
        /// 重新开始执行的命令
        /// </summary>
        ICommand RepeatCmd { get; set; }

        bool IsChecked { get;set; }

        /// <summary>
        /// 子任务
        /// </summary>
        ObservableCollection<ICaliTaskViewModel>? SubTaskViewModels { get; set; }

        /// <summary>
        /// 是否勾选了全部子项
        /// </summary>
        bool IsCheckedAllSub { get; set; }

        /// <summary>
        /// 子项勾选计数
        /// </summary>
        int ChildCheckedCount { get; set; }

        /// <summary>
        /// 子项未勾选计数
        /// </summary>
        int ChildUnCheckedCount { get; set; }

        /// <summary>
        /// 父级节点
        /// </summary>
        ICaliTaskViewModel Parent { get; set; }

        Dictionary<string, object> TaskParams { get; set; }
    }
}
