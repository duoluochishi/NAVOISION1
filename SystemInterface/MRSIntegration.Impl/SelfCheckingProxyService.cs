//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期         		版本号       创建人
// 2023/10/19 9:44:48           V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.FacadeProxy.Common.Enums.SelfCheck;
using NV.CT.FacadeProxy.Common.EventArguments;
using NV.CT.FacadeProxy.Common.Models.SelfCheck;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class SelfCheckingProxyService : ISelfCheckingProxyService
{
	private readonly ILogger<SelfCheckingProxyService> _logger;
	private readonly IMapper _mapper;

	public event EventHandler<SelfCheckEventArgs>? SelfCheckStatusChanged;


	public SelfCheckingProxyService(ILogger<SelfCheckingProxyService> logger, IMapper mapper)
	{
		_logger = logger;
		_mapper = mapper;
		SelfCheckProxy.Instance.SelfCheckStatusChanged += SelfCheckProxy_SelfCheckStatusChanged;
	}

	public BaseCommandResult StartSelfChecking(SelfCheckPartType partType)
	{
		try
		{
			var result = SelfCheckProxy.Instance.RunSelfCheck(partType);

			if (result is null || result.Status != CommandStatus.Success)
			{
				_logger.LogDebug(
					$"StartSelfChecking({partType.ToString()}) failed: {result?.Status.ToString()}, {JsonConvert.SerializeObject(result?.ErrorCodes.Codes)}");
			}

			return new BaseCommandResult
			{
				Status = _mapper.Map<CommandExecutionStatus>(result?.Status)
				,
				Details = result.ErrorCodes.Codes.Select(code => (code, string.Empty)).ToList()
			};
		}
		catch (Exception ex)
		{
			_logger.LogError($"proxy self check error {ex.Message}-{ex.StackTrace}");
			return new BaseCommandResult();
		}
	}

	public List<SelfCheckInfo> GetResults()
	{
		try
		{
			var selfCheckPartStatus = NV.CT.FacadeProxy.SelfCheckProxy.Instance.GetCompleteSelfCheckInfos().ToList();
			return selfCheckPartStatus;
		}
		catch (Exception ex)
		{
			_logger.LogError($"SystemInterface get self check result error : {ex.Message}");
			return new List<SelfCheckInfo>();
		}
	}

	private void SelfCheckProxy_SelfCheckStatusChanged(object? sender, SelfCheckEventArgs e)
	{
		try
		{
			SelfCheckStatusChanged?.Invoke(sender, e);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, $"SystemInterface SelfCheckStatusChanged error : {ex.Message}");
		}
	}
}
