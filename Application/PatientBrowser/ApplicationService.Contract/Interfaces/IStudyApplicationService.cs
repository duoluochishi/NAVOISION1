//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.PatientBrowser.ApplicationService.Contract.Models;

namespace NV.CT.PatientBrowser.ApplicationService.Contract.Interfaces;
public interface IStudyApplicationService
{
    bool Insert(bool isAddProcedure, bool isGotoExam, PatientModel patientModel, StudyModel studyModel);

    event EventHandler<EventArgs<(PatientModel, StudyModel, DataOperateType)>> RefreshWorkList;

    event EventHandler<EventArgs<(PatientModel, StudyModel)>> SelectItemChanged;

    void RaiseSelectItemChanged((PatientModel, StudyModel) item);

    public string GotoEmergencyExamination(string patientId, string patientName);

    bool Update(bool isGotoExam, PatientModel patientModel, StudyModel studyModel);

    bool Delete(StudyModel studyModel);

    List<(PatientModel patientModel, StudyModel studyModel)> GetPatientStudyListWithNotStartedStudyDate(string startDate, string endDate);

    List<(PatientModel patientModel, StudyModel studyModel)> GetPatientStudyListWithNotStarted();

    event EventHandler<EventArgs<SearchTimeType>> RefreshRearchDateType;

    void RefreshRearchDateTyped(SearchTimeType patientQueryTimeType);
}