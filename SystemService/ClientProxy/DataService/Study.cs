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
using Newtonsoft.Json;
using NV.MPS.Communication;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.CTS.Extensions;
using NV.CT.CTS.Models;
using NV.CT.DatabaseService.Contract;
using NV.CT.DatabaseService.Contract.Models;
using NV.CT.DatabaseService.Contract.Entities;

namespace NV.CT.ClientProxy.DataService;

public class Study : IStudyService
{
    private readonly MCSServiceClientProxy _clientProxy;

#pragma warning disable 67
    public event EventHandler<EventArgs<(PatientModel, StudyModel, DataOperateType)>>? UpdateStudyInformation;
    //public event EventHandler<EventArgs<(PatientModel, StudyModel, DataOperateType)>>? RefreshPatientManagementStudyList;
    public event EventHandler<EventArgs<(PatientModel, StudyModel)>>? GotoExam;
#pragma warning restore 67

    public Study(MCSServiceClientProxy clientProxy)
    {
        _clientProxy = clientProxy;
    }

    public bool Update(bool isGotoExam, PatientModel patientModel, StudyModel studyModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.Update),
            Data = Tuple.Create(isGotoExam, patientModel, studyModel).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public bool UpdateProtocolByStudyId(UpdateStudyProtocolReq req)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.UpdateProtocolByStudyId),
            Data = req.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
	}

    public string GetStudyIdWithAbnoramlClosed()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetStudyIdWithAbnoramlClosed)
        });
        if (commandResponse.Success)
        {
            return commandResponse.Data;
        }
        return string.Empty;
    }

    public void UpdateStudyClosedWithAbnormalStatus()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.UpdateStudyClosedWithAbnormalStatus),

        });
    }

    public bool UpdateStudyProtocol(StudyModel studyModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.UpdateStudyProtocol),
            Data = studyModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return default;
    }

    public (StudyModel Study, PatientModel Patient) Get(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.Get),
            Data = studyId
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<(StudyModel Study, PatientModel Patient)>(commandResponse.Data);
            return res;
        }

        return (new StudyModel(), new PatientModel());
    }

    public (PatientModel Patient, StudyModel Study) GetWithUID(string studyInstanceUID)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetWithUID),
            Data = studyInstanceUID
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<(PatientModel Patient, StudyModel Study)>();
            return res;
        }
        return default;
    }

    public List<(PatientModel, StudyModel)> GetPatientStudyListWithNotStartedStudyDate(string startDate, string endDate)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetPatientStudyListWithNotStartedStudyDate),
            Data = Tuple.Create(startDate, endDate).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<(PatientModel, StudyModel)>>(commandResponse.Data);
            return res;
        }

        return new List<(PatientModel, StudyModel)>();
    }

    public List<(PatientModel, StudyModel)> GetPatientStudyListByFilter(StudyListFilterModel filter)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetPatientStudyListByFilter),
            Data = filter.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<(PatientModel, StudyModel)>>(commandResponse.Data);
            return res;
        }

        return new List<(PatientModel, StudyModel)>();
    }

	public List<(PatientModel, StudyModel)> GetPatientStudyListByFilterSimple(StudyQueryModel queryModel)
	{
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetPatientStudyListByFilterSimple),
            Data = queryModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<(PatientModel, StudyModel)>>(commandResponse.Data);
            return res;
        }

        return new List<(PatientModel, StudyModel)>();
	}

    public List<(PatientModel, StudyModel)> GetPatientStudyListWithNotStarted()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetPatientStudyListWithNotStarted),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = JsonConvert.DeserializeObject<List<(PatientModel, StudyModel)>>(commandResponse.Data);
            return res;
        }

        return new List<(PatientModel, StudyModel)>();
    }

    public List<(PatientModel, StudyModel)> GetPatientStudyListWithEnd()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetPatientStudyListWithEnd),
            Data = string.Empty
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<List<(PatientModel, StudyModel)>>();
            return res;
        }
        return default;
    }

    public List<(PatientModel, StudyModel)> GetPatientStudyListWithEndStudyDate(string startDate, string endDate)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetPatientStudyListWithEndStudyDate),
            Data = Tuple.Create(startDate, endDate).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<List<(PatientModel, StudyModel)>>();
            return res;
        }
        return default;
    }

    public StudyModel GetStudyModelByPatientIdAndStudyStatus(string patientId, string studyStatus)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetStudyModelByPatientIdAndStudyStatus),
            Data = Tuple.Create(patientId, studyStatus).ToJson()
        });
        if (commandResponse.Success)
        {
            if (string.IsNullOrEmpty(commandResponse.Data))
            {
                return null;
            }
            var res = commandResponse.Data.DeserializeTo<StudyModel>();
            return res;

        }
        return null;
    }

    public PatientModel GetPatientModelById(string patientId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetPatientModelById),
            Data = patientId
        });
        if (commandResponse.Success)
        {
            if (string.IsNullOrEmpty(commandResponse.Data))
            {
                return null;
            }
            var res = commandResponse.Data.DeserializeTo<PatientModel>();
            return res;
        }
        return default;
    }

    public bool ResumeExamination(StudyModel studyModel, string StudyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.ResumeExamination),
            Data = Tuple.Create(studyModel, StudyId).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool SwitchLockStatus(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.SwitchLockStatus),
            Data = studyId
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool UpdateStudyStatus(string studyId, WorkflowStatus examStatus)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.UpdateStudyStatus),
            Data = Tuple.Create(studyId, examStatus).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool UpdateStudyExaming(string studyId, DateTime studyTime, WorkflowStatus workflowStatus)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.UpdateStudyExaming),
            Data = Tuple.Create(studyId, studyTime, workflowStatus).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool InsertPatientListStudyListAndSeriesList(List<PatientModel> patientModels, List<StudyModel> studyModels, List<SeriesModel> seriesModels)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.InsertPatientListStudyListAndSeriesList),
            Data = Tuple.Create(patientModels, studyModels, seriesModels).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
        // return _studyRepository.InsertPatientListStudyListAndSeriesList(_mapper.Map<List<PatientModel>, List<PatientEntity>>(patientModels), _mapper.Map<List<StudyModel>, List<StudyEntity>>(studyModels), _mapper.Map<List<SeriesModel>, List<SeriesEntity>>(seriesModels));
    }

    public bool Delete(StudyModel studyModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.Delete),
            Data = studyModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool DeleteByGuid(StudyModel studyModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.DeleteByGuid),
            Data = studyModel.ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public string GotoEmergencyExamination(string patientId, string patientName)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GotoEmergencyExamination),
            Data = (patientId, patientName).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToString(commandResponse.Data);
            return res;
        }
        return string.Empty;
    }

    public bool Insert(bool isAddProcedure, bool isGotoExam, PatientModel patientModel, StudyModel studyModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.Insert),
            Data = Tuple.Create(isAddProcedure, isGotoExam, patientModel, studyModel).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool UpdateArchiveStatus(List<StudyModel> studyModels)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.UpdateArchiveStatus),
            Data = studyModels.ToJson(),
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool UpdatePrintStatus(List<StudyModel> studyModels)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.UpdatePrintStatus),
            Data = studyModels.ToJson(),
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool SetStudyArchiveFail()
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.SetStudyArchiveFail),

        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public StudyModel[] GetStudiesByIds(string[] studyIdList)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetStudiesByIds),
            Data = studyIdList.ToJson(),

        });
        if (commandResponse.Success)
        {
            var result = JsonConvert.DeserializeObject<StudyModel[]>(commandResponse.Data);
            return result;
        }
        return new StudyModel[] { };
    }

    public List<StudyModel> GetStudiesByPatient(string patientGuid)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetStudiesByPatient),
            Data = patientGuid,

        });
        if (commandResponse.Success)
        {
            var result = JsonConvert.DeserializeObject<List<StudyModel>>(commandResponse.Data);
            return result;
        }
        return new List<StudyModel> { };
    }

    public StudyEntity GetStudyById(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetStudyById),
            Data = studyId,

        });
        if (commandResponse.Success)
        {
            var result = JsonConvert.DeserializeObject<StudyEntity>(commandResponse.Data);
            return result;
        }
        return null;
    }

    public bool Correct(PatientModel patientModel, StudyModel studyModel, string editor)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.Correct),
            Data = Tuple.Create(patientModel, studyModel, editor).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }

        return false;
    }

    public List<(PatientModel, StudyModel)> GetCorrectionHistoryList(string studyId)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.GetCorrectionHistoryList),
            Data = studyId
        });
        if (commandResponse.Success)
        {
            var res = commandResponse.Data.DeserializeTo<List<(PatientModel, StudyModel)>>();
            return res;
        }
        return default;
    }

    public bool UpdateWorklistByStudy(PatientModel patientModel, StudyModel studyModel)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.UpdateWorklistByStudy),
            Data = Tuple.Create(patientModel, studyModel).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;
    }

    public bool UpdatePrintConfigPath(string studyId, string printConfigPath)
    {
        var commandResponse = _clientProxy.ExecuteCommand(new CommandRequest()
        {
            Namespace = typeof(IStudyService).Namespace,
            SourceType = nameof(IStudyService),
            ActionName = nameof(IStudyService.UpdatePrintConfigPath),
            Data = Tuple.Create(studyId, printConfigPath).ToJson()
        });
        if (commandResponse.Success)
        {
            var res = Convert.ToBoolean(commandResponse.Data);
            return res;
        }
        return false;

    }

}

