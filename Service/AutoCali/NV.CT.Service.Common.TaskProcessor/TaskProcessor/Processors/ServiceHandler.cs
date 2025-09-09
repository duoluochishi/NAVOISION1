using NV.CT.Service.Common.TaskProcessor.Interfaces;
using NV.CT.Service.Common.TaskProcessor.Models;
using System.Threading;

namespace NV.CT.Service.Common.TaskProcessor.Processors
{
    // 服务基类
    //public abstract class ServiceHandlerBase : IExternalService
    //{
    //    private ServiceStatus _status = ServiceStatus.Idle;
    //    private CancellationTokenSource _cts;

    //    public event EventHandler<StatusChangedEventArgs> StatusChanged;
    //    public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

    //    public ServiceStatus CurrentStatus => _status;

    //    protected virtual void OnStatusChanged(ServiceStatus newStatus)
    //    {
    //        _status = newStatus;
    //        StatusChanged?.Invoke(this, new StatusChangedEventArgs(newStatus));
    //    }

    //    protected virtual void OnProgressChanged(int progress, string message)
    //    {
    //        ProgressChanged?.Invoke(this, new ProgressChangedEventArgs(progress));
    //    }

    //    public bool RequestStart()
    //    {
    //        if (_status == ServiceStatus.Running)
    //            throw new InvalidOperationException("Service is already running");

    //        //_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    //        OnStatusChanged(ServiceStatus.Running);

    //        try
    //        {
    //            _= ExecuteAsync();
    //            OnStatusChanged(ServiceStatus.Completed);
    //            return Task.FromResult(true);
    //        }
    //        catch (OperationCanceledException)
    //        {
    //            OnStatusChanged(ServiceStatus.Stopped);
    //        }
    //        catch
    //        {
    //            OnStatusChanged(ServiceStatus.Error);
    //            throw;
    //        }
    //    }

    //    public async Task<bool> RequestStop()
    //    {
    //        if (_status != ServiceStatus.Running) return false;

    //        _cts?.Cancel();
    //        OnStatusChanged(ServiceStatus.Stopped);
    //        await Task.CompletedTask;
    //    }

    //    protected abstract bool ExecuteAsync();
    //}

    ///// <summary>
    ///// 模拟离线校准服务
    ///// </summary>
    //public class OfflineCalibrationServiceHandler_Mock : ServiceHandlerBase
    //{
    //    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    //    {
    //        try
    //        {
    //            OnProgressChanged(0, "Starting calibration...");

    //            for (int i = 1; i <= 5; i++)
    //            {
    //                cancellationToken.ThrowIfCancellationRequested();
    //                await Task.Delay(1000, cancellationToken);
    //                OnProgressChanged(i * 20, $"Calibration phase {i}/5");
    //            }

    //            OnProgressChanged(100, "Calibration successful");
    //        }
    //        catch
    //        {
    //            OnProgressChanged(0, "Calibration failed");
    //            throw;
    //        }
    //    }
    //}
}
