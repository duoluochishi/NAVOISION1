//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Contract.Models;

namespace NV.CT.DatabaseService.Contract;
public interface IStudyService
{
	bool Insert(bool isAddProcedure, bool isGotoExam, PatientModel patientModel, StudyModel studyModel);

	bool Update(bool isGotoExam, PatientModel patientModel, StudyModel studyModel);
	bool UpdateArchiveStatus(List<StudyModel> studyModels);

	bool UpdatePrintStatus(List<StudyModel> studyModels);

	bool UpdateStudyProtocol(StudyModel studyModel);
	
	bool UpdateProtocolByStudyId(UpdateStudyProtocolReq req);

	string GetStudyIdWithAbnoramlClosed();

	void UpdateStudyClosedWithAbnormalStatus();

	bool Delete(StudyModel studyModel);

	bool DeleteByGuid(StudyModel studyModel);

    string GotoEmergencyExamination(string patientId, string patientName);

	event EventHandler<EventArgs<(PatientModel, StudyModel, DataOperateType)>>? UpdateStudyInformation;
	//public event EventHandler<EventArgs<(PatientModel, StudyModel, DataOperateType)>> RefreshPatientManagementStudyList;
	event EventHandler<EventArgs<(PatientModel, StudyModel)>> GotoExam;

	(StudyModel? Study, PatientModel? Patient) Get(string studyId);

	(PatientModel Patient, StudyModel Study) GetWithUID(string studyInstanceUID);

	List<(PatientModel, StudyModel)> GetPatientStudyListWithNotStarted();
	List<(PatientModel, StudyModel)> GetPatientStudyListWithNotStartedStudyDate(string startDate, string endDate);

	List<(PatientModel, StudyModel)> GetPatientStudyListByFilter(StudyListFilterModel filter);

	List<(PatientModel, StudyModel)> GetPatientStudyListByFilterSimple(StudyQueryModel queryModel);

	List<(PatientModel, StudyModel)> GetPatientStudyListWithEnd();
	List<(PatientModel, StudyModel)> GetPatientStudyListWithEndStudyDate(string startDate, string endDate);
	StudyModel GetStudyModelByPatientIdAndStudyStatus(string patientId, string studyStatus);
	PatientModel GetPatientModelById(string patientId);

	StudyModel[] GetStudiesByIds(string[] studyIdList);

	List<StudyModel> GetStudiesByPatient(string patientGuid);

    StudyEntity GetStudyById(string studyId);

    bool ResumeExamination(StudyModel studyModel, string studyId);
	bool SwitchLockStatus(string studyId);
	bool UpdateStudyStatus(string studyId, WorkflowStatus examStatus);

	bool UpdateStudyExaming(string studyId, DateTime studyTime, WorkflowStatus workflowStatus);
	bool InsertPatientListStudyListAndSeriesList(List<PatientModel> patientModels, List<StudyModel> studyModels, List<SeriesModel> seriesModels);
	bool SetStudyArchiveFail();

	bool Correct(PatientModel patientModel, StudyModel studyModel, string editor);

	bool UpdateWorklistByStudy(PatientModel patientModel, StudyModel studyModel);

	List<(PatientModel, StudyModel)> GetCorrectionHistoryList(string studyId);

	bool UpdatePrintConfigPath(string studyId, string printConfigPath);
}