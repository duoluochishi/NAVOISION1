using NV.CT.Service.Common.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.Models
{
    public enum TaskStatus
    {
        Standby,
        Sleep,
        Working,
        Done,
        Error,
        Cancelled
    }

    public class WarmUpTask : EditableObject
    {
        public WarmUpTask()
        {
            Locker = new object();
        }

        public event EventHandler TaskSave;

        private int id;

        public int Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        /// <summary>
        /// 休息时间
        /// </summary>
        private int restTimeInterval;

        public int RestTimeInterval
        {
            get { return restTimeInterval; }
            set { SetProperty(ref restTimeInterval, value); }
        }

        private int kv;

        public int KV
        {
            get { return kv; }
            set { SetProperty(ref kv, value); }
        }

        private int ma;

        public int MA
        {
            get { return ma; }
            set { SetProperty(ref ma, value); }
        }

        public object Locker { get; private set; }
        private TaskStatus status;

        public TaskStatus Status
        {
            get { return status; }
            set { SetProperty(ref status, value); }
        }

        private int scanTimes;

        public int ScanTimes
        {
            get { return scanTimes; }
            set { SetProperty(ref scanTimes, value); }
        }

        private int completeTimes;

        public int CompleteTimes
        {
            get { return completeTimes; }
            set { SetProperty(ref completeTimes, value); }
        }

        protected override void EndEditCore()
        {
            //保存数据
            this.TaskSave?.Invoke(this, null);
        }
    }
}