//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
using NV.CT.AppService.Contract;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Models;
using NV.CT.PatientManagement.ApplicationService.Contract.Models;

namespace NV.CT.PatientManagement.ApplicationService.Contract.Interfaces;

public interface IStudyApplicationService
{
    List<(PatientModel, StudyModel)> GetPatientStudyListWithEnd();
    bool Delete(StudyModel studyModel);
    StudyModel GetStudyModelByPatientIdAndStudyStatus(string patientId, string studyStatus);
    PatientModel GetPatientModelById(string patientId);
    bool ResumeExamination(StudyModel studyModel, string StudyId);
    bool SwitchLockStatus(string studyId);
    bool UpdateArchiveStatus(List<StudyModel> studyModels);
    public event EventHandler<EventArgs<string>> SelectItemChanged;
    void RaiseSelectItemChanged(string studyId);
    event EventHandler<EventArgs<(PatientModel, StudyModel, DataOperateType)>> RefreshPatientManagementStudyList;
    event EventHandler<EventArgs<(PatientModel, StudyModel)>> GotoExam;

    event EventHandler<EventArgs<SearchTimeType>> RefreshRearchDateType;

    void RefreshRearchDateTyped(SearchTimeType patientQueryTimeType);

    List<(PatientModel, StudyModel)> GetPatientStudyListWithEnd(string startDate, string endDate);

    List<(PatientModel, StudyModel)> GetPatientStudyListByFilter(StudyListFilterModel filter);

    StudyModel[] GetStudiesByIds(string[] studyIdList);

    bool GetStudyByStudyInstanceUID(string studyInstanceUID);

    bool GetSeriesBySeriesInstanceUID(string seriesnstanceUID);

    bool Update(bool isGotoExam, PatientModel patientModel, StudyModel studyModel);

    List<(PatientModel, StudyModel)> GetCorrectionHistoryList(string studyId);

    bool Correct(PatientModel patientModel, StudyModel studyModel, string editor);

    (string,string) ImportRaw(string dataPath);

    public bool AddSeries(SeriesModel seriesModel);

}