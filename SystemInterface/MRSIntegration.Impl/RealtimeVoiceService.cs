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
using NV.CT.FacadeProxy.Common.Models;
using NV.CT.FacadeProxy.Extensions;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class RealtimeVoiceService: IRealtimeVoiceService
{
    private readonly ILogger<RealtimeVoiceService> _logger;
    private readonly IMapper _mapper;
    private readonly IRealtimeProxyService _proxyService;
    private AuxBoard _auxBoard;

    public RealtimeVoiceService(ILogger<RealtimeVoiceService> logger, IMapper mapper, IRealtimeProxyService proxyService)
    {
        _logger = logger;
        _mapper = mapper;
        _proxyService = proxyService;
        _auxBoard = _proxyService.AuxBoard;
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
}
