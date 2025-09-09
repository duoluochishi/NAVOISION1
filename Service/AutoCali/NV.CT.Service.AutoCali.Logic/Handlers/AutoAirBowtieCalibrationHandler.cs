using NV.CT.FacadeProxy.Common.Models;
using System.Threading.Tasks;

namespace NV.CT.Service.AutoCali.Logic.Handlers
{
    public class AutoAirBowtieCalibrationHandler
    {
        public ScanReconParam X_ScanReconParam { get; set; }
        public AutoAirBowtieCalibrationHandler(ScanReconParam scanReconParam)
        {
            this.X_ScanReconParam = scanReconParam;
        }

        public Task Scan()
        {
            return Task.FromResult(0);
        }

        public Task Calibration()
        {
            return Task.FromResult(0);
        }

    }
}
