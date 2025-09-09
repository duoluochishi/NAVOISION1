using NV.CT.CTS.Enums;
using NV.CT.UI.ViewModel;
using System;

namespace NV.CT.JobViewer.Models;

public class JobTaskBaseInfo : BaseViewModel
{
    protected readonly string _finishedColor = "#1665D8";
    protected readonly string _processingColor = "#FFA640";
    protected readonly string _stoppedColor = "#FF0000";

    private string _jobId = string.Empty;
    public string JobId
    {
        get
        {
            return this._jobId;
        }
        set
        {
            this.SetProperty(ref this._jobId, value);
        }
    }

    private string _workflowId = string.Empty;
    public string WorkflowId
    {
        get
        {
            return this._workflowId;
        }
        set
        {
            this.SetProperty(ref this._workflowId, value);
        }
    }

    private string _taskDescription = string.Empty;
    public string TaskDescription
    {
        get
        {
            return this._taskDescription;
        }
        set
        {
            this.SetProperty(ref this._taskDescription, value);
        }
    }

    private string _jobStatus = string.Empty;
    public string JobStatus
    {
        get
        {
            return this._jobStatus;
        }
        set
        {
            this.SetProperty(ref this._jobStatus, value);
        }
    }

    private short _priority = 0;
    public short Priority
    {
        get
        {
            return this._priority;
        }
        set
        {
            this.SetProperty(ref this._priority, value);
        }
    }

    private float _progress = 0f;
    public float Progress
    {
        get
        {
            return this._progress;
        }
        set
        {
            this.SetProperty(ref this._progress, value);
        }
    }
    private string _foreground = string.Empty;
    public string Foreground
    {
        get
        {
            return this._foreground;
        }
        set
        {
            this.SetProperty(ref this._foreground, value);
        }
    }

    private DateTime _createdTime;
    public DateTime CreatedTime
    {
        get
        {
            return this._createdTime;
        }
        set
        {
            this.SetProperty(ref this._createdTime, value);
        }
    }

    private DateTime? _startTime;
    public DateTime? StartTime
    {
        get
        {
            return this._startTime;
        }
        set
        {
            this.SetProperty(ref this._startTime, value);
        }
    }

    private DateTime? _endTime;
    public DateTime? EndTime
    {
        get
        {
            return this._endTime;
        }
        set
        {
            this.SetProperty(ref this._endTime, value);
        }
    }

    private string _errorMessage = string.Empty;
    public string ErrorMessage
    {
        get
        {
            return this._errorMessage;
        }
        set
        {
            this.SetProperty(ref this._errorMessage, value);
        }
    }

    public void SetForegroundColor(JobTaskStatus taskStatus, float? progress = null)
    {
        JobStatus = taskStatus.ToString();
        switch (taskStatus)
        {
            case JobTaskStatus.Queued:
                Progress = 0f;
                break;
            case JobTaskStatus.Processing:
            case JobTaskStatus.Paused:
                Progress = progress is null ? 50f : progress.Value;
                Foreground = _processingColor;
                break;
            case JobTaskStatus.PartlyCompleted:
                Progress = 50f;
                Foreground = _finishedColor;
                break;
            case JobTaskStatus.Completed:
                Progress = 100f;
                Foreground = _finishedColor;
                break;
            case JobTaskStatus.Failed:
            case JobTaskStatus.Cancelled:
                Progress = 100f;
                Foreground = _stoppedColor;
                break;
            default:
                break;
        }

    }
}
