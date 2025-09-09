//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/28 9:48:00           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using Microsoft.Extensions.Logging;
using NV.CT.CTS;
using NV.CT.Examination.ApplicationService.Impl.ValidateRules;
using NV.CT.SystemInterface.MCSRuntime.Contract;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.CT.WorkflowService.Contract;

namespace NV.CT.Examination.ApplicationService.Impl;

public class GoService : IGoService
{
    private readonly ILogger<GoService> _logger;
    public event EventHandler<EventArgs<bool>>? Validated;
    public event EventHandler<EventArgs<bool>>? ReconAllValidated;
    public event EventHandler<EventArgs<bool>>? ParameterValidated;
    public event EventHandler<EventArgs<bool>>? ParameterLogicValidated;
    private readonly IHeatCapacityService _heatCapacityService;

    public IList<IGoValidateRule> StausValidateModels { get; set; } = new List<IGoValidateRule>();

    public IList<IGoValidateRule> ReconAllValidateModels { get; set; } = new List<IGoValidateRule>();

    public IList<IGoValidateRule> ParameterValidateModels { get; set; } = new List<IGoValidateRule>();

    public IList<IGoValidateRule> ParameterLogicModels { get; set; } = new List<IGoValidateRule>();

    public bool IsStopValidated { get; set; } = false;

    public GoService(ILogger<GoService> logger,
        IGoValidateDialogService goValidateService,
        ISpecialDiskService specialDiskService,
        ISystemReadyService systemReadyService,
        IHeatCapacityService heatCapacityService,
        IProtocolHostService protocolHostService,
        IStudyHostService studyHost,
        ITablePositionService tablePositionService,
        IAuthorization authorization,
        IDoseEstimateService doseEstimateService)
    {
        _logger = logger;
        _heatCapacityService = heatCapacityService;
        StausValidateModels.Add(new SpecialDiskRule(specialDiskService, goValidateService));
        StausValidateModels.Add(new DoseNatificationRule(goValidateService, protocolHostService, authorization, doseEstimateService));
        StausValidateModels.Add(new DoseAlterRule(goValidateService, protocolHostService, authorization, doseEstimateService));
        StausValidateModels.Add(new SystemStatusRule(systemReadyService, goValidateService));
        StausValidateModels.Add(new HeatCapacityRule(heatCapacityService, goValidateService));

        ReconAllValidateModels.Add(new SpecialDiskRule(specialDiskService, goValidateService));

        ParameterValidateModels.Add(new ScanningParameterValidationRule(protocolHostService, goValidateService));
        ParameterValidateModels.Add(new AcquisitionParameterValidationRule(protocolHostService, goValidateService));
        ParameterValidateModels.Add(new CollimatorParameterValidationRule(protocolHostService, goValidateService));
        ParameterValidateModels.Add(new TableParameterValidationRule(protocolHostService, goValidateService, studyHost, tablePositionService));
        ParameterValidateModels.Add(new GantryParameterValidationRule(protocolHostService, goValidateService));

        ParameterLogicModels.Add(new ParameterLogicRule(protocolHostService, goValidateService, tablePositionService));
    }

    public void GoValidate()
    {
        if (_heatCapacityService.Current is not null)
        {
            _logger.LogDebug($"StartScan.HeatCapacities : {JsonConvert.SerializeObject(_heatCapacityService.Current)}");
        }

        IsStopValidated = false;
        foreach (var item in StausValidateModels)
        {
            if (!IsStopValidated)
            {
                item.ValidateGo();
            }
            else
            {
                return;
            }
        }
        Validated?.Invoke(this, new EventArgs<bool>(true));
    }

    public void ReconAllValidate()
    {
        IsStopValidated = false;
        foreach (var item in ReconAllValidateModels)
        {
            if (!IsStopValidated)
            {
                item.ValidateGo();
            }
            else
            {
                return;
            }
        }
        ReconAllValidated?.Invoke(this, new EventArgs<bool>(true));
    }

    public void StopValidated(bool isStop)
    {
        IsStopValidated = isStop;
    }

    public void ParamsValidate()
    {
        IsStopValidated = false;
        foreach (var item in ParameterValidateModels)
        {
            if (!IsStopValidated)
            {
                item.ValidateGo();
            }
            else
            {
                return;
            }
        }
        ParameterValidated?.Invoke(this, new EventArgs<bool>(true));
    }

    public void ParamsLogicValidate()
    {
        IsStopValidated = false;
        foreach (var item in ParameterLogicModels)
        {
            if (!IsStopValidated)
            {
                item.ValidateGo();
            }
            else
            {
                return;
            }
        }
        ParameterLogicValidated?.Invoke(this, new EventArgs<bool>(true));
    }
}