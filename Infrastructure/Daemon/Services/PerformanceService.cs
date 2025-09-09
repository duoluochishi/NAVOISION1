using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NV.CT.Daemon.Helpers;
using NV.CT.Daemon.Models;
using NV.CT.Daemon.Natives;
using System.Diagnostics;
using System.Text;

namespace NV.CT.Daemon.Services;

public class PerformanceService : IHostedService
{
    private List<AppInfo> _performanceApplications;
    private System.Timers.Timer _timer;
    private readonly ILogger<PerformanceService> _logger;

    public PerformanceService(IOptions<List<AppInfo>> applications, ILogger<PerformanceService> logger)
    {
        _performanceApplications = new List<AppInfo>();
        foreach(var appInfo in applications.Value)
        {
            if (appInfo.IsLaunch)
            {
                _performanceApplications.Add(appInfo);
            }
        }
        _logger = logger;
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _logger.LogInformation($"{_performanceApplications.Count} services...");

        var memoryInfo = WindowMemory.GetMemory();
        var totalPhysicalMemory = SizeInfo.Get((long)memoryInfo.TotalPhysicalMemory);
        var availablePhysicalMemory = SizeInfo.Get((long)memoryInfo.AvailablePhysicalMemory);
        var totalVirtualMemory = SizeInfo.Get((long)memoryInfo.TotalVirtualMemory);
        var availableVirtualMemory = SizeInfo.Get((long)memoryInfo.AvailableVirtualMemory);
        _logger.LogInformation($"Memory Usage: {memoryInfo.UsedPercentage}%, Total Physical Memory: {totalPhysicalMemory.Size.ToString("F2")}{totalPhysicalMemory.SizeType}, Available Physical Memory: {availablePhysicalMemory.Size.ToString("F2")}{availablePhysicalMemory.SizeType}, Total Virtual Memory: {totalVirtualMemory.Size.ToString("F2")}{totalVirtualMemory.SizeType}, Available Virtual Memory: {availableVirtualMemory.Size.ToString("F2")}{availableVirtualMemory.SizeType}");

        var processes = Process.GetProcesses().Where(p => p.ProcessName.Contains("NV.CT.")).ToList();
        if (!processes.Any())
        {
            _logger.LogInformation("No NV.CT.* processes.");
            return;
        }

        var sb = new StringBuilder();
        foreach (var process in processes)
        {
            sb.Append($"Process Name: {process.ProcessName}");
            sb.Append($", Execution File: {process.MainModule.FileName}");
            sb.Append($", Version Info: {process.MainModule.FileVersionInfo.FileVersion}");
            sb.Append($", Thread Count: {process.Threads.Count}");
            sb.Append($", UsingTime: {process.TotalProcessorTime}");
            var workingSet = SizeInfo.Get(process.WorkingSet64);
            sb.Append($", UsingMemory: {workingSet.Size}{workingSet.SizeType}");

            if (_performanceApplications.Any())
            {
                if (_performanceApplications.Any(p => process.ProcessName.Contains(p.ProcessName)))
                {
                    sb.Append($", Monitor Performance: true");
                }
            }

            if (Global.Instance.Applications.Values.Any())
            {
                var applications = Global.Instance.Applications.Values.ToArray();
                if (!applications.Any(p => process.ProcessName.Contains(p.ProcessName)))
                {
                    sb.Append($", Default Launch: false");
                }
            }

            _logger.LogInformation(sb.ToString());

            sb.Clear();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_performanceApplications is not null && _performanceApplications.Count > 0)
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += Timer_Elapsed;
            _timer.Interval = 30000;
            _timer.Start();
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }

        return Task.CompletedTask;
    }
}
