using NV.CT.Service.Common.Framework;
using NV.CT.Service.TubeWarmUp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.TubeWarmUp.ViewModels
{
    public class WarmUpProgressViewModel : ViewModelBase
    {
        private Timer _timer;
        private bool _timerActive;
        public WarmUpProgressViewModel()
        {
            this._timer = new Timer(OnTimer, null, Timeout.Infinite, Timeout.Infinite);
        }
        private TimeSpan totalTime;

        public TimeSpan TotalTime
        {
            get { return totalTime; }
            set { SetProperty(ref totalTime, value); }
        }
        private TimeSpan costTime;

        public TimeSpan CostTime
        {
            get { return costTime; }
            set { SetProperty(ref costTime, value); }
        }
        private int progress;

        public int Progress
        {
            get { return progress; }
            set { SetProperty(ref progress, value); }
        }

        private TubeHeatCapStatus heatCapStatus;

        public TubeHeatCapStatus HeatCapStatus
        {
            get { return heatCapStatus; }
            set { SetProperty(ref heatCapStatus, value); }
        }


        public void StartTimer()
        {
            this.CostTime = TimeSpan.Zero;
            this._timerActive = true;
            this._timer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
        }
        private void OnTimer(object? state)
        {
            if (this._timerActive)
            {
                this._timer.Change(Timeout.Infinite, Timeout.Infinite);
                this.CostTime = this.CostTime.Add(TimeSpan.FromSeconds(1));
                this._timer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
            }
        }
        public void StopTime()
        {
            this._timerActive = false;
            this._timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
