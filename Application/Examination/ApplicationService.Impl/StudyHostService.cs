//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using System.Diagnostics;

namespace NV.CT.Examination.ApplicationService.Impl;

public class StudyHostService : IStudyHostService
{
    private readonly IMapper _mapper;
    private readonly IStudyService _studyService;
    public StudyModel Instance { get; private set; } = new();

    public event EventHandler<CTS.EventArgs<StudyModel>> StudyChanged = delegate { };

    public StudyHostService(IMapper mapper, IStudyService studyService)
    {
        _mapper = mapper;
        _studyService = studyService;
    }

    private string _studyId = string.Empty;
    public string StudyId
    {
        get => _studyId;
        set
        {
            if (!_studyId.Equals(value))
            {
                _studyId = value;
                Instance = GetStudy(_studyId);
                StudyChanged?.Invoke(this, new CTS.EventArgs<StudyModel>(Instance));
            }
        }
    }

    private StudyModel GetStudy(string studyId)
    {
        //todo:临时解决检查状态
        var processName = Process.GetCurrentProcess().ProcessName;
        var isExamApp = processName.Contains("NV.CT.Examination");
        var patientStudy = _studyService.Get(studyId);
        if (!string.IsNullOrEmpty(studyId) && isExamApp && patientStudy.Study.StudyStatus == WorkflowStatus.NotStarted.ToString())
        {
            _studyService.UpdateStudyExaming(studyId, DateTime.Now, WorkflowStatus.ExaminationStarting);
        }

        var model = _mapper.Map<StudyModel>(patientStudy.Patient);
        _mapper.Map(patientStudy.Study, model);
        model.PatientSex = patientStudy.Patient.PatientSex;
        return model;
    }

    public bool UpdateProtocol(StudyModel study, ProtocolModel protocol)
    {
        if (study is null || protocol is null || protocol.Descriptor is null)
        {
            return false;
        }
        var protocolContent = ProtocolHelper.Serialize(protocol);
        var studyModel = new DatabaseService.Contract.Models.StudyModel();
        studyModel.StudyDescription = protocol.Descriptor.Name;
        studyModel.Protocol = protocolContent;
        studyModel.Id = study.ID;
        studyModel.StudyId = study.StudyID;
        studyModel.BodyPart = protocol.BodyPart.ToString();

        return _studyService.UpdateStudyProtocol(studyModel);
    }

    public bool UpdateStudyStatus(string studyId, WorkflowStatus examStatus)
    {
        return _studyService.UpdateStudyStatus(studyId, examStatus);
    }
}