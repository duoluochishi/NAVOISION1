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
using System.Text;
using NV.CT.FacadeProxy.Common.Enums;
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Environment;

namespace NV.CT.Examination.ApplicationService.Impl.ValidateRules;

public class ParameterLogicRule : IGoValidateRule
{
    private readonly IGoValidateDialogService _goValidateDialogService;
    private readonly IProtocolHostService _protocolHostService;
    private readonly ITablePositionService _tablePositionService;


    public ParameterLogicRule(IProtocolHostService protocolHostService,
        IGoValidateDialogService goValidateDialogService,
        ITablePositionService tablePositionService)
    {
        _protocolHostService = protocolHostService;
        _goValidateDialogService = goValidateDialogService;
        _tablePositionService = tablePositionService;
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
        stringBuilder.Append(ValidateVolumeStartPosition(scanModel));
        stringBuilder.Append(ValidateTablePositionHeiht(scanModel));
        return stringBuilder.ToString();
    }

    private string ValidateVolumeStartPosition(ScanModel scanModel)
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (scanModel.ScanOption == ScanOption.Axial
            || scanModel.ScanOption == ScanOption.TestBolus
            || scanModel.ScanOption == ScanOption.BolusTracking
            || scanModel.ScanOption == ScanOption.NVTestBolus
            || scanModel.ScanOption == ScanOption.NVTestBolusBase)
        {
            if (scanModel.TableDirection == TableDirection.In)
            {
                if (scanModel.TableStartPosition < scanModel.TableEndPosition)
                {
                    stringBuilder.Append(string.Format($"Bed start point less than end point position;"));
                }
            }
            if (scanModel.TableDirection == TableDirection.Out)
            {
                if (scanModel.TableStartPosition > scanModel.TableEndPosition)
                {
                    stringBuilder.Append(string.Format($"Bed start point greater than end point position;"));
                }
            }
        }
        else
        {
            if (scanModel.TableDirection == TableDirection.In)
            {
                if (scanModel.TableStartPosition <= scanModel.TableEndPosition)
                {
                    stringBuilder.Append(string.Format($"Bed start point less than or equal to end point position;"));
                }
            }
            if (scanModel.TableDirection == TableDirection.Out)
            {
                if (scanModel.TableStartPosition >= scanModel.TableEndPosition)
                {
                    stringBuilder.Append(string.Format($"Bed start point greater than or equal to end point position;"));
                }
            }
        }
        return stringBuilder.ToString();
    }

    private string ValidateTablePositionHeiht(ScanModel scanModel)
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (!_tablePositionService.ValidatePosition(scanModel.TableStartPosition, (int)_tablePositionService.CurrentTablePosition.VerticalPosition))
        {
            stringBuilder.Append(string.Format($"The bed won't reach {UnitConvert.Micron2Millimeter((double)scanModel.TableStartPosition)} at {UnitConvert.Micron2Millimeter((double)_tablePositionService.CurrentTablePosition.VerticalPosition)}."));
        }
        if (!_tablePositionService.ValidatePosition(scanModel.TableEndPosition, (int)_tablePositionService.CurrentTablePosition.VerticalPosition))
        {
            stringBuilder.Append(string.Format($"The bed won't reach {UnitConvert.Micron2Millimeter((double)scanModel.TableEndPosition)} at {UnitConvert.Micron2Millimeter((double)_tablePositionService.CurrentTablePosition.VerticalPosition)}."));
        }
        return stringBuilder.ToString();
    }
}