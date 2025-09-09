using NV.CT.Service.Common.TaskProcessor.Interfaces;
using NV.CT.Service.Common.TaskProcessor.Models;
using System.Text.RegularExpressions;
using System.Threading;

namespace NV.CT.Service.Common.TaskProcessor.Processors
{
    public class ServiceController : IDisposable
    {
        private readonly IExternalService _externalService;
        private ServiceStatus _currentStatus;
        private CancellationTokenSource _startTimeoutCts;
        private TaskCompletionSource<bool> _processingTaskCompletionSource;
        private CancellationTokenSource _processingCancellationTokenSource;

        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public ServiceStatus CurrentStatus => _currentStatus;

        public ServiceController(IExternalService externalService)
        {
            _externalService = externalService ?? throw new ArgumentNullException(nameof(externalService));
            _currentStatus = ServiceStatus.Idle;
        }

        public async Task<bool> StartAsync()
        {
            string logLineHead = $"[{nameof(ServiceController)}] [{nameof(StartAsync)}]";
            if (_currentStatus != ServiceStatus.Idle)
                return false;

            //_startTimeoutCts = new CancellationTokenSource();
            try
            {
                StartWatchUntilFinished();

                UpdateStatus(ServiceStatus.Starting);


                //处理开始服务的响应结果：成功 / 失败
                Console.WriteLine($"{logLineHead} Beginning to _externalService.RequestStart() in thread '{Thread.CurrentThread.ManagedThreadId}';");
                bool startSuccess = _externalService.RequestStart();
                if (!startSuccess)
                {
                    //错误码
                    UpdateStatus(ServiceStatus.Error);
                    return false;
                }

                //更新服务状态：运行中
                if (_currentStatus != ServiceStatus.Running)
                {
                    UpdateStatus(ServiceStatus.Running);
                }

                //创建消令牌，当收到停止命令时，触发令牌消息
                _processingTaskCompletionSource = new TaskCompletionSource<bool>();
                _processingCancellationTokenSource = new CancellationTokenSource();

                Console.WriteLine($"{logLineHead} Ended to  _externalService.RequestStart() in thread '{Thread.CurrentThread.ManagedThreadId}';");
                return await _processingTaskCompletionSource.Task;
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"{logLineHead} catch the designed exception '{nameof(OperationCanceledException)}' in thread '{Thread.CurrentThread.ManagedThreadId}';");
                UpdateStatus(ServiceStatus.Cancelled);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{logLineHead} catch a unexpected exception {ex} in thread '{Thread.CurrentThread.ManagedThreadId}';");
                UpdateStatus(ServiceStatus.Error);
                return false;
            }
            finally
            {
                Console.WriteLine($"{logLineHead} step into finally in thread '{Thread.CurrentThread.ManagedThreadId}';");
                _startTimeoutCts?.Dispose();
                _startTimeoutCts = null;
            }
        }

        public async Task<bool> StopAsync()
        {
            string logLineHead = $"[{nameof(ServiceController)}] [{nameof(StopAsync)}]";
            if (_currentStatus is not (ServiceStatus.Running or ServiceStatus.Starting))
                return false;

            try
            {
                UpdateStatus(ServiceStatus.Cancelling);

                Console.WriteLine($"{logLineHead} _processingCts?.Cancel() in thread '{Thread.CurrentThread.ManagedThreadId}';");

                _processingCancellationTokenSource?.Cancel();

                Console.WriteLine($"{logLineHead} bool stopResult = await _externalService.StopAsync(_processingCts.Token) in thread '{Thread.CurrentThread.ManagedThreadId}';");

                bool stopResult = _externalService.RequestStop();
                Console.WriteLine($"{logLineHead} stopResult={stopResult}");

                _processingTaskCompletionSource?.TrySetResult(stopResult);

                UpdateStatus(ServiceStatus.Idle);
                return stopResult;
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException)
                {
                    Console.WriteLine($"{logLineHead} catch the designed TaskCanceledException in thread '{Thread.CurrentThread.ManagedThreadId}';");
                    UpdateStatus(ServiceStatus.Cancelled);
                }
                else
                {
                    Console.WriteLine($"{logLineHead} catch a exception:{ex} in thread '{Thread.CurrentThread.ManagedThreadId}';");
                    UpdateStatus(ServiceStatus.Error);
                }

                return false;
            }
        }

        private void OnStatusChanged(object sender, StatusChangedEventArgs e)
        {
            var newStatus = e.NewStatus;
            if (_currentStatus != newStatus)
            {
                this._manualResetEventSlim.Set();
            }

            UpdateStatus(newStatus);

            if (newStatus is ServiceStatus.Completed or ServiceStatus.Error)
            {
                _processingTaskCompletionSource?.TrySetResult(newStatus == ServiceStatus.Completed);
            }
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        private void UpdateStatus(ServiceStatus newStatus)
        {
            _currentStatus = newStatus;
            StatusChanged?.Invoke(this, new StatusChangedEventArgs(_currentStatus));
        }

        public void Dispose()
        {
            _startTimeoutCts?.Dispose();
            _processingCancellationTokenSource?.Dispose();

            UnRegisterServiceEvent();
        }

        private ManualResetEventSlim _manualResetEventSlim;

        private void RegisterServiceEvent()
        {
            UnRegisterServiceEvent();

            _externalService.StatusChanged += OnStatusChanged;
            _externalService.ProgressChanged += OnProgressChanged;
        }
        private void UnRegisterServiceEvent()
        {
            _externalService.StatusChanged -= OnStatusChanged;
            _externalService.ProgressChanged -= OnProgressChanged;
        }

        private void StartWatchUntilFinished()
        {
            if (_manualResetEventSlim == null)
            {
                _manualResetEventSlim = new(false);

                RegisterServiceEvent();
            }

            //_ = 
            Task.Factory.StartNew(() =>
            {
                try
                {
                    int loopCount = 1;
                    while (true)
                    {
                        Console.WriteLine($"[{nameof(Monitor)}] [{loopCount}] Beginning to _manualResetEventSlim.Wait() in thread '{Thread.CurrentThread.ManagedThreadId}';");

                        _manualResetEventSlim.Wait();

                        Console.WriteLine($"[{nameof(Monitor)}] [{loopCount}]  Ended to _manualResetEventSlim.Wait() in thread '{Thread.CurrentThread.ManagedThreadId}';");

                        if (_currentStatus >= ServiceStatus.Cancelled)
                        {
                            Console.WriteLine($"[{nameof(Monitor)}] [{loopCount}]  Finished and breaking the loop due to service Status is {_currentStatus} in thread '{Thread.CurrentThread.ManagedThreadId}';");

                            _processingTaskCompletionSource?.TrySetResult(true);
                            break;
                        }
                        _manualResetEventSlim.Reset();
                        loopCount++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{nameof(StartAsync)}] catch a exception:{ex} in thread '{Thread.CurrentThread.ManagedThreadId}';");
                }
            });
        }
    }
}
