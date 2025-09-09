using NV.CT.Protocol.Models;

namespace NV.CT.Protocol.Interfaces
{
    public interface IProtocolValidationService
    {
        bool Validate(BaseModel model);
    }
}
