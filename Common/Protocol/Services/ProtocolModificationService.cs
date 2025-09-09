using Newtonsoft.Json;
using NV.CT.CTS;
using NV.CT.CTS.Extensions;
using NV.CT.Protocol.Interfaces;
using NV.CT.Protocol.Models;
using NV.MPS.Exception;

namespace NV.CT.Protocol.Services;
public class ProtocolModificationService : IProtocolModificationService
{

    private List<ILinkedModificationRule> linkedModificationRules = new List<ILinkedModificationRule>();

    public event EventHandler<EventArgs<(BaseModel, List<string>)>>? ParameterChanged;

    public event EventHandler<EventArgs<(BaseModel, List<string>)>>? VolumnChanged;

    public event EventHandler<EventArgs<BaseModel>>? DescriptorChanged;

    private List<string> ReconVolumeRelatedParameterNameList = new List<string>
    {
        ProtocolParameterNames.RECON_CENTER_FIRST_X,
        ProtocolParameterNames.RECON_CENTER_FIRST_Y,
        ProtocolParameterNames.RECON_CENTER_FIRST_Z,
        ProtocolParameterNames.RECON_CENTER_LAST_X,
        ProtocolParameterNames.RECON_CENTER_LAST_Y,
        ProtocolParameterNames.RECON_CENTER_LAST_Z,
        ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_X,
        ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Y,
        ProtocolParameterNames.RECON_FOV_DIRECTION_HORIZONTAL_Z,
        ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_X,
        ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Y,
        ProtocolParameterNames.RECON_FOV_DIRECTION_VERTICAL_Z,
        ProtocolParameterNames.RECON_FOV_LENGTH_HORIZONTAL,
        ProtocolParameterNames.RECON_FOV_LENGTH_VERTICAL,
    };

    public void SetModelName(BaseModel model, string modelName)
    {
        model.Descriptor.Name = modelName;
        DescriptorChanged?.Invoke(this, new EventArgs<BaseModel>(model));
    }

    public void AddLinkModificationRules(ILinkedModificationRule rule)
    {
        if (linkedModificationRules.Contains(rule)) return;
        linkedModificationRules.Add(rule);
    }

    /// <summary>
    /// 修改一组参数，不参与关联参数修改过程。
    /// </summary>
    /// <param name="model"></param>
    /// <param name="parameters"></param>
    public void SetParameters(BaseModel model, List<ParameterModel> parameters)
    {
        var allResults = new Dictionary<BaseModel, List<ParameterModel>>();
        allResults.Add(model, parameters);

        SetParameters(allResults);
    }

    /// <summary>
    /// 修改一组参数，不参与关联参数修改过程。
    /// </summary>
    /// <param name="model"></param>
    /// <param name="parameters"></param>
    public void SetParameters(Dictionary<BaseModel, List<ParameterModel>> modelParameters)
    {
        var volumnChangedRecons = new List<ReconModel>();

        foreach (var item in modelParameters)
        {
            foreach (var parameter in item.Value)
            {
                var tempParameter = item.Key.Parameters.FirstOrDefault(p => p.Name == parameter.Name);
                if (tempParameter is not null)
                {
                    item.Key.Parameters.Remove(tempParameter);
                }
            }
            item.Key.Parameters.AddRange(item.Value);
            var names = item.Value.Select(p => p.Name).ToList();
            ParameterChanged?.Invoke(this, new EventArgs<(BaseModel, List<string>)>((item.Key, names)));

            //统计所有Volume变化的recon
            if (item.Key is ReconModel && item.Value.Any(x => ReconVolumeRelatedParameterNameList.Contains(x.Name)))
            {
                volumnChangedRecons.Add(item.Key as ReconModel);
            }
        }

        if (!volumnChangedRecons.IsEmpty())
        {
            //弹出以ScanModel为单位的Volumn变化事件。
            var groupedList = volumnChangedRecons.GroupBy(x => x.Parent);

            foreach (var groupedRecons in groupedList)
            {
                var scanModel = groupedRecons.First().Parent;
                var reconIDs = groupedRecons.Select(x => x.Descriptor.Id).ToList();

                VolumnChanged?.Invoke(this, new EventArgs<(BaseModel, List<string>)>((scanModel, reconIDs)));
            }
        }
    }

