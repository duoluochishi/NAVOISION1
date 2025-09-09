using NV.CT.Service.QualityTest.Models;

namespace NV.CT.Service.QualityTest.Services
{
    public interface IIntegrationPhantomService
    {
        public TablePosition GetLocatePosition();
        public TablePosition GetWater30Position();
        public TablePosition GetWater20Position();
        public TablePosition GetPhysicalPosition();
        public TablePosition GetAirPosition();
    }
}