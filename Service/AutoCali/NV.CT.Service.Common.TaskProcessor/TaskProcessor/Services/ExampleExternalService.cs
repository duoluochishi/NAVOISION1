using NV.CT.Service.Common.TaskProcessor.Interfaces;
using NV.CT.Service.Common.TaskProcessor.Models;
using System;
using System.Threading;

namespace NV.CT.Service.Common.TaskProcessor.Services
{
    public class ExampleExternalService : IExternalService
    {
        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public bool RequestStart()
        {
            string logLineHead = $"[{nameof(ExampleExternalService)}] [{nameof(RequestStart)}]";

            // 模拟启动过程
            int launchMilliSeconds = Random.Shared.Next(500, 3000);
            Console.WriteLine($"[{nameof(ExampleExternalService)}] [{nameof(RequestStart)}] will be launch in {launchMilliSeconds}ms");
            Task.Delay(launchMilliSeconds).Wait();
            //await xxxProxy.StartXxx();
            OnStatusChanged(ServiceStatus.Running);

            //模拟异步执行，不等待
            ExecuteAsync();
            return true;
        }

        public bool RequestStop()
        {
            string logLineHead = $"[{nameof(ExampleExternalService)}] [{nameof(RequestStop)}]";
            Console.WriteLine($"{logLineHead}] Entering");

            try
            {
                // 模拟停止过程
                Console.WriteLine($"{logLineHead}] await Task.Delay(3000);in thread '{Thread.CurrentThread.ManagedThreadId}';");
                _cts.Cancel();
                Task.Delay(3000).Wait();

                Console.WriteLine($"{logLineHead}] StatusChanged?.Invoke(this, new StatusChangedEventArgs(ServiceStatus.Stopping));in thread '{Thread.CurrentThread.ManagedThreadId}';");
                OnStatusChanged(ServiceStatus.Cancelling);
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                {
                    Console.WriteLine($"{logLineHead} catch the designed TaskCanceledException in thread '{Thread.CurrentThread.ManagedThreadId}';");
                }
                else
                {
                    Console.WriteLine($"{logLineHead} catch a exception:{ex}in thread '{Thread.CurrentThread.ManagedThreadId}';");
                }
            }
            return true;
        }

        private CancellationTokenSource _cts = new CancellationTokenSource();

        public string TaskId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private void ExecuteAsync()
        {
            string logLineHead = $"[{nameof(ExampleExternalService)}] [{nameof(ExecuteAsync)}]";
            // 模拟长时间处理
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    for (int i = 1; i <= 101; i += 10)
                    {
                        Console.WriteLine($"{logLineHead} Progress {i} / 100");
                        Console.WriteLine($"{logLineHead} Beginning to await Task.Delay(500) in thread '{Thread.CurrentThread.ManagedThreadId}';");
                        await Task.Delay(500, _cts.Token);
                        Console.WriteLine($"{logLineHead} ended to await Task.Delay(500) in thread '{Thread.CurrentThread.ManagedThreadId}';");
                        //Console.WriteLine($"{logLineHead}] ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(i));");
                        //ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(i));
                        OnProgressChanged(i);
                    }

                    OnStatusChanged(ServiceStatus.Completed);
                }
                catch (Exception ex)
                {
                    if (ex is TaskCanceledException)
                    {
                        Console.WriteLine($"{logLineHead}] Task is cancelled in thread '{Thread.CurrentThread.ManagedThreadId}';");
                        OnStatusChanged(ServiceStatus.Cancelled);
                    }
                    else
                    {
                        Console.WriteLine($"{logLineHead}] catch a exception:{ex} in thread '{Thread.CurrentThread.ManagedThreadId}';");
                        OnStatusChanged(ServiceStatus.Error);
                    }
                }
            });
        }

        protected virtual void OnStatusChanged(ServiceStatus serviceStatus)
        {
            StatusChanged?.Invoke(this, new(serviceStatus));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="progressPercentage">百分数，比如，0.5065，对应50.65%</param>
        protected virtual void OnProgressChanged(double progressPercentage)
        {
            ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(progressPercentage));
        }
    }
}