    /// <summary>
    /// 修改某个模型的指定参数。可以应用关联参数修改。
    /// </summary>
    /// <param name="model"></param>
    /// <param name="parameters"></param>
    public void SetParameter(BaseModel model, ParameterModel parameter, bool linkModification = true)
    {
        var allResults = new Dictionary<BaseModel, List<ParameterModel>>();
        allResults.Add(model, new List<ParameterModel> { parameter });   //添加初始参数变化

        if (linkModification)
        {
            foreach (var rule in linkedModificationRules)
            {
                if (rule.CanAccept(model, parameter.Name))
                {
                    var ruleResults = rule.GetLinkedModificationParam(model, parameter);
                    //合并结果
                    foreach (var key in ruleResults.Keys)
                    {
                        if (!allResults.ContainsKey(key))
                        {
                            allResults.Add(key, ruleResults[key]);
                        }
                        else
                        {
                            foreach (var ruleParam in ruleResults[key])
                            {
                                var match = allResults[key].SingleOrDefault(x => x.Name == ruleParam.Name);
                                if (match is not null)
                                {
                                    match.Value = ruleParam.Value;
                                }
                                else
                                {
                                    allResults[key].Add(ruleParam);
                                }
                            }
                        }
                    }
                }
            }
        }

        SetParameters(allResults);
    }

    //todo: 通过转化调用SetParameter(BaseModel model, ParameterModel parameter, bool linkModification)，从而应用那啥。
    public void SetParameter<TParameter>(BaseModel model, string parameterName, TParameter parameter, bool linkModification = true)
    {
        var parameterModel = new ParameterModel();
        parameterModel.Name = parameterName;

        var parameterType = typeof(TParameter);

        if (parameterType.IsArray || parameterType.IsGenericType)
        {
            //todo: 待验证，XML的转义问题
            parameterModel.Value = JsonConvert.SerializeObject(parameter);
        }
        else if (parameter is not null)
        {
            parameterModel.Value = parameter.ToString();
        }

        SetParameter(model, parameterModel, linkModification);

    }

    /// <summary>
    /// 此方法暂不支持ReconModel参数设置，待后续完善
    /// </summary>
    /// <typeparam name="TParameter"></typeparam>
    /// <param name="instance"></param>
    /// <param name="modelId"></param>
    /// <param name="parameterName"></param>
    /// <param name="parameter"></param>
    /// <exception cref="NanoException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public void SetParameter<TParameter>(ProtocolModel instance, string modelId, string parameterName, TParameter parameter)
    {
        if (instance is null || instance.Children.IsEmpty())
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, "InstanceProtocol"), new ArgumentNullException("instance"));
        }

        if (string.IsNullOrEmpty(modelId))
        {
            throw new NanoException(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, string.Format(ErrorCodes.ErrorCodeResource.MCS_Common_Argument_IsNull_Code, "Model Id"), new ArgumentNullException("modelId"));
        }

        var items = ProtocolHelper.Expand(instance);
        BaseModel model = items.FirstOrDefault(item => item.Frame.Descriptor.Id == modelId).Frame;
        if (model is null)
        {
            model = items.FirstOrDefault(item => item.Measurement.Descriptor.Id == modelId).Measurement;
        }
        if (model is null)
        {
            model = items.FirstOrDefault(item => item.Scan.Descriptor.Id == modelId).Scan;
        }

        //TODO: ReconModel的处理暂缓实现

        if (model is null)
        {
            throw new NanoException("", $"The id of model ({modelId}) is error!", new NotSupportedException("recon model"));
        }

        SetParameter(model, parameterName, parameter);
    }
}