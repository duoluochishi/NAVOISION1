//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.WorkflowService.Contract;
using NV.CT.DatabaseService.Contract;

namespace NV.CT.WorkflowService.Impl;

public class PrintService : IPrint
{
    private readonly ILogger<WorkflowService> _logger;
    private readonly IPrintConfigManager _printConfigManager;

    public event EventHandler<string>? StudyChanged;
    public event EventHandler<EventArgs<(string, JobTaskStatus)>>? PrintStatusChanged;
    public event EventHandler<string>? PrintStarted;
    public event EventHandler<string>? PrintClosed;

    private string _studyId = string.Empty;
    public string StudyId
    {
        get => _studyId;
        set
        {
            if (value != _studyId)
            {
                if (!string.IsNullOrEmpty(_studyId))
                {
                    this.SavePreviousPrintConfig(_studyId);
                }
                _studyId = value;
                StudyChanged?.Invoke(this, _studyId);
            }
        }
    }

    private JobTaskStatus _printStatus = JobTaskStatus.Unknown;
    public JobTaskStatus PrintStatus
    {
        get => _printStatus;
        set
        {
            _printStatus = value;
            PrintStatusChanged?.Invoke(this, new EventArgs<(string, JobTaskStatus)>((StudyId, value)));
        }
    }

    public PrintService(ILogger<WorkflowService> logger, IPrintConfigManager printConfigManager)
    {
        _logger = logger;
        _printConfigManager = printConfigManager;
    }

    public bool ChceckExists()
    {
        _logger.LogDebug($"Check print exist: {!string.IsNullOrEmpty(this.StudyId)}");
        return !string.IsNullOrEmpty(this.StudyId);
    }

    public void StartPrint(string studyId)
    {
        _logger.LogDebug($"Start Print for study: {studyId}");
        this.StudyId = studyId;
        this.PrintStatus = CTS.Enums.JobTaskStatus.Queued;

        PrintStarted?.Invoke(this, studyId);
    }

    public void ClosePrint()
    {
        _logger.LogDebug($"Close print for current study with id: {this.StudyId}");
        if (!string.IsNullOrEmpty(this.StudyId))
        {
            this.SavePreviousPrintConfig(this.StudyId);
            _logger.LogDebug($"Close print: {this.StudyId}");
            this.StudyId = string.Empty;
            this.PrintStatus = CTS.Enums.JobTaskStatus.Completed;
        }

        PrintClosed?.Invoke(this, this.StudyId);
    }

    public string GetCurrentStudyId()
    {
        return this.StudyId;    
    }

    private void SavePreviousPrintConfig(string studyId)
    {
        try
        {
            _printConfigManager.Save(_studyId);
        }
        catch (Exception ex) 
        {
            this._logger.LogDebug($"Failed to SavePreviousPrintConfig in PrintService with execption:{ex.Message}");
        }           
    
    }

}