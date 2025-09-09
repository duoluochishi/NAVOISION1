namespace NV.CT.Service.TubeCali.Services.Interface
{
    public interface IConfigService
    {
        (int TubeCount, int TubeInterfaceCount, int TubeCountPerTubeInterface) GetTubeAndTubeInterfaceCount();
    }
}