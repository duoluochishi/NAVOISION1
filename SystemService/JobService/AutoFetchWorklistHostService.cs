//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/10/18 13:45:36    V1.0.0        胡安
// </summary>
//-----------------------------------------------------------------------
using AutoMapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NV.CT.ConfigService.Contract;
using NV.CT.ConfigService.Models.UserConfig;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.Job.Contract.Model;
using NV.CT.JobService.Contract;
using NV.MPS.Configuration;
using System.Windows.Threading;

namespace NV.CT.JobService;
public class AutoFetchWorklistHostService : IHostedService
{
    private readonly ILogger<AutoFetchWorklistHostService> _logger;
    private readonly IJobRequestService _jobRequestService;
    //private DispatcherTimer _timer = new DispatcherTimer();
    private System.Timers.Timer _timer=new System.Timers.Timer();
    //private readonly int _interval = 30000; //秒
    //private LocalDicomSettingInfo _localDicomSettings;
    private WorklistInfo _worklist;
    private IPatientConfigService _patientConfigService;

    public AutoFetchWorklistHostService(ILogger<AutoFetchWorklistHostService> logger, IJobRequestService jobRequestService,IPatientConfigService patientConfigService)
    {
        this._logger = logger;
        this._jobRequestService = jobRequestService;
        _patientConfigService=patientConfigService;
        this.StartTimer();
    }

    private void StartTimer()
    {
        try
        {
            //_localDicomSettings = UserConfig.LocalDicomSettingConfig.LocalDicomSetting;
            var patientConfig=  _patientConfigService.GetConfigs();
            var worklistConfig = UserConfig.WorklistConfig;
             _worklist = worklistConfig.Worklists.FirstOrDefault(r => r.IsDefault);
            if (_worklist is null) return;
            //_timer.Interval = new TimeSpan(0, 0, _interval);
            //_timer.Tick += Timer_Tick;
            _timer.Interval = patientConfig.RefreshTimeInterval.Interval*1000;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }
        catch
        {
            this._logger.LogError("Failed to load LocalDicomSettings in AutoFetchWorklistHostService.StartTimer().");
            return;
        }
    }

    private void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        this.FetchAutoWorklist();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        this.FetchAutoWorklist();
    }

    private void FetchAutoWorklist()
    {
        var jobRequest = new WorklistJobRequest();
        jobRequest.Id = Guid.NewGuid().ToString();
        jobRequest.WorkflowId = Guid.NewGuid().ToString();
        jobRequest.Priority = 5;
        jobRequest.JobTaskType = JobTaskType.WorklistJob;
        jobRequest.Creator = string.Empty;
        jobRequest.Host = _worklist.IP;
        jobRequest.Port = _worklist.Port;
        jobRequest.AECaller = _worklist.Name;
        jobRequest.AETitle = _worklist.AETitle;
        jobRequest.PatientName = string.Empty;
        jobRequest.PatientId = string.Empty;
        jobRequest.PatientSex = string.Empty;
        jobRequest.AccessionNumber = string.Empty;
        jobRequest.ReferringPhysicianName = string.Empty;
        jobRequest.ReferringPhysicianName = string.Empty;
        jobRequest.ReferringPhysicianName = string.Empty;
        jobRequest.StudyDateStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        jobRequest.StudyDateEnd = jobRequest.StudyDateStart; // already has been handled in Executor like this: jobRequest.StudyDateStart.AddDays(1).AddSeconds(-1);
        jobRequest.Parameter = jobRequest.ToJson();

        this._jobRequestService.EnqueueJobRequest(jobRequest);
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_timer is not null)
        {
            //_timer.IsEnabled = false;
            //_timer.Stop();
        }

        return Task.CompletedTask;
    }
}