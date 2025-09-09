using AutoMapper;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.ImageViewer.ApplicationService.Contract.Interfaces;
using NV.CT.ImageViewer.ApplicationService.Contract.Models;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.ImageViewer.ApplicationService.Impl;

public class ViewerService : IViewerService
{
    private readonly IViewer _viewer;
    private readonly IMapper _mapper;
    private readonly IStudyService _studyService;
    private readonly ISeriesService _seriesService;
    //IApplicationCommunicationService _applicationCommunicationService;
    public event EventHandler<string>? ViewerChanged;
    public event EventHandler? SelectSeriesChanged;

    //public event EventHandler<ApplicationResponse>? ApplicationClosing;

    public ViewerService(IViewer viewer, IStudyService studyService, ISeriesService seriesService, IMapper mapper)
    {
        _viewer = viewer;
        _studyService = studyService;
        _mapper = mapper;
        _viewer.ViewerChanged += _viewer_ViewerChanged;
        _seriesService = seriesService;
        //_applicationCommunicationService = applicationCommunicationService;
        //_applicationCommunicationService.NotifyApplicationClosing += _applicationCommunicationService_NotifyApplicationClosing;
    }

    //private void _applicationCommunicationService_NotifyApplicationClosing(object? sender, ApplicationResponse e)
    //{
    //    ApplicationClosing?.Invoke(sender, e);
    //}

    private void _viewer_ViewerChanged(object? sender, string e)
    {
        ViewerChanged?.Invoke(this, e);
    }

    public (StudyModel Study, PatientModel Patient) Get(string studyId)
    {
        var result = _studyService.Get(studyId);
        return _mapper.Map<(StudyModel Study, PatientModel Patient)>(result);
    }

    public List<SeriesModel> GetSeriesByStudyId(string studyId)
    {
        var result = _seriesService.GetSeriesByStudyId(studyId);
        var list = _mapper.Map<List<DatabaseService.Contract.Models.SeriesModel>, List<SeriesModel>>(result);
        return list;
    }

	public List<(PatientModel,StudyModel)> GetPatientStudyListByFilterSimple(StudyQueryModel queryModel)
	{
        var result = _studyService.GetPatientStudyListByFilterSimple(queryModel);
        var list = new List<(PatientModel, StudyModel)>();
        //list = _mapper.Map<List<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel)>, List<(Contract.Models.PatientModel, Contract.Models.StudyModel)>>(result);

        //return list;

        foreach (var (patientModel,studyModel) in result)
        {
            var pm = new PatientModel();
            pm.Id = patientModel.Id;
            pm.PatientName = patientModel.PatientName;
            pm.PatientId=patientModel.PatientId;
            pm.PatientSex=patientModel.PatientSex;

            var sm = new StudyModel();
            sm.Id= studyModel.Id;
            sm.BodyPart = studyModel.BodyPart;
            sm.ExamStartTime = studyModel.ExamStartTime;
            sm.ExamEndTime= studyModel.ExamEndTime;
            sm.CreateTime = studyModel.CreateTime;
            sm.Birthday=studyModel.Birthday;
            sm.StudyStatus=studyModel.StudyStatus;
            sm.PatientType = studyModel.PatientType;

            list.Add((pm,sm));
        }

        return list;
    }

    public void SelectHistoryStudy(string studyId)
    {
        SelectHistoryStudyChanged?.Invoke(this,studyId);
    }

    public event EventHandler<string>? SelectHistoryStudyChanged;
}