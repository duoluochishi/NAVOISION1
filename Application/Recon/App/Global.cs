using NV.CT.CTS.Extensions;

namespace NV.CT.Recon;

public class Global
{
	private static readonly Lazy<Global> _instance = new(() => new Global());

	private ClientInfo? _clientInfo;
	private MCSServiceClientProxy? _serviceClientProxy;
	private JobClientProxy? _jobClientProxy;

	public string StudyId { get; set; } = string.Empty;

	public static Global Instance => _instance.Value;

	private Global()
	{
	}

	public void ResumeReconStates()
	{
		var reconService = CTS.Global.ServiceProvider.GetService<IReconService>();
		reconService?.ResumeReconStates(Global.Instance.StudyId);
	}

	public void Initialize()
	{
		try
		{
			// 1601ms
			PreInitObjects();

			var sp = CTS.Global.ServiceProvider;
			var studyHostService = sp.GetService<IStudyHostService>();
			if (studyHostService is null)
				return;

			studyHostService.StudyId = StudyId;

			// 37ms
			var protocolModel = ProtocolHelper.Deserialize(studyHostService.Instance.Protocol);
			var protocolHostService = sp.GetService<IProtocolHostService>();
			if (protocolHostService is null)
				return;

			// 61ms
			protocolHostService.ResetProtocol(protocolModel);

		}
		catch (Exception ex)
		{
			CTS.Global.Logger.LogError($"Recon Global Error {ex.Message}-{ex.StackTrace}", ex);
		}
	}

	public void Subscribe()
	{
		_clientInfo = new ClientInfo { Id = $"[Recon]_{IdGenerator.Next(0)}" };

		_serviceClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
		_serviceClientProxy?.Subscribe(_clientInfo);

		_jobClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<JobClientProxy>();
		_jobClientProxy?.Subscribe(_clientInfo);
	}

	public void Unsubscribe()
	{
		if (_clientInfo != null)
		{
			_serviceClientProxy?.Unsubscribe(_clientInfo);
			_jobClientProxy?.Unsubscribe(_clientInfo);
		}
	}

	/// <summary>
	/// 预加载解析
	/// </summary>
	private void PreInitObjects()
	{
		// 236ms
		CTS.Global.ServiceProvider.GetRequiredService<ReconTaskListViewModel>();
		
		// 1255ms
		CTS.Global.ServiceProvider.GetRequiredService<IDicomImageViewModel>();
		
		// 0ms
		CTS.Global.ServiceProvider.GetRequiredService<ScanMainViewModel>();
		
		// 161ms
		CTS.Global.ServiceProvider.GetRequiredService<ReconScanControlsViewModel>();
		
		// 0ms
		CTS.Global.ServiceProvider.GetRequiredService<ScanDefaultViewModel>();
		
		// 0ms
		CTS.Global.ServiceProvider.GetRequiredService<ISelectionManager>();
		
		// 1ms
		CTS.Global.ServiceProvider.GetRequiredService<ScanReconViewModel>();
		
		// 0ms
		CTS.Global.ServiceProvider.GetRequiredService<ProtocolHostServiceExtension>();
		
		//CTS.Global.ServiceProvider.GetRequiredService<TimelineViewModel>();
	}

	/// <summary>
	/// 如果是急诊
	/// </summary>
	private void HandleEmergencyPatient()
	{
		var protocolHostService = CTS.Global.ServiceProvider?.GetRequiredService<IProtocolHostService>();
		var studyService = CTS.Global.ServiceProvider?.GetRequiredService<IStudyService>();
		var studyHostService = CTS.Global.ServiceProvider?.GetRequiredService<IStudyHostService>();
		var protocolStructureService = CTS.Global.ServiceProvider?.GetRequiredService<IProtocolStructureService>();

		if (studyService is null || studyHostService is null)
		{
			return;
		}

		var (studyModel, _) = studyService.Get(studyHostService.StudyId);


		if (studyModel is not null && studyModel.PatientType == (int)PatientType.Emergency)
		{
			var protocolOperation = CTS.Global.ServiceProvider?.GetRequiredService<IProtocolOperation>();
			var emergencyProtocol = protocolOperation?.GetEmergencyProtocolTemplate().Protocol;
			if (emergencyProtocol is not null && protocolHostService is not null && protocolStructureService is not null)
			{
				protocolStructureService.ReplaceProtocol(protocolHostService.Instance, emergencyProtocol, new System.Collections.Generic.List<string>(), false, false);
			}
		}
	}
}