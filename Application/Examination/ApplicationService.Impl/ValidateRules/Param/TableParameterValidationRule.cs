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
using NV.CT.SystemInterface.MRSIntegration.Contract.Interfaces;
using NV.MPS.Configuration;
using NV.MPS.Environment;
using System.Text;

namespace NV.CT.Examination.ApplicationService.Impl.ValidateRules;

public class TableParameterValidationRule : IGoValidateRule
{
    private readonly IGoValidateDialogService _goValidateDialogService;
    private readonly IProtocolHostService _protocolHostService;
    private readonly IStudyHostService _studyHost;
    private readonly ITablePositionService _tablePositionService;

    public TableParameterValidationRule(IProtocolHostService protocolHostService,
        IGoValidateDialogService goValidateDialogService,
        IStudyHostService studyHost,
        ITablePositionService tablePositionService)
    {
        _protocolHostService = protocolHostService;
        _goValidateDialogService = goValidateDialogService;
        _studyHost = studyHost;
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
        stringBuilder.Append(ValidateTableStartPosition(scanModel.TableStartPosition));
        stringBuilder.Append(ValidateTableEndPosition(scanModel.TableEndPosition));
        stringBuilder.Append(ValidateTableAcceleration(scanModel.TableAcceleration));
        stringBuilder.Append(ValidateTableSpeed(scanModel.TableSpeed));
        stringBuilder.Append(ValidateTableMaxLoad(_studyHost.Instance.PatientWeight));
        stringBuilder.Append(ValidateVerticalPosition(_tablePositionService.CurrentTablePosition.VerticalPosition));
        return stringBuilder.ToString();
    }

    private string ValidateTableStartPosition(int tableStartPosition)
    {
        StringBuilder stringBuilder = new StringBuilder();
        TableInfo node = SystemConfig.TableConfig.Table;
        if (node.MinZ.Value > tableStartPosition)
        {
            stringBuilder.Append(string.Format($"Table start position cannot be smaller than {UnitConvert.Micron2Millimeter((double)node.MinZ.Value)};"));
        }
        if (node.MaxZ.Value < tableStartPosition)
        {
            stringBuilder.Append(string.Format($"Table start position cannot be greater  than {UnitConvert.Micron2Millimeter((double)node.MaxZ.Value)};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateTableEndPosition(int tableEndPosition)
    {
        StringBuilder stringBuilder = new StringBuilder();
        TableInfo node = SystemConfig.TableConfig.Table;

        if (node.MinZ.Value > tableEndPosition)
        {
            stringBuilder.Append(string.Format($"Table end position cannot be smaller than {UnitConvert.Micron2Millimeter((double)node.MinZ.Value)};"));
        }
        if (node.MaxZ.Value < tableEndPosition)
        {
            stringBuilder.Append(string.Format($"Table end position cannot be greater  than {UnitConvert.Micron2Millimeter((double)node.MaxZ.Value)};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateTableAcceleration(uint tableAcceleration)
    {
        StringBuilder stringBuilder = new StringBuilder();
        TableInfo node = SystemConfig.TableConfig.Table;

        if (node.MaxZAcc.Value < tableAcceleration)
        {
            stringBuilder.Append(string.Format($"Table acceleration cannot be greater than {node.MaxZAcc.Value};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateTableSpeed(uint tableSpeed)
    {
        StringBuilder stringBuilder = new StringBuilder();
        TableInfo node = SystemConfig.TableConfig.Table;

        if (node.MinZSpeed.Value > tableSpeed)
        {
            stringBuilder.Append(string.Format($"Table speed cannot be smaller than {node.MinZSpeed.Value};"));
        }
        if (node.MaxZSpeed.Value < tableSpeed)
        {
            stringBuilder.Append(string.Format($"Table speed cannot be greater than {node.MaxZSpeed.Value};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateTableMaxLoad(double? tableLoad)
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (tableLoad is null || tableLoad == 0)
        {
            return string.Empty;
        }
        TableInfo node = SystemConfig.TableConfig.Table;
        if (node.MaxLoad.Min > tableLoad)
        {
            stringBuilder.Append(string.Format($"Patient weight cannot be smaller than {node.MaxLoad.Min};"));
        }
        if (node.MaxLoad.Max < tableLoad)
        {
            stringBuilder.Append(string.Format($"Patient weight cannot be greater than {node.MaxLoad.Max};"));
        }
        return stringBuilder.ToString();
    }

    private string ValidateVerticalPosition(uint verticalPosition)
    {
        StringBuilder stringBuilder = new StringBuilder();
        TableInfo node = SystemConfig.TableConfig.Table;
        if (node.MinY.Value > verticalPosition)
        {
            stringBuilder.Append(string.Format($"table vertical position cannot be smaller than {node.MinY.Value};"));
        }
        if (node.MaxY.Value < verticalPosition)
        {
            stringBuilder.Append(string.Format($"table vertical position cannot be greater than {node.MaxY.Value};"));
        }
        return stringBuilder.ToString();
    }
}