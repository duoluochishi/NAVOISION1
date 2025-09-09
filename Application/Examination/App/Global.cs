//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.Examination.ViewModel;
using NV.CT.Protocol;

namespace NV.CT.Examination;

public class Global
{
    private static readonly Lazy<Global> _instance = new Lazy<Global>(() => new Global());

    private ClientInfo? _clientInfo;
    private MCSServiceClientProxy? _serviceClientProxy;
    private JobClientProxy? _jobClientProxy;
    private SyncServiceClientProxy? _uiSyncClientProxy;
    public string StudyId { get; set; } = string.Empty;

    public static Global Instance => _instance.Value;

    private Global()
    {
    }

    public void Initialize()
    {
        try
        {
            var _workflow = CTS.Global.ServiceProvider.GetRequiredService<IWorkflow>();
            //_workflow.ReplaceProtocolChanged += Workflow_ReplaceProtocolChanged;

            var studyId = _workflow.GetCurrentStudy();
            if (!string.IsNullOrEmpty(studyId))
            {
                StudyId = studyId;
            }

            PreInitObjects();

            var studyHostService = CTS.Global.ServiceProvider?.GetService<IStudyHostService>();
            if (studyHostService is not null)
            {
                studyHostService.StudyId = StudyId;
                var studyService = CTS.Global.ServiceProvider?.GetRequiredService<IStudyService>();
                if (studyService is not null)
                {
                    var (studyModel, _) = studyService.Get(studyHostService.StudyId);
                    if (studyModel is not null && string.IsNullOrEmpty(studyModel.Protocol) && studyModel.PatientType == (int)PatientType.Emergency)
                    {
                        HandleEmergencyPatient(studyHostService, studyService);
                    }
                    else if (studyModel is not null && !string.IsNullOrEmpty(studyModel.Protocol))
                    {
                        RecoverPatient(studyHostService, studyService);
                    }
                    else if (studyModel is not null
                        && !string.IsNullOrEmpty(studyHostService.Instance.StudyDescription)
                        && CTS.Global.ServiceProvider?.GetRequiredService<IProtocolOperation>() is IProtocolOperation protocolOperation
                        && protocolOperation.GetProtocolSettingItems().Count(t => t.OtherBodyPart.Equals(studyHostService.Instance.StudyDescription)) > 0)
                    {
                        RecoverProtocolSettingItemsPatient(studyHostService, protocolOperation);
                    }
                }
            }
            if (studyHostService is not null)
            {
                SendMessageToPatientBrowser(studyHostService);
            }
        }
        catch (Exception ex)
        {
            CTS.Global.Logger?.LogError($"Examination Global Error {0}", ex);
        }
    }

    ///// <summary>
    ///// 协议替换，监听来自RGT的请求
    ///// </summary>
    //private void Workflow_ReplaceProtocolChanged(object? sender, string e)
    //{
    //    try
    //    {
    //        var _protocolOperation = CTS.Global.ServiceProvider.GetRequiredService<IProtocolOperation>();
    //        var protocolTemplate = _protocolOperation.GetProtocolTemplate(e);
    //        if (protocolTemplate is not null)
    //        {
    //            var _protocolHostService = CTS.Global.ServiceProvider.GetRequiredService<IProtocolHostService>();
    //            var targetProtocol = protocolTemplate.Protocol.Clone();
    //            _protocolHostService.ReplaceProtocol(targetProtocol);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        CTS.Global.Logger?.LogError($"Examination handle rgt replace_protocol error {0}", ex);
    //    }
    //}

    private void SendMessageToPatientBrowser(IStudyHostService _studyHostService)
    {
        if (!string.IsNullOrEmpty(_studyHostService.StudyId))
        {
            Task.Run(() =>
            {
                _studyHostService.UpdateStudyStatus(_studyHostService.StudyId, WorkflowStatus.Examinating);
            });
        }
    }

    public void Subscribe()
    {
        _clientInfo = new ClientInfo { Id = $"[Examination]_{IdGenerator.Next(0)}" };

        _serviceClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<MCSServiceClientProxy>();
        _serviceClientProxy?.Subscribe(_clientInfo);

        _jobClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<JobClientProxy>();
        _jobClientProxy?.Subscribe(_clientInfo);

        _uiSyncClientProxy = CTS.Global.ServiceProvider?.GetRequiredService<SyncServiceClientProxy>();
        _uiSyncClientProxy?.Subscribe(_clientInfo);
    }

