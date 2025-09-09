//-----------------------------------------------------------------------
// <copyright company="纳米维景">
// 版权所有(C) 2023,纳米维景(上海)医疗科技有限公司
// </copyright>
//-----------------------------------------------------------------------
// <summary>
//     修改日期           版本号       创建人
// 2023/4/19 8:39:28     V1.0.0       朱正广
// </summary>
//-----------------------------------------------------------------------
// <key>
//
// </key>
//-----------------------------------------------------------------------

using NV.CT.Protocol.Models;

namespace NV.CT.ProtocolManagement.ApplicationService.Impl;

public static class ProtocolManagerHelper
{
    public static void ResetId(ReconModel reconModel)
    {
        reconModel.Descriptor.Id = Guid.NewGuid().ToString();
    }

    public static void ResetId(ScanModel scanModel)
    {
        scanModel.Descriptor.Id = Guid.NewGuid().ToString();
        scanModel.Children.ForEach(reconModel => ResetId(reconModel));
    }

    public static void ResetId(MeasurementModel measurementModel)
    {
        measurementModel.Descriptor.Id = Guid.NewGuid().ToString();
        measurementModel.Children.ForEach(scanModel => ResetId(scanModel));
    }

    public static void ResetId(FrameOfReferenceModel frameModel)
    {
        frameModel.Descriptor.Id = Guid.NewGuid().ToString();
        frameModel.Children.ForEach(measurementModel => ResetId(measurementModel));
    }

    public static void ResetId(ProtocolModel protocolModel)
    {
        protocolModel.Descriptor.Id = Guid.NewGuid().ToString();
        protocolModel.Children.ForEach(frameModel => ResetId(frameModel));
    }

    public static void ResetId(ProtocolTemplateModel templateModel)
    {
        templateModel.Descriptor.Id = Guid.NewGuid().ToString();
        ResetId(templateModel.Protocol);
    }

    public static void SetParameter(BaseModel model, ParameterModel parameter)
    {
        SetParameters(model, new List<ParameterModel> { parameter });
    }

    public static void SetParameters(BaseModel model, List<ParameterModel> parameters)
    {
        SetParameters(new Dictionary<BaseModel, List<ParameterModel>> { { model, parameters } });
    }

    public static void SetParameters(Dictionary<BaseModel, List<ParameterModel>> parameters)
    {
        foreach(var parameter in parameters)
        {
            foreach(var parameterModel in parameter.Value)
            {
                var parameterTemp = parameter.Key.Parameters.FirstOrDefault(p => p.Name == parameterModel.Name);
                if (parameterTemp is not null)
                {
                    parameter.Key.Parameters.Remove(parameterTemp);
                }
            }
            parameter.Key.Parameters.AddRange(parameter.Value);
        }
    }
}
