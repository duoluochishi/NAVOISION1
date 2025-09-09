//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:36    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.MPS.Communication;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.ClientProxy.Workflow;

public class Recon : IReconService
{
	private readonly MCSServiceClientProxy _clientProxy;

	public Recon(MCSServiceClientProxy clientProxy)
	{
		_clientProxy = clientProxy;
	}

    public bool CheckReconApplication(string studyId)
    {
        var response = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconService).Namespace,
            SourceType = nameof(IReconService),
            ActionName = nameof(IReconService.CheckReconApplication),
            Data = studyId
        });

        if (response.Success)
        {
            var result = Convert.ToBoolean(response.Data);
            return result;
        }

        return false;
    }

    public void CloseReconApplication(string studyId)
    {
        _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconService).Namespace,
            SourceType = nameof(IReconService),
            ActionName = nameof(IReconService.CloseReconApplication),
            Data = studyId
        });
    }

    public void OpenReconApplication(string studyId)
    {
        _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IReconService).Namespace,
            SourceType = nameof(IReconService),
            ActionName = nameof(IReconService.OpenReconApplication),
            Data = studyId
        });
    }

    public void ResumeReconStates(string studyId)
	{
		_clientProxy.ExecuteCommand(new CommandRequest()
		{
			Namespace = typeof(IReconService).Namespace,
			SourceType = nameof(IReconService),
			ActionName = nameof(IReconService.ResumeReconStates),
			Data = studyId
		});
	}
}