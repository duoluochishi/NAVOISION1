using NV.CT.Protocol.Models;

namespace NV.CT.Protocol.Interfaces
{
    public interface ILinkedModificationRule
    {
        bool CanAccept(BaseModel model, string parameterName);

        Dictionary<BaseModel, List<ParameterModel>> GetLinkedModificationParam(BaseModel model, ParameterModel parameter);
    }
}
