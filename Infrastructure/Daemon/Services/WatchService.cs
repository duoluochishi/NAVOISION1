//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/20 12:46:45     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NV.CT.Daemon.Models;
using System.Diagnostics;

namespace NV.CT.Daemon.Services;

public class WatchService : IHostedService
{
    private readonly IList<AppInfo> _monitorApplications;
    private System.Timers.Timer _timer;
    private readonly ILogger<WatchService> _logger;

    public WatchService(IOptions<List<AppInfo>> applications, ILogger<WatchService> logger)
    {
        _monitorApplications = new List<AppInfo>();
        foreach(var app in applications.Value)
        {
            if (app.IsLaunch && (app.IsMonitor || app.IsRestart))
            {
                _monitorApplications.Add(app);
            }
        }
        _logger = logger;
    }

    private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        _logger.LogInformation($"Monitor {_monitorApplications.Count} services...");
        var applications = Global.Instance.Applications.Values.ToArray();
        foreach (var app in _monitorApplications)
        {
            var application = applications.FirstOrDefault(a => a.Name == app.Name);
            if (application is null)
            {
                _logger.LogInformation($"{app.ProcessName} is not exists.");
                continue;
            }
            var process = Process.GetProcessById(application.ProcessId);
            if (process is null)
            {
                _logger.LogInformation($"{application.ProcessName} is not exists.");
                continue;
            }

            _logger.LogInformation($"{application.ProcessName} is exists.");
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_monitorApplications is not null && _monitorApplications.Count > 0)
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += Timer_Elapsed;
            _timer.Interval = 1000;
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
