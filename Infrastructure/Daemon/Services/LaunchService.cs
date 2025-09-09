//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/3/20 12:46:04     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NV.CT.Daemon.Models;
using NV.MPS.Environment;
using System.Diagnostics;

namespace NV.CT.Daemon.Services;

public class LaunchService
{
    private readonly IList<AppInfo> _applications;
    private readonly ILogger<LaunchService> _logger;

    public LaunchService(IOptions<List<AppInfo>> applications, ILogger<LaunchService> logger)
    {
        _applications = applications.Value;
        _logger = logger;
    }

    public void Start()
    {
        _logger.LogInformation("Begin to start NV.CT system...");
        foreach (var application in _applications)
        {
            if (!application.IsLaunch) continue;
            StartProcess(application);
            Thread.Sleep(1000);
            CheckProcess(application);
        }
        _logger.LogInformation("Finish to start.");
    }

    private void Process_Exited(object? sender, EventArgs e)
    {
        var process = sender as Process;
        _logger.LogInformation($"{process.ProcessName} exit.");
        var info = Global.Instance.Applications.FirstOrDefault(p => p.Value.ProcessId == process.Id);
        if (info.Value is not null)
        {
            Global.Instance.Applications.TryRemove(info);
            if (info.Value.IsMonitor || info.Value.IsRestart)
            {
                StartProcess(new AppInfo {
                    Name = info.Value.Name,
                    IsRelative = info.Value.IsRelative,
                    Path = info.Value.Path,
                    IsLaunch = info.Value.IsLaunch,
                    IsMonitor = info.Value.IsMonitor,
                    IsRestart = info.Value.IsRestart,
                    IsShowWindow = info.Value.IsShowWindow
                });
            }
            _logger.LogInformation($"{process.ProcessName} is removed.");
        }
    }

    private void StartProcess(AppInfo application)
    {
        string fileName = string.Empty;

        _logger.LogInformation($"{application.IsRelative}, {(application.IsRelative ? RuntimeConfig.Console.MCSBin.Path : application.Path)}, {application.Name}");
        if (application.IsRelative)
        {
            fileName = Path.Combine(RuntimeConfig.Console.MCSBin.Path, application.Name);
        }
        else
        {
            fileName = Path.Combine(application.Path, application.Name);
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = application.IsShowWindow,
                CreateNoWindow = !application.IsShowWindow,
            }
        };
        if (application.IsMonitor || application.IsRestart)
        {
            process.EnableRaisingEvents = true;
            process.Exited += Process_Exited;
        }
        process.Start();
        application.ProcessId = process.Id;
        application.ProcessName = process.ProcessName;

        StartMonitorDump(process);

        Global.Instance.Applications.TryAdd(application.ProcessName, application);
    }

    private void CheckProcess(AppInfo application)
    {
        var process = Process.GetProcesses().FirstOrDefault(p => application.Name.Contains(p.ProcessName));
        if (process is null)
        {
            _logger.LogWarning($"{application.Name} startup failure.");
        }
    }

    private void StartMonitorDump(Process process)
    {
        //todo:ProcDump不可用于商业软件
        //var dumpProcess = new Process
        //{
        //    StartInfo = new ProcessStartInfo
        //    {
        //        FileName = Path.Combine(RuntimeConfig.Instance.Bin, "procdump64.exe"),
        //        UseShellExecute = false,
        //        CreateNoWindow = true,
        //        Arguments = $"-ma -t {process.Id}"
        //    }
        //};
        //dumpProcess.Start();
    }
}
