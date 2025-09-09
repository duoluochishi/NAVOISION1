using NV.CT.Service.HardwareTest.Share.Enums.Integrations;
using System.Threading.Tasks;

namespace NV.CT.Service.HardwareTest.Models.Integrations.DataAcquisition.Abstractions
{
    public abstract class AbstractCorrection
    {
        public CorrectionType MajorCorrectionType { get; set; }
        public bool Valid { get; set; }
        protected float[]? CorrectionTable { get; set; }
        public string? CorrectionTableRootPath { get; set; }
        public string? CorrectionTableFileName { get; set; }
        public abstract Task<bool> LoadCorrectionTableAsync();
        public abstract bool ExecuteCorrection(ref float[] image, int width, int height, int sourceID = 0);
    }

}
