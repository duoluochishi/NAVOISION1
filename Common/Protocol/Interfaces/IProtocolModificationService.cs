using NV.CT.CTS;
using NV.CT.Protocol.Models;

namespace NV.CT.Protocol.Interfaces
{
    public interface IProtocolModificationService
    {
        void SetParameters(BaseModel model, List<ParameterModel> parameters);

        void SetParameters(Dictionary<BaseModel, List<ParameterModel>> modelParameters);

        void SetParameter(BaseModel model, ParameterModel parameter,bool linkModification = true);

        void SetParameter<TParameter>(BaseModel model, string parameterName, TParameter parameter, bool linkModification=true);

        void SetParameter<TParameter>(ProtocolModel instance, string modelId, string parameterName, TParameter parameter);

        event EventHandler<EventArgs<(BaseModel, List<string>)>> ParameterChanged;

        event EventHandler<EventArgs<(BaseModel, List<string>)>> VolumnChanged;

        void SetModelName(BaseModel model, string modelName);

        event EventHandler<EventArgs<BaseModel>> DescriptorChanged;
    }
}
