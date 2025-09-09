//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2025/7/30 14:28:29           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using System.Threading;

namespace NV.CT.UI.Exam.ViewModel;

public class ProtocolHostServiceExtension
{
	private readonly IProtocolHostService _protocolHostService;
	private readonly IStudyHostService _studyHostService;
	private readonly ILogger<ProtocolHostServiceExtension> _logger;

	Semaphore semaphore = new Semaphore(3, 3);
	public ProtocolHostServiceExtension(IStudyHostService studyHostService,
		IProtocolHostService protocolHostService,
		ILogger<ProtocolHostServiceExtension> logger)
	{
		_studyHostService = studyHostService;
		_protocolHostService = protocolHostService;
		_logger = logger;
		_protocolHostService.StructureChanged -= ProtocolHostService_StructureChanged;
		_protocolHostService.StructureChanged += ProtocolHostService_StructureChanged;
		_protocolHostService.ParameterChanged -= ProtocolHostService_ParameterChanged;
		_protocolHostService.ParameterChanged += ProtocolHostService_ParameterChanged;
		_protocolHostService.PerformStatusChanged -= ProtocolHostService_PerformStatusChanged;
		_protocolHostService.PerformStatusChanged += ProtocolHostService_PerformStatusChanged;
	}

	private void ProtocolHostService_PerformStatusChanged(object? sender, EventArgs<(BaseModel Model, PerformStatus OldStatus, PerformStatus NewStatus)> e)
	{
		if (string.IsNullOrEmpty(_studyHostService.Instance.ID) || e is null || e.Data.Model is null)
		{
			return;
		}
		SaveProtocolHostInstanceToDB();
	}

	private void ProtocolHostService_ParameterChanged(object? sender, EventArgs<(BaseModel baseModel, List<string> list)> e)
	{
		if (string.IsNullOrEmpty(_studyHostService.Instance.ID) || e is null || e.Data.baseModel is null)
		{
			return;
		}
		SaveProtocolHostInstanceToDB();
	}

	private void ProtocolHostService_StructureChanged(object? sender, EventArgs<(BaseModel Parent, BaseModel Current, StructureChangeType ChangeType)> e)
	{
		if (string.IsNullOrEmpty(_studyHostService.Instance.ID) || e is null || e.Data.Current is null)
		{
			return;
		}
		SaveProtocolHostInstanceToDB();
	}

	private void SaveProtocolHostInstanceToDB()
	{
		Task.Run(() =>
		{
			try
			{
				semaphore.WaitOne(120000);
				_studyHostService.UpdateProtocol(_studyHostService.Instance, _protocolHostService.Instance);
				semaphore.Release();
			}
			catch (Exception ex)
			{
				_logger.LogError($"Synchronizing protocolhostinstance to database error: {ex.Message}-{ex.StackTrace}");
			}
		});
	}
}