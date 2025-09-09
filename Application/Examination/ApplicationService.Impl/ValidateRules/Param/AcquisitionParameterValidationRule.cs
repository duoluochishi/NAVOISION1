//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/7/31 14:45:22           V1.0.0       jianggang
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------
using NV.MPS.Configuration;
using NV.MPS.Environment;
using System.Text;

namespace NV.CT.Examination.ApplicationService.Impl.ValidateRules;

public class AcquisitionParameterValidationRule : IGoValidateRule
{
    private readonly IGoValidateDialogService _goValidateDialogService;
    private readonly IProtocolHostService _protocolHostService;

    public AcquisitionParameterValidationRule(IProtocolHostService protocolHostService, IGoValidateDialogService goValidateDialogService)
    {
        _protocolHostService = protocolHostService;
        _goValidateDialogService = goValidateDialogService;
    }

    public void ValidateGo()
    {
        var activeItem = _protocolHostService.Models.FirstOrDefault(item => item.Measurement.Descriptor.Id == _protocolHostService.CurrentMeasurementID);
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var scan in activeItem.Measurement.Children)
        {
            if (scan.Status == PerformStatus.Unperform)
            {
                stringBuilder.Append(IsCondition(scan));
            }
        }
        if (stringBuilder.Capacity > 0)
        {
            _goValidateDialogService.PopValidateMessageChanged(stringBuilder.ToString(), RuleDialogType.CommonNotificationDialog);
        }
    }

    private string IsCondition(ScanModel scanModel)
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append(ValidateExposureTime(scanModel.ExposureTime));
        stringBuilder.Append(ValidateFrameTime(scanModel.FrameTime));
        stringBuilder.Append(ValidateAvailableFramesPerCycle((int)scanModel.FramesPerCycle));
        return stringBuilder.ToString();
    }

    private string ValidateExposureTime(uint exposureTime)
    {
        StringBuilder stringBuilder = new StringBuilder();
        AcquisitionInfo node = SystemConfig.AcquisitionConfig.Acquisition;

        if (node.ExposureTime.Min > exposureTime)
        {
            stringBuilder.Append(string.Format($"Exposure time cannot be smaller than {UnitConvert.Microsecond2Second((double)node.ExposureTime.Min)};"));
        }
        if (node.ExposureTime.Max < exposureTime)
        {
            stringBuilder.Append(string.Format($"Exposure time cannot be greater  than {UnitConvert.Microsecond2Second((double)node.ExposureTime.Max)};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateFrameTime(uint frameTime)
    {
        StringBuilder stringBuilder = new StringBuilder();
        AcquisitionInfo node = SystemConfig.AcquisitionConfig.Acquisition;

        if (node.FrameTime.Min > frameTime)
        {
            stringBuilder.Append(string.Format($"Frame time cannot be smaller than {UnitConvert.Microsecond2Second((double)node.FrameTime.Min)};"));
        }
        if (node.FrameTime.Max < frameTime)
        {
            stringBuilder.Append(string.Format($"Frame time cannot be greater  than {UnitConvert.Microsecond2Second((double)node.FrameTime.Max)};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateAvailableFramesPerCycle(int framesPerCycle)
    {
        StringBuilder stringBuilder = new StringBuilder();
        AcquisitionInfo node = SystemConfig.AcquisitionConfig.Acquisition;
        if (!node.FramesPerCycle.Ranges.Contains(framesPerCycle))
        {
            stringBuilder.Append(string.Format($"Capture Laps Unavailable: {framesPerCycle};"));
        }
        return stringBuilder.ToString();
    }
}