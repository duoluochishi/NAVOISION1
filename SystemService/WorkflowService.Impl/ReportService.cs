//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/5/22 13:11:38     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using NV.CT.DatabaseService.Contract;
using NV.CT.Protocol;
using NV.CT.Protocol.Models;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.WorkflowService.Impl;

public class ReportService
{
    private readonly ILogger<ReportService> _logger;
    private readonly IStudyService _studyService;
    private readonly IScanTaskService _scanService;
    private readonly IReconTaskService _reconService;
    private readonly ISeriesService _seriesService;
    private readonly DoseReport _doseReport;
    private readonly BlackImageReport _blackImageReport;

    public ReportService(ILogger<ReportService> logger, IStudyService studyService, IScanTaskService scanService, IReconTaskService reconService, ISeriesService seriesService, DoseReport doseReport, BlackImageReport blackImageReport)
    {
        _logger = logger;
        _studyService = studyService;
        _scanService = scanService;
        _reconService = reconService;
        _seriesService = seriesService;
        _doseReport = doseReport;
        _blackImageReport = blackImageReport;
    }

    public void CreateReport(string studyId)
    {
        //TODO:剂量报告待实现
        //获取Study信息，解析协议信息，获取Scan/Recon/Seriers信息
        var (studyModel, patientModel) = _studyService.Get(studyId);
        if (string.IsNullOrEmpty(studyModel.Protocol))
        {
            _logger.LogDebug($"No protocol data of examination, study id: {studyId}, and can not create dose sr.");
            return;
        }
        ProtocolModel protocolModel;
        try
        {
            protocolModel = ProtocolHelper.Deserialize(studyModel.Protocol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Protocol deserialize failed, study id: {studyId}");
            return;
        }

        _logger.LogDebug($"DoseInfo.CreateReport study id : {studyId}");
        var scanTasks = _scanService.GetAll(studyId);
        var reconTasks = _reconService.GetAll(studyId);
        var seriesModels = _seriesService.GetSeriesByStudyId(studyId);

        /*
        var filePath = Path.Combine(RuntimeConfig.Instance.Data, "ImageData", studyModel.StudyInstanceUID);
        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }
        */

        try
        {
            _doseReport.CreateReport(studyModel, patientModel, protocolModel, scanTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create dose report (CreateStructureReport, {studyId}): {ex.Message}");
        }

        try
        {
            _blackImageReport.CreateReport(studyModel, patientModel, protocolModel, scanTasks);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, $"Failed to create dose report (CreateBlackImageReport, {studyId}): {ex.Message}");
        }
    }
}
