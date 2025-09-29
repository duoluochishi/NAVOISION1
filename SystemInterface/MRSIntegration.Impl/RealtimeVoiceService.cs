//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/8/7 8:58:50     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Helpers;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Arguments;
using NV.CT.FacadeProxy.Common.Enums.AudioFileEnums;
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Extensions;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using System.Collections.Concurrent;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class RealtimeVoiceService : IRealtimeVoiceService
{
    private readonly ILogger<RealtimeVoiceService> _logger;
    private readonly IMapper _mapper;
    private readonly IRealtimeProxyService _proxyService;
    private AuxBoard _auxBoard;
    private ConcurrentDictionary<(AudioFileType, uint), WeakReference<Action<AudioPlaybackEventArgs>>> playAudioCallbacks = new ConcurrentDictionary<(AudioFileType, uint), WeakReference<Action<AudioPlaybackEventArgs>>>();

    public RealtimeVoiceService(ILogger<RealtimeVoiceService> logger, IMapper mapper, IRealtimeProxyService proxyService)
    {
        _logger = logger;
        _mapper = mapper;
        _proxyService = proxyService;
        _auxBoard = _proxyService.AuxBoard;
        DeviceInteractProxy.Instance.AudioPlaybackCompleted += AudioPlaybackCompleted;
    }

    private void AudioPlaybackCompleted(object? sender, AudioPlaybackEventArgs e)
    {
        if (playAudioCallbacks.TryRemove((e.FileType, e.ID), out var weakRef))
        {
            if (weakRef.TryGetTarget(out var action))
            {
                try
                {
                    action.Invoke(e);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error in AudioPlaybackCompleted callback");
                }
            }
        }
    }

    public RealtimeCommandResult AddOrUpdate(ushort id, string filePath)
    {
        _logger.LogInformation($"Add or update API: {filePath}");

        try
        {
            var result = PerformanceMonitorHelper.Execute("AuxBoard.AddOrUpdate", () => _auxBoard.AddOrUpdateAPI(id, filePath));
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"AddOrUpdate failed, exception: {ex.Message} {System.Environment.NewLine} {ex.StackTrace}");
            var errorResult = new RealtimeCommandResult { Status = CommandExecutionStatus.Failure };
            errorResult.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "AuxBoard.AddOrUpdate")) };
            return errorResult;
        }
    }

    public RealtimeCommandResult Delete(ushort id)
    {
        _logger.LogInformation($"Delete API: {id}");

        try
        {
            var result = PerformanceMonitorHelper.Execute("AuxBoard.Delete", () => _auxBoard.DeleteAPI(id));
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Delete failed, exception: {ex.Message} {System.Environment.NewLine} {ex.StackTrace}");
            var errorResult = new RealtimeCommandResult { Status = CommandExecutionStatus.Failure };
            errorResult.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "AuxBoard.Delete")) };
            return errorResult;
        }
    }

    public RealtimeCommandResult GetAll(out ushort[] ids)
    {
        _logger.LogInformation($"Get API list");

        try
        {
            var result = _auxBoard.GetAPIs(out ids);
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetAPIs failed, exception: {ex.Message} {System.Environment.NewLine} {ex.StackTrace}");
            ids = new List<ushort>().ToArray();
            var errorResult = new RealtimeCommandResult { Status = CommandExecutionStatus.Failure };
            errorResult.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "AuxBoard.GetAll")) };
            return errorResult;
        }
    }

    public bool PlayAudioFile(ushort id, AudioFileType fileType, Action<AudioPlaybackEventArgs> callback)
    {
        _logger.LogInformation($"PlayAudioFile invoked -> id:{id} fileType:{fileType.ToString()} callback is null:{callback is null}");

        try
        {
            if (callback != null)
                playAudioCallbacks[(fileType, id)] = new WeakReference<Action<AudioPlaybackEventArgs>>(callback);
            DeviceInteractProxy.Instance.PlayAudioFile(id, fileType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PlayAudioFile error");
            return false;
        }
    }

    public RealtimeCommandResult AddOrUpdateAudioFile(ushort id, string filePath, AudioFileType fileType)
    {
        _logger.LogInformation($"AddOrUpdateAudioFile invoked -> id:{id} filePath:{filePath} fileType:{fileType.ToString()}");
        try
        {
            var result = DeviceInteractProxy.Instance.AddOrUpdateAudioFile(id, filePath, fileType);
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"AddOrUpdateAudioFile failed, exception: {ex.Message} {System.Environment.NewLine} {ex.StackTrace}");
            var errorResult = new RealtimeCommandResult { Status = CommandExecutionStatus.Failure };
            errorResult.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "DeviceInteractProxy.AddOrUpdateAudioFile")) };
            return errorResult;
        }
    }

    public RealtimeCommandResult DeleteAudioFile(ushort id, AudioFileType fileType)
    {
        _logger.LogInformation($"DeleteAudioFile invoked -> id:{id} fileType:{fileType.ToString()}");
        try
        {
            var result = DeviceInteractProxy.Instance.DeleteAudioFile(id, fileType);
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"DeleteAudioFile failed, exception: {ex.Message} {System.Environment.NewLine} {ex.StackTrace}");
            var errorResult = new RealtimeCommandResult { Status = CommandExecutionStatus.Failure };
            errorResult.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "DeviceInteractProxy.DeleteAudioFile")) };
            return errorResult;
        }
    }

    public RealtimeCommandResult GetAudioFiles(AudioFileType fileType, out ushort[]? audioFileIDs)
    {
        _logger.LogInformation($"GetAudioFiles invoked -> fileType:{fileType.ToString()}");

        try
        {
            var result = DeviceInteractProxy.Instance.GetAudioFiles(fileType, out audioFileIDs);
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetAudioFiles failed, exception: {ex.Message} {System.Environment.NewLine} {ex.StackTrace}");
            audioFileIDs = null;
            var errorResult = new RealtimeCommandResult { Status = CommandExecutionStatus.Failure };
            errorResult.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "DeviceInteractProxy.GetAudioFiles")) };
            return errorResult;
        }
    }

    public RealtimeCommandResult SetVolume(uint volume)
    {
        _logger.LogInformation($"SetVolume invoked -> volume:{volume}");

        try
        {
            var result = DeviceInteractProxy.Instance.SetVolume(volume);
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"SetVolume failed, exception: {ex.Message} {System.Environment.NewLine} {ex.StackTrace}");
            var errorResult = new RealtimeCommandResult { Status = CommandExecutionStatus.Failure };
            errorResult.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "DeviceInteractProxy.SetVolume")) };
            return errorResult;
        }
    }

    public RealtimeCommandResult SetBreathTraningPlayList(IEnumerable<ushort> audioFileIDs)
    {
        _logger.LogInformation($"SetBreathTraningPlayList invoked -> audioFileIDs:{audioFileIDs}");
        try
        {
            var result = DeviceInteractProxy.Instance.SetBreathTrainingPlayList(audioFileIDs);//SetBreathTraningPlayList
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"SetBreathTraningPlayList failed, exception: {ex.Message} {System.Environment.NewLine} {ex.StackTrace}");
            var errorResult = new RealtimeCommandResult { Status = CommandExecutionStatus.Failure };
            errorResult.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "DeviceInteractProxy.SetBreathTraningPlayList")) };
            return errorResult;
        }
    }

    public RealtimeCommandResult ClearBreathTraningPlayList()
    {
        _logger.LogInformation($"ClearBreathTraningPlayList invoked");
        try
        {
            var result = DeviceInteractProxy.Instance.ClearBreathTrainingPlayList();
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"ClearBreathTraningPlayList failed, exception: {ex.Message} {System.Environment.NewLine} {ex.StackTrace}");
            var errorResult = new RealtimeCommandResult { Status = CommandExecutionStatus.Failure };
            errorResult.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "DeviceInteractProxy.SetBreathTraningPlayList")) };
            return errorResult;
        }
    }

    public RealtimeCommandResult GetBreathTrainingPlayList(out ushort[]? audioFileIDs)
    {
        _logger.LogInformation($"GetBreathTrainingPlayList invoked");
        try
        {
            var result = DeviceInteractProxy.Instance.GetBreathTrainingPlayList(out audioFileIDs);
            return new RealtimeCommandResult
            {
                Status = _mapper.Map<CommandExecutionStatus>(result.Status),
                Details = result.ErrorCodes.Codes.Select(code => (code, GlobalHelper.GetErrorMessage(code))).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetBreathTrainingPlayList failed, exception: {ex.Message} {System.Environment.NewLine} {ex.StackTrace}");
            var errorResult = new RealtimeCommandResult { Status = CommandExecutionStatus.Failure };
            errorResult.Details = new List<(string Code, string Message)> { (ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Execution_Unkown_Description, "DeviceInteractProxy.SetBreathTraningPlayList")) };
            audioFileIDs = null;
            return errorResult;
        }
    }
}