    public void Unsubscribe()
    {
        if (_clientInfo is not null)
        {
            _serviceClientProxy?.Unsubscribe(_clientInfo);
            _jobClientProxy?.Unsubscribe(_clientInfo);
            _uiSyncClientProxy?.Unsubscribe(_clientInfo);
        }
    }

    /// <summary>
    /// 预加载解析
    /// </summary>
    private void PreInitObjects()
    {		
		CTS.Global.ServiceProvider?.GetRequiredService<ScanMainViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<TaskListViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<ScanControlsViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<ScanDefaultViewMode>();
        CTS.Global.ServiceProvider?.GetRequiredService<ScanReconViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<ScanParameterViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<ReconParameterViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<TimelineViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<BodyPositionViewModel>();
		CTS.Global.ServiceProvider?.GetRequiredService<ToolsViewModel>();

		CTS.Global.ServiceProvider?.GetRequiredService<DoseAlertViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<DoseNotificationViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<CommonNotificationViewModel>();
        CTS.Global.ServiceProvider?.GetRequiredService<CommonWarningViewModel>();
		CTS.Global.ServiceProvider?.GetRequiredService<TimeDensityViewModel>();
		CTS.Global.ServiceProvider?.GetRequiredService<ProtocolHostServiceExtension>();
		CTS.Global.ServiceProvider?.GetRequiredService<EnhancedScanExtension>();

		CTS.Global.ServiceProvider?.GetRequiredService<UIRelatedStatusServiceExtension>();
	}

    /// <summary>
    /// 如果是急诊
    /// </summary>
    private void HandleEmergencyPatient(IStudyHostService studyHostService, IStudyService studyService)
    {
        var protocolHostService = CTS.Global.ServiceProvider?.GetRequiredService<IProtocolHostService>();
        var protocolStructureService = CTS.Global.ServiceProvider?.GetRequiredService<IProtocolStructureService>();
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

    private void RecoverPatient(IStudyHostService studyHostService, IStudyService studyService)
    {
        var protocolHostService = CTS.Global.ServiceProvider?.GetRequiredService<IProtocolHostService>();
        var protocolStructureService = CTS.Global.ServiceProvider?.GetRequiredService<IProtocolStructureService>();
        var (studyModel, _) = studyService.Get(studyHostService.StudyId);
        if (protocolHostService is not null && !string.IsNullOrEmpty(studyModel.Protocol))
        {
            var protocol = ProtocolHelper.Deserialize(studyModel.Protocol);
            if (protocol is not null && protocolHostService is not null && protocolStructureService is not null)
            {
                protocolStructureService.ReplaceProtocol(protocolHostService.Instance, protocol, new System.Collections.Generic.List<string>(), false, false);
            }
        }
    }

    private void RecoverProtocolSettingItemsPatient(IStudyHostService studyHostService, IProtocolOperation protocolOperation)
    {
        var protocolHostService = CTS.Global.ServiceProvider?.GetRequiredService<IProtocolHostService>();
        var protocolStructureService = CTS.Global.ServiceProvider?.GetRequiredService<IProtocolStructureService>();
        var list = protocolOperation.GetPresentations();
        var psl = protocolOperation.GetProtocolSettingItems().FindAll(t => t.OtherBodyPart.Equals(studyHostService.Instance.StudyDescription));
        if (protocolHostService is not null && protocolStructureService is not null && psl is not null && list is not null)
        {
            int index = 0;
            foreach (var item in psl)
            {
                var pro = list.FirstOrDefault(t => t.Id.Equals(item.Id));
                if (pro is not null)
                {
                    var p = protocolOperation.GetProtocolTemplate(item.Id);
                    if (p is not null && index == 0)
                    {
                        protocolStructureService.ReplaceProtocol(protocolHostService.Instance, p.Protocol.Clone(), new System.Collections.Generic.List<string>(), false, false);
                    }
                    else if (p is not null)
                    {
                        protocolStructureService.AddProtocol(protocolHostService.Instance, p.Protocol.Clone(), new System.Collections.Generic.List<string>(), false, false);
                    }
                    index += 1;
                }
            }
        }
    }
}