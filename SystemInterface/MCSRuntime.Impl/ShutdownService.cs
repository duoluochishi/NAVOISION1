//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:19    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.CT.CTS.Models;
using NV.CT.SystemInterface.MCSRuntime.Contract;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace NV.CT.SystemInterface.MCSRuntime.Impl;

public class ShutdownService : IShutdownService
{
    private readonly ILogger<ShutdownService> _logger;
	public ShutdownService(ILogger<ShutdownService> logger)
	{
		_logger= logger;
	}
    public BaseCommandResult CanRestart()
    {
        //todo:待实现
        return new BaseCommandResult { Status = CTS.Enums.CommandExecutionStatus.Success };
    }

    public BaseCommandResult CanShutdown()
    {
        //todo:待实现
        return new BaseCommandResult { Status = CTS.Enums.CommandExecutionStatus.Success };
    }

    public BaseCommandResult Restart()
    {
	    _logger.LogInformation($"MCS shutdown service call restart method");
		Task.Run(() => {
            Process.Start("shutdown", "-r -f -t 0");
		});

        return new BaseCommandResult {
            Status = CTS.Enums.CommandExecutionStatus.Success
        };
    }

    public BaseCommandResult Shutdown()
    {
        _logger.LogInformation($"MCS shutdown service call shutdown method");
        Task.Run(() => {
			Process.Start("shutdown", "-s -f -t 0");
		});

        return new BaseCommandResult
        {
            Status = CTS.Enums.CommandExecutionStatus.Success
        };
    }

}
