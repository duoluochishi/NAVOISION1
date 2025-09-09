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
using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Helpers;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Entities;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.DatabaseService.Impl.Repository;
using System.Diagnostics;
using System.Text;

namespace NV.CT.DatabaseService.Impl;

public class StudyService : IStudyService
{
	private readonly IMapper _mapper;
	private readonly PatientRepository _patientRepository;
	private readonly StudyRepository _studyRepository;
	public event EventHandler<EventArgs<(PatientModel, StudyModel, DataOperateType)>>? UpdateStudyInformation;
	//public event EventHandler<EventArgs<(PatientModel, StudyModel, DataOperateType)>>? RefreshPatientManagementStudyList;
	public event EventHandler<EventArgs<(PatientModel, StudyModel)>>? GotoExam;

	private readonly ILogger<StudyService> _logger;

	public StudyService(IMapper mapper, PatientRepository patientRepository, StudyRepository studyRepository, ILogger<StudyService> logger)
	{
		_mapper = mapper;
		_patientRepository = patientRepository;
		_studyRepository = studyRepository;
		_logger = logger;
	}

	public bool Insert(bool isAddProcedure, bool isGotoExam, PatientModel patientModel, StudyModel studyModel)
	{
		//TODO:操作多个表的时候，尽量使用事务封装
		_logger.LogInformation("InsertMany patient/study");
		var patientEntity = _mapper.Map<PatientEntity>(patientModel);
		var studyEntity = _mapper.Map<StudyEntity>(studyModel);
		var savedResult = false;
		if (isAddProcedure)
		{
			ValidateBodyPart(studyModel);
			savedResult = _studyRepository.Insert(studyEntity);
		}
		else
		{
			ValidateBodyPart(studyModel);

			savedResult = _studyRepository.InsertPatientAndStudy(patientEntity, studyEntity);
		}
		if (savedResult)
		{
            UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((patientModel, studyModel, DataOperateType.Add)));
			if (isGotoExam)
			{
				GotoExam?.Invoke(this, new EventArgs<(PatientModel, StudyModel)>((patientModel, studyModel)));
			}
		}
		return savedResult;
	}

	public bool InsertStudyModel(StudyModel studyModel)
	{
		_logger.LogInformation("Add procedure");
		var studyEntity = _mapper.Map<StudyEntity>(studyModel);

		ValidateBodyPart(studyModel);

		var savedResult = _studyRepository.Insert(studyEntity);
		if (savedResult)
		{
            UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((new PatientModel(), studyModel, DataOperateType.Add)));
		}
		return savedResult;
	}

	public (StudyModel? Study, PatientModel? Patient) Get(string studyId)
	{

		if (string.IsNullOrWhiteSpace(studyId))
		{
			return (null, null);
		}

		var entity = _studyRepository.Get(studyId);
		var item1 = _mapper.Map<StudyModel>(entity.Study);
		var item2 = _mapper.Map<PatientModel>(entity.Patient);
		item1.StudyDate = entity.Study.StudyTime;

		return (item1, item2);
	}

	public (PatientModel Patient, StudyModel Study) GetWithUID(string studyInstanceUID)
	{
		var entity = _studyRepository.GetWithUID(studyInstanceUID);
		return (_mapper.Map<PatientModel>(entity.Patient), _mapper.Map<StudyModel>(entity.Study));
	}

	public List<(PatientModel, StudyModel)> GetPatientStudyListWithNotStarted()
	{
		string[] statusList = new string[] { WorkflowStatus.NotStarted.ToString() };
		var result = _studyRepository.GetPatientStudyListByStatus(statusList);
		var list = new List<(PatientModel, StudyModel)>();
		list = _mapper.Map<List<(PatientEntity, StudyEntity)>, List<(PatientModel, StudyModel)>>(result);
		return list;
	}

	public List<(PatientModel, StudyModel)> GetPatientStudyListWithNotStartedStudyDate(string startDate, string endDate)
	{
        var result = _studyRepository.GetPatientStudyListWithNotStartedStudyDate(startDate, endDate);
		var list = new List<(PatientModel, StudyModel)>();
		list = _mapper.Map<List<(PatientEntity, StudyEntity)>, List<(PatientModel, StudyModel)>>(result);
		return list;
	}

    public List<(PatientModel, StudyModel)> GetPatientStudyListByFilter(StudyListFilterModel filter)
    {
        var result = _studyRepository.GetPatientStudyListByFilter(filter);
        var list = new List<(PatientModel, StudyModel)>();
        list = _mapper.Map<List<(PatientEntity, StudyEntity)>, List<(PatientModel, StudyModel)>>(result);
        return list;
    }

    public List<(PatientModel, StudyModel)> GetPatientStudyListByFilterSimple(StudyQueryModel queryModel)
    {
	    var result = _studyRepository.GetPatientStudyListByFilterSimple(queryModel);
	    var list = new List<(PatientModel, StudyModel)>();
	    list = _mapper.Map<List<(PatientEntity, StudyEntity)>, List<(PatientModel, StudyModel)>>(result);
	    return list;
	}

    public List<(PatientModel, StudyModel)> GetPatientStudyListWithEnd()
	{
		string[] statusList = new string[] { WorkflowStatus.ExaminationClosed.ToString(), WorkflowStatus.Examinating.ToString(), WorkflowStatus.ExaminationStarting.ToString() };
		var result = _studyRepository.GetPatientStudyListByStatus(statusList);
		var list = new List<(PatientModel, StudyModel)>();
		list = _mapper.Map<List<(PatientEntity, StudyEntity)>, List<(PatientModel, StudyModel)>>(result);
		return list;
	}

	public List<(PatientModel, StudyModel)> GetPatientStudyListWithEndStudyDate(string startDate, string endDate)
	{
		var result = _studyRepository.GetPatientStudyListWithEndStudyDate(startDate, endDate);
		var list = new List<(PatientModel, StudyModel)>();
		list = _mapper.Map<List<(PatientEntity, StudyEntity)>, List<(PatientModel, StudyModel)>>(result);
		return list;
	}

	public StudyModel GetStudyModelByPatientIdAndStudyStatus(string patientId, string studyStatus)
	{
		var entity = _studyRepository.GetStudyEntityByPatientIdAndStudyStatus(patientId, studyStatus);
		return _mapper.Map<StudyModel>(entity);
	}

	public bool Update(bool isGotoExam, PatientModel patientModel, StudyModel studyModel)
	{
		_logger.LogInformation("Update patient/study");
		var patientEntity = _mapper.Map<PatientEntity>(patientModel);
		var studyEntity = _mapper.Map<StudyEntity>(studyModel);

		ValidateBodyPart(studyModel);

		var savedResult = _studyRepository.UpdatePatientAndStudy(patientEntity, studyEntity);
		if (savedResult)
		{
            UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((patientModel, studyModel, DataOperateType.Update)));
			if (isGotoExam)
			{
				GotoExam?.Invoke(this, new EventArgs<(PatientModel, StudyModel)>((patientModel, studyModel)));
			}
		}
		return savedResult;
	}

	public bool UpdateProtocolByStudyId(UpdateStudyProtocolReq req)
	{
		return _studyRepository.UpdateProtocol(req.StudyId, req.Protocol);
	}

	public string GetStudyIdWithAbnoramlClosed()
	{
		return _studyRepository.GetStudyIdWithAbnoramlClosed();
	}

	public void UpdateStudyClosedWithAbnormalStatus()
	{
		_studyRepository.UpdateStudyClosedWithAbnormalStatus();
	}

	public bool Delete(StudyModel studyModel)
	{
		_logger.LogInformation("Delete patient/study");
		var studyEntity = _mapper.Map<StudyEntity>(studyModel);
		var savedResult = _studyRepository.Delete(studyEntity);
		if (savedResult)
		{
            UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((new PatientModel(), studyModel, DataOperateType.Delete)));

			//RefreshPatientManagementStudyList?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((new PatientModel(), studyModel, DataOperateType.Delete)));

		}
		return savedResult;
	}

	public bool DeleteByGuid(StudyModel studyModel)
	{
        var studyEntity = _mapper.Map<StudyEntity>(studyModel);
        var deleteResult= _studyRepository.DeleteByGuid(studyEntity);
        if (deleteResult)
        {
            UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((new PatientModel(), studyModel, DataOperateType.Delete)));
        }
        return deleteResult;
    }


    public string GotoEmergencyExamination(string patientId, string patientName)
	{
		//todo:上移到应用服务层，不放在数据库服务上
		_logger.LogInformation("Goto emergency examination");
		var patientEntity = new PatientEntity
		{
			Id = Guid.NewGuid().ToString(),
			CreateTime = DateTime.Now,
			PatientId = patientId,
			PatientName = patientName,
			PatientSex = Gender.Other,
		};
		var studyEntity = new StudyEntity
		{
			Id = Guid.NewGuid().ToString(),
			InternalPatientId = patientEntity.Id,
			Age = 30,
			AgeType = AgeType.Year,
			StudyTime = DateTime.Now,
			StudyInstanceUID = UIDHelper.CreateStudyInstanceUID(),
			StudyId = UIDHelper.CreateStudyID(),
		};
		WorkflowStatus workflowStatus = (WorkflowStatus)Enum.Parse(typeof(WorkflowStatus), WorkflowStatus.NotStarted.ToString());
		studyEntity.StudyStatus = ((int)workflowStatus).ToString();
		studyEntity.PatientType = (int)PatientType.Emergency;
		//结束
		var savedResult = _patientRepository.GotoEmergencyExamination(patientEntity, studyEntity);
		if (savedResult)
		{
			var patientModel = _mapper.Map<PatientModel>(patientEntity);
			var studyModel = _mapper.Map<StudyModel>(studyEntity);
            UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((patientModel, studyModel, DataOperateType.Add)));
			GotoExam?.Invoke(this, new EventArgs<(PatientModel, StudyModel)>((patientModel, studyModel)));
		}
		return savedResult == true ? studyEntity.Id : string.Empty;
	}

	public bool UpdateStudyProtocol(StudyModel studyModel)
	{
		//_logger.LogInformation("Update study protocol");
		ValidateBodyPart(studyModel);

		StudyEntity studyEntity = new StudyEntity();
		studyEntity.Id = studyModel.Id;
		studyEntity.StudyId = studyModel.StudyId;
		studyEntity.StudyDescription = studyModel.StudyDescription;
		studyEntity.Protocol = studyModel.Protocol;
		studyEntity.BodyPart = studyModel.BodyPart;
		return _studyRepository.UpdateProtocol(studyEntity);
	}

	public bool UpdateStudyStatus(string studyId, WorkflowStatus examStatus)
	{
		_logger.LogInformation("Update study status");
		var savedResult = _studyRepository.UpdateStudyStatus(studyId, examStatus);

		if (savedResult)
		{
            UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((new PatientModel(), new StudyModel { Id = studyId, StudyStatus = examStatus.ToString() }, DataOperateType.UpdateStudyStatus)));
		}

		return savedResult;
	}

	public bool UpdateStudyExaming(string studyId, DateTime studyTime, WorkflowStatus workflowStatus)
	{
		var savedResult = _studyRepository.UpdateStudyExaming(studyId, studyTime, workflowStatus);
        if (savedResult)
        {
            UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((new PatientModel(), new StudyModel { Id = studyId, StudyStatus = workflowStatus.ToString() }, DataOperateType.UpdateStudyStatus)));
        }

        return savedResult;
    }

	public bool InsertPatientListStudyListAndSeriesList(List<PatientModel> patientModels, List<StudyModel> studyModels, List<SeriesModel> seriesModels)
	{
		foreach (var studyModel in studyModels)
		{
			ValidateBodyPart(studyModel);
		}

		return _studyRepository.SavePatientStudySeries(_mapper.Map<List<PatientModel>, List<PatientEntity>>(patientModels), _mapper.Map<List<StudyModel>, List<StudyEntity>>(studyModels), _mapper.Map<List<SeriesModel>, List<SeriesEntity>>(seriesModels));

	}

	public PatientModel GetPatientModelById(string patientId)
	{
		var patientEntity = _studyRepository.GetPatientEntityById(patientId);
		return _mapper.Map<PatientModel>(patientEntity);
	}

	public bool ResumeExamination(StudyModel studyModel, string StudyId)
	{
		ValidateBodyPart(studyModel);

		var result = _studyRepository.ResumeExamination(_mapper.Map<StudyEntity>(studyModel), StudyId);
		if (result)
		{
			var patientEntity = _studyRepository.GetPatientEntityById(studyModel.InternalPatientId);
			if (patientEntity != null)
			{
                UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((_mapper.Map<PatientModel>(patientEntity), studyModel, DataOperateType.ResumeExamination)));
			}
		}
		return result;
	}

	public bool SwitchLockStatus(string studyId)
	{
		var result = _studyRepository.SwitchLockStatus(studyId);

		if (!result) return false;

        var data = _studyRepository.Get(studyId);
        if (data.Patient == null)
        {
            data.Patient = new PatientEntity();
        }
        if (data.Study == null)
        {
            data.Study = new StudyEntity();
        }

        UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((_mapper.Map<PatientModel>(data.Patient), _mapper.Map<StudyModel>(data.Study), DataOperateType.SwitchLockStatus)));

        return true;
    }

    public bool UpdateArchiveStatus(List<StudyModel> studyModels)
	{
		_logger.LogInformation("UpdateStudyArchiveStatus");
		var studyEntities = _mapper.Map<List<StudyModel>, List<StudyEntity>>(studyModels);
		var savedResult = _studyRepository.UpdateArchiveStatus(studyEntities);
		if (savedResult)
		{
			//todo:20250713 仅需要一次更新事件
			foreach (var item in studyModels)
			{
                UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((new PatientModel(), _mapper.Map<StudyModel>(item), DataOperateType.UpdateArchiveStatus)));
			}
		}
		return savedResult;
	}

	public bool UpdatePrintStatus(List<StudyModel> studyModels)
	{
		_logger.LogInformation("UpdateStudyPrintStatus");
		var studyEntities = _mapper.Map<List<StudyModel>, List<StudyEntity>>(studyModels);
		var savedResult = _studyRepository.UpdatePrintStatus(studyEntities);
		if (savedResult)
		{
            //todo:20250713 仅需要一次更新事件
            foreach (var item in studyModels)
			{
                UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((new PatientModel(), _mapper.Map<StudyModel>(item), DataOperateType.UpdatePrintStatus)));
			}
		}
		return savedResult;
	}


	public bool SetStudyArchiveFail()
	{
		return _studyRepository.SetStudyArchiveFail();
	}

	public StudyModel[] GetStudiesByIds(string[] studyIdList)
	{
		var seriesEntities = _studyRepository.GetStudiesByIds(studyIdList);
		return _mapper.Map<StudyModel[]>(seriesEntities);
	}

    public List<StudyModel> GetStudiesByPatient(string patientGuid)
	{
        var studies = _studyRepository.GetStudiesByPatient(patientGuid);
        return _mapper.Map<List<StudyModel>>(studies);
    }

    public StudyEntity GetStudyById(string studyId)
    {
        return _studyRepository.GetStudyById(studyId);
    }

    /// <summary>
    /// 检查BodyPart，预期值是枚举BodyPart的字符串，如果发现是整数值就输出错误信息，便于排查。
    /// </summary>
    /// <param name="studyModel"></param>
    /// <returns></returns>
    private void ValidateBodyPart(StudyModel studyModel)
	{
		string bodyPart = studyModel.BodyPart;
		bool isInteger = int.TryParse(bodyPart, out var result);
		if (isInteger)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat($"********** invalid bodyPart in StudyService.ValidateBodyPart:{bodyPart}.");
			stringBuilder.AppendFormat($"Current stack frame:{new StackFrame(0, true)};");
			stringBuilder.AppendFormat($"Upper 1 level stack frame:{new StackFrame(1, true)};");
			stringBuilder.AppendFormat($"Upper 2 level stack frame:{new StackFrame(2, true)};");
			_logger.LogWarning(stringBuilder.ToString());
		}
	}

	public bool Correct(PatientModel patientModel, StudyModel studyModel, string editor)
	{
		_logger.LogInformation("Correct patient/study");
		var patientEntity = _mapper.Map<PatientEntity>(patientModel);
		var studyEntity = _mapper.Map<StudyEntity>(studyModel);

		ValidateBodyPart(studyModel);

		var savedResult = _studyRepository.CorrectPatientAndStudy(patientEntity, studyEntity, editor);
		if (savedResult)
		{
            UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((patientModel, studyModel, DataOperateType.Update)));
		}
		return savedResult;
	}

	public List<(PatientModel, StudyModel)> GetCorrectionHistoryList(string studyId)
	{
		var result = _studyRepository.GetCorrectionHistoryList(studyId);
		var list = new List<(PatientModel, StudyModel)>();
		list = _mapper.Map<List<(PatientEntity, StudyEntity)>, List<(PatientModel, StudyModel)>>(result);

		return list;
	}

	public bool UpdateWorklistByStudy(PatientModel patientModel, StudyModel studyModel)
	{
		var patientEntity = _mapper.Map<PatientEntity>(patientModel);
		var studyEntity = _mapper.Map<StudyEntity>(studyModel);
		var result = _studyRepository.UpdateWorklistByStudy(patientEntity, studyEntity);

		bool executionResult = result.Item1;
		bool needToRefreshWorkList = result.Item2;
		if (needToRefreshWorkList)
		{
            UpdateStudyInformation?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((patientModel, studyModel, DataOperateType.AddFromHisRis)));
		}
		return executionResult;
	}

    public bool UpdatePrintConfigPath(string studyId, string printConfigPath)
	{
		_logger.LogInformation("UpdatePrintConfigPath");
		return _studyRepository.UpdatePrintConfigPath(studyId, printConfigPath);
	}
}