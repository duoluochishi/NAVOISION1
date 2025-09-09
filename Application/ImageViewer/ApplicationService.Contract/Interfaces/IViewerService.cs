using NV.CT.CTS.Models;
using NV.CT.ImageViewer.ApplicationService.Contract.Models;

namespace NV.CT.ImageViewer.ApplicationService.Contract.Interfaces
{
    public interface IViewerService
    {
        event EventHandler<string> ViewerChanged;
        (StudyModel Study, PatientModel Patient) Get(string studyId);
        List<SeriesModel> GetSeriesByStudyId(string studyId);

        List<(PatientModel, StudyModel)> GetPatientStudyListByFilterSimple(StudyQueryModel queryModel);

        void SelectHistoryStudy(string studyId);
        event EventHandler<string>? SelectHistoryStudyChanged;
    }
}
