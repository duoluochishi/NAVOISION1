using NV.CT.Service.TubeCali.Services.Interface;
using NV.MPS.Configuration;

namespace NV.CT.Service.TubeCali.Services
{
    public class ConfigService : IConfigService
    {
        public (int TubeCount, int TubeInterfaceCount, int TubeCountPerTubeInterface) GetTubeAndTubeInterfaceCount()
        {
            var tubeCount = (int)SystemConfig.SourceComponentConfig.SourceComponent.SourceCount;
            var tubeInterfaceCount = (int)SystemConfig.SourceComponentConfig.SourceComponent.TubeInterfaceCount;
            var tubeCountPerTubeInterface = tubeCount / tubeInterfaceCount;
            return (tubeCount, tubeInterfaceCount, tubeCountPerTubeInterface);
        }
    }
}