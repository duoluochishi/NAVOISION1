using CommunityToolkit.Mvvm.DependencyInjection;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.HardwareTest.Models.Components.Detector;
using NV.CT.Service.HardwareTest.Share.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NV.CT.Service.HardwareTest.Services.Components.Detector
{
    public class QueuedDataAcquisitionService
    {
        private readonly ILogService logService;               
        private readonly IEnumerable<DataAcquisitionTask> tasks;

        public QueuedDataAcquisitionService(IEnumerable<DataAcquisitionTask> dataAcquisitionTasks)
        {
            this.logService = Ioc.Default.GetRequiredService<ILogService>(); 
            this.tasks = dataAcquisitionTasks;
            this.TaskQueue = new Queue<DataAcquisitionTask>(dataAcquisitionTasks);
        }

        #region Fields & Properties

        /// <summary>
        /// 任务顺序执行信号量
        /// </summary>
        private SemaphoreSlim semaphore = null!;
        /// <summary>
        /// 任务队列
        /// </summary>
        private Queue<DataAcquisitionTask> TaskQueue { set; get; }
        /// <summary>
        /// 是否可执行
        /// </summary>
        private bool CanExecute { set; get; } = false;
        /// <summary>
        /// 当前任务数据类型
        /// </summary>
        public AcquisitionType CurrentTaskAcquisitionType { get; set; } = AcquisitionType.DarkField;

        #endregion

        #region Events

        public event EventHandler<string>? OnTaskExecutionProcessMessageUpdated;

        #endregion

        public async Task StartAsync() 
        {

            //重置信号量
            ResetSignal();
            //标志位置为可执行
            CanExecute = true;
            //遍历队列
            foreach (var task in TaskQueue) 
            {
                //Stop校验
                if (!CanExecute)
                {
                    OnTaskExecutionProcessMessageUpdated?.Invoke(this, $"The {nameof(QueuedDataAcquisitionService)} execution has been stopped.");
                    return;
                }
                //更新过程信息
                OnTaskExecutionProcessMessageUpdated?.Invoke(this, $"Prepare to execute [{task.Name}] task.");
                //更新当前数据类型
                CurrentTaskAcquisitionType = task.AcquisitionType;
                //显示当前采集类型
                OnTaskExecutionProcessMessageUpdated?.Invoke(this, $"CurrentTaskAcquisitionType: {CurrentTaskAcquisitionType}.");
                //延时
                await Task.Delay(1000);
                //下发采集指令
                var response = await task.ExecuteAsync();
                //任务执行校验
                if (!response.Status)
                {
                    OnTaskExecutionProcessMessageUpdated?.Invoke(this, $"Failed to execute [{task.Name}] task.");

                    return;
                }
                else
                {
                    OnTaskExecutionProcessMessageUpdated?.Invoke(this, $"[{task.Name}] commands has been sent, waiting for data.");
                    //等待信号
                    semaphore.Wait();
                }
            }
            //最后一次采集等待数据信号
            if (CanExecute)
            {
                //结束信号
                OnTaskExecutionProcessMessageUpdated?.Invoke(this, $"All data acquisition tasks has been finished.");
            }
        }

        public void Stop() 
        {
            //打印
            OnTaskExecutionProcessMessageUpdated?.Invoke(this, $"Prepare to stop {nameof(QueuedDataAcquisitionService)}.");
            //重置
            Reset();
        }

        public void Reset() 
        {
            //标志位置为不可执行
            CanExecute = false;
            //释放信号
            if (semaphore.CurrentCount == 0)
            {
                semaphore.Release();
            }
            //重置队列
            TaskQueue = new Queue<DataAcquisitionTask>(tasks);
            //重置信号量
            ResetSignal();
        }

        /// <summary>
        /// 重置信号量
        /// </summary>
        private void ResetSignal() 
        {
            semaphore = new SemaphoreSlim(0, 1);
        }

        /// <summary>
        /// 释放信号量
        /// </summary>
        public void ReleaseSignal() 
        {
            if (semaphore.CurrentCount == 0)
            {
                semaphore.Release();
            }
        }

    }

}
