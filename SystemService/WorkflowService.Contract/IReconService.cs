namespace NV.CT.WorkflowService.Contract;

public interface IReconService
{
	void ResumeReconStates(string studyId);

    void OpenReconApplication(string studyId);

    void CloseReconApplication(string studyId);

    bool CheckReconApplication(string studyId);
}