//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.FacadeProxy.Common.Enums.SelfCheck;
using NV.CT.FacadeProxy.Common.EventArguments;
using NV.CT.FacadeProxy.Common.Models.SelfCheck;

namespace NV.CT.AppService.Impl.SelfCheck;

/*
 MRS相关:
    *磁盘分区检测(devicedeamon)	"1，检查主控计算机磁盘盘符数目 CDEF；
   2，检查离线重建计算机磁盘盘符数目CDE；"
      
   磁盘映射检测(devicedeamon)	"1，检测离线重建计算机的E盘是否共享,
   2，检测主控计算机D,F盘是否共享；"
      
   磁盘访问检查(devicedeamon)	1，读取磁盘文件，看是否能够读取；
      
   采集卡温度检测(devicedeamon)	1，采集卡温度是否在正常区间；
      
   采集卡在线检测(devicedeamon)	1，采集卡是否能够正常收取数据
      
   校准表检测(acqrecon)	"1,检测校准表目录是否存在；
   2，按照校准表类别，检测必要的类别的校准表是否存在；"
      
   重建控制连接检测(proxy)	1，检测离线重建控制进程是否Alive
 */

public class EmbeddedSystemSelfCheckingExecutor : ISelfCheckingExecutor
{
	private readonly ILogger<EmbeddedSystemSelfCheckingExecutor> _logger;
	private readonly ISelfCheckingProxyService _proxyService;
	private readonly Dictionary<SelfCheckPartType, string> _checkingItems;

	public event EventHandler<SelfCheckResult>? SelfCheckStatusChanged;

	private List<SelfCheckInfo> _cachedSelfCheckInfos=new();

	public EmbeddedSystemSelfCheckingExecutor()
	{
		_logger = Global.ServiceProvider.GetRequiredService<ILogger<EmbeddedSystemSelfCheckingExecutor>>();
		_proxyService = Global.ServiceProvider.GetRequiredService<ISelfCheckingProxyService>();

		_proxyService.SelfCheckStatusChanged += ProxyService_SelfCheckStatusChanged;

		_checkingItems = typeof(SelfCheckPartType).ToDictionary<SelfCheckPartType>();
	}

	private void ProxyService_SelfCheckStatusChanged(object? sender, SelfCheckEventArgs e)
	{
		var targetedPart=_cachedSelfCheckInfos.FirstOrDefault(n => n.PartType == e.SelfCheckInfo.PartType);

		//如果没找到对应部件 , 如果缓存状态和最新状态一致 ,  都不处理
		if (targetedPart is null || targetedPart.Status==e.SelfCheckInfo.Status)
		{
			return;
		}

		//只发送变更状态的事件出去
		//_logger.LogInformation($"embedded self check status changed to : {e.ToJson()}");

		var selfCheckResult = SelfcheckHelper.TransferTo(e);
		SelfCheckStatusChanged?.Invoke(this, selfCheckResult);
	}

	public BaseCommandResult StartSelfChecking()
	{
		//_logger.LogInformation("embedded start self check");
		var result = new BaseCommandResult
		{
			Status = CTS.Enums.CommandExecutionStatus.Success
		};

		foreach (var item in _checkingItems)
		{
			var checkingResult = _proxyService.StartSelfChecking(item.Key);
			if (checkingResult.Status != CTS.Enums.CommandExecutionStatus.Success)
			{
				result.Status = checkingResult.Status;
				result.Details.AddRange(checkingResult.Details);
			}
		}
		//_logger.LogInformation($"Embedded self check with result {result.ToJson()}");
		return result;
	}

	public List<SelfCheckResult> GetSelfCheckResults()
	{
		_cachedSelfCheckInfos = _proxyService.GetResults();

		//_logger.LogInformation($"Embedded self check GetSelfCheckResults : {_cachedSelfCheckInfos.ToJson()}");

		var list = new List<SelfCheckResult>();

		foreach (var item in _cachedSelfCheckInfos)
		{
			var tmp = new SelfCheckResult();
			tmp.CheckName = item.PartType.ToString();
			tmp.Timestamp = DateTime.Now;
			tmp.CheckStatus = item.Status;
			tmp.DetailedSelfCheckInfos = item.DetailedSelfCheckInfos;
			list.Add(tmp);
		}

		return list;
	}


}
