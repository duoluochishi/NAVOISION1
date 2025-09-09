//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2024, 纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2024/4/22 13:45:19    V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using AutoMapper;
using Microsoft.Extensions.Logging;
using NV.CT.CTS.Models;
using NV.CT.FacadeProxy.Helpers;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.CT.SystemInterface.MRSIntegration.Contract.Models;
using NV.CT.FacadeProxy.Essentials.ThirdPartyLibraryCallers.CTDICalculate;

namespace NV.CT.SystemInterface.MRSIntegration.Impl;

public class DoseEstimateService : IDoseEstimateService
{
    private IMapper _mapper;
    private ILogger<DoseEstimateService> _logger;
    private readonly CTDICalculateHelper _helper;
    private const int _timeUnit = 1000;

    public DoseEstimateService(ILogger<DoseEstimateService> logger, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
        _helper = new CTDICalculateHelper();
    }

    public EstimateDoseInfo GetEstimateDoseInfo(DoseEstimateParam param)
    {
        CTDICalculateResult ctdiInfo;
        bool isSuceess = false;
        switch (param.ScanOption)
        {
            case ScanOption.Surview:
                var topoParam = GetTopoAxialCTDIParam(param);
                isSuceess = _helper.CalculateTopoCTDI(topoParam, out ctdiInfo).Status == CommandStatus.Success;
                break;
            case ScanOption.Axial:
                var axialParam = GetTopoAxialCTDIParam(param);
                isSuceess = _helper.CalculateAxialCTDI(axialParam, out ctdiInfo).Status == CommandStatus.Success;
                break;
            case ScanOption.Helical:
                var helicalParam = GetHeliCTDIParam(param);
                isSuceess = _helper.CalculateHeliCTDI(helicalParam, out ctdiInfo).Status == CommandStatus.Success;
                break;
            case ScanOption.DualScout:
                var dualParam = GetTopoAxialCTDIParam(param);
                isSuceess = _helper.CalculateTopoCTDI(dualParam, out ctdiInfo).Status == CommandStatus.Success;
                ctdiInfo.CTDI = ctdiInfo.CTDI * 2;
                ctdiInfo.DLP = ctdiInfo.DLP * 2;
                break;
            default:
                ctdiInfo = default;
                break;
        }
        return GetEstimateDoseInfo(ctdiInfo, isSuceess);
    }

    private TopogramAxialCTDIParameter GetTopoAxialCTDIParam(DoseEstimateParam estParam)
    {
        var param = new TopogramAxialCTDIParameter();
        param.BodyPart = _mapper.Map<CTS.Enums.BodyPart, FacadeProxy.Common.Enums.BodyPart>(estParam.BodyPart);
        param.ExposureMode = estParam.ExposureMode;
        param.KV = estParam.KV;
        param.MA = estParam.MA;
        param.FramePerCycle = estParam.FramePerCycle;
        param.ExposureTime = estParam.ExposureTime / _timeUnit;
        param.TableFeed = estParam.TableFeed;
        param.ScanLength = estParam.ScanLength;
        param.CollimatorOpenWidth = estParam.CollimatorOpenWidth;

        return param;
    }

    private HelicalCTDIParameter GetHeliCTDIParam(DoseEstimateParam estParam)
    {
        var param = new HelicalCTDIParameter();
        param.BodyPart = _mapper.Map<CTS.Enums.BodyPart, FacadeProxy.Common.Enums.BodyPart>(estParam.BodyPart);
        param.ExposureMode = estParam.ExposureMode;
        param.KV = estParam.KV;
        param.MA = estParam.MA;
        param.FramePerCycle = estParam.FramePerCycle;
        param.ExposureTime = estParam.ExposureTime / _timeUnit;
        param.Pitch = estParam.Pitch;
        param.ScanLength = estParam.ScanLength;
        param.CollimatorOpenWidth = estParam.CollimatorOpenWidth;

        return param;
    }

    private EstimateDoseInfo GetEstimateDoseInfo(CTDICalculateResult info, bool isEstimateSuccess)
    {
        return new EstimateDoseInfo
        {
            CTDIvol = info.CTDI,
            DLP = info.DLP,
            PhantomType = info.PhantomType,
            IsEstimateSuccess = isEstimateSuccess
        };
    }
}