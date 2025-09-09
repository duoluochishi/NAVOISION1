using NV.CT.Service.HardwareTest.Share.Enums.Integrations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition.Abstractions
{
    public abstract class AbstractCalibration
    {
        public CalibrationType MajorCalibrationType { get; set; } 
        public string? CalibrationTableRootPath { get; set; } = string.Empty;
        public string? CalibrationTableFileName { get; set; } = string.Empty;
        public abstract Task<bool> ExecuteCalibrationAsync(List<AbstractRawDataInfo> rawDataSet);
    }
}
