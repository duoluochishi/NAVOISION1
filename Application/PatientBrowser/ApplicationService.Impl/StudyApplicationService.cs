//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有 (C)2022,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------

using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.CTS.Enums;
using NV.CT.DatabaseService.Contract;
using NV.CT.PatientBrowser.ApplicationService.Contract.Interfaces;
using NV.CT.PatientBrowser.ApplicationService.Contract.Models;
using NV.MPS.Exception;

namespace NV.CT.PatientBrowser.ApplicationService.Impl;
public class StudyApplicationService : IStudyApplicationService
{
    private readonly IMapper _mapper;
    private readonly IStudyService _studyService;
    private readonly ILogger<StudyApplicationService> _logger;
    public event EventHandler<EventArgs<(PatientModel, StudyModel)>>? SelectItemChanged;
    public event EventHandler<EventArgs<(PatientModel, StudyModel, DataOperateType)>>? RefreshWorkList;
    public event EventHandler<EventArgs<SearchTimeType>>? RefreshRearchDateType;
    public StudyApplicationService(IMapper mapper, IStudyService studyService, ILogger<StudyApplicationService> logger)
    {
        _mapper = mapper;
        _studyService = studyService;
        _logger = logger;
        _studyService.UpdateStudyInformation += OnStudyInformationUpdated;
    }
    private void OnStudyInformationUpdated(object? sender, EventArgs<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel, DataOperateType)> e)
    {
        PatientModel patientModel = _mapper.Map<PatientModel>(e.Data.Item1);
        StudyModel studyModel = _mapper.Map<StudyModel>(e.Data.Item2);
        RefreshWorkList?.Invoke(this, new EventArgs<(PatientModel, StudyModel, DataOperateType)>((patientModel, studyModel, e.Data.Item3)));
    }

    public void RaiseSelectItemChanged((PatientModel, StudyModel) selectedItem)
    {
        SelectItemChanged?.Invoke(this, new EventArgs<(PatientModel, StudyModel)>(selectedItem));
    }

    public bool Insert(bool isAddProcedure, bool isGotoExam, PatientModel patientModel, StudyModel studyModel)
    {        
        _logger.LogInformation("Patient Register");
        var _patientModel = _mapper.Map<DatabaseService.Contract.Models.PatientModel>(patientModel);
        var _studyModel = _mapper.Map<DatabaseService.Contract.Models.StudyModel>(studyModel);
        try
        {
            return _studyService.Insert(isAddProcedure, isGotoExam, _patientModel, _studyModel);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to register patient with exception: {ex.Message}");
            throw new NanoException("MCS005000001", "Failed to register patient."); // Will be replaced with the following line code later.
            //throw new NanoException(ErrorCodeResource.MCS_PatientBrowserRegisterFailed, "Failed to register patient.", ex); 
        }
    }

    public bool Update(bool isGotoExam, PatientModel patientModel, StudyModel studyModel)
    {
        _logger.LogInformation("Patient Update");
        var _patientModel = _mapper.Map<DatabaseService.Contract.Models.PatientModel>(patientModel);
        var _studyModel = _mapper.Map<DatabaseService.Contract.Models.StudyModel>(studyModel);
        try
        {
            return _studyService.Update(isGotoExam, _patientModel, _studyModel);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to update patient with exception: {ex.Message}");
            throw new NanoException("MCS005000003", "Failed to update patient."); // Will be replaced with the following line code later.
            //throw new NanoException(ErrorCodeResource.MCS_PatientBrowserUpdateFailed, "Failed to update patient.", ex); 
        }
    }

    public List<(PatientModel, StudyModel)> GetPatientStudyListWithNotStartedStudyDate(string startDate, string endDate)
    {
        var result = _studyService.GetPatientStudyListWithNotStartedStudyDate(startDate, endDate);
        try
        {
            List<(PatientModel, StudyModel)> list = _mapper.Map<List<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel)>, List<(PatientModel, StudyModel)>>(result);
            return list;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to query patient list with exception: {ex.Message}");
            throw new NanoException("MCS005000002", "Failed to query patient list."); // Will be replaced with the following line code later.
            //throw new NanoException(ErrorCodeResource.MCS_PatientBrowserQueryFailed, "Failed to query patient list.", ex); 
        }
    }

    public List<(PatientModel, StudyModel)> GetPatientStudyListWithNotStarted()
    {
        var result = _studyService.GetPatientStudyListWithNotStarted();
        try
        {
            List<(PatientModel, StudyModel)> list = _mapper.Map<List<(DatabaseService.Contract.Models.PatientModel, DatabaseService.Contract.Models.StudyModel)>, List<(PatientModel, StudyModel)>>(result);
            return list;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to query patient list with exception: {ex.Message}");
            throw new NanoException("MCS005000002", "Failed to query patient list."); // Will be replaced with the following line code later.
            //throw new NanoException(ErrorCodeResource.MCS_PatientBrowserQueryFailed, "Failed to query patient list.", ex); 
        }
    }

    public bool Delete(StudyModel studyModel)
    {
        _logger.LogInformation("Delete patient ");
        var _studyModel = _mapper.Map<DatabaseService.Contract.Models.StudyModel>(studyModel);
        try
        {
            return _studyService.Delete(_studyModel);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to delete patient with exception: {ex.Message}");
            throw new NanoException("MCS005000004", "Failed to delete patient."); // Will be replaced with the following line code later.
            //throw new NanoException(ErrorCodeResource.MCS_PatientBrowserDeleteFailed, "Failed to delete patient.", ex); 
        }
    }

    public string GotoEmergencyExamination(string patientId, string patientName)
    {
        _logger.LogInformation("GotoEmergencyExamination");
        try
        {
            return _studyService.GotoEmergencyExamination(patientId, patientName);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, $"Failed to go to emergency exam with exception: {ex.Message}");
            throw new NanoException("MCS005000006", "Failed to go to emergency exam."); // Will be replaced with the following line code later.
            //throw new NanoException(ErrorCodeResource.MCS_PatientBrowserGotoExamFailed, "Failed to delete patient.", ex); 
        }
    }

    public void RefreshRearchDateTyped(SearchTimeType searchTimeType)
    {
        RefreshRearchDateType?.Invoke(this, new EventArgs<SearchTimeType>(searchTimeType));
    }
}