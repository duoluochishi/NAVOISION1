//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2025/07/14 11:22:16           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.CTS.Models;
using NV.CT.JobService.Contract;

namespace NV.CT.Examination.ApplicationService.Impl;

public class OfflineReconService : IOfflineReconService
{
    private readonly ILogger<OfflineReconService> _logger;
    private readonly IOfflineTaskService _offlineService;

    public OfflineReconService(ILogger<OfflineReconService> logger,
        IOfflineTaskService offlineService)
    {
        _logger = logger;
        _offlineService = offlineService;       
    }

    public OfflineTaskInfo GetReconTask(string studyId, string scanId, string reconId)
    {
        try
        {
            var response = _offlineService.GetTask(reconId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"GetReconTaskStatus exception: {ex.Message}");
        }
        return default!;
    }

    public OfflineCommandResult CreateReconTask(string studyId, string scanId, string reconId)
    {
        try
        {
            var response = _offlineService.CreateTask(studyId, scanId, reconId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"CreateReconTask exception: {ex.Message}");
        }
        return default!;
    }

    public OfflineCommandResult StartReconTask(string studyId, string scanId, string reconId)
    {
        try
        {
            return _offlineService.CreateTask(studyId, scanId, reconId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"StartReconTask rpc exception: {ex.Message}");
        }
        return default!;
    }

    public OfflineCommandResult CloseReconTask(string studyId, string scanId, string reconId)
    {
        try
        {
            return _offlineService.StopTask(reconId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"CloseReconTask exception: {ex.Message}");
        }
        return default!;
    }

    public void DeleteTask(string studyId, string scanId, string reconId)
    {
        try
        {
            _offlineService.DeleteTask(reconId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"StartReconTask rpc exception: {ex.Message}");
        }
    }

    public void StartAutoRecons(string studyId)
    {
        try
        {
            _offlineService.StartAutoRecons(studyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"StartAllReconTasks exception: {ex.Message}");
        }
    }

    public OfflineCommandResult StartAllReconTasks(string studyId)
    {
        try
        {
            _offlineService.CreateTasks(studyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"StartAllReconTasks exception: {ex.Message}");
        }
        return default!;
    }
}