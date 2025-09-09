using NV.CT.Service.QualityTest.Models;
using NV.CT.Service.QualityTest.Models.Phantoms;

namespace NV.CT.Service.QualityTest.Services.Impls
{
    internal class IntegrationPhantomService : IIntegrationPhantomService
    {
        private readonly ITableService _tableService;
        private readonly IntegrationPhantomModel _integrationPhantom;

        public IntegrationPhantomService(ITableService tableService, IntegrationPhantomModel integrationPhantom)
        {
            _tableService = tableService;
            _integrationPhantom = integrationPhantom;
        }

        public TablePosition GetLocatePosition()
        {
            return _tableService.GetSavedTablePosition();
        }

        public TablePosition GetWater30Position()
        {
            var tablePos = _tableService.GetSavedTablePosition();
            return tablePos with { Horizontal = tablePos.Horizontal - _integrationPhantom.Water30Distance };
        }

        public TablePosition GetWater20Position()
        {
            var tablePos = _tableService.GetSavedTablePosition();
            return tablePos with { Horizontal = tablePos.Horizontal - _integrationPhantom.Water20Distance };
        }

        public TablePosition GetPhysicalPosition()
        {
            var tablePos = _tableService.GetSavedTablePosition();
            return tablePos with { Horizontal = tablePos.Horizontal - _integrationPhantom.PhysicalDistance };
        }

        public TablePosition GetAirPosition()
        {
            var tablePos = _tableService.GetSavedTablePosition();
            return tablePos with { Horizontal = tablePos.Horizontal - _integrationPhantom.AirDistance };
        }
    }
}