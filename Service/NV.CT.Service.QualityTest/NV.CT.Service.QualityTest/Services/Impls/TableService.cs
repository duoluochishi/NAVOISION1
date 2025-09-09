using NV.CT.FacadeProxy.Common.Models;
using NV.CT.Service.Common;
using NV.CT.Service.Common.Interfaces;
using NV.CT.Service.QualityTest.Models;
using NV.MPS.Environment;

namespace NV.CT.Service.QualityTest.Services.Impls
{
    internal class TableService : ITableService
    {
        private readonly ILogService _logger;
        private TablePosition _tablePos;

        public TableService(ILogService logger)
        {
            _logger = logger;
        }

        public TablePosition GetSavedTablePosition()
        {
            return _tablePos;
        }

        public void SaveCurrentTablePosition()
        {
            var horizontal = (double)DeviceSystem.Instance.Table.HorizontalPosition;
            var vertical = (double)DeviceSystem.Instance.Table.VerticalPosition;
            _tablePos.Horizontal = horizontal.Micron2Millimeter();
            _tablePos.Vertical = vertical.Micron2Millimeter();
            _logger.Info(ServiceCategory.QualityTest, $"Save Table Position. HorizontalPosition {horizontal} VerticalPosition {vertical}");
        }
    }
}